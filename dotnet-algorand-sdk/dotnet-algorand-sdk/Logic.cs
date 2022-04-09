using System.Collections.Generic;
using System;
using Newtonsoft.Json;
using System.IO;
using System.Reflection;

namespace Algorand
{
    /// <summary>
    /// Logic class provides static checkProgram function that can be used for client-side program validation for size and execution cost.
    /// </summary>
    public class Logic
    {
        private static int MAX_COST = 20000;
        private static int MAX_LENGTH = 1000;
        private const int INTCBLOCK_OPCODE = 32;
        private const int BYTECBLOCK_OPCODE = 38;
        private const int PUSHBYTES_OPCODE = 128;
        private const int PUSHINT_OPCODE = 129;

        private static LangSpec langSpec;
        private static Operation[] opcodes;

        public Logic()
        {
        }
        /// <summary>
        /// Performs basic program validation: instruction count and program cost
        /// </summary>
        /// <param name="program"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static bool CheckProgram(byte[] program, List<byte[]> args)
        {
            return ReadProgram(program, args).good;
        }
        internal static string GetFromResources(string resourceName)
        {
            Assembly assem = Assembly.GetExecutingAssembly();

            using (Stream stream = assem.GetManifestResourceStream(assem.GetName().Name + '.' + resourceName))
            {
                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        private class Operation
        {
            public int Opcode;
            public string Name;
            public int Cost;
            public ulong Size;
            public string Returns;
            public string[] ArgEnum;
            public string ArgEnumTypes;
            public string Doc;
            public string ImmediateNote;
            public string[] Group;
        }

        private class LangSpec
        {
            public ulong EvalMaxVersion;
            public int LogicSigVersion;
            public Operation[] Ops;
        }

        /// <summary>
        /// Given a varint, get the integer value
        /// </summary>
        /// <param name="buffer">serialized varint</param>
        /// <param name="bufferOffset">position in the buffer to start reading from</param>
        /// <returns>pair of values in in array: value, read size</returns>
        public static VarintResult GetUVarint(byte[] buffer, int bufferOffset)
        {
            int x = 0;
            int s = 0;
            for (int i = 0; i < buffer.Length; i++)
            {
                int b = buffer[bufferOffset + i] & 0xff;
                if (b < 0x80)
                {
                    if (i > 9 || i == 9 && b > 1)
                    {
                        return new VarintResult(0, -(i + 1));
                    }
                    return new VarintResult(x | (b & 0xff) << s, i + 1);
                }
                x |= ((b & 0x7f) & 0xff) << s;
                s += 7;
            }
            return new VarintResult();
        }
        /// <summary>
        /// Given a varint, get the integer value
        /// </summary>
        /// <param name="buffer">serialized varint</param>
        /// <param name="bufferOffset">position in the buffer to start reading from</param>
        /// <returns>pair of values in in array: value, read size</returns>
        public static VarintResult GetUVarint(byte[] buffer, ulong bufferOffset)
        {
            ulong x = 0;
            int s = 0;
            for (int i = 0; i < buffer.Length; i++)
            {
                ulong ii = Convert.ToUInt32(i);
                int b = buffer[bufferOffset + ii] & 0xff;
                if (b < 0x80)
                {
                    if (i > 9 || i == 9 && b > 1)
                    {
                        return new VarintResult(0, -(i + 1));
                    }
                    var bb1 = Convert.ToUInt64(b & 0xff);
                    var bb2 = bb1 << s;
                    var value = x | bb2;
                    if (value < 0)
                    {
                        throw new Exception("Error, value cannot be lower than zero");
                    }
                    return new VarintResult(Convert.ToUInt64(value), i + 1);
                }
                var b1 = Convert.ToUInt64(b & 0x7f);
                var b2 = b1 & 0xff;
                var b3 = b2 << s;
                var xx = x | b3;
                x = Convert.ToUInt64(xx);
                s += 7;
            }
            return new VarintResult();
        }
        public static ulongConstBlock ReadIntConstBlock(byte[] program, ulong pc)
        {
            List<ulong> results = new List<ulong>();

            ulong size = 1;
            VarintResult result = GetUVarint(program, pc + size);
            if (result.length <= 0)
            {
                throw new ArgumentException(
                    string.Format("could not decode int const block at pc=%d", pc)
                );
            }
            size += result.length;
            ulong numInts = result.value;
            ulong legth = Convert.ToUInt64(program.Length);
            for (ulong i = 0; i < numInts; i++)
            {
                if (pc + size >= legth)
                {
                    throw new ArgumentException("int const block exceeds program length");
                }
                result = GetUVarint(program, pc + size);
                if (result.length <= 0)
                {
                    throw new ArgumentException(
                        string.Format("could not decode int const[%d] block at pc=%d", i, pc + size)
                    );
                }
                size += result.length;
                results.Add(result.value);
            }
            return new ulongConstBlock(size, results);
        }

        public static ByteConstBlock ReadByteConstBlock(byte[] program, ulong pc)
        {
            List<byte[]> results = new List<byte[]>();
            ulong size = 1;
            VarintResult result = GetUVarint(program, pc + size);
            if (result.length <= 0)
            {
                throw new ArgumentException(
                    string.Format("could not decode byte[] const block at pc=%d", pc)
                );
            }
            size += result.length;
            ulong numInts = result.value;
            ulong legth = Convert.ToUInt64(program.Length);

            for (ulong i = 0; i < numInts; i++)
            {
                if (pc + size >= legth)
                {
                    throw new ArgumentException("byte[] const block exceeds program length");
                }
                result = GetUVarint(program, pc + size);
                if (result.length <= 0)
                {
                    throw new ArgumentException(
                        string.Format("could not decode byte[] const[%d] block at pc=%d", i, pc + size)
                    );
                }
                size += result.length;
                if (pc + size >= legth)
                {
                    throw new ArgumentException("byte[] const block exceeds program length");
                }
                byte[] buff = new byte[result.value];
                JavaHelper<byte>.SystemArrayCopy(program, Convert.ToInt32(pc + size), buff, 0, Convert.ToInt32(result.value));
                results.Add(buff);
                size += result.value;
            }
            return new ByteConstBlock(size, results);
        }

        protected static ulongConstBlock ReadPushIntOp(byte[] program, ulong pc)
        {
            ulong size = 1;
            VarintResult result = GetUVarint(program, pc + size);
            if (result.length <= 0)
            {
                throw new ArgumentException($"could not decode push int const at pc={pc}");
            }
            size += result.length;

            return new ulongConstBlock(size, new List<ulong>() { result.value });
        }

        protected static ByteConstBlock ReadPushByteOp(byte[] program, ulong pc)
        {
            ulong size = 1;
            VarintResult result = GetUVarint(program, pc + size);
            if (result.length <= 0)
            {
                throw new ArgumentException($"could not decode push []byte const size at pc={pc}");
            }

            size += result.length;
            ulong legth = Convert.ToUInt64(program.Length);

            if (pc + size + result.value > legth)
            {
                throw new ArgumentException("pushbytes ran past end of program");
            }

            byte[] buff = new byte[result.value];
            JavaHelper<byte>.SystemArrayCopy(program, Convert.ToInt32(pc + size), buff, 0, Convert.ToInt32(result.value));
            size += result.value;

            var res = new List<byte[]>();
            res.Add(buff);
            return new ByteConstBlock(size, res);
        }

        /// <summary>
        /// Performs basic program validation: instruction count and program cost
        /// </summary>
        /// <param name="program">program Program to validate</param>
        /// <param name="args">Program arguments to validate</param>
        /// <returns></returns>
        public static ProgramData ReadProgram(byte[] program, List<byte[]> args)
        {
            List<ulong> ints = new List<ulong>();
            List<byte[]> bytes = new List<byte[]>();

            if (langSpec == null)
            {
                LoadLangSpec();
            }

            VarintResult result = GetUVarint(program, 0);
            ulong vlen = result.length;
            if (vlen <= 0)
            {
                throw new ArgumentException("version parsing error");
            }

            ulong version = result.value;
            if (version > langSpec.EvalMaxVersion)
            {
                throw new ArgumentException("unsupported version");
            }

            if (args == null)
            {
                args = new List<byte[]>();
            }

            int cost = 0;
            int length = program.Length;
            for (int i = 0; i < args.Count; i++)
            {
                length += args[i].Length;
            }

            if (length > MAX_LENGTH)
            {
                throw new ArgumentException("program too long");
            }

            if (opcodes == null)
            {
                opcodes = new Operation[256];
                for (int i = 0; i < langSpec.Ops.Length; i++)
                {
                    Operation op = langSpec.Ops[i];
                    opcodes[op.Opcode] = op;
                }
            }

            ulong pc = vlen;
            while (pc < Convert.ToUInt64(program.Length))
            {
                int opcode = program[pc] & 0xFF;
                Operation op = opcodes[opcode];
                if (op == null)
                {
                    throw new ArgumentException("invalid instruction: " + opcode);
                }

                cost += op.Cost;
                ulong size = op.Size;
                if (size == 0)
                {
                    switch (op.Opcode)
                    {
                        case INTCBLOCK_OPCODE:
                            ulongConstBlock intsBlock = ReadIntConstBlock(program, pc);
                            size += intsBlock.size;
                            ints.AddRange(intsBlock.results);
                            break;
                        case BYTECBLOCK_OPCODE:
                            ByteConstBlock bytesBlock = ReadByteConstBlock(program, pc);
                            size += bytesBlock.size;
                            bytes.AddRange(bytesBlock.results);
                            break;
                        case PUSHINT_OPCODE:
                            ulongConstBlock pushInt = ReadPushIntOp(program, pc);
                            size += pushInt.size;
                            ints.AddRange(pushInt.results);
                            break;
                        case PUSHBYTES_OPCODE:
                            ByteConstBlock pushBytes = ReadPushByteOp(program, pc);
                            size += pushBytes.size;
                            bytes.AddRange(pushBytes.results);
                            break;
                        default:
                            throw new ArgumentException("invalid instruction: " + op.Opcode);
                    }
                }
                pc += size;
            }
            // costs calculated dynamically starting in v4
            if (version < 4 && cost > MAX_COST)
            {
                throw new ArgumentException("program too costly to run");
            }

            return new ProgramData(true, ints, bytes);
        }
        private static void LoadLangSpec()
        {
            if (langSpec != null)
            {
                return;
            }
            string json = GetFromResources("langspec.json");
            langSpec = JsonConvert.DeserializeObject<LangSpec>(json);
        }
        /// <summary>
        /// TEAL supported version
        /// </summary>
        /// <returns>int</returns>
        public static int GetLogicSigVersion()
        {
            if (langSpec == null)
            {
                LoadLangSpec();
            }
            return langSpec.LogicSigVersion;
        }

        /// <summary>
        /// Retrieves max supported version of TEAL evaluator
        /// </summary>
        /// <returns></returns>
        public static ulong GetEvalMaxVersion()
        {
            if (langSpec == null)
            {
                LoadLangSpec();
            }
            return langSpec.EvalMaxVersion;
        }

        public class Uvarint
        {
            Uvarint()
            {
            }

            public static VarintResult GetUvarint(byte[] data)
            {
                int x = 0;
                int s = 0;

                for (int i = 0; i < data.Length; ++i)
                {
                    int b = data[i] & 255;
                    if (b < 128)
                    {
                        if (i <= 9 && (i != 9 || b <= 1))
                        {
                            return new VarintResult(x | (b & 255) << s, i + 1);
                        }

                        return new VarintResult(0, -(i + 1));
                    }

                    x |= (b & 127 & 255) << s;
                    s += 7;
                }

                return new VarintResult();
            }
        }
    }   
    
    public class VarintResult
    {
        public ulong value;
        public ulong length;

        public VarintResult(int value, int length)
        {
            if (length < 0) throw new Exception("Variant result length lower than zero");
            if (value < 0) throw new Exception("Variant result value lower than zero");
            this.value = Convert.ToUInt64(value);
            this.length = Convert.ToUInt64(length);
        }
        public VarintResult(ulong value, int length)
        {
            if (length < 0) throw new Exception("Variant result length lower than zero");
            this.value = value;
            this.length = Convert.ToUInt64(length);
        }
        public VarintResult(ulong value, ulong length)
        {
            this.value = value;
            this.length = length;
        }

        public VarintResult()
        {
            this.value = 0;
            this.length = 0;
        }
    }

    public class IntConstBlock
    {
        public int size;
        public List<int> results;

        public IntConstBlock(int size, List<int> results)
        {
            this.size = size;
            this.results = results;
        }
    }
    public class ulongConstBlock
    {
        public ulong size;
        public List<ulong> results;

        public ulongConstBlock(ulong size, List<ulong> results)
        {
            this.size = size;
            this.results = results;
        }
    }

    public class ByteConstBlock
    {
        public ulong size;
        public List<byte[]> results;

        public ByteConstBlock(ulong size, List<byte[]> results)
        {
            this.size = size;
            this.results = results;
        }
    }

    /// <summary>
    /// Metadata related to a teal program.
    /// </summary>
    public class ProgramData
    {
        public bool good;
        public List<ulong> intBlock;
        public List<byte[]> byteBlock;

        public ProgramData(bool good, List<ulong> intBlock, List<byte[]> byteBlock)
        {
            this.good = good;
            this.intBlock = intBlock;
            this.byteBlock = byteBlock;
        }
    }
}
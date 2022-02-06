using VoteCoinMonitor.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoteCoinApi.Model.Config;

namespace VoteCoinMonitor.Utils
{
    internal class AlgoExtensions
    {

        /// <summary>
        /// encode and submit signed transactions using algod v2 api
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="signedTx"></param>
        /// <returns></returns>
        public static async Task<Algorand.V2.Algod.Model.PostTransactionsResponse> SubmitTransactions(Algorand.V2.Algod.DefaultApi instance, IEnumerable<Algorand.SignedTransaction> signedTxs) //throws Exception
        {
            List<byte> byteList = new List<byte>();
            foreach (var signedTx in signedTxs)
            {
                byteList.AddRange(Algorand.Encoder.EncodeToMsgPack(signedTx));
            }
            using (MemoryStream ms = new MemoryStream(byteList.ToArray()))
            {
                return await instance.TransactionsAsync(ms);
            }
        }
        private static byte[] AVoteText = null;
        public static bool IsAvote(byte[] note)
        {
            if (AVoteText == null) AVoteText = Encoding.ASCII.GetBytes("avote-");
            if (note == null) return false;
            if (note.Length < 10) return false;
            if (note[0] == AVoteText[0]
                && note[1] == AVoteText[1]
                && note[2] == AVoteText[2]
                && note[3] == AVoteText[3]
                && note[4] == AVoteText[4]
                ) return true;
            return false;
        }
    }
}

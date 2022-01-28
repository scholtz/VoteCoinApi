namespace VoteCoinApi.Model
{
    public class SpaceBase
    {
        public string Name { get; set; }
        public ulong Asa { get; internal set; }
        public string Unit { get; internal set; }
        public string IconPath { get; internal set; }
        public decimal Order { get; set; }
        public string Url { get; set; }
        public bool IsVerified { get; set; }
    }
}

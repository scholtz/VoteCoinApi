namespace VoteCoinApi.Model
{
    public class SpaceWithIcon : SpaceBase
    {
        public string IconMimeType { get; internal set; }
        public byte[] Icon { get; internal set; }
    }
}

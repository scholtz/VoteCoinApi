namespace VoteCoinApi.Model
{
    public class TinyInfoCover
    {
        public int Count { get; set; }  
        public string next { get; set; }        
        public string previous { get; set; }    
        public List<TinyInfo> Results { get; set; } 
    }
}

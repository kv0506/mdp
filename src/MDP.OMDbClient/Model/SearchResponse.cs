namespace MDP.OMDb.Model;

public class SearchResponse : OMDbResponseBase
{
    public IList<OMDbMovie> Search { get; set; }
}
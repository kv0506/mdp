namespace MDP.ServiceModel;

public class Video
{
    public string Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public ThumbnailDetails Thumbnails { get; set; }
}
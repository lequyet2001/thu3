namespace thu3.Model
{
    public class Video
    {
        public string id { get; set; } = string.Empty;
        public string video { get; set; } = string.Empty;
        public string id_post { get; set; } = string.Empty;
        public string thumbnail { get; set; } = string.Empty;
        public Dictionary<string, object> ConvertToDictionary()
        {
            Dictionary<string, object> properties = new Dictionary<string, object>();

            properties.Add(nameof(id), id);
            properties.Add(nameof(video), video);
            properties.Add(nameof(id_post), id_post);
            properties.Add(nameof(thumbnail), thumbnail);

            return properties;
        }
    }
}

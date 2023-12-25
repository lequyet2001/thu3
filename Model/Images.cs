namespace thu3.Model
{
    public class Images
    {
        public string id { get; set; } = string.Empty;
        public string image { get; set; } = string.Empty;
        public string index {  get; set; } = string.Empty;
        public string id_post { get; set; } = string.Empty;
        public Dictionary<string, object> ConvertToDictionary()
        {
            Dictionary<string, object> properties = new Dictionary<string, object>();

            properties.Add(nameof(id), id);
            properties.Add(nameof(image), image);
            properties.Add(nameof(index), index);
            properties.Add(nameof(id_post), id_post);

            return properties;
        }
    }
}

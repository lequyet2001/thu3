namespace thu3.Model
{
    public class Category
    {
        public string id { get; set; }
        public string name { get; set; }
        public string has_name { get; set; }
        public Dictionary<string, object> ConvertToDictionary()
        {
            Dictionary<string, object> properties = new Dictionary<string, object>();

            properties.Add(nameof(id), id);
            properties.Add(nameof(name), name);
            properties.Add(nameof(has_name), has_name);

            return properties;
        }
    }
}

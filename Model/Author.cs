using System.Drawing;

namespace thu3.Model
{
    public class Author
    {
        public string id {  get; set; }=string.Empty;
        public string name { get; set; } = string.Empty;
        public string avatar { get; set; } = string.Empty;
        public string coints { get; set; } = string.Empty;
        public List<string>   listing   {  get; set; }
        public Dictionary<string, object> ConvertToDictionary()
        {
            Dictionary<string, object> properties = new Dictionary<string, object>();

            properties.Add(nameof(id), id);
            properties.Add(nameof(name), name);
            properties.Add(nameof(avatar), avatar);
            properties.Add("Coins", coints);
            properties.Add($"{nameof(listing)}", listing);
            // Thêm các thuộc tính khác tùy theo cần thiết

            return properties;
        }
    }
}

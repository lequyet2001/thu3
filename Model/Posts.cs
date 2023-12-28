using Thu6.model;

namespace thu3.Model
{
    public class Posts
    {
        public string id { get; set; } =string.Empty;
        public string id_modified { get; set; }=string.Empty;
        public string name {  get; set; } = string.Empty;
        public DateTime created { get; set; }
        public string described {  get; set; } = string.Empty;
        public DateTime modified { get; set; } 
        public int fake { get; set; } = 0;
        public int trust { get; set; } = 0;
        public int kudos { get; set; } = 0;
        public int disappointed { get; set; } = 0;
        public int is_rate { get; set; } =  0;
        public int is_marked { get; set; } = 0;
        public Video video { get; set; }
        public List<Images> image { get; set; } 
        public Author author { get; set; } 
        public Category category {  get; set; }
        public string state { get; set; } = string.Empty;
        public int is_blocked { get; set; }
        public Boolean can_edit { get; set; } = false;
        public int comments { get; set; } = 0;
        public int banned { get; set; } 
        public int can_mark { get; set; } = 1;
        public int can_rate { get; set; } = 1;
        public string uri { get; set; }= string.Empty;
        public List<string> message { get; set; } 

        public string id_user { get; set; } = string.Empty;
        public string status { get; set; }= string.Empty;

        public Dictionary<string, object> ConvertToDictionary()
        {
            Dictionary<string, object> properties = new Dictionary<string, object>();
            
            properties.Add("Id", id.ToString());
            properties.Add("Id_modified", id_modified.ToString());
            properties.Add("Name",name.ToString());
            properties.Add("Created", created.ToString());
            properties.Add("Described", described.ToString());
            properties.Add("Modified", modified.ToString());
            properties.Add("Fake", fake.ToString());
            properties.Add("Trust", trust.ToString());
            properties.Add(nameof(comments), comments.ToString());
            properties.Add("Kudos", kudos.ToString());
            properties.Add("Disappointed", disappointed.ToString());
            properties.Add("Author",author.ConvertToDictionary());
            properties.Add("Video",video!=null?video.ConvertToDictionary():"null");

            List<Dictionary<string, object>> imageList = new List<Dictionary<string, object>>();
            if (image != null)
            {
                foreach (var img in image)
                {
                    imageList.Add(img.ConvertToDictionary());
                }
            }
            properties.Add(nameof(image), imageList);
            properties.Add("Is_rate", is_rate.ToString());
            properties.Add("Is_marked", is_marked.ToString());
            properties.Add("State", state.ToString());
            properties.Add("Is_blocked", is_blocked.ToString());
            properties.Add("Can_edit", can_edit.ToString());
            properties.Add("Banned", banned.ToString());
            properties.Add("Can_mark", can_mark.ToString());
            properties.Add("Can_rate", can_rate.ToString());
            properties.Add("Uri", uri.ToString());
            

            // Thêm các thuộc tính khác tùy theo cần thiết

            return properties;
        }

    }
}

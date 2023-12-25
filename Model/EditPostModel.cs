using Microsoft.AspNetCore.Mvc;

namespace thu3.Model
{
    public class EditPostModel
    {
        public string token {  get; set; } 
        public string id { get; set; }
        public string? described { get; set; }
        public string? status { get; set; }
        public List<IFormFile>? images { get; set; } // danh sách ảnh 
        public List<string>? image_del { get; set; } // danh sách ảnh xóa
        public string? video_del {  get; set; } // video xóa
        public List<int>? image_sort { get; set; } // mảng chứ vị trí ảnh
        public IFormFile? video { get; set; } // ảnh thay thế 
        public string? auto_accept { get; set; } 
        
    }
}

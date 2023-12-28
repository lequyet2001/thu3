using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using OpenAI_API;
using OpenAI_API.Completions;
namespace thu3.sp
    {

public class SearchFunction
{
    private string ApiKey { get; }
    private OpenAIAPI Api { get; }

    // Constructor với API key mặc định
    public SearchFunction(string apiKey = "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")
    {
        ApiKey = apiKey;
        Api = new OpenAIAPI(apiKey);
    }
        // Hàm sử dụng OpenAI API để xử lý chuỗi
    public async Task<string> ProcessStringWithOpenAI(string input)
        {
            // Gửi yêu cầu đến OpenAI API để xử lý chuỗi
            var prompt = $"Process the following string: {input}";
            var response = await Api.Completions.CreateCompletionAsync(prompt);

            // Lấy kết quả từ phản hồi của OpenAI API
            string processedString = response.Organization;

            return processedString;
        }


    }
}
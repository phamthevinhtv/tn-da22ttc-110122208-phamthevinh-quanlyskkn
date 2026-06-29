using System.Text;
using System.Text.Json;

namespace ql_sang_kien_kinh_nghiem.Services;

public class GeminiService
{
    private readonly IConfiguration _config;
    private readonly IWebHostEnvironment _env;
    private readonly HttpClient _httpClient;

    public GeminiService(
        IConfiguration config,
        IWebHostEnvironment env,
        HttpClient httpClient)
    {
        _config = config;
        _env = env;
        _httpClient = httpClient;
    }

    public async Task<string> TaoMoTaMonHoc(string tenNganh, string tenMon)
    {
        string apiKey = _config["Gemini:ApiKey"]!;
        string model = _config["Gemini:Model"]!;

        string path = Path.Combine(_env.ContentRootPath, "Prompts", "mo_ta_mon_hoc.txt");
        string prompt = await File.ReadAllTextAsync(path);

        prompt = prompt
            .Replace("{tenNganh}", tenNganh)
            .Replace("{tenMon}", tenMon);

        var url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}";

        var body = new
        {
            contents = new[]
            {
                new
                {
                    parts = new[]
                    {
                        new { text = prompt }
                    }
                }
            }
        };

        var json = JsonSerializer.Serialize(body);

        var response = await _httpClient.PostAsync(
            url,
            new StringContent(json, Encoding.UTF8, "application/json")
        );

        var result = await response.Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(result);

        return doc.RootElement
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString() ?? "";
    }
}
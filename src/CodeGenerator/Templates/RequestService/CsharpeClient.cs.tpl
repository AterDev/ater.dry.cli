using Share.Models.AuthDtos;
namespace Dusi.Manage.Client;

public class ManagerClient
{
    private string BaseUrl { get; set; } = "";
    public string? AccessToken { get; set; }
    public HttpClient Http { get; set; }
    public JsonSerializerOptions JsonSerializerOptions { get; set; }
    public ErrorResult? ErrorMsg { get; set; } = null;

    ${Properties}

    public ManagerClient(AuthOption option)
    {
        Http = new HttpClient()
        {
            BaseAddress = new Uri(BaseUrl),
        };
        JsonSerializerOptions = new JsonSerializerOptions()
        {
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
        };
        _ = RefreshTokenAsync(option.Key, option.Secret).Result;

        ${InitProperties}
    }

    public void SetBaseUrl(string url)
    {
        this.BaseUrl = url;
    }

}

public class ErrorResult
{
    public string? Title { get; set; }
    public string? Detail { get; set; }
    public int Status { get; set; } = 500;
    public string? TraceId { get; set; }
}

public class AuthOption
{
    public required string Key { get; set; }
    public required string Secret { get; set; }
}

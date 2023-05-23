using ${Namespace}.Services;
namespace ${Namespace};

public class ${ClassName}Client
{
    private string BaseUrl { get; set; } = "";
    public string? AccessToken { get; set; }
    public HttpClient Http { get; set; }
    public JsonSerializerOptions JsonSerializerOptions { get; set; }
    public ErrorResult? ErrorMsg { get; set; } = null;

    #region api services
${Properties}
    #endregion

    public ${ClassName}Client(string? baseUrl = null, HttpClientHandler? handler = null)
    {
        BaseUrl = baseUrl ?? "";
        Http = handler == null ?
            new HttpClient()
            {
                BaseAddress = new Uri(BaseUrl),
                Timeout = TimeSpan.FromSeconds(15)
            } :
            new HttpClient(handler)
            {
                BaseAddress = new Uri(BaseUrl),
                Timeout = TimeSpan.FromSeconds(15)
            };
        JsonSerializerOptions = new JsonSerializerOptions()
        {
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
        };

        #region api services
${InitProperties}
        #endregion
    }

    public void SetToken(string token)
    {
        Http.DefaultRequestHeaders.Clear();
        Http.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Bearer " + token);
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

using #@Namespace#.Services;
namespace #@Namespace#;

public class #@ClassName#Client
{
    public string? AccessToken { get; set; }
    private readonly HttpClient Http;
    public JsonSerializerOptions JsonSerializerOptions { get; set; }
    public ResponseContent? ResponseContent { get; set; } = null;

    #region api services
//[@Properties]
    #endregion

    public #@ClassName#Client(HttpClient http)
    {
        Http = http;
        JsonSerializerOptions = new JsonSerializerOptions()
        {
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
        };

        #region api services
//[@InitProperties]
        #endregion
    }

    public void SetToken(string token)
    {
        Http.DefaultRequestHeaders.Clear();
        Http.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Bearer " + token);
    }
}

public class ResponseContent
{
    public string? Content { get; set; }
    public int StatusCode { get; set; }
    public string? ReasonPhrase { get; set; }
}

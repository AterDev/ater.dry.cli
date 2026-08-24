namespace #@Namespace#.Services;
public class BaseService
{
    protected HttpClient Http { get; init; }
    public JsonSerializerOptions JsonSerializerOptions { get; set; }
    public ResponseContent? ResponseContent { get; set; }

    public BaseService(IHttpClientFactory httpClient)
    {
        Http = httpClient.CreateClient("#@Namespace#");
        JsonSerializerOptions = new JsonSerializerOptions()
        {
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
        };
    }

    /// <summary>
    /// json post 封装
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="route"></param>
    /// <param name="data"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected async Task<TResult?> PostJsonAsync<TResult>(string route, object? data = null, CancellationToken cancellationToken = default)
    {
        return await SendJsonAsync<TResult>(HttpMethod.Post, route, data, cancellationToken);
    }

    /// <summary>
    /// Put
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="route"></param>
    /// <param name="data"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected async Task<TResult?> PutJsonAsync<TResult>(string route, object? data = null, CancellationToken cancellationToken = default)
    {
        return await SendJsonAsync<TResult>(HttpMethod.Put, route, data, cancellationToken);
    }

    /// <summary>
    /// Patch
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="route"></param>
    /// <param name="data"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected async Task<TResult?> PatchJsonAsync<TResult>(string route, object? data = null, CancellationToken cancellationToken = default)
    {
        return await SendJsonAsync<TResult>(HttpMethod.Patch, route, data, cancellationToken);
    }

    /// <summary>
    /// get
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="route"></param>
    /// <param name="dic"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected async Task<TResult?> GetJsonAsync<TResult>(string route, Dictionary<string, string?>? dic = null, CancellationToken cancellationToken = default)
    {
        return await SendJsonAsync<TResult>(HttpMethod.Get, route, dic, cancellationToken);
    }

    /// <summary>
    /// delete
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="route"></param>
    /// <param name="data"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected async Task<TResult?> DeleteJsonAsync<TResult>(string route, Dictionary<string, string?>? dic = null, CancellationToken cancellationToken = default)
    {
        return await SendJsonAsync<TResult>(HttpMethod.Delete, route, dic, cancellationToken);
    }

    /// <summary>
    /// upload file
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="route"></param>
    /// <param name="file"></param>
    /// <param name="fileName"></param>
    /// <param name="fieldName"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected Task<TResult?> UploadFileAsync<TResult>(string route, StreamContent file, string fileName = "file", string fieldName = "file", CancellationToken cancellationToken = default)
    {
        return UploadFileAsync<TResult>(HttpMethod.Post, route, file, fileName, fieldName, cancellationToken);
    }

    protected async Task<TResult?> UploadFileAsync<TResult>(HttpMethod method, string route, StreamContent file, string fileName = "file", string fieldName = "file", CancellationToken cancellationToken = default)
    {
        var content = new MultipartFormDataContent
        {
            { file, fieldName, fileName }
        };
        return await SendMultipartAsync<TResult>(method, route, content, cancellationToken);
    }

    /// <summary>
    /// multipart/form-data 请求封装。
    /// </summary>
    protected Task<TResult?> SendMultipartAsync<TResult>(string route, MultipartFormDataContent content, CancellationToken cancellationToken = default)
    {
        return SendMultipartAsync<TResult>(HttpMethod.Post, route, content, cancellationToken);
    }

    protected async Task<TResult?> SendMultipartAsync<TResult>(HttpMethod method, string route, MultipartFormDataContent content, CancellationToken cancellationToken = default)
    {
        using (content)
        {
            using HttpResponseMessage res = await SendMultipartRequestAsync(method, route, content, cancellationToken);
            if (res.IsSuccessStatusCode)
            {
                return await ReadJsonResponseAsync<TResult>(res, cancellationToken);
            }

            await SetResponseContentAsync(res, cancellationToken);
            return default;
        }
    }

    protected Task SendMultipartAsync(string route, MultipartFormDataContent content, CancellationToken cancellationToken = default)
    {
        return SendMultipartAsync(HttpMethod.Post, route, content, cancellationToken);
    }

    protected async Task SendMultipartAsync(HttpMethod method, string route, MultipartFormDataContent content, CancellationToken cancellationToken = default)
    {
        using (content)
        {
            using HttpResponseMessage res = await SendMultipartRequestAsync(method, route, content, cancellationToken);
            if (res.IsSuccessStatusCode)
            {
                return;
            }

            await SetResponseContentAsync(res, cancellationToken);
        }
    }

    private async Task<HttpResponseMessage> SendMultipartRequestAsync(
        HttpMethod method,
        string route,
        MultipartFormDataContent content,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(method, BuildRequestUrl(route))
        {
            Content = content,
        };
        return await Http.SendAsync(request, cancellationToken);
    }

    protected static StreamContent CreateMultipartFileContent(MultipartFile file)
    {
        var content = new StreamContent(file.Content);
        if (!string.IsNullOrWhiteSpace(file.ContentType))
        {
            content.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
        }

        return content;
    }

    protected static string ToFormValue(object? value)
    {
        return value switch
        {
            null => string.Empty,
            bool boolean => boolean ? "true" : "false",
            IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture) ?? string.Empty,
            _ => value.ToString() ?? string.Empty,
        };
    }

    /// <summary>
    /// download file
    /// </summary>
    /// <param name="route"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected async Task<Stream?> DownloadFileAsync(string route, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage? res = await Http.GetAsync(BuildRequestUrl(route), cancellationToken);
        if (res != null && res.IsSuccessStatusCode)
        {
            return await res.Content.ReadAsStreamAsync(cancellationToken);
        }
        else
        {
            await SetResponseContentAsync(res!, cancellationToken);
            return default;
        }
    }

    protected static string ToUrlParameters(Dictionary<string, string?> dic)
    {
        return string.Join("&", dic.Where(d => d.Value != null)
            .Select(d => string.Format("{0}={1}", d.Key, d.Value))
            );
    }

    protected async Task SendNoContentAsync(HttpMethod method, string route, object? data = null, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(method, BuildRequestUrl(route));
        if (data != null)
        {
            request.Content = JsonContent.Create(data, data.GetType(), options: JsonSerializerOptions);
        }

        using HttpResponseMessage res = await Http.SendAsync(request, cancellationToken);
        if (res.IsSuccessStatusCode)
        {
            return;
        }

        await SetResponseContentAsync(res, cancellationToken);
    }

    protected async Task<TResult?> SendJsonAsync<TResult>(HttpMethod method, string route, object? data, CancellationToken cancellationToken = default)
    {
        route = BuildRequestUrl(route);
        HttpResponseMessage? res = null;
        if (method == HttpMethod.Post)
        {
            res = await Http.PostAsJsonAsync(route, data, JsonSerializerOptions, cancellationToken);
        }
        else if (method == HttpMethod.Put)
        {
            res = await Http.PutAsJsonAsync(route, data, JsonSerializerOptions, cancellationToken);
        }
        else if (method == HttpMethod.Patch)
        {
            res = await Http.PatchAsJsonAsync(route, data, JsonSerializerOptions, cancellationToken);
        }
        if (res != null && res.IsSuccessStatusCode)
        {
            return await ReadJsonResponseAsync<TResult>(res, cancellationToken);
        }
        else
        {
            await SetResponseContentAsync(res!, cancellationToken);
            return default;
        }
    }

    protected async Task<TResult?> SendJsonAsync<TResult>(HttpMethod method, string route, Dictionary<string, string?>? dic = null, CancellationToken cancellationToken = default)
    {
        route = BuildRequestUrl(route);
        if (dic != null)
        {
            route = route + "?" + ToUrlParameters(dic);
        }
        HttpResponseMessage? res = null;
        if (method == HttpMethod.Get)
        {
            res = await Http.GetAsync(route, cancellationToken);

        }
        else if (method == HttpMethod.Delete)
        {
            res = await Http.DeleteAsync(route, cancellationToken);

        }
        if (res != null && res.IsSuccessStatusCode)
        {
            return await ReadJsonResponseAsync<TResult>(res, cancellationToken);
        }
        else
        {
            await SetResponseContentAsync(res!, cancellationToken);
            return default;
        }
    }

    private static async Task<TResult?> ReadJsonResponseAsync<TResult>(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        if ((response.StatusCode is System.Net.HttpStatusCode.NoContent
                or System.Net.HttpStatusCode.ResetContent
                or System.Net.HttpStatusCode.NotModified)
            || response.Content.Headers.ContentLength == 0)
        {
            return default;
        }

        return await response.Content.ReadFromJsonAsync<TResult>(cancellationToken: cancellationToken);
    }

    private async Task SetResponseContentAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        ResponseContent = new ResponseContent
        {
            Content = content,
            StatusCode = (int)response.StatusCode,
            ReasonPhrase = response.ReasonPhrase,
        };
    }

    private string BuildRequestUrl(string route)
    {
        if (Uri.TryCreate(route, UriKind.Absolute, out _))
        {
            return route;
        }

        var baseAddress = Http.BaseAddress?.ToString();
        if (string.IsNullOrWhiteSpace(baseAddress))
        {
            return route;
        }

        return $"{baseAddress.TrimEnd('/')}/{route.TrimStart('/')}";
    }
}

public class ResponseContent
{
    public string Content { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public string? ReasonPhrase { get; set; }
}

public sealed record MultipartFile(Stream Content, string FileName, string? ContentType = null);

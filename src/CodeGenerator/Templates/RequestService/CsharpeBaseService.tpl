
namespace ${Namespace}.Services;
public class BaseService
{
    public HttpClient Http { get; set; }
    public JsonSerializerOptions JsonSerializerOptions { get; set; }
    public ErrorResult? ErrorMsg { get; set; } = null;

    public BaseService(HttpClient httpClient)
    {
        Http = httpClient;
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
    /// <returns></returns>
    protected async Task<TResult?> PostJsonAsync<TResult>(string route, object? data = null)
    {
        return await SendJsonAsync<TResult>(HttpMethod.Post, route, data);
    }

    /// <summary>
    /// Put
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="route"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    protected async Task<TResult?> PutJsonAsync<TResult>(string route, object? data = null)
    {
        return await SendJsonAsync<TResult>(HttpMethod.Put, route, data);
    }

    /// <summary>
    /// get
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="route"></param>
    /// <param name="dic"></param>
    /// <returns></returns>
    protected async Task<TResult?> GetJsonAsync<TResult>(string route, Dictionary<string, string?>? dic = null)
    {
        return await SendJsonAsync<TResult>(HttpMethod.Get, route, dic);
    }

    /// <summary>
    /// delete
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="route"></param>
    /// <param name="dic"></param>
    /// <returns></returns>
    protected async Task<TResult?> DeleteJsonAsync<TResult>(string route, Dictionary<string, string?>? dic = null)
    {
        return await SendJsonAsync<TResult>(HttpMethod.Delete, route, dic);
    }

    protected static string ToUrlParameters(Dictionary<string, string?> dic)
    {
        return string.Join("&", dic.Where(d => d.Value != null)
            .Select(d => string.Format("{0}={1}", d.Key, d.Value))
            );
    }

    protected async Task<TResult?> SendJsonAsync<TResult>(HttpMethod method, string route, object? data)
    {
        HttpResponseMessage? res = null;
        if (method == HttpMethod.Post)
        {
            res = await Http.PostAsJsonAsync(route, data, JsonSerializerOptions);
        }
        else if (method == HttpMethod.Put)
        {
            res = await Http.PutAsJsonAsync(route, data, JsonSerializerOptions);
        }
        if (res != null && res.IsSuccessStatusCode)
        {
            return await res.Content.ReadFromJsonAsync<TResult>();
        }
        else
        {
            ErrorMsg = await res!.Content.ReadFromJsonAsync<ErrorResult>();
            return default;
        }
    }

    protected async Task<TResult?> SendJsonAsync<TResult>(HttpMethod method, string route, Dictionary<string, string?>? dic = null)
    {
        if (dic != null)
        {
            route = route + "?" + ToUrlParameters(dic);
        }
        HttpResponseMessage? res = null;
        if (method == HttpMethod.Get)
        {
            res = await Http.GetAsync(route);

        }
        else if (method == HttpMethod.Delete)
        {
            res = await Http.DeleteAsync(route);

        }
        if (res != null && res.IsSuccessStatusCode)
        {
            return await res.Content.ReadFromJsonAsync<TResult>();
        }
        else
        {
            ErrorMsg = await res!.Content.ReadFromJsonAsync<ErrorResult>();
            return default;
        }
    }
}

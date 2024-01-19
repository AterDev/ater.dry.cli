using Polly;
using Polly.Extensions.Http;

namespace ${Namespace};
public static class Extension
{
    public static IServiceCollection Add${Namespace}(this IServiceCollection services, Action<HttpClient> option)
    {
        services.AddSingleton<InterceptHttpHandler>();
        services.AddHttpClient("${Namespace}", option)
            .AddHttpMessageHandler<InterceptHttpHandler>()
            .SetHandlerLifetime(TimeSpan.FromMinutes(5))
            .AddPolicyHandler(GetRetryPolicy());
${AddServices}
        return services;
    }

    /// <summary>
    /// 失败重试策略
    /// </summary>
    /// <returns></returns>
    public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(
                6,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
    }
}

/// <summary>
/// http拦截
/// </summary>
public class InterceptHttpHandler : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var res = new HttpResponseMessage();
        try
        {
            res = await base.SendAsync(request, cancellationToken);
            return res;
        }
        catch (TaskCanceledException ex)
        {
            if (ex.CancellationToken.IsCancellationRequested)
            {
            }
            else
            {
                res.ReasonPhrase = "The operation has timed out.";
            }
            return res;
        }
    }
}

using EntityFramework.DBProvider;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Share.Options;

namespace ${Namespace};

/// <summary>
/// 应用配置常量
/// </summary>
public static class AppSetting
{
    public const string None = "none";
    public const string PostgreSQL = "postgresql";
    public const string SQLServer = "sqlserver";
    public const string Redis = "redis";
    public const string Memory = "memory";
    public const string Otlp = "otlp";

    public const string CommandDB = "CommandDb";
    public const string QueryDB = "QueryDb";
    public const string Cache = "Cache";
    public const string CacheInstanceName = "CacheInstanceName";
    public const string Logging = "Logging";
}

/// <summary>
/// 服务注册扩展
/// </summary>
public static class AppServiceCollectionExtensions
{

    /// <summary>
    /// 添加应用组件,如数据库/缓存/日志等
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    public static IServiceCollection AddAppComponents(this IServiceCollection services, IConfiguration configuration)
    {
        var components = configuration.GetSection("Components").Get<AppComponentConfig>();
        if (components != null)
        {
            var db = components.Database;
            var cache = components.Cache;
            var logging = components.Logging;
            if (db != null)
            {
                switch (db.ToLower())
                {
                    case AppSetting.PostgreSQL:
                        services.AddPgsqlDbContext(configuration);
                        break;
                    case AppSetting.SQLServer:
                        services.AddSqlServerDbContext(configuration);
                        break;
                }
            }

            if (cache != null)
            {
                switch (cache.ToLower())
                {
                    case AppSetting.Redis:
                        services.AddRedisCache(configuration);
                        break;
                    case AppSetting.Memory:
                        services.AddMemoryCache();
                        break;
                }
            }

            if (logging != null)
            {
                switch (logging.ToLower())
                {
                    case AppSetting.Otlp:
                        services.AddOpenTelemetry("MyProjectName", opt =>
                        {
                            opt.Endpoint = new Uri(configuration.GetConnectionString(AppSetting.Logging)
                                ?? "http://localhost:4317");
                        });
                        break;
                }
            }

        }
        return services;
    }

    /// <summary>
    /// add postgresql config
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    public static IServiceCollection AddPgsqlDbContext(this IServiceCollection services, IConfiguration configuration)
    {
        var commandString = configuration.GetConnectionString(AppSetting.CommandDB);
        var queryString = configuration.GetConnectionString(AppSetting.QueryDB);
        services.AddDbContextPool<QueryDbContext>(option =>
        {
            option.UseNpgsql(queryString, sql =>
            {
                sql.MigrationsAssembly("Http.API");
                sql.CommandTimeout(10);
            });
        });
        services.AddDbContextPool<CommandDbContext>(option =>
        {
            option.UseNpgsql(commandString, sql =>
            {
                sql.MigrationsAssembly("Http.API");
                sql.CommandTimeout(10);
            });
        });
        return services;
    }

    /// <summary>
    /// add sql server config
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    public static IServiceCollection AddSqlServerDbContext(this IServiceCollection services, IConfiguration configuration)
    {
        var commandString = configuration.GetConnectionString(AppSetting.CommandDB);
        var queryString = configuration.GetConnectionString(AppSetting.QueryDB);
        services.AddDbContextPool<QueryDbContext>(option =>
        {
            option.UseSqlServer(queryString, sql =>
            {
                sql.MigrationsAssembly("Http.API");
                sql.CommandTimeout(10);
            });
        });
        services.AddDbContextPool<CommandDbContext>(option =>
        {
            option.UseSqlServer(commandString, sql =>
            {
                sql.MigrationsAssembly("Http.API");
                sql.CommandTimeout(10);
            });
        });
        return services;
    }

    /// <summary>
    /// add redis cache config
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    public static IServiceCollection AddRedisCache(this IServiceCollection services, IConfiguration configuration)
    {
        return services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration.GetConnectionString(AppSetting.Cache);
            options.InstanceName = configuration.GetConnectionString(AppSetting.CacheInstanceName);
        });
    }
    /// <summary>
    /// 添加opentelemetry 服务及选项
    /// </summary>
    /// <param name="services"></param>
    /// <param name="serviceName"></param>
    /// <param name="otlpOptions"></param>
    /// <param name="loggerOptions"></param>
    /// <param name="tracerProvider"></param>
    /// <param name="meterProvider"></param>
    public static IServiceCollection AddOpenTelemetry(this IServiceCollection services,
        string serviceName,
        Action<OtlpExporterOptions>? otlpOptions = null,
        Action<OpenTelemetryLoggerOptions>? loggerOptions = null,
        Action<TracerProviderBuilder>? tracerProvider = null,
        Action<MeterProviderBuilder>? meterProvider = null)
    {
        ResourceBuilder resource = ResourceBuilder.CreateDefault()
            .AddService(serviceName: serviceName, serviceInstanceId: Environment.MachineName);

        otlpOptions ??= new Action<OtlpExporterOptions>(opt =>
        {
            opt.Endpoint = new Uri("http://localhost:4317");
        });

        loggerOptions ??= new Action<OpenTelemetryLoggerOptions>(options =>
        {
            options.SetResourceBuilder(resource);
            options.AddOtlpExporter(otlpOptions);
            //options.ParseStateValues = true;
            //options.IncludeFormattedMessage = true;
            //options.IncludeScopes = true;
#if DEBUG
            options.AddConsoleExporter();
#endif
        });
        // 返回内容最大长度截取
        var maxLength = 2048;
        tracerProvider ??= new Action<TracerProviderBuilder>(options =>
        {
            options.AddSource(serviceName)
                .SetResourceBuilder(resource)
                .AddHttpClientInstrumentation(options =>
                {
                    options.RecordException = true;
                    options.EnrichWithHttpRequestMessage = (activity, httpRequestMessage) =>
                    {
                        if (httpRequestMessage.Content != null)
                        {
                            System.Net.Http.Headers.HttpContentHeaders headers = httpRequestMessage.Content.Headers;
                            // 过滤过长或文件类型
                            var contentLength = headers.ContentLength ?? 0;
                            var contentType = headers.ContentType?.ToString();
                            if (contentLength > maxLength * 2
                            || contentType != null && contentType.Contains("multipart/form-data")) { }
                            else
                            {
                                string body = httpRequestMessage.Content.ReadAsStringAsync().Result;
                                activity.SetTag("requestBody", body);
                            }
                        }
                    };

                    options.EnrichWithHttpResponseMessage = (activity, httpResponseMessage) =>
                    {
                        // TODO:添加自定义过滤
                        if (httpResponseMessage.Content != null)
                        {
                            if (httpResponseMessage.Content.Headers?.ContentLength < maxLength)
                            {
                                // 不要使用await:The stream was already consumed. It cannot be read again
                                string body = httpResponseMessage.Content.ReadAsStringAsync().Result;
                                body = body.Length > maxLength ? body[0..maxLength] : body;
                                activity.SetTag("responseBody", body);
                            }
                        }
                    };
                    options.EnrichWithException = (activity, exception) =>
                    {
                    };
                })
                .AddAspNetCoreInstrumentation(options =>
                {
                    options.RecordException = true;
                    options.EnrichWithHttpRequest = async (activity, request) =>
                    {
                        IHeaderDictionary headers = request.Headers;
                        // 过滤过长或文件类型
                        long contentLength = request.ContentLength ?? 0;
                        string? contentType = request.ContentType?.ToString();
                        if (contentLength > maxLength * 2
                        || contentType != null && contentType.Contains("multipart/form-data"))
                        {
                            activity.SetTag("requestBody", "file upload");
                        }
                        else
                        {
                            request.EnableBuffering();
                            request.Body.Position = 0;
                            StreamReader reader = new(request.Body);
                            activity.SetTag("requestBody", await reader.ReadToEndAsync());
                            request.Body.Position = 0;
                        }
                    };

                    options.EnrichWithHttpResponse = (activity, response) =>
                    {
                    };

                    options.EnrichWithException = (activity, exception) =>
                    {
                        activity.SetTag("stackTrace", exception.StackTrace);
                        activity.SetTag("message", exception.Message);
                    };
                })
                .AddSqlClientInstrumentation()
#if DEBUG
            .AddConsoleExporter()
#endif
                .AddOtlpExporter(otlpOptions);
        });

        meterProvider = new Action<MeterProviderBuilder>(options =>
        {
            options.AddMeter(serviceName)
                .SetResourceBuilder(resource)
                .AddHttpClientInstrumentation()
                .AddAspNetCoreInstrumentation()
                .AddOtlpExporter(otlpOptions);
        });

        services.AddLogging(loggerBuilder =>
        {
            loggerBuilder.ClearProviders();
            loggerBuilder.AddOpenTelemetry(loggerOptions);
        });
        services.AddOpenTelemetry()
            .WithTracing(tracerProvider)
            .WithMetrics(meterProvider);

        return services;
    }
}

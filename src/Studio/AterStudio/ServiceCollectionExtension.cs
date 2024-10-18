using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using Application.Services;
using Ater.Web.Abstraction;
using Ater.Web.Core.Converters;
using AterStudio;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Share.Services;

namespace AterStudio;

public static class ServiceCollectionExtension
{
    /// <summary>
    /// 注册和配置Web服务依赖
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static IServiceCollection AddDefaultWebServices(this WebApplicationBuilder builder)
    {
        builder.Services.ConfigWebComponents(builder.Configuration);
        builder.Services.AddHttpContextAccessor();

        builder.Services.AddScoped<IProjectContext, ProjectContext>();
        builder.Services.AddScoped<IUserContext, UserContext>();

        builder.Services.AddScoped<CodeAnalysisService>();
        builder.Services.AddScoped<CodeGenService>();
        builder.Services.AddScoped<SolutionService>();
        builder.Services.AddSingleton<AIService>();

        builder.Services.AddManagers();

        builder.Services.AddControllers()
            .ConfigureApiBehaviorOptions(o =>
            {
                o.InvalidModelStateResponseFactory = context =>
                {
                    return new CustomBadRequest(context, null);
                };
            }).AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                options.JsonSerializerOptions.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All);
                options.JsonSerializerOptions.Converters.Add(new DateOnlyJsonConverter());
                options.JsonSerializerOptions.ReadCommentHandling = JsonCommentHandling.Skip;
            });
        return builder.Services;
    }

    public static WebApplication UseDefaultWebServices(this WebApplication app)
    {
        // 异常统一处理
        app.UseExceptionHandler(ExceptionHandler.Handler());
        if (app.Environment.IsProduction())
        {
            app.UseCors(AterConst.Default);
            // app.UseHsts();
            // app.UseHttpsRedirection();
        }
        else
        {
            app.UseCors(AterConst.Default);
#if DEBUG
            app.UseSwagger();
#endif
        }
        app.UseStaticFiles();
        app.UseRouting();
        app.MapControllers();
        app.MapFallbackToFile("index.html");

        return app;
    }

    /// <summary>
    /// 添加web服务组件，如身份认证/授权/swagger/cors
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    public static IServiceCollection ConfigWebComponents(this IServiceCollection services, IConfiguration configuration)
    {
#if DEBUG
        services.AddOpenApi();
#endif
        return services;
    }

    /// <summary>
    /// 添加本地化支持
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddLocalizer(this IServiceCollection services)
    {
        services.AddLocalization();
        services.AddRequestLocalization(options =>
        {
            // TODO:添加更多语言支持
            var supportedCultures = new[] { "zh-CN", "en-US" };
            options.SetDefaultCulture(supportedCultures[0])
                .AddSupportedCultures(supportedCultures)
                .AddSupportedUICultures(supportedCultures);
            options.FallBackToParentCultures = true;
            options.FallBackToParentUICultures = true;
            options.ApplyCurrentCultureToResponseHeaders = true;
        });
        return services;
    }

    /// <summary>
    /// 添加 jwt 验证
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(cfg =>
        {
            cfg.SaveToken = true;
            var sign = configuration.GetSection("Authentication")["Jwt:Sign"];
            if (string.IsNullOrEmpty(sign))
            {
                throw new Exception("未找到有效的Jwt配置");
            }
            cfg.TokenValidationParameters = new TokenValidationParameters()
            {
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(sign)),
                ValidIssuer = configuration.GetSection("Authentication")["Jwt:ValidIssuer"],
                ValidAudience = configuration.GetSection("Authentication")["Jwt:ValidAudiences"],
                ValidateIssuer = true,
                ValidateLifetime = true,
                RequireExpirationTime = true,
                ValidateIssuerSigningKey = true
            };
        });
        return services;
    }

    /// <summary>
    /// 添加swagger服务
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddOpenApi(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("admin", new OpenApiInfo
            {
                Title = "AterStudio API doc",
                Description = "Studio API 文档. 更新时间:" + DateTime.Now.ToString("yyyy-MM-dd H:mm:ss"),
                Version = "v1"
            });
            var xmlFiles = Directory.GetFiles(AppContext.BaseDirectory, "*.xml", SearchOption.TopDirectoryOnly);
            foreach (var item in xmlFiles)
            {
                try
                {
                    c.IncludeXmlComments(item, includeControllerXmlComments: true);
                }
                catch (Exception) { }
            }
            c.SupportNonNullableReferenceTypes();
            c.DescribeAllParametersInCamelCase();
            c.CustomOperationIds((z) =>
            {
                var descriptor = (ControllerActionDescriptor)z.ActionDescriptor;
                return $"{descriptor.ControllerName}_{descriptor.ActionName}";
            });
            c.SchemaFilter<EnumSchemaFilter>();
            c.MapType<DateOnly>(() => new OpenApiSchema
            {
                Type = "string",
                Format = "date"
            });
        });
        return services;
    }

    public static IServiceCollection AddCors(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy(AterConst.Default, builder =>
            {
                builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
            });
        });
        return services;
    }

    public static IServiceCollection AddAuthorize(this IServiceCollection services)
    {
        services.AddAuthorizationBuilder()
            .AddPolicy(AterConst.AdminUser, policy => policy.RequireRole(""));
        return services;
    }
}

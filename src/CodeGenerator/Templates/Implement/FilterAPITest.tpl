using System;
using System.Data;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Application.Services;
using Microsoft.Extensions.Configuration;
using Share.Models.AuthDtos;

namespace Http.API.Test;

/// <summary>
/// 测试筛选的请求
/// </summary>
public class FilterAPITest : BaseTest
{
    private readonly HttpClient _client;
    public FilterAPITest(WebApplicationFactory<Program> factory) : base(factory)
    {
        _client = _factory.CreateClient();
        var _config = _service.GetRequiredService<IConfiguration>();

        string? sign = _config.GetSection("Authentication")["Schemes:Bearer:Sign"];
        string? issuer = _config.GetSection("Authentication")["Schemes:Bearer:ValidIssuer"];
        string? audience = _config.GetSection("Authentication")["Schemes:Bearer:ValidAudiences"];
        JwtService jwt = new(sign, audience, issuer)
        {
            TokenExpires = 60 * 24 * 1,
        };
        string token = jwt.GetToken(Guid.NewGuid().ToString(), "Admin");
        _client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Bearer " + token);
    }


    [Theory]
    [InlineData("/api/Auth")]
    public async Task Should_QueryTokenAsync(string url)
    {
        var data = new LoginDto
        {
            UserName = "admin",
            Password = "123456",
        };
        var response = await _client.PostAsJsonAsync(url, data);
        response.EnsureSuccessStatusCode();
        var authRes = await response.Content.ReadFromJsonAsync<AuthResult>();
        Assert.NotNull(authRes);
    }

${TestContent}
}

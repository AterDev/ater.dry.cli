namespace AterStudio.Advance;

public class DusiHttpClient
{
    private readonly HttpClient Client;

    public DusiHttpClient(HttpClient client)
    {
        client.BaseAddress = new Uri("https://dusi.dev/");
        Client = client;
    }

    public void SetToken(string token)
    {
        if (!Client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", token))
        {
            Client.DefaultRequestHeaders.Clear();
            Client.DefaultRequestHeaders.Add("Authorization", token);
        }
    }

    public async Task<string?> GetTokenAsync(string username, string password)
    {
        var response = await Client.PostAsJsonAsync("/api/user/login", new
        {
            UserName = username,
            Password = password
        });
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<JsonElement>();
            return result.GetProperty("token").GetString();
        }
        return default;
    }

    public async Task<List<string>?> GetEntityAsync(string name, string description)
    {
        var response = await Client.GetAsync($"/api/EntityModel/generate?name={name}&description={description}");

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<List<string>>();
            return result;
        }
        return default;
    }
}

namespace Application.Implement;
public class UserContext : IUserContext
{
    public bool IsRole(string roleName)
    {
        throw new NotImplementedException();
    }

    public Task<bool> ExistAsync()
    {
        throw new NotImplementedException();
    }

    public string? GetIpAddress()
    {
        throw new NotImplementedException();
    }

    public Task<TUser?> GetUserAsync<TUser>() where TUser : class
    {
        throw new NotImplementedException();
    }

    public Guid UserId { get; init; }
    public string? Username { get; init; }
    public string? Email { get; set; }
    public bool IsAdmin { get; init; }
    public string? CurrentRole { get; set; }
    public List<string>? Roles { get; set; }
    public Guid? GroupId { get; init; }
}

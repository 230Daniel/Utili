namespace Utili.Database.Entities;

public class User
{
    public ulong UserId { get; internal set; }
    public string Email { get; set; }

    public User(ulong userId)
    {
        UserId = userId;
    }

    internal User() { }
}
namespace CebuFinalApp.Models;

public class UserCredentials
{
    public string email { get; set; }
    public string password { get; set; }
    public bool returnSecureToken { get; set; } = true;
}
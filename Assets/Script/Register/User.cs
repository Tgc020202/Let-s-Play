public class User
{
    // Defines
    public string username;
    public string password;
    public bool status;

    public User(string username, string password, bool status)
    {
        this.username = username;
        this.password = password;
        this.status = status;
    }
}

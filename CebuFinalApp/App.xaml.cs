using Firebase.Auth;

namespace CebuFinalApp;

public partial class App : Application
{
    // Add this line to store the client globally
    public static FirebaseAuthClient CurrentAuthClient { get; set; }
    public static string? CurrentUserId =>
    CurrentAuthClient?.User?.Uid;

    public App()
    {
        InitializeComponent();
        MainPage = new AppShell();
    }
}
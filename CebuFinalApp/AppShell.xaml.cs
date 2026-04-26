namespace CebuFinalApp;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Standard Route Registration
        Routing.RegisterRoute(nameof(MainPage), typeof(MainPage));
        Routing.RegisterRoute(nameof(FavoritesPage), typeof(FavoritesPage));
        Routing.RegisterRoute(nameof(DiscoverPage), typeof(DiscoverPage));
        Routing.RegisterRoute(nameof(TipsPage), typeof(TipsPage));
        Routing.RegisterRoute(nameof(ProfilePage), typeof(ProfilePage));
        Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
    }

    // Call this method from your LoginPage logic
    public void EnableMainApp()
    {
        // 1. Show the tabs
        MainTabBar.IsVisible = true;

        // 2. Redirect to the first tab (Explore)
        // The "//" clears the navigation stack so user can't go back to Login
        GoToAsync("//MainPage");
    }
}
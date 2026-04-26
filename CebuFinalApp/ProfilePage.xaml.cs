using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Firebase.Auth;
using Firebase.Storage;
using CebuFinalApp.Services;

namespace CebuFinalApp;

public partial class ProfilePage : ContentPage
{
    private readonly string _storageBucket = "explorecebu-f369e.appspot.com";
    private const string FirebaseApiKey = "AIzaSyDxMs6FIejTPAVN_-kFDajRfGN_proaMnw";

    public ProfilePage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadUserProfile();
        LoadFavoritesCount();
    }

    // ─── Load Profile ────────────────────────────────────────────────────

    private void LoadUserProfile()
    {
        var client = App.CurrentAuthClient;
        if (client?.User == null) return;

        var user = client.User;
        UserEmailLabel.Text = user.Info?.Email ?? "No email found";
        UserNameLabel.Text = string.IsNullOrEmpty(user.Info?.DisplayName)
                              ? "Cebu Traveler"
                              : user.Info.DisplayName;

        if (!string.IsNullOrEmpty(user.Info?.PhotoUrl))
            ProfileImage.Source = user.Info.PhotoUrl;

        // Load saved background image from preferences
        var savedBg = Preferences.Default.Get($"bg_{user.Uid}", string.Empty);
        if (!string.IsNullOrEmpty(savedBg))
            BackgroundImage.Source = savedBg;
    }

    private void LoadFavoritesCount()
    {
        FavoritesCountLabel.Text = FavoritesService.Instance.Favorites.Count.ToString();
    }

    // ─── Edit Profile Picture ────────────────────────────────────────────

    private async void OnEditPictureTapped(object sender, EventArgs e)
    {
        await UpdatePhotoLogic();
    }

    // ─── Edit Background Picture ─────────────────────────────────────────

    private async void OnEditBackgroundTapped(object sender, EventArgs e)
    {
        await UpdateBackgroundLogic();
    }

    private async Task UpdateBackgroundLogic()
    {
        var client = App.CurrentAuthClient;
        if (client?.User == null) return;

        try
        {
            var result = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Select Background Picture",
                FileTypes = FilePickerFileType.Images
            });

            if (result == null) return;

            UserNameLabel.Text = "Uploading...";

            using var stream = await result.OpenReadAsync();

            // Upload to Firebase Storage under bg_pics folder
            var task = new FirebaseStorage(_storageBucket)
                .Child("bg_pics")
                .Child($"{client.User.Uid}_bg.jpg")
                .PutAsync(stream);

            string downloadUrl = await task;

            // Save URL locally so it loads on next app open
            Preferences.Default.Set($"bg_{client.User.Uid}", downloadUrl);

            // Update the background image on screen
            BackgroundImage.Source = downloadUrl;
            UserNameLabel.Text = client.User.Info?.DisplayName ?? "Cebu Traveler";

            await DisplayAlert("Success", "Background picture updated!", "OK");
        }
        catch
        {
            UserNameLabel.Text = client?.User?.Info?.DisplayName ?? "Cebu Traveler";
            await DisplayAlert("Error", "Upload failed. Check connection.", "OK");
        }
    }

    // ─── Edit Profile Name + Photo ───────────────────────────────────────

    private async void OnEditProfileTapped(object sender, EventArgs e)
    {
        string action = await DisplayActionSheet("Edit Profile", "Cancel", null,
                                                 "Update Name", "Change Photo");
        if (action == "Update Name") await UpdateNameLogic();
        else if (action == "Change Photo") await UpdatePhotoLogic();
    }

    private async Task UpdateNameLogic()
    {
        var client = App.CurrentAuthClient;
        if (client?.User == null) return;

        string newName = await DisplayPromptAsync(
            "Edit Name", "Enter your new name:",
            initialValue: UserNameLabel.Text, maxLength: 25);

        if (string.IsNullOrWhiteSpace(newName)) return;

        try
        {
            var idToken = await client.User.GetIdTokenAsync();
            var http = new HttpClient();
            var content = new StringContent(
                JsonSerializer.Serialize(new
                {
                    idToken = idToken,
                    displayName = newName,
                    returnSecureToken = true
                }),
                Encoding.UTF8, "application/json");

            var response = await http.PostAsync(
                $"https://identitytoolkit.googleapis.com/v1/accounts:update?key={FirebaseApiKey}",
                content);

            if (response.IsSuccessStatusCode)
            {
                UserNameLabel.Text = newName;
                await DisplayAlert("Success", "Name updated!", "OK");
            }
            else
            {
                await DisplayAlert("Error", "Failed to update name.", "OK");
            }
        }
        catch
        {
            await DisplayAlert("Error", "Something went wrong.", "OK");
        }
    }

    private async Task UpdatePhotoLogic()
    {
        var client = App.CurrentAuthClient;
        if (client?.User == null) return;

        try
        {
            var result = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Select Profile Picture",
                FileTypes = FilePickerFileType.Images
            });

            if (result == null) return;

            UserNameLabel.Text = "Uploading...";

            using var stream = await result.OpenReadAsync();
            var task = new FirebaseStorage(_storageBucket)
                .Child("profile_pics")
                .Child($"{client.User.Uid}.jpg")
                .PutAsync(stream);

            string downloadUrl = await task;

            var idToken = await client.User.GetIdTokenAsync();
            var http = new HttpClient();
            var content = new StringContent(
                JsonSerializer.Serialize(new
                {
                    idToken = idToken,
                    photoUrl = downloadUrl,
                    returnSecureToken = true
                }),
                Encoding.UTF8, "application/json");

            var response = await http.PostAsync(
                $"https://identitytoolkit.googleapis.com/v1/accounts:update?key={FirebaseApiKey}",
                content);

            if (response.IsSuccessStatusCode)
            {
                ProfileImage.Source = downloadUrl;
                UserNameLabel.Text = client.User.Info?.DisplayName ?? "Cebu Traveler";
                await DisplayAlert("Success", "Profile picture updated!", "OK");
            }
            else
            {
                await DisplayAlert("Error", "Failed to update photo.", "OK");
            }
        }
        catch
        {
            UserNameLabel.Text = client?.User?.Info?.DisplayName ?? "Cebu Traveler";
            await DisplayAlert("Error", "Upload failed. Check connection.", "OK");
        }
    }

    // ─── Change Password ─────────────────────────────────────────────────

    private async void OnChangePasswordTapped(object sender, EventArgs e)
    {
        var client = App.CurrentAuthClient;
        if (client?.User == null) return;

        bool confirm = await DisplayAlert(
            "Reset Password",
            $"Send a password reset link to {client.User.Info?.Email}?",
            "Send", "Cancel");

        if (!confirm) return;

        try
        {
            await client.ResetEmailPasswordAsync(client.User.Info?.Email ?? "");
            await DisplayAlert("Sent!", "Check your email for the reset link.", "OK");
        }
        catch
        {
            await DisplayAlert("Error", "Could not send reset email.", "OK");
        }
    }

    // ─── Notifications ───────────────────────────────────────────────────

    private void OnNotificationToggled(object sender, ToggledEventArgs e)
    {
        string status = e.Value ? "enabled" : "disabled";
        DisplayAlert("Notifications", $"Notifications {status}.", "OK");
    }

    // ─── About ───────────────────────────────────────────────────────────

    private async void OnAboutTapped(object sender, EventArgs e)
    {
        await DisplayAlert(
            "About ExploreCebu",
            "ExploreCebu v1.0.0\n\nYour guide to discovering the best of Cebu — from tourist spots and local food to hotels and adventures.\n\nDeveloped with ❤️ in Cebu.",
            "OK");
    }

    // ─── Rate App ────────────────────────────────────────────────────────

    private async void OnRateAppTapped(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert(
            "Rate ExploreCebu",
            "Enjoying the app? Rate us on the Play Store!",
            "Rate Now", "Maybe Later");

        if (confirm)
        {
            await Launcher.Default.OpenAsync(
                "https://play.google.com/store/apps/details?id=com.cebu.explorecebu");
        }
    }

    // ─── Logout ──────────────────────────────────────────────────────────

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert(
            "Log Out", "Sign out of ExploreCebu?", "Yes", "No");

        if (confirm)
        {
            FavoritesService.Instance.ClearUser();
            App.CurrentAuthClient?.SignOut();
            await Shell.Current.GoToAsync("//LoginPage");
        }
    }
}
using CebuFinalApp.Services;
using Firebase.Auth;
using Firebase.Auth.Providers;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace CebuFinalApp;

public partial class LoginPage : ContentPage
{
    private readonly FirebaseAuthClient _authClient;
    private const string FirebaseApiKey = "AIzaSyDxMs6FIejTPAVN_-kFDajRfGN_proaMnw";

    public LoginPage()
    {
        InitializeComponent();

        var config = new FirebaseAuthConfig
        {
            ApiKey = FirebaseApiKey,
            AuthDomain = "explorecebu-f369e.firebaseapp.com",
            Providers = new FirebaseAuthProvider[] { new EmailProvider() }
        };
        _authClient = new FirebaseAuthClient(config);
    }

    // --- 🔐 AUTHENTICATION LOGIC ---

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(EmailEntry.Text) ||
            string.IsNullOrWhiteSpace(PasswordEntry.Text)) return;
        try
        {
            LoadingBus.IsRunning = true;
            var credential = await _authClient.SignInWithEmailAndPasswordAsync(
                                 EmailEntry.Text, PasswordEntry.Text);

            App.CurrentAuthClient = _authClient;

            // ✅ Pass both uid AND idToken
            var idToken = await credential.User.GetIdTokenAsync();
            FavoritesService.Instance.SetUser(credential.User.Uid, idToken);
            await FavoritesService.Instance.LoadFavoritesAsync();

            Application.Current.MainPage = new AppShell();
            await Shell.Current.GoToAsync("//Main/MainPage");
        }
        catch
        {
            await DisplayAlert("Login Error", "Invalid email or password.", "OK");
        }
        finally { LoadingBus.IsRunning = false; }
    }

    private async void OnSubmitRegistration(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(RegEmail.Text) || string.IsNullOrWhiteSpace(RegPass.Text)) return;
        try
        {
            LoadingBus.IsRunning = true;
            var userCredential = await _authClient.CreateUserWithEmailAndPasswordAsync(RegEmail.Text, RegPass.Text);
            await SaveUserToFirestore(userCredential.User.Uid, RegEmail.Text);
            await DisplayAlert("Success", "Account created!", "OK");
            await HidePopupAsync(RegOverlay);
        }
        catch (Exception ex) { await DisplayAlert("Error", ex.Message, "OK"); }
        finally { LoadingBus.IsRunning = false; }
    }

    private async Task SaveUserToFirestore(string uid, string email)
    {
        try
        {
            using var client = new HttpClient();
            var url = $"https://firestore.googleapis.com/v1/projects/explorecebu-f369e/databases/(default)/documents/users/{uid}";
            var data = new { fields = new { email = new { stringValue = email }, role = new { stringValue = "user" } } };
            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            await client.PatchAsync(url, content);
        }
        catch (Exception ex) { Debug.WriteLine(ex.Message); }
    }

    private async void OnSendResetLink(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(RecoverEmail.Text)) return;
        try
        {
            LoadingBus.IsRunning = true;
            await _authClient.ResetEmailPasswordAsync(RecoverEmail.Text);
            await DisplayAlert("Success", "Reset link sent to your email.", "OK");
            await HidePopupAsync(RecoverOverlay);
        }
        catch (Exception ex) { await DisplayAlert("Error", ex.Message, "OK"); }
        finally { LoadingBus.IsRunning = false; }
    }

    // --- 🖼️ UI HELPERS ---

    private async void OnRegisterClicked(object sender, EventArgs e) => await ShowPopupAsync(RegOverlay);
    private async void OnCancelRegistration(object sender, EventArgs e) => await HidePopupAsync(RegOverlay);
    private async void OnRecoverPasswordClicked(object sender, EventArgs e) => await ShowPopupAsync(RecoverOverlay);
    private async void OnCancelRecover(object sender, EventArgs e) => await HidePopupAsync(RecoverOverlay);

    private async Task ShowPopupAsync(Grid popup) { popup.IsVisible = true; await popup.FadeTo(1, 250); }
    private async Task HidePopupAsync(Grid popup) { await popup.FadeTo(0, 250); popup.IsVisible = false; }

    private void OnGoogleLoginClicked(object sender, EventArgs e) { }
    private void OnFacebookLoginClicked(object sender, EventArgs e) { }
}
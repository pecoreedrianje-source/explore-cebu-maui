using CebuFinalApp.Models;
using CebuFinalApp.Services;
using System.Collections.ObjectModel;

namespace CebuFinalApp;

public partial class DiscoverPage : ContentPage
{
    private readonly ApiService _apiService = new();
    private readonly FavoritesService _favs = FavoritesService.Instance;

    public ObservableCollection<Place> PopularPlaces { get; set; } = new();

    public DiscoverPage()
    {
        InitializeComponent();
        PopularPlacesView.ItemsSource = PopularPlaces;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (PopularPlaces.Count == 0)
        {
            await _favs.LoadFavoritesAsync();
            await LoadData("Spots");
        }
        else
        {
            SyncFavoriteStates();
        }
    }

    private async void OnCategoryTapped(object sender, TappedEventArgs e)
    {
        string category = e.Parameter?.ToString() ?? "Spots";
        await LoadData(category);
    }

    private async Task LoadData(string category)
    {
        if (LoadingIcon != null) LoadingIcon.IsRunning = true;
        try
        {
            var data = await _apiService.GetCebuPlacesAsync(category);
            PopularPlaces.Clear();
            foreach (var p in data)
            {
                p.IsFavorite = _favs.IsFavorite(p);
                PopularPlaces.Add(p);
            }
        }
        catch
        {
            await DisplayAlert("Error", "Could not load data.", "OK");
        }
        finally
        {
            if (LoadingIcon != null) LoadingIcon.IsRunning = false;
        }
    }

    private void SyncFavoriteStates()
    {
        foreach (var place in PopularPlaces)
            place.IsFavorite = _favs.IsFavorite(place);
    }

    private async void OnFavoriteTapped(object sender, EventArgs e)
    {
        try
        {
            if (sender is not Button btn) return;
            if (btn.CommandParameter is not Place place) return;
            await _favs.ToggleAsync(place);
            HapticFeedback.Default.Perform(HapticFeedbackType.Click);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Debug Error", ex.Message, "OK");
        }
    }

    // ─── Read More — shows photo + description popup ─────────────────────
    private async void OnReadMoreTapped(object sender, TappedEventArgs e)
    {
        if (e.Parameter is not Place p) return;

        // Populate popup fields
        DetailImage.Source = p.ImageSource;
        DetailName.Text = p.Name;
        DetailLocation.Text = p.Location;
        DetailDescription.Text = p.Description;

        // Animate in
        DetailOverlay.IsVisible = true;
        DetailOverlay.Opacity = 0;
        DetailCard.Scale = 0.85;
        await Task.WhenAll(
            DetailOverlay.FadeTo(1, 200),
            DetailCard.ScaleTo(1, 200, Easing.CubicOut)
        );
    }

    private async void OnDetailCloseTapped(object sender, EventArgs e)
    {
        // Animate out
        await Task.WhenAll(
            DetailOverlay.FadeTo(0, 180),
            DetailCard.ScaleTo(0.85, 180, Easing.CubicIn)
        );
        DetailOverlay.IsVisible = false;
    }
}
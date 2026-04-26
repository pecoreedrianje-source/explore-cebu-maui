using CebuFinalApp.Models;
using CebuFinalApp.Services;

namespace CebuFinalApp;

public partial class FavoritesPage : ContentPage
{
    private readonly FavoritesService _favs = FavoritesService.Instance;

    public FavoritesPage()
    {
        InitializeComponent();
        FavsCollection.ItemsSource = _favs.Favorites;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // ✅ Only reload from Firestore if collection is empty
        if (_favs.Favorites.Count == 0)
            await _favs.LoadFavoritesAsync();
    }

    private async void OnRemoveFavoriteClicked(object sender, EventArgs e)
    {
        if (sender is ImageButton btn && btn.CommandParameter is Place place)
        {
            bool confirm = await DisplayAlert(
                "Remove", $"Remove {place.Name} from favorites?", "Yes", "No");

            if (confirm)
            {
                await _favs.RemoveAsync(place);
                HapticFeedback.Default.Perform(HapticFeedbackType.Click);
            }
        }
    }
}
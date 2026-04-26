using CebuFinalApp.Models;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace CebuFinalApp.Services
{
    public class FavoritesService
    {
        private static FavoritesService? _instance;
        public static FavoritesService Instance => _instance ??= new FavoritesService();

        private const string ProjectId = "explorecebu-f369e";
        private string? _userId;
        private string? _idToken;

        public ObservableCollection<Place> Favorites { get; } = new();

        private FavoritesService() { }

        // ─── User Management ────────────────────────────────────────────────

        public void SetUser(string uid, string idToken)
        {
            _userId = uid;
            _idToken = idToken;
            Favorites.Clear();
        }

        public void ClearUser()
        {
            _userId = null;
            _idToken = null;
            Favorites.Clear();
        }

        // ─── Firestore URL Helpers ───────────────────────────────────────────

        // ✅ Unique key per category + name
        private string UniqueKey(Place place) =>
            Uri.EscapeDataString($"{place.Category}_{place.Name}");

        private string DocUrl(Place place) =>
            $"https://firestore.googleapis.com/v1/projects/{ProjectId}" +
            $"/databases/(default)/documents/users/{_userId}/favorites/{UniqueKey(place)}";

        private string CollectionUrl =>
            $"https://firestore.googleapis.com/v1/projects/{ProjectId}" +
            $"/databases/(default)/documents/users/{_userId}/favorites";

        // ─── Auth Header Helper ──────────────────────────────────────────────

        private HttpClient CreateAuthClient()
        {
            var client = new HttpClient();
            if (!string.IsNullOrEmpty(_idToken))
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_idToken}");
            return client;
        }

        // ─── Public API ──────────────────────────────────────────────────────

        // ✅ Unique check by both Category AND Name
        public bool IsFavorite(Place place) =>
            Favorites.Any(f => f.Name == place.Name && f.Category == place.Category);

        public async Task LoadFavoritesAsync()
        {
            if (_userId == null) return;
            try
            {
                await RefreshTokenAsync();

                using var client = CreateAuthClient();
                var response = await client.GetAsync(CollectionUrl);

                var json = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"LOAD FAVORITES — Status: {response.StatusCode}");

                if (!response.IsSuccessStatusCode) return;

                var doc = JsonDocument.Parse(json);
                Favorites.Clear();

                if (!doc.RootElement.TryGetProperty("documents", out var documents)) return;

                foreach (var item in documents.EnumerateArray())
                {
                    if (!item.TryGetProperty("fields", out var fields)) continue;

                    var place = new Place
                    {
                        Name = GetString(fields, "name"),
                        Location = GetString(fields, "location"),
                        ImageSource = GetString(fields, "imageSource"),
                        Description = GetString(fields, "description"),
                        Category = GetString(fields, "category"),
                        IsFavorite = true
                    };

                    if (!string.IsNullOrEmpty(place.Name))
                        Favorites.Add(place);
                }

                Debug.WriteLine($"LOAD FAVORITES — Loaded {Favorites.Count} items");
            }
            catch (Exception ex) { Debug.WriteLine($"LoadFavorites error: {ex.Message}"); }
        }

        public async Task AddAsync(Place place)
        {
            if (_userId == null || IsFavorite(place)) return;
            try
            {
                await RefreshTokenAsync();

                using var client = CreateAuthClient();
                var body = new
                {
                    fields = new
                    {
                        name = new { stringValue = place.Name },
                        location = new { stringValue = place.Location },
                        imageSource = new { stringValue = place.ImageSource },
                        description = new { stringValue = place.Description },
                        category = new { stringValue = place.Category }
                    }
                };

                var json = JsonSerializer.Serialize(body);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PatchAsync(DocUrl(place), content);

                var responseBody = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"ADD FAVORITE — Status: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    place.IsFavorite = true;
                    Favorites.Add(place);
                }
                else
                {
                    Debug.WriteLine($"FAILED to save: {responseBody}");
                }
            }
            catch (Exception ex) { Debug.WriteLine($"AddFavorite error: {ex.Message}"); }
        }

        public async Task RemoveAsync(Place place)
        {
            if (_userId == null) return;
            try
            {
                await RefreshTokenAsync();

                using var client = CreateAuthClient();
                var response = await client.DeleteAsync(DocUrl(place));

                Debug.WriteLine($"REMOVE FAVORITE — Status: {response.StatusCode}");

                var existing = Favorites.FirstOrDefault(f =>
                    f.Name == place.Name && f.Category == place.Category);

                if (existing != null)
                {
                    existing.IsFavorite = false;
                    Favorites.Remove(existing);
                }

                place.IsFavorite = false;
            }
            catch (Exception ex) { Debug.WriteLine($"RemoveFavorite error: {ex.Message}"); }
        }

        public async Task ToggleAsync(Place place)
        {
            if (_userId == null)
            {
                Debug.WriteLine("FavoritesService: No user set.");
                return;
            }

            if (IsFavorite(place)) await RemoveAsync(place);
            else await AddAsync(place);
        }

        // ─── Token Refresh ───────────────────────────────────────────────────

        private async Task RefreshTokenAsync()
        {
            try
            {
                var authClient = App.CurrentAuthClient;
                if (authClient?.User == null) return;
                _idToken = await authClient.User.GetIdTokenAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"RefreshToken error: {ex.Message}");
            }
        }

        // ─── Helper ──────────────────────────────────────────────────────────

        private static string GetString(JsonElement fields, string key)
        {
            if (fields.TryGetProperty(key, out var prop) &&
                prop.TryGetProperty("stringValue", out var val))
                return val.GetString() ?? string.Empty;
            return string.Empty;
        }
    }
}
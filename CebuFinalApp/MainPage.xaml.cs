using CommunityToolkit.Maui.Media;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;

namespace CebuFinalApp;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadGeoapifyMap();
    }

    private void LoadGeoapifyMap()
    {
        // Default to Cebu City center
        double lat = 10.3157;
        double lon = 123.8854;

        string htmlSource = $@"
        <html>
        <head>
            <meta name='viewport' content='width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=no' />
            <link rel='stylesheet' href='https://unpkg.com/leaflet@1.9.4/dist/leaflet.css'/>
            <script src='https://unpkg.com/leaflet@1.9.4/dist/leaflet.js'></script>
            <style>
                html, body, #map {{ height: 100%; width: 100%; margin: 0; padding: 0; background-color: #f4f9f8; }}
            </style>
        </head>
        <body>
            <div id='map'></div>
            <script>
                var map = L.map('map', {{ zoomControl: false }}).setView([{lat}, {lon}], 13);
                var standard = L.tileLayer('https://{{s}}.tile.openstreetmap.org/{{z}}/{{x}}/{{y}}.png').addTo(map);
                var satellite = L.tileLayer('https://server.arcgisonline.com/ArcGIS/rest/services/World_Imagery/MapServer/tile/{{z}}/{{y}}/{{x}}');
                var marker = L.marker([{lat}, {lon}]).addTo(map);

                function moveTo(lat, lon) {{
                    map.flyTo([lat, lon], 15);
                    marker.setLatLng([lat, lon]);
                }}

                function setStyle(s) {{
                    if (s === 'Satellite') {{ 
                        map.removeLayer(standard); 
                        satellite.addTo(map); 
                    }} else {{ 
                        map.removeLayer(satellite); 
                        standard.addTo(map); 
                    }}
                }}
            </script>
        </body>
        </html>";

        MapWebView.Source = new HtmlWebViewSource { Html = htmlSource };
    }

    // --- SEARCH LOGIC ---
    private async void OnSearchCompleted(object sender, EventArgs e)
    {
        await PerformSearch(SearchEntry.Text);
    }

    private async Task PerformSearch(string query)
    {
        if (string.IsNullOrWhiteSpace(query)) return;

        using HttpClient client = new HttpClient();
        // Nominatim requires a User-Agent or it will return a 403 Forbidden error
        client.DefaultRequestHeaders.Add("User-Agent", "CebuTravelApp_Project");

        try
        {
            var response = await client.GetStringAsync($"https://nominatim.openstreetmap.org/search?format=json&q={Uri.EscapeDataString(query + ", Cebu")}");
            var results = System.Text.Json.JsonDocument.Parse(response).RootElement;

            if (results.GetArrayLength() > 0)
            {
                double lat = double.Parse(results[0].GetProperty("lat").GetString() ?? "0");
                double lon = double.Parse(results[0].GetProperty("lon").GetString() ?? "0");

                await MapWebView.EvaluateJavaScriptAsync($"moveTo({lat}, {lon})");
                SearchEntry.Unfocus();
            }
            else
            {
                await DisplayAlert("Not Found", $"Could not find '{query}' in Cebu.", "OK");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Search Error: {ex.Message}");
            await DisplayAlert("Error", "Search failed. Check your connection.", "OK");
        }
    }

    // --- MICROPHONE LOGIC ---
    private async void OnMicClicked(object sender, EventArgs e)
    {
        var status = await Permissions.RequestAsync<Permissions.Microphone>();
        if (status != PermissionStatus.Granted)
        {
            await DisplayAlert("Permission", "Microphone access is required for voice search.", "OK");
            return;
        }

        try
        {
            // Simple animation feedback for the vivo Y20
            await MicButton.ScaleTo(0.8, 100);
            await MicButton.ScaleTo(1.0, 100);

            var speech = SpeechToText.Default;

            // Some SpeechToText builds require asking for permissions via the API as well
            try
            {
                var apiPerms = await speech.RequestPermissions();
                if (!apiPerms)
                {
                    await DisplayAlert("Permission", "Microphone access is required for voice search.", "OK");
                    return;
                }
            }
            catch
            {
                // ignore if the API does not implement RequestPermissions in this build
            }

            var tcs = new TaskCompletionSource<string?>(TaskCreationOptions.RunContinuationsAsynchronously);

            // Use the toolkit events to receive interim and final results.
            EventHandler<SpeechToTextRecognitionResultUpdatedEventArgs>? updated = null;
            EventHandler<SpeechToTextRecognitionResultCompletedEventArgs>? completed = null;

            updated = (s, a) =>
            {
                // Use reflection to read a "Text" property if present so we don't depend on exact event-arg names
                var text = a?.GetType().GetProperty("Text", BindingFlags.Public | BindingFlags.Instance)?.GetValue(a) as string;
                if (!string.IsNullOrEmpty(text))
                {
                    // Update entry with partial result on UI thread
                    MainThread.BeginInvokeOnMainThread(() => SearchEntry.Text = text);
                }
            };

            completed = (s, a) =>
            {
                var text = a?.GetType().GetProperty("Text", BindingFlags.Public | BindingFlags.Instance)?.GetValue(a) as string;
                tcs.TrySetResult(text);
            };

            speech.RecognitionResultUpdated += updated;
            speech.RecognitionResultCompleted += completed;

            // Start listening. Some speech API overloads accept options; passing null is acceptable for many builds.
            await speech.StartListenAsync(null);

            // Wait for final result (or cancellation/timeouts you may add)
            var finalText = await tcs.Task;

            // Stop listening to free resources
            try { await speech.StopListenAsync(); } catch { }

            speech.RecognitionResultUpdated -= updated;
            speech.RecognitionResultCompleted -= completed;

            if (!string.IsNullOrWhiteSpace(finalText))
            {
                SearchEntry.Text = finalText;
                await PerformSearch(finalText);
            }
            else
            {
                await DisplayAlert("Voice Search", "Could not recognize speech. Try again.", "OK");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"STT Error: {ex.Message}");
        }
    }

    // --- MAP UI ACTIONS ---
    private async void OnLayersClicked(object sender, EventArgs e)
    {
        string action = await DisplayActionSheet("Map Type", "Cancel", null, "Standard", "Satellite");
        if (action != "Cancel")
            await MapWebView.EvaluateJavaScriptAsync($"setStyle('{action}')");
    }

    private async void OnGPSClicked(object sender, EventArgs e)
    {
        try
        {
            var location = await Geolocation.Default.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(5)));
            if (location != null)
                await MapWebView.EvaluateJavaScriptAsync($"moveTo({location.Latitude}, {location.Longitude})");
        }
        catch (Exception)
        {
            await DisplayAlert("GPS", "Could not determine location. Ensure GPS is enabled.", "OK");
        }
    }
}
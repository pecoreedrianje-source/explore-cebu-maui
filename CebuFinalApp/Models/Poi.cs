namespace CebuFinalApp.Models;

public class Poi
{
    public string Name { get; set; }
    public string Category { get; set; }
    public string Address { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }

    // Helper to display a nice icon based on category
    public string Icon => Category.ToLower().Contains("restaurant") ? "🍴" : "📍";
}
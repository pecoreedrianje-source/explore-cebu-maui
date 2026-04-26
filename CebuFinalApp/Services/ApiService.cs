using CebuFinalApp.Models;

namespace CebuFinalApp.Services;

public class ApiService
{
    private readonly Random _random = new();

    public async Task<List<Place>> GetCebuPlacesAsync(string category = "Spots")
    {
        await Task.Delay(500);
        var list = new List<Place>();

        switch (category)
        {
            case "Food":
                list.Add(new Place { Category = "Food", Name = "Lantaw Floating", Location = "Cordova", ImageSource = "lantaw.jpg", Description = "A scenic dining experience on the water with fresh Filipino seafood and a view of the city skyline." });
                list.Add(new Place { Category = "Food", Name = "Rico's Lechon", Location = "Cebu City", ImageSource = "ricos.jpg", Description = "The original home of spicy lechon in Cebu, famous for its perfectly crunchy skin and flavorful meat." });
                list.Add(new Place { Category = "Food", Name = "House of Lechon", Location = "Cebu City", ImageSource = "house_lechon.jpg", Description = "Elegant dining specializing in Carcar-style lechon with a modern twist." });
                list.Add(new Place { Category = "Food", Name = "CNT Lechon", Location = "Cebu City", ImageSource = "cnt.jpg", Description = "A local favorite known for its savory skin and tender meat, best paired with puso." });
                list.Add(new Place { Category = "Food", Name = "La Vie Parisienne", Location = "Cebu City", ImageSource = "lavie.jpg", Description = "A French-inspired bakery and wine library featuring a beautiful outdoor garden with lit-up trees." });
                break;

            case "Hotels":
                list.Add(new Place { Category = "Hotels", Name = "Shangri-La Mactan", Location = "Lapu-Lapu", ImageSource = "shangrila.jpg", Description = "A luxury 5-star resort with a private beach, world-class marine sanctuary, and tropical gardens." });
                list.Add(new Place { Category = "Hotels", Name = "Radisson Blu", Location = "Cebu City", ImageSource = "radisson.jpg", Description = "An upscale city hotel located next to SM City Cebu, offering premium rooms and a famous buffet." });
                list.Add(new Place { Category = "Hotels", Name = "Seda Ayala Center", Location = "Cebu City", ImageSource = "seda.jpg", Description = "A modern hotel in the business district, offering sleek rooms and easy access to Ayala Center Cebu." });
                list.Add(new Place { Category = "Hotels", Name = "Jpark Island Resort", Location = "Lapu-Lapu", ImageSource = "jpark.jpg", Description = "A premier waterpark resort featuring multiple pools, slides, and a private beach." });
                list.Add(new Place { Category = "Hotels", Name = "Cebu City Marriott", Location = "Cebu City", ImageSource = "marriott.jpg", Description = "A well-established hotel known for its excellent service and proximity to major business hubs." });
                break;

            case "Activities":
                list.Add(new Place { Category = "Activities", Name = "Kawasan Canyoneering", Location = "Badian", ImageSource = "kawayan.jpg", Description = "An adrenaline-pumping adventure involving jumping off cliffs and swimming through canyons." });
                list.Add(new Place { Category = "Activities", Name = "Oslob Whale Shark", Location = "Oslob", ImageSource = "whaleshark.jpg", Description = "A unique opportunity to swim and snorkel with the gentle giants of the ocean." });
                list.Add(new Place { Category = "Activities", Name = "Safari Park", Location = "Carmen", ImageSource = "safari.jpg", Description = "The largest safari park in the Philippines, featuring exotic animals and adventure activities." });
                list.Add(new Place { Category = "Activities", Name = "Moalboal Sardines", Location = "Moalboal", ImageSource = "sardines.jpg", Description = "Experience the famous sardine run where millions of fish create a shimmering wall of silver." });
                list.Add(new Place { Category = "Activities", Name = "Sky Experience", Location = "Cebu City", ImageSource = "skywalk.jpg", Description = "Test your nerves with the Edge Coaster or Sky Walk on top of a 38-story skyscraper." });
                break;

            default: // Spots
                list.Add(new Place { Category = "Spots", Name = "Magellan's Cross", Location = "Cebu City", ImageSource = "magellans_cross.jpg", Description = "A historical landmark marking the arrival of Christianity in the Philippines." });
                list.Add(new Place { Category = "Spots", Name = "Sirao Garden", Location = "Busay", ImageSource = "sirao_garden.jpg", Description = "The 'Little Amsterdam' of Cebu, known for its vibrant flower fields." });
                list.Add(new Place { Category = "Spots", Name = "Fort San Pedro", Location = "Cebu City", ImageSource = "fort_san_pedro.jpg", Description = "The oldest triangular bastion fort in the Philippines, built during the Spanish era." });
                list.Add(new Place { Category = "Spots", Name = "Temple of Leah", Location = "Busay", ImageSource = "temple_of_leah.jpg", Description = "A Roman-style temple built as a symbol of undying love, offering panoramic city views." });
                list.Add(new Place { Category = "Spots", Name = "Simala Shrine", Location = "Sibonga", ImageSource = "simala_shrine.jpg", Description = "A majestic castle-like church known for its beautiful architecture and religious importance." });
                break;
        }

        foreach (var p in list)
            p.Rating = Math.Round(_random.NextDouble() * (5.0 - 4.3) + 4.3, 1);

        return list;
    }
}
using System.Text;
using System.Text.Json;

namespace CebuFinalApp;

public class FirestoreService
{
    public async Task SaveUserAsync(string uid, string email)
    {
        var client = new HttpClient();

        var url = $"https://firestore.googleapis.com/v1/projects/explorecebu-f369e/databases/(default)/documents/users?documentId={uid}";

        var data = new
        {
            fields = new
            {
                email = new { stringValue = email },
                role = new { stringValue = "user" }
            }
        };

        var json = JsonSerializer.Serialize(data);

        var content = new StringContent(json, Encoding.UTF8, "application/json");

        await client.PostAsync(url, content);
    }
}
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[System.Serializable]
public class UserResponse
{
    // These match the "ID" and "Username" keys in your JSON response
    public int ID;
    public string Username;
}

[System.Serializable]
public class CombatIdResponse
{
    public int latestID; // Ensure this matches the key in your JSON
}

public class NetworkManager : SingletonPersistent<NetworkManager>
{
    private static readonly HttpClient client = new HttpClient();

    public static async Task<UserResponse> SavePlayerData(object data)
    {
        // Serialize your object to JSON
        string json = JsonUtility.ToJson(data);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        try
        {
            string uri = LoadEnv.apiKey + "/users";
// // Debug.Log(uri);
            // Await the POST request
            HttpResponseMessage response = await client.PostAsync(uri, content);

            // Throw an exception if the status code is a failure
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            UserResponse createdUser = JsonUtility.FromJson<UserResponse>(responseBody);
            return createdUser;
        }
        catch (HttpRequestException e)
        {
            Debug.LogError($"Error: {e.Message}");
            throw; // Re-throw the exception to be handled by the caller
        }
    }
    public static async Task SaveCombatActionLogs(object data)
    {
        string json = JsonUtility.ToJson(data);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        try
        {
            string uri = LoadEnv.apiKey + "/combat-action-logs";
            HttpResponseMessage response = await client.PostAsync(uri, content);
            response.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException e)
        {
            Debug.LogError($"Error: {e.Message}");
            throw;
        }
    }
    public static async Task<int> GetLatestCombatID()
    {
        try
        {
            string uri = LoadEnv.apiKey + "/combat-action-logs/latest-id";
            HttpResponseMessage response = await client.GetAsync(uri);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            CombatIdResponse combatIdResponse = JsonUtility.FromJson<CombatIdResponse>(responseBody);
            return combatIdResponse.latestID;
        }
        catch (HttpRequestException e)
        {
            Debug.LogError($"Error: {e.Message}");
            throw;
        }
    }
    public static async Task SaveCombatSkillLoadoutLogs(object data)
    {
        try
        {
            string json = JsonUtility.ToJson(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            string uri = LoadEnv.apiKey + "/combat-skill-loadouts";
            HttpResponseMessage response = await client.PostAsync(uri, content);
            response.EnsureSuccessStatusCode();

        }
        catch (HttpRequestException e)
        {
            Debug.LogError($"Error: {e.Message}");
            throw;
        }
    }
}

using System;
using UnityEngine;

public class LoadEnv : SingletonPersistent<LoadEnv>
{
    public static string apiKey;

    // Automatically runs when the game starts
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void InitializeEnv()
    {
        DotEnv.Load();
    }

    void Start()
    {
        // Read the variable
        string envApiKey = Environment.GetEnvironmentVariable("API_URL");
        if (!string.IsNullOrEmpty(envApiKey))
        {
            apiKey = envApiKey;
        }
        else if (string.IsNullOrEmpty(apiKey))
        {
            apiKey = "https://moonbloom.narutchai.com";
        }
    }
}

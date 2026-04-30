using System;
using System.IO;
using UnityEngine;

public static class DotEnv
{
    public static void Load()
    {
        // Points to the root of the Unity project (one level up from Assets)
        string filePath = Path.Combine(Application.dataPath, "../.env");

        if (!File.Exists(filePath))
        {
            Debug.LogWarning(".env file not found at: " + filePath);
            return;
        }

        foreach (var line in File.ReadAllLines(filePath))
        {
            // Ignore empty lines and comments
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;

            // Split by the first '=' found
            var parts = line.Split(new[] { '=' }, 2, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2) continue;

            // Set the environment variable
            Environment.SetEnvironmentVariable(parts[0].Trim(), parts[1].Trim());
        }
    }
}

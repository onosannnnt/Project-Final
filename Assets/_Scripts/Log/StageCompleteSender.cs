using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

[Serializable]
public class CombatLog
{
    public string session_id;
    public int player_id;
    public int character_id;
    public int wave_number;
    public int turn_index;
    public int skill_id;
    public int skill_target_id;
    public float target_max_hp;
    public float target_current_hp;
    public float damage_dealt;
    public float damage_recieve;
    public int caster_current_sp;
    public float caster_current_hp;
    public float caster_max_hp;
    public int current_frenzy_stack;
    public float heal_amount;
    public float current_corrupt_blood_gain;
    public float corrupt_blood_by_max_hp;
    public string weather;
    public int momentum_gain;
    public int momentum_used;
}

[Serializable]
public class StageCompleteRequest
{
    public int player_id;
    public List<CombatLog> combat_logs;
}

[Serializable]
public class StageCompleteResponse
{
    public string message;
    public string new_type;
    public int new_tier;
}

public class StageCompleteSender : MonoBehaviour
{
    [SerializeField] private string apiBaseUrl = "https://moonbloom.narutchai.com";
    [SerializeField] private string stageCompletePath = "/ml/stage-complete";
    [SerializeField] private bool logResponses = true;
    [SerializeField] private UserData userData;

    public IEnumerator SendStageComplete(StageCompleteRequest payload)
    {
        if (payload == null) yield break;

        // Use LoadEnv.apiKey if available, otherwise fallback to Inspector field or hardcoded default
        string baseUrl = LoadEnv.apiKey;
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            baseUrl = string.IsNullOrWhiteSpace(apiBaseUrl) ? "https://moonbloom.narutchai.com" : apiBaseUrl;
        }
        
        string path = string.IsNullOrWhiteSpace(stageCompletePath) ? "/ml/stage-complete" : stageCompletePath;
        string url = CombineUrl(baseUrl, path);
        string json = JsonUtility.ToJson(payload);

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Stage-complete failed: {request.error} | {request.downloadHandler.text}");
        }
        else 
        {
            if (logResponses)
            {
                Debug.Log($"Stage-complete ok: {request.downloadHandler.text}");
            }

        // Parse response and update AFM data
        try 
        {
            StageCompleteResponse response = JsonUtility.FromJson<StageCompleteResponse>(request.downloadHandler.text);
            UserData currentData = userData != null ? userData : (TurnManager.Instance != null ? TurnManager.Instance.UserData : null);
            
            if (response != null && currentData != null)
            {
                bool changed = false;
                if (!string.IsNullOrEmpty(response.new_type))
                {
                    if (TryParseAFMType(response.new_type, out AFMType parsedType))
                    {
                        if (currentData.AFMType != parsedType)
                        {
                            currentData.AFMType = parsedType;
                            changed = true;
                        }
                    }
                }

                if (response.new_tier > 0)
                {
                    if (currentData.AFMTier != response.new_tier)
                    {
                        currentData.AFMTier = response.new_tier;
                        changed = true;
                    }
                }

                if (changed)
                {
                    Debug.Log($"[StageCompleteSender] Updated UserData with new AFM: {currentData.AFMType} T{currentData.AFMTier}");
                    
                    // Notify AFMManager to apply buffs immediately if it exists
                    AFMManager afm = FindObjectOfType<AFMManager>();
                    if (afm != null)
                    {
                        afm.RefreshBuffsFromUserData();
                    }
                }
            }
        }
            catch (Exception e)
            {
                Debug.LogWarning($"[StageCompleteSender] Failed to parse AFM response: {e.Message}");
            }
        }
    }

    private bool TryParseAFMType(string raw, out AFMType result)
    {
        result = AFMType.EL;
        if (string.IsNullOrWhiteSpace(raw)) return false;
        string val = raw.Trim().ToLowerInvariant();
        switch (val)
        {
            case "el": result = AFMType.EL; return true;
            case "ce": result = AFMType.CE; return true;
            case "rv": result = AFMType.RV; return true;
            default: return false;
        }
    }

    private static string CombineUrl(string baseUrl, string path)
    {
        if (string.IsNullOrEmpty(baseUrl)) return path ?? string.Empty;
        if (string.IsNullOrEmpty(path)) return baseUrl;
        return $"{baseUrl.TrimEnd('/')}/{path.TrimStart('/')}";
    }
}

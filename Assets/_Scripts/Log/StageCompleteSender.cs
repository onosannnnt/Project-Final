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

public class StageCompleteSender : MonoBehaviour
{
    [SerializeField] private string apiBaseUrl = "https://moonbloom.narutchai.com/";
    [SerializeField] private string stageCompletePath = "/ml/stage-complete";
    [SerializeField] private bool logResponses = true;

    public IEnumerator SendStageComplete(StageCompleteRequest payload)
    {
        if (payload == null) yield break;

        string baseUrl = string.IsNullOrWhiteSpace(apiBaseUrl) ? "https://moonbloom.narutchai.com/" : apiBaseUrl;
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
        else if (logResponses)
        {
            Debug.Log($"Stage-complete ok: {request.downloadHandler.text}");
        }
    }

    private static string CombineUrl(string baseUrl, string path)
    {
        if (string.IsNullOrEmpty(baseUrl)) return path ?? string.Empty;
        if (string.IsNullOrEmpty(path)) return baseUrl;
        return $"{baseUrl.TrimEnd('/')}/{path.TrimStart('/')}";
    }
}

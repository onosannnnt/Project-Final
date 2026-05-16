using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class AFMApiResponse
{
    [Tooltip("Player entity id from API payload")]
    public int player_id;

    [Tooltip("Player type from API payload")]
    public AFMType type;

    [Tooltip("Tier from API payload: 1-4 (4 is test)")]
    [Range(1, 4)]
    public int tier = 1;
}

public enum AFMType
{
    EL,
    CE,
    RV
}

[Serializable]
public class AFMBuffRule
{
    [Tooltip("Player type key")]
    public AFMType type;

    [Tooltip("Tier key: 1-4 (4 is test)")]
    [Range(1, 4)]
    public int tier = 1;

    [Tooltip("Buff to apply when type+tier matches")]
    public Buff buff;
}

public class AFMManager : MonoBehaviour
{
    [Header("Mock API")]
    [SerializeField] private bool applyOnStart = true;
    [SerializeField] private bool onlyAliveMembers = true;
    [SerializeField] private bool verboseLog = true;
    [SerializeField] private bool applyOnlyOnce = true;
    [SerializeField] private bool skipIfBuffAlreadyExists = true;
    [SerializeField] private bool applyToAllControlledPlayers = true;

    [Tooltip("Single mock API response using shape: { player_id, type, tier }")]
    [SerializeField] private AFMApiResponse mockResponse = new();

    [Header("Buff Mapping (type + tier => buff)")]
    [SerializeField] private List<AFMBuffRule> buffRules = new();

    private void Start()
    {
        if (verboseLog)
        {
            Debug.Log("[AFMManager] Started.");
        }

        if (applyOnStart)
        {
            ApplyMockResponse();
        }
    }

    private bool hasApplied;

    [ContextMenu("Apply Mock Response")]
    public void ApplyMockResponse()
    {
        if (ShouldSkipAFM())
        {
            if (verboseLog) Debug.Log("[AFMManager] Skip apply: Tutorial or Quest 1 mode detected.");
            return;
        }

        if (applyOnlyOnce && hasApplied)
        {
            if (verboseLog)
            {
                Debug.Log("[AFMManager] Skip apply: already applied once.");
            }
            return;
        }

        bool success = applyToAllControlledPlayers
            ? ApplyResponseToAllControlledPlayers(mockResponse)
            : ApplySingleResponse(mockResponse);

        if (success)
        {
            hasApplied = true;
        }
    }

    public bool ApplySingleResponse(AFMApiResponse response)
    {
        if (ShouldSkipAFM()) return false;

        if (response == null)
        {
            Debug.LogWarning("[AFMManager] Response is null.");
            return false;
        }

        if (!IsValidTier(response.tier))
        {
            Debug.LogWarning($"[AFMManager] Invalid tier '{response.tier}' for player_id {response.player_id}. Tier must be 1-4 (4 is test).");
            return false;
        }

        Buff buff = ResolveBuff(response.type, response.tier);
        if (buff == null)
        {
            Debug.LogWarning($"[AFMManager] No buff rule matched type '{ToApiType(response.type)}', tier '{response.tier}'.");
            return false;
        }

        PlayerEntity member = FindPlayerById(response.player_id);
        if (!ShouldApply(member))
        {
            Debug.LogWarning($"[AFMManager] No valid player found for player_id {response.player_id}.");
            return false;
        }

        return ApplyBuffToMember(member, buff, response);
    }

    public bool ApplySingleResponse(int playerId, string type, int tier)
    {
        if (!TryParseType(type, out AFMType parsedType))
        {
            Debug.LogWarning($"[AFMManager] Invalid type '{type}' for player_id {playerId}. Use el, ce, or rv.");
            return false;
        }

        AFMApiResponse response = new AFMApiResponse
        {
            player_id = playerId,
            type = parsedType,
            tier = tier
        };

        return ApplySingleResponse(response);
    }

    private bool ApplyResponseToAllControlledPlayers(AFMApiResponse response)
    {
        if (response == null)
        {
            Debug.LogWarning("[AFMManager] Response is null.");
            return false;
        }

        if (!IsValidTier(response.tier))
        {
            Debug.LogWarning($"[AFMManager] Invalid tier '{response.tier}'. Tier must be 1-4 (4 is test).");
            return false;
        }

        Buff buff = ResolveBuff(response.type, response.tier);
        if (buff == null)
        {
            Debug.LogWarning($"[AFMManager] No buff rule matched type '{ToApiType(response.type)}', tier '{response.tier}'.");
            return false;
        }

        List<PlayerEntity> members = GetControlledPlayers();
        if (members.Count == 0)
        {
            Debug.LogWarning("[AFMManager] No controlled players found.");
            return false;
        }

        int appliedCount = 0;
        for (int i = 0; i < members.Count; i++)
        {
            PlayerEntity member = members[i];
            if (!ShouldApply(member))
            {
                continue;
            }

            if (ApplyBuffToMember(member, buff, response))
            {
                appliedCount++;
            }
        }

        if (verboseLog)
        {
            Debug.Log($"[AFMManager] Applied '{buff.BuffName}' to {appliedCount}/{members.Count} controlled player(s).");
        }

        return appliedCount > 0;
    }

    private bool ApplyBuffToMember(PlayerEntity member, Buff buff, AFMApiResponse response)
    {
        if (member == null || buff == null)
        {
            return false;
        }

        if (skipIfBuffAlreadyExists)
        {
            ActiveBuff existing = member.buffController.GetBuffByName(buff.BuffName);
            if (existing != null)
            {
                if (verboseLog)
                {
                    Debug.Log($"[AFMManager] Skip apply: '{buff.BuffName}' already exists on '{member.gameObject.name}'.");
                }
                return false;
            }
        }

        member.buffController.AddBuff(buff);

        if (verboseLog)
        {
            Debug.Log($"[AFMManager] Applied buff '{buff.BuffName}' to '{member.gameObject.name}' (player_id={member.GetEntityID()}, type={ToApiType(response.type)}, tier={response.tier}).");
        }

        return true;
    }

    private List<PlayerEntity> GetControlledPlayers()
    {
        List<PlayerEntity> members = new List<PlayerEntity>();

        if (PlayerTeamManager.Instance != null && PlayerTeamManager.Instance.ActiveTeamMembers != null)
        {
            for (int i = 0; i < PlayerTeamManager.Instance.ActiveTeamMembers.Count; i++)
            {
                PlayerEntity member = PlayerTeamManager.Instance.ActiveTeamMembers[i];
                if (member != null && !members.Contains(member))
                {
                    members.Add(member);
                }
            }
        }

        if (members.Count == 0 && PlayerCombat.instance != null)
        {
            members.Add(PlayerCombat.instance);
        }

        return members;
    }

    private PlayerEntity FindPlayerById(int playerId)
    {
        List<PlayerEntity> members = GetControlledPlayers();
        for (int i = 0; i < members.Count; i++)
        {
            PlayerEntity member = members[i];
            if (member != null && member.GetEntityID() == playerId)
            {
                return member;
            }
        }

        return null;
    }

    private Buff ResolveBuff(AFMType responseType, int tier)
    {
        if (buffRules == null || buffRules.Count == 0)
        {
            return null;
        }

        for (int i = 0; i < buffRules.Count; i++)
        {
            AFMBuffRule rule = buffRules[i];
            if (rule == null || rule.buff == null)
            {
                continue;
            }

            if (rule.type == responseType && rule.tier == tier)
            {
                return rule.buff;
            }
        }

        return null;
    }

    private bool ShouldApply(PlayerEntity member)
    {
        if (member == null || member.buffController == null)
        {
            return false;
        }

        if (onlyAliveMembers && member.CurrentHealth <= 0f)
        {
            return false;
        }

        return true;
    }

    private static bool IsValidTier(int tier)
    {
        return tier >= 1 && tier <= 4;
    }

    private static bool TryParseType(string rawType, out AFMType parsed)
    {
        parsed = AFMType.EL;
        if (string.IsNullOrWhiteSpace(rawType))
        {
            return false;
        }

        string value = rawType.Trim().ToLowerInvariant();
        switch (value)
        {
            case "el":
                parsed = AFMType.EL;
                return true;
            case "ce":
                parsed = AFMType.CE;
                return true;
            case "rv":
                parsed = AFMType.RV;
                return true;
            default:
                return false;
        }
    }

    private static string ToApiType(AFMType type)
    {
        return type.ToString().ToLowerInvariant();
    }

    private bool ShouldSkipAFM()
    {
        // 1. Check UserData via TurnManager
        if (TurnManager.Instance != null && TurnManager.Instance.UserData != null)
        {
            int index = TurnManager.Instance.UserData.SelectedQuestIndex;
            if (index == UserData.TutorialQuestIndex || index == UserData.Quest1Index)
                return true;
        }

        // 2. Check current quest data directly (Tutorial flag)
        if (EnemyGenerator.Instance != null)
        {
            var quest = EnemyGenerator.Instance.GetCurrentQuest();
            if (quest != null && quest.isTutorial)
                return true;
        }

        return false;
    }
}

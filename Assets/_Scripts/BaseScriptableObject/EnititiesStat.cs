using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "EntitiesStat", menuName = "ScriptableObjects/EntitiesStat", order = 1)]
public class EntitiesBaseStat : ScriptableObject
{
    [System.Serializable]
    public struct StatOverride
    {
        public StatType statType;
        public float value;
    }

    [Header("Basic Information")]
    [Tooltip("Name of the Player")]
    public string EntityName;
    [Tooltip("Icon of the Player")]
    public Sprite Icon;

    [Header("Legacy/Core Stats (Keep for backwards compatibility)")]
    [Tooltip("Maximum Health of the Player")]
    public float MaxHealth;
    [Tooltip("Maximum Skill Points of the Player")]
    public int MaxSkillPoint;
    [Tooltip("Max Armor points required to trigger a Break state")]
    public int MaxBreakArmor;
    [Tooltip("Action Speed of the Player")]
    public float ActionSpeed;

    [Header("Scalable Stats")]
    [Tooltip("Add any additional stats here without needing to modify the script.")]
    public List<StatOverride> AdditionalStats = new List<StatOverride>();

    private Dictionary<StatType, float> statDictionary;

    public void InitializeStats()
    {
        if (statDictionary != null) return;

        statDictionary = new Dictionary<StatType, float>
        {
            { StatType.MaxHealth, MaxHealth },
            { StatType.MaxSkillPoint, MaxSkillPoint },
            { StatType.MaxBreakArmor, MaxBreakArmor },
            { StatType.ActionSpeed, ActionSpeed }
        };

        // Apply any overrides/additional stats defined in the inspector
        foreach (var stat in AdditionalStats)
        {
            statDictionary[stat.statType] = stat.value;
        }
    }

    public float GetBase(StatType stat)
    {
        if (statDictionary == null) InitializeStats();

        if (statDictionary.TryGetValue(stat, out float value))
        {
            return value;
        }
        
        Debug.LogWarning($"Stat {stat} not initialized/handled in EntitiesBaseStat: {EntityName}");
        return 0f;
    }

    public void SetBase(StatType stat, float value)
    {
        if (statDictionary == null) InitializeStats();
        statDictionary[stat] = value;
    }

    public EntitiesBaseStat Clone()
    {
        return Instantiate(this);
    }
    public string GetName()
    {
        return EntityName;
    }
    public Sprite GetIcon()
    {
        return Icon;
    }
}

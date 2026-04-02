using UnityEngine;

[CreateAssetMenu(fileName = "EntitiesStat", menuName = "ScriptableObjects/EntitiesStat", order = 1)]
public class EntitiesBaseStat : ScriptableObject
{
    [Header("Basic Information")]
    [Tooltip("Name of the Player")]
    public string EntityName;
    [Tooltip("Icon of the Player")]
    public Sprite Icon;
    [Tooltip("Maximum Health of the Player")]
    public float MaxHealth;
    [Tooltip("Maximum Skill Points of the Player")]
    public int MaxSkillPoint;
    [Tooltip("Max Armor points required to trigger a Break state")]
    public int MaxBreakArmor;
    [Tooltip("Action Speed of the Player")]
    public float ActionSpeed;

    public float GetBase(StatType stat)
    {
        switch (stat)
        {
            case StatType.MaxHealth:
                return MaxHealth;

            case StatType.MaxSkillPoint:
                return MaxSkillPoint;

            case StatType.ActionSpeed:
                return ActionSpeed;

            default:
                Debug.LogWarning($"Stat {stat} not handled in GetBase");
                return 0f;
        }
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

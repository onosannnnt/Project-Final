using System.Collections.Generic;
using UnityEngine;

public enum ModifierType
{
    Flat, Percent
}

public enum StackMultiplierType
{
    Linear,
    DiminishingReturn,
}
[System.Serializable]
public class StatModifier
{
    public StatType Stat;
    public ModifierType Type;
    [Tooltip("Amount of modification. If Percent, input as percentage (e.g., 0.1 for 10%) and if reduce, input negative value.")]
    public float Value;
}
public enum BuffType
{
    Buff,
    Debuff,
    CrowdControl,
}
public enum BuffOrder
{
    PreDamage,
    PostDamage,
}

[CreateAssetMenu(fileName = "Buff", menuName = "ScriptableObjects/Buff/Buff")]
public class Buff : ScriptableObject
{
    [Header("General Info")]
    public string BuffName;
    [TextArea(2, 4)] public string Description;
    public Sprite Icon;
    public BuffType buffType;

    [Header("Duration Settings")]
    public bool isPermanent = false; // Check this if the buff should stay forever until manually removed
    public int Duration = 3;

    [Header("Stacking Mechanics")]
    public bool isStackable = false;
    public int MaxStack = 0; // 0 means no limit
    public StackMultiplierType StackCalculationType;
    [Tooltip("Used for Target to do something when stack reach certain amount")]
    public int Threshold;
    [Tooltip("Used for Diminishing Return Stack Calculation")]
    public float DiminishingReturnDecayFactor;

    [Header("Effects & Modifiers")]
    public TargetType targetType;
    public List<StatModifier> modifiers = new List<StatModifier>();

    [Header("Special Flags")]
    [Tooltip("If enabled, this buff prevents the owner from dropping below 1 HP from damage.")]
    public bool preventLethalDamage = false;

    public virtual void OnApply(Entity owner, ActiveBuff buffState) { }
    public virtual void OnTurnStart(Entity owner, CombatActionLog log, ActiveBuff buffState)
    {
        buffState.wasReappliedThisTurn = false;
    }
    public virtual void OnTurnEnd(Entity owner, ActiveBuff buffState)
    {
        if (isPermanent) return;

        if (buffState.isInitialized)
        {
// // Debug.Log("Buff duration countdown started");
            buffState.isInitialized = false;
            return;
        }
        if (buffState.wasReappliedThisTurn)
        {
// // Debug.Log("Buff was reapplied this turn, skipping duration decrease");
            return;
        }
        buffState.CurrentDuration -= 1;
    }
    public virtual void OnRefresh(Entity owner, ActiveBuff buffState)
    {
        // default ไม่ทำอะไร
    }
    public virtual void OnRemove(Entity owner, ActiveBuff buffState) { }
}

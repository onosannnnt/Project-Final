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
    public string BuffName;
    public string Description;
    public Sprite Icon;
    public BuffType buffType;
    public int Duration = 3;
    public bool isStackable = false;
    public int Stack = 1;
    public StackMultiplierType StackCalculationType;
    [Tooltip("Used for Target to do something when stack reach certain amount")]
    public int Threshold;
    [Tooltip("Used for Diminishing Return Stack Calculation")]
    public float DiminishingReturnDecayFactor;
    public TargetType targetType;
    public List<StatModifier> modifiers = new List<StatModifier>();
    public bool isInitialized = true;

    public virtual void OnApply(Entity owner) { }
    public virtual void OnTurnStart(Entity owner, CombatActionLog log) { }
    public virtual void OnTurnEnd(Entity owner)
    {
        if (isInitialized)
        {
            Debug.Log("Buff duration countdown started");
            isInitialized = false;
            return;
        }
        Duration -= 1;
    }
    public virtual void OnRefresh(Entity owner)
    {
        // default ไม่ทำอะไร
    }
    public virtual void OnRemove(Entity owner) { }

    public Buff Clone()
    {
        return Instantiate(this);
    }
}
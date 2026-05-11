using UnityEngine;
using System.Collections.Generic;


[CreateAssetMenu(fileName = "StackPreserveBuff", menuName = "ScriptableObjects/Buff/StackPreserveBuff")]
public class StackPreserveBuff : Buff
{
    [Header("Preservation Rules")]
    [Tooltip("If enabled, this buff can preserve stacks for any consumed buff.")]
    public bool AppliesToAnyBuff = false;

    [Tooltip("Optional specific buffs that this preservation applies to.")]
    public List<Buff> TargetBuffs = new();

    [Tooltip("Fallback target buff names when TargetBuffs are not assigned.")]
    public List<string> TargetBuffNames = new() { "Frenzy" };

    [Range(0f, 100f)]
    public float PreserveChance = 20f;

    [Tooltip("Extra chance per stack of this buff (beyond the first stack).")]
    public float PreserveChancePerStack = 0f;

    public bool ShouldPreserve(ActiveBuff consumedBuff, ActiveBuff preserveState)
    {
        if (consumedBuff == null || preserveState == null)
        {
            return false;
        }

        if (!AppliesToAnyBuff && !IsTargetBuff(consumedBuff.Data))
        {
            return false;
        }

        float chance = GetPreserveChance(preserveState);
        if (chance <= 0f)
        {
            return false;
        }

        return Random.Range(0f, 100f) <= chance;
    }

    private float GetPreserveChance(ActiveBuff preserveState)
    {
        float bonus = Mathf.Max(0, preserveState.CurrentStack - 1) * PreserveChancePerStack;
        return Mathf.Clamp(PreserveChance + bonus, 0f, 100f);
    }

    private bool IsTargetBuff(Buff consumedBuff)
    {
        if (consumedBuff == null)
        {
            return false;
        }

        if (TargetBuffs != null)
        {
            foreach (Buff buff in TargetBuffs)
            {
                if (buff != null && !string.IsNullOrWhiteSpace(buff.BuffName) && consumedBuff.BuffName == buff.BuffName)
                {
                    return true;
                }
            }
        }

        if (TargetBuffNames != null)
        {
            foreach (string buffName in TargetBuffNames)
            {
                if (!string.IsNullOrWhiteSpace(buffName) && consumedBuff.BuffName == buffName)
                {
                    return true;
                }
            }
        }

        return false;
    }
}

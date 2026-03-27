using System;
using System.Collections.Generic;

[Serializable]
public class CombatActionLog
{
    public int CombatID;
    public int WaveID;
    public int TurnID;
    public int ActionID;
    public int CasterID;
    public string CasterName;
    public int TargetID;
    public string TargetName;
    public int SkillID;
    public string SkillName;
    public int ActionPointUsed;
    public int ActionPointRecovery;
    public float ActionSpeed;
    public List<DamageEffectLog> DamageEffectLogs;
    public List<BuffEffectLog> BuffEffectLogs;
    public List<HealEffectLog> HealEffectLogs;
    public List<EntityStatData> EntityLogs;
    public void AddDamageEffectLog(DamageEffectLog log)
    {
        if (DamageEffectLogs == null) return;
        DamageEffectLogs.Add(log);
    }
    public void AddBuffEffectLog(BuffEffectLog log)
    {
        if (BuffEffectLogs == null) return;
        BuffEffectLogs.Add(log);
    }
    public void AddHealEffectLog(HealEffectLog log)
    {
        if (HealEffectLogs == null) return;
        HealEffectLogs.Add(log);
    }
    public void AddEntityLog(EntityStatData log)
    {
        if (EntityLogs == null) return;
        // UnityEngine.Debug.Log("Entity log added");
        EntityLogs.Add(log);
    }
}

[Serializable]
public class DamageEffectLog
{
    public int AppliedTargetID;
    public string AppliedTarget;
    public DamageEffectData Damage;
}
[Serializable]
public class BuffEffectLog
{
    public int AppliedTargetID;
    public string AppliedTarget;
    public BuffEffectData Buff;
}
[Serializable]
public class HealEffectLog
{
    public int AppliedTargetID;
    public string AppliedTarget;
    public float HealAmount;
}

[Serializable]
public class DamageEffectData
{
    public string Type;
    public float Amount;
    public bool IsCriticalHit;

    public DamageEffectData(Damage damage)
    {
        Type = damage.Type.ToString(); // Convert enum to string
        Amount = damage.Amount;
        IsCriticalHit = damage.IsCriticalHit;
    }
}

[Serializable]
public class BuffEffectData
{
    // ข้อมูลบัฟ
    public string BuffName;
    public string BuffType;
    public int Duration;

    // ข้อมูล Modifier (ถ้าบัฟนั้นมีการแก้สเตตัส)
    public List<StatModifierData> statModifiers;

    // Constructor รับค่าจากเกมเพลย์มาแปลงร่าง
    public BuffEffectData(Buff buffAsset)
    {
        BuffName = buffAsset.BuffName;
        BuffType = buffAsset.buffType.ToString(); // แปลง Enum เป็น String
        Duration = buffAsset.Duration;
        statModifiers = new List<StatModifierData>();
        foreach (var modifier in buffAsset.modifiers)
        {
            statModifiers.Add(new StatModifierData(modifier));
        }
    }
}

[Serializable]
public class StatModifierData
{
    public string Stat;
    public string Type;
    public float Value;
    public StatModifierData(StatModifier stat)
    {
        Stat = stat.Stat.ToString();
        Type = stat.Type.ToString();
        Value = stat.Value;
    }
}

[Serializable]
public class EntityStatData
{
    public int EntityID;
    public string EntityName;
    public int Level;
    public float MaxHealth;
    public float CurrentHealth;
    public int MaxSkillPoint;
    public int CurrentSkillPoint;
    public float PhysicalAttack;
    public float MagicAttack;
    public float FireDamageMultiplier;
    public float ColdDamageMultiplier;
    public float LightningDamageMultiplier;
    public float Armour;
    public float FireResistance;
    public float ColdResistance;
    public float LightningResistance;
    public float ActionSpeed;
    public float CriticalHitChance;
    public float CriticalHitDamageMultiplier;
    public float Accuracy;
    public float EvasionRate;
    public float StatusEffectResistance;
    public float StatusHitChance;
    public EntityStatData(Entity entity)
    {
        EntityID = entity.GetEntityID();
        EntityName = entity.Stats.EntityName;
        Level = entity.Stats.Level;
        MaxHealth = entity.GetStat(StatType.MaxHealth);
        CurrentHealth = entity.CurrentHealth;
        MaxSkillPoint = (int)entity.GetStat(StatType.MaxSkillPoint);
        CurrentSkillPoint = entity.CurrentSP;
        PhysicalAttack = entity.GetStat(StatType.PhysicalAttack);
        MagicAttack = entity.GetStat(StatType.MagicAttack);
        FireDamageMultiplier = entity.GetStat(StatType.FireDamageMultiplier);
        ColdDamageMultiplier = entity.GetStat(StatType.ColdDamageMultiplier);
        LightningDamageMultiplier = entity.GetStat(StatType.LightningDamageMultiplier);
        Armour = entity.GetStat(StatType.Armour);
        FireResistance = entity.GetStat(StatType.FireResistance);
        ColdResistance = entity.GetStat(StatType.ColdResistance);
        LightningResistance = entity.GetStat(StatType.LightningResistance);
        ActionSpeed = entity.GetStat(StatType.ActionSpeed);
        CriticalHitChance = entity.GetStat(StatType.CriticalHitChance);
        CriticalHitDamageMultiplier = entity.GetStat(StatType.CriticalDamageMultiplier);
        Accuracy = entity.GetStat(StatType.Accuracy);
        EvasionRate = entity.GetStat(StatType.EvasionRate);
        StatusEffectResistance = entity.GetStat(StatType.StatusEffectResistance);
        StatusHitChance = entity.GetStat(StatType.StatusHitChance);
    }
}
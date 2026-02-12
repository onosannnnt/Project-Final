using UnityEngine;

public enum SkillEffectPhase
{
    PreDamage,   // Buff / Debuff
    Damage,
    PostDamage   // On-hit, extra effect
}

[System.Serializable]
public abstract class SkillEffect : ScriptableObject
{
    public abstract SkillEffectPhase Phase { get; }
    public abstract void Execute(Entity caster, Entity target);
}
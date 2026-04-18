using UnityEngine;

[CreateAssetMenu(fileName = "RepeatOnKillDamageEffect", menuName = "ScriptableObjects/SkillEffect/RepeatOnKillDamageEffect")]
public class RepeatOnKillDamageEffect : SkillEffect
{
    [Header("Damage Settings")]
    [SerializeField] public float DamageAmount = 300f;
    [SerializeField] public DamageElement Element = DamageElement.Physical;
    [SerializeField] public float Accuracy = 100f;
    [Range(0f, 100f)]
    [SerializeField] public float CriticalHitChance = 5f;
    [SerializeField] public float CriticalDamageMultiplier = 1.5f;

    public override bool Execute(Entity caster, Entity target, CombatActionLog log)
    {
        if (caster == null || target == null) return false;

        bool firstHitLanded = ApplyHit(caster, target, log, out bool firstHitKilledTarget);
        if (!firstHitLanded)
        {
            return false;
        }

        if (!firstHitKilledTarget)
        {
            return true;
        }

        EnemyCombat killedEnemy = target as EnemyCombat;
        if (killedEnemy == null)
        {
            return true;
        }

        EnemyCombat repeatTarget = FindAnotherAliveEnemy(killedEnemy);
        if (repeatTarget == null)
        {
            return true;
        }

        // Repeat exactly once on another alive enemy.
        ApplyHit(caster, repeatTarget, log, out _);
        return true;
    }

    private bool ApplyHit(Entity caster, Entity target, CombatActionLog log, out bool didKill)
    {
        didKill = false;

        if (Random.Range(0f, 100f) > Accuracy)
        {
            target.ShowDamage(0, Color.white);
            return false;
        }

        bool isCrit = Random.Range(0f, 100f) <= CriticalHitChance;
        float finalDamage = DamageAmount;
        if (isCrit)
        {
            finalDamage *= CriticalDamageMultiplier;
        }

        float healthBefore = target.CurrentHealth;

        Damage damage = new Damage(finalDamage, Element, isCrit);
        DamageCtx ctx = new DamageCtx(caster, target, damage);
        DamageSystem.Process(ctx, log);

        didKill = healthBefore > 0f && target.CurrentHealth <= 0f;
        return true;
    }

    private EnemyCombat FindAnotherAliveEnemy(EnemyCombat excluded)
    {
        EnemyCombat[] enemies = FindObjectsOfType<EnemyCombat>();
        foreach (EnemyCombat enemy in enemies)
        {
            if (enemy == null) continue;
            if (enemy == excluded) continue;
            if (enemy.IsDead()) continue;
            return enemy;
        }

        return null;
    }
}

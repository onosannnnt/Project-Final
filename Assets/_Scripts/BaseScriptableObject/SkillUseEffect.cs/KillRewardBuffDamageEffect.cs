using UnityEngine;

[CreateAssetMenu(fileName = "KillRewardBuffDamageEffect", menuName = "ScriptableObjects/SkillEffect/KillRewardBuffDamageEffect")]
public class KillRewardBuffDamageEffect : SkillEffect
{
    [Header("Damage Settings")]
    [SerializeField] public float DamageAmount = 300f;
    [SerializeField] public DamageElement Element = DamageElement.Physical;
    [SerializeField] public float Accuracy = 100f;
    [Range(0f, 100f)]
    [SerializeField] public float CriticalHitChance = 5f;
    [SerializeField] public float CriticalDamageMultiplier = 1.5f;

    [Header("Kill Reward")]
    [Tooltip("Buff gained by the caster if this hit kills the target")]
    public Buff BuffToGain;
    [Tooltip("How many stacks to gain when the hit kills")]
    public int StacksOnKill = 1;

    public override bool Execute(Entity caster, Entity target, CombatActionLog log)
    {
        if (caster == null || target == null) return false;

        // 1) Accuracy check
        if (Random.Range(0f, 100f) > Accuracy)
        {
            target.ShowDamage(0, Color.white);
            return false;
        }

        // 2) Damage + crit
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

        // 3) Reward stacks if this hit kills
        bool didKill = healthBefore > 0f && target.CurrentHealth <= 0f;
        if (didKill && BuffToGain != null && StacksOnKill > 0)
        {
            for (int i = 0; i < StacksOnKill; i++)
            {
                caster.buffController.AddBuff(BuffToGain);
            }

            log?.AddBuffEffectLog(new BuffEffectLog()
            {
                AppliedTargetID = caster.GetEntityID(),
                AppliedTarget = caster.gameObject.name,
                Buff = new BuffEffectData(BuffToGain)
            });
        }

        return true;
    }
}

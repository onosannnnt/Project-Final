using UnityEngine;

[CreateAssetMenu(fileName = "SharedPainEffect", menuName = "ScriptableObjects/SkillEffect/SharedPainEffect")]
public class SharedPainEffect : SkillEffect
{
    [Tooltip("How much of caster-received damage is mirrored to the debuffed target.")]
    [Range(0f, 100f)]
    public float RedirectPercent = 30f;

    [Tooltip("Template debuff that stores duration, icon, and UI metadata.")]
    public SharedPainDebuff sharedPainDebuffTemplate;

    public override bool Execute(Entity caster, Entity target, CombatActionLog log)
    {
        if (caster == null || target == null || sharedPainDebuffTemplate == null)
        {
            return false;
        }

        SharedPainDebuff debuff = Instantiate(sharedPainDebuffTemplate);
        debuff.LinkedCaster = caster;
        debuff.RedirectRatio = Mathf.Clamp01(RedirectPercent / 100f);

        target.buffController.AddBuff(debuff);

        log.AddBuffEffectLog(new BuffEffectLog()
        {
            AppliedTargetID = target.GetEntityID(),
            AppliedTarget = target.gameObject.name,
            Buff = new BuffEffectData(debuff)
        });

        return true;
    }
}

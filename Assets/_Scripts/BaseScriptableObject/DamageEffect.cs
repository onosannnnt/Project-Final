using UnityEngine;

[CreateAssetMenu(fileName = "DamageEffect", menuName = "ScriptableObjects/SkillEffect/DamageEffect")]
public class DamageEffect : SkillEffect
{
    [SerializeField] public float BaseDamage;
    [SerializeField] public DamageElement Element;
    [SerializeField] public float Accuracy = 100f;
    [Range(0f, 100f)]
    [SerializeField] public float CriticalHitChance = 5f;

    public override bool IsElementalAttackEffect => true;
    public override bool IsDotElementSource => false;

    //     public override bool Execute(Entity caster, Entity target, CombatActionLog log)
    //     {
    // // // Debug.Log(caster.gameObject.name + " dealt damage on " + target.gameObject.name);

    //         // 1. Accuracy Check
    //         if (Random.Range(0f, 100f) > Accuracy)
    //         {
    //             // // Debug.Log($"{caster.gameObject.name}'s attack but missed!");
    //             target.ShowDamage(0, Color.white);
    //             return false; 
    //         }

    //         // 2. Base Damage Calculation with +- 15% variance
    //         float variance = Random.Range(0.85f, 1.15f);
    //         float finalDamage = BaseDamage * variance;

    //         // 3. Critical Hit Check
    //         bool isCrit = Random.Range(0f, 100f) <= CriticalHitChance;
    //         if (isCrit)
    //         {
    //             finalDamage *= 1.5f;
    //      // Debug.Log($"Critical Hit!");
    //         }

    //      // Debug.Log($"Base Damage: {BaseDamage}, Variance: {variance}, Final Damage: {finalDamage}");

    //         Damage damage = new Damage(finalDamage, Element, isCrit);
    //         DamageCtx ctx = new DamageCtx(caster, target, damage);
    //         DamageSystem.Process(ctx, log);
    //         return true;
    //     }
    public override bool Execute(Entity caster, Entity target, CombatActionLog log)
    {
        // 1. Accuracy Check
        if (Random.Range(0f, 100f) > Accuracy)
        {
            target.ShowDamage(0, Color.white);
            return false;
        }

        // 2. Base Damage Calculation with +- 15% variance
        float variance = Random.Range(0.85f, 1.15f);
        float finalDamage = BaseDamage * variance;

        // 3. Critical Hit Check
        bool isCrit = Random.Range(0f, 100f) <= CriticalHitChance;
        if (isCrit)
        {
            finalDamage *= 1.5f;
        }

        // ==========================================
        // 4. บัฟดาเมจของบอส (เพิ่มส่วนนี้เข้าไป)
        // เช็คว่าคนที่ใช้สกิลนี้คือ NecromancerBossCombat หรือไม่
        // ==========================================
        if (caster is NecromancerBossCombat necromancer)
        {
            float bossMultiplier = necromancer.GetDamageMultiplier();
            finalDamage *= bossMultiplier;

            // (Optional) เปิด Debug ดูว่าดาเมจถูกคูณจริงไหม
            // Debug.Log($"[Boss Buff] ดาเมจถูกคูณ {bossMultiplier} เท่า! ดาเมจสุดท้าย = {finalDamage}");
        }
        // ==========================================

        Damage damage = new Damage(finalDamage, Element, isCrit);
        DamageCtx ctx = new DamageCtx(caster, target, damage);
        DamageSystem.Process(ctx, log);
        return true;
    }
}

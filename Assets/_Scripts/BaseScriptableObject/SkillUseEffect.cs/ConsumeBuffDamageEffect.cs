using UnityEngine;

[CreateAssetMenu(fileName = "ConsumeBuffDamageEffect", menuName = "ScriptableObjects/SkillEffect/ConsumeBuffDamageEffect")]
public class ConsumeBuffDamageEffect : DamageEffect
{
    [Header("Buff Consumption Bonus")]
    [SerializeField] public string BuffNameToConsume = "Frenzy";
    [SerializeField] public int MaxStacksToConsume = 3;
    [SerializeField] public float DamagePerStack = 50f;

    [System.NonSerialized] private int _lastActionID = -1;
    [System.NonSerialized] private int _cachedStacksConsumed = 0;

    public override bool Execute(Entity caster, Entity target, CombatActionLog log)
    {
// // Debug.Log(caster.gameObject.name + " dealt damage on " + target.gameObject.name);

        // 1. Accuracy Check
        if (Random.Range(0f, 100f) > Accuracy)
        {
            // // Debug.Log($"{caster.gameObject.name}'s attack but missed!");
            target.ShowDamage(0, Color.white);
            return false; 
        }

        // Calculate Base + Bonus from Buff
        float totalDamage = BaseDamage;

        // Process buff consumption ONCE per action (useful for AoE attacks where this runs multiple times)
        if (log != null && log.ActionID != _lastActionID)
        {
            _lastActionID = log.ActionID;
            _cachedStacksConsumed = 0;

            ActiveBuff buff = caster.buffController.GetBuffByName(BuffNameToConsume);
// // Debug.Log($"[ConsumeBuffDamageEffect] Looking for buff: {BuffNameToConsume}. Found on caster: {buff != null}");
            
            if (buff == null) 
            {
                buff = target.buffController.GetBuffByName(BuffNameToConsume);
// // Debug.Log($"[ConsumeBuffDamageEffect] Looking for buff: {BuffNameToConsume}. Found on target: {buff != null}");
            }

            if (buff != null && buff.CurrentStack > 0)
            {
// // Debug.Log($"[ConsumeBuffDamageEffect] {BuffNameToConsume} found with {buff.CurrentStack} stacks.");
                _cachedStacksConsumed = Mathf.Min(buff.CurrentStack, MaxStacksToConsume);

                if (caster.buffController.GetBuffByName(BuffNameToConsume) != null)
                {
                    caster.buffController.ConsumeBuffStack(buff, _cachedStacksConsumed);
                }
                else
                {
                    target.buffController.ConsumeBuffStack(buff, _cachedStacksConsumed);
                }

// // Debug.Log($"Consumed {_cachedStacksConsumed} stacks of {BuffNameToConsume} for bonus damage.");
            }
        }

        // Apply bonus damage equivalent to the cached consumed stacks
        totalDamage += _cachedStacksConsumed * DamagePerStack;

        // 2. Variance
        float variance = Random.Range(0.85f, 1.15f);
        float finalDamage = totalDamage * variance;

        // 3. Critical Hit Check
        bool isCrit = Random.Range(0f, 100f) <= CriticalHitChance;
        if (isCrit)
        {
            finalDamage *= 1.5f;
// // Debug.Log($"Critical Hit!");
        }

// // Debug.Log($"Base Damage: {totalDamage}, Variance: {variance}, Final Damage: {finalDamage}");

        Damage damage = new Damage(finalDamage, Element, isCrit);
        DamageCtx ctx = new DamageCtx(caster, target, damage);
        DamageSystem.Process(ctx, log);
        return true;
    }
}

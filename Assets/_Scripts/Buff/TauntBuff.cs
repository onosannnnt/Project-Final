using UnityEngine;

[CreateAssetMenu(fileName = "TauntBuff", menuName = "ScriptableObjects/Buff/TauntBuff")]
public class TauntBuff : Buff
{
    // Who the target is forced to attack (hidden because TauntEffect sets it)
    [HideInInspector] public Entity Taunter;
    
    // Extra damage taken while taunted (hidden because TauntEffect sets it)
    [HideInInspector] public float ExtraDamageTaken;
    
    public override void OnApply(Entity owner, ActiveBuff buffState)
    {
        base.OnApply(owner, buffState);
        IncomingVulnerabilityModifier modifier = new IncomingVulnerabilityModifier(ExtraDamageTaken);
        buffState.CustomState["modifier"] = modifier;
        owner.AddModifier(modifier, EntityModifierType.Incoming);

        string taunterName = Taunter != null ? Taunter.gameObject.name : "Unknown";
// // Debug.Log($"{owner.gameObject.name} is TAUNTED by {taunterName} and will take +{ExtraDamageTaken} damage from all attacks!");
    }

    public override void OnRemove(Entity owner, ActiveBuff buffState)
    {
        base.OnRemove(owner, buffState);
        if (buffState.CustomState.TryGetValue("modifier", out object modifierObj))
        {
            owner.RemoveModifier((IncomingVulnerabilityModifier)modifierObj, EntityModifierType.Incoming);
        }
// // Debug.Log($"{owner.gameObject.name} is no longer taunted.");
    }

    private class IncomingVulnerabilityModifier : IDamageModifier
    {
        private float extraDamage;

        public IncomingVulnerabilityModifier(float extraDamage)
        {
            this.extraDamage = extraDamage;
        }

        public void Modify(DamageCtx ctx)
        {
            // Increase the incoming damage amount
            ctx.Damage.Amount += extraDamage;
// // Debug.Log($"Taunt Vulnerability applied! +{extraDamage} damage.");
        }
    }
}
    

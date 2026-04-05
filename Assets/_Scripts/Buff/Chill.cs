using UnityEngine;

[CreateAssetMenu(fileName = "Buff", menuName = "ScriptableObjects/Buff/Chill")]
public class Chill : Buff
{
    [SerializeField] private Buff FreezeBuff;
    public override void OnApply(Entity owner, ActiveBuff buffState)
    {
        base.OnApply(owner, buffState);
        ActiveBuff chilledBuff = owner.buffController.GetBuffByName("Chill");
        Debug.Log(owner.gameObject.name + " has " + chilledBuff.CurrentStack + " stacks of Chill.");
        if (chilledBuff == null) return;
        if (chilledBuff.CurrentStack >= Threshold)
        {
            Debug.Log(owner.gameObject.name + " has reached the threshold for Chill buff. Removing Chill and applying Freeze.");
            owner.buffController.RemoveBuff(chilledBuff);
            owner.buffController.AddBuff(FreezeBuff);
        }
    }

}

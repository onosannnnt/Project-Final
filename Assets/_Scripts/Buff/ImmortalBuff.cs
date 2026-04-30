using UnityEngine;

[CreateAssetMenu(fileName = "ImmortalBuff", menuName = "ScriptableObjects/Buff/ImmortalBuff")]
public class ImmortalBuff : Buff
{
    private void OnEnable()
    {
        // Keep this buff non-lethal by default when created as an asset.
        preventLethalDamage = true;
    }
}

using UnityEngine.Video;

[System.Serializable]
public class StatusBuff
{
    public BuffType buffType;
    public float duration;
    public bool isApplied = false;
    public bool isStackable = false;
    public int Stack = 1;
    public float amountMultiplier = 0f;
    public float amountFlat = 0f;
    public float PhysicalDamageMultiplierByMaxHealth = 0f;
    public float fireDamageMultiplierByMaxHealth = 0f;
    public float iceDamageMultiplierByMaxHealth = 0f;
    public float lightningDamageMultiplierByMaxHealth = 0f;
    public float PhysicalDamageFlat = 0f;
    public float FireDamageFlat = 0f;
    public float IceDamageFlat = 0f;
    public float LightningDamageFlat = 0f;


    public void IncreaseStack(int amount)
    {
        if (isStackable)
        {
            Stack += amount;
        }
    }

    public void DecreaseDuration(float amount)
    {
        duration -= amount;
    }

    public void ResetDuration(float newDuration)
    {
        duration = newDuration;
    }
    public void SetStack(int newStack)
    {
        Stack = newStack;
    }
    public StatusBuff Clone()
    {
        StatusBuff cloneBuff = new StatusBuff();
        cloneBuff.amountFlat = this.amountFlat;
        cloneBuff.amountMultiplier = this.amountMultiplier;
        cloneBuff.buffType = this.buffType;
        cloneBuff.duration = this.duration;
        cloneBuff.isApplied = this.isApplied;
        cloneBuff.isStackable = this.isStackable;
        cloneBuff.Stack = this.Stack;
        cloneBuff.PhysicalDamageFlat = this.PhysicalDamageFlat;
        cloneBuff.PhysicalDamageMultiplierByMaxHealth = this.PhysicalDamageMultiplierByMaxHealth;
        cloneBuff.FireDamageFlat = this.FireDamageFlat;
        cloneBuff.fireDamageMultiplierByMaxHealth = this.fireDamageMultiplierByMaxHealth;
        cloneBuff.IceDamageFlat = this.IceDamageFlat;
        cloneBuff.iceDamageMultiplierByMaxHealth = this.iceDamageMultiplierByMaxHealth;
        cloneBuff.LightningDamageFlat = this.LightningDamageFlat;
        cloneBuff.lightningDamageMultiplierByMaxHealth = this.lightningDamageMultiplierByMaxHealth;
        return cloneBuff;
    }
}
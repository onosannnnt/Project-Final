[System.Serializable]
public class StatusBuff
{
    public BuffType BuffType;
    public float Duration;
    public bool IsApplied = false;
    public bool IsStackable = false;
    public int Stack = 1;
    public float AmountMultiplier = 0f;
    public float AmountFlat = 0f;
    public float PhysicalDamageMultiplierByMaxHealth = 0f;
    public float FireDamageMultiplierByMaxHealth = 0f;
    public float IceDamageMultiplierByMaxHealth = 0f;
    public float LightningDamageMultiplierByMaxHealth = 0f;
    public float PhysicalDamageFlat = 0f;
    public float FireDamageFlat = 0f;
    public float IceDamageFlat = 0f;
    public float LightningDamageFlat = 0f;
    public StatusBuff Clone()
    {
        StatusBuff cloneBuff = new StatusBuff();
        cloneBuff.AmountFlat = this.AmountFlat;
        cloneBuff.AmountMultiplier = this.AmountMultiplier;
        cloneBuff.BuffType = this.BuffType;
        cloneBuff.Duration = this.Duration;
        cloneBuff.IsApplied = this.IsApplied;
        cloneBuff.IsStackable = this.IsStackable;
        cloneBuff.Stack = this.Stack;
        cloneBuff.PhysicalDamageFlat = this.PhysicalDamageFlat;
        cloneBuff.PhysicalDamageMultiplierByMaxHealth = this.PhysicalDamageMultiplierByMaxHealth;
        cloneBuff.FireDamageFlat = this.FireDamageFlat;
        cloneBuff.FireDamageMultiplierByMaxHealth = this.FireDamageMultiplierByMaxHealth;
        cloneBuff.IceDamageFlat = this.IceDamageFlat;
        cloneBuff.IceDamageMultiplierByMaxHealth = this.IceDamageMultiplierByMaxHealth;
        cloneBuff.LightningDamageFlat = this.LightningDamageFlat;
        cloneBuff.LightningDamageMultiplierByMaxHealth = this.LightningDamageMultiplierByMaxHealth;
        return cloneBuff;
    }
}
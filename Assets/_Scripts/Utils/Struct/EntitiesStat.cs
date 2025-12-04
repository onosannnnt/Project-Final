public struct Damage
{
    public float PhysicalDamage;
    public float FireDamage;
    public float ColdDamage;
    public float LightningDamage;

    public Damage(float physical, float fire, float cold, float lightning)
    {
        PhysicalDamage = physical;
        FireDamage = fire;
        ColdDamage = cold;
        LightningDamage = lightning;
    }
}
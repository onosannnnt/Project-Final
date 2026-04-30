public enum PlayerActionState
{
    Idle,
    Targeting,
    Action,
}
public enum EntityState
{
    None,
    Immobilized,   // ขยับไม่ได้
    Silenced,      // ใช้สกิลไม่ได้
    Disarmed,      // ตีไม่ได้
    Frozen,        // รวม immobilize + stun
}

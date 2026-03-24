public class ActionQueue
{
    public Entity Caster;
    public Entity Target;
    public Skill Skill;
    public float ActionSpeed;
    public ActionQueue(Entity caster, Entity target, Skill skill, float actionSpeed)
    {
        Caster = caster;
        Target = target;
        Skill = skill;
        ActionSpeed = actionSpeed;
    }
}
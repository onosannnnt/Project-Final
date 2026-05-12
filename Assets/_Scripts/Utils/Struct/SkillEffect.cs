using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection;

public enum SkillEffectPhase
{
    PreDamage,   // Buff / Debuff
    Damage,
    PostDamage   // On-hit, extra effect
}

public abstract class SkillEffect : ScriptableObject
{
    public SkillEffectPhase Phase = SkillEffectPhase.Damage;

    private const string ElementMemberName = "Element";
    private static readonly Dictionary<Type, FieldInfo> ElementFieldCache = new();
    private static readonly Dictionary<Type, PropertyInfo> ElementPropertyCache = new();
    private static readonly HashSet<Type> NoElementCache = new();

    // Only attack effects should opt-in to provide a skill element.
    public virtual bool IsElementalAttackEffect => false;

    // Only true DoT effects should opt-in to provide DamageElement.Dot as skill element.
    public virtual bool IsDotElementSource => false;

    public virtual bool TryGetElement(out DamageElement element)
    {
        return TryGetElementByConvention(out element);
    }

    public virtual bool TrySetElement(DamageElement element)
    {
        return TrySetElementByConvention(element);
    }

    [Header("Refactor Settings")]
    [Tooltip("If true, this effect runs only once per skill cast (useful for Caster buffs or one-time calculations). If false, it runs for every target in the skill.")]
    public bool ExecuteOnce = false;

    /// <summary>
    /// Called ONCE before the targeting loop begins. Use this for resource consumption, 
    /// complex scaling calculations, or setting up state that should be consistent across all targets.
    /// </summary>
    public virtual void OnSkillStarted(Entity caster, List<Entity> targets, CombatActionLog log) { }

    protected bool TryGetElementByConvention(out DamageElement element)
    {
        Type type = GetType();

        if (NoElementCache.Contains(type))
        {
            element = DamageElement.None;
            return false;
        }

        if (ElementFieldCache.TryGetValue(type, out FieldInfo cachedField))
        {
            element = (DamageElement)cachedField.GetValue(this);
            return true;
        }

        if (ElementPropertyCache.TryGetValue(type, out PropertyInfo cachedProperty))
        {
            element = (DamageElement)cachedProperty.GetValue(this);
            return true;
        }

        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        FieldInfo field = type.GetField(ElementMemberName, flags);
        if (field != null && field.FieldType == typeof(DamageElement))
        {
            ElementFieldCache[type] = field;
            element = (DamageElement)field.GetValue(this);
            return true;
        }

        PropertyInfo property = type.GetProperty(ElementMemberName, flags);
        if (property != null && property.PropertyType == typeof(DamageElement) && property.GetIndexParameters().Length == 0)
        {
            ElementPropertyCache[type] = property;
            element = (DamageElement)property.GetValue(this);
            return true;
        }

        NoElementCache.Add(type);
        element = DamageElement.None;
        return false;
    }

    protected bool TrySetElementByConvention(DamageElement element)
    {
        Type type = GetType();
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        FieldInfo field = null;
        if (ElementFieldCache.TryGetValue(type, out FieldInfo cachedField))
        {
            field = cachedField;
        }
        else
        {
            field = type.GetField(ElementMemberName, flags);
            if (field != null && field.FieldType == typeof(DamageElement))
            {
                ElementFieldCache[type] = field;
            }
        }

        if (field != null && field.FieldType == typeof(DamageElement) && !field.IsInitOnly && !field.IsLiteral)
        {
            field.SetValue(this, element);
            NoElementCache.Remove(type);
            return true;
        }

        PropertyInfo property = null;
        if (ElementPropertyCache.TryGetValue(type, out PropertyInfo cachedProperty))
        {
            property = cachedProperty;
        }
        else
        {
            property = type.GetProperty(ElementMemberName, flags);
            if (property != null && property.PropertyType == typeof(DamageElement) && property.GetIndexParameters().Length == 0)
            {
                ElementPropertyCache[type] = property;
            }
        }

        if (property != null && property.PropertyType == typeof(DamageElement) && property.CanWrite)
        {
            property.SetValue(this, element);
            NoElementCache.Remove(type);
            return true;
        }

        return false;
    }

    public abstract bool Execute(Entity caster, Entity target, CombatActionLog log, SkillStyle style = SkillStyle.None);
}

using UnityEngine;

[CreateAssetMenu(fileName = "LockWeatherEffect", menuName = "ScriptableObjects/SkillEffect/LockWeatherEffect")]
public class LockWeatherEffect : SkillEffect
{
    [SerializeField] private int duration = 2;

    public override bool Execute(Entity caster, Entity target, CombatActionLog log)
    {
        if (WeatherManager.Instance != null)
        {
            WeatherManager.Instance.LockWeather(duration);
            return true;
        }
        return false;
    }
}

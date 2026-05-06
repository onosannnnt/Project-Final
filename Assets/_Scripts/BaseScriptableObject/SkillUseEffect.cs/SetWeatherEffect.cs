using UnityEngine;

[CreateAssetMenu(fileName = "SetWeatherEffect", menuName = "ScriptableObjects/SkillEffect/SetWeatherEffect")]
public class SetWeatherEffect : SkillEffect
{
    [SerializeField] private WeatherType nextWeather;

    public override bool Execute(Entity caster, Entity target, CombatActionLog log)
    {
        if (WeatherManager.Instance != null)
        {
            WeatherManager.Instance.SetNextWeather(nextWeather);
            return true;
        }
        return false;
    }
}

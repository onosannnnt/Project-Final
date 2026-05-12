using UnityEngine;

[CreateAssetMenu(fileName = "SetWeatherEffect", menuName = "ScriptableObjects/SkillEffect/SetWeatherEffect")]
public class SetWeatherEffect : SkillEffect
{
    [SerializeField] private WeatherType nextWeather;

    private void Awake()
    {
        ExecuteOnce = true;
    }

    public override bool Execute(Entity caster, Entity target, CombatActionLog log, SkillStyle style = SkillStyle.None)
    {
        if (WeatherManager.Instance != null)
        {
            WeatherManager.Instance.SetNextWeather(nextWeather);
            return true;
        }
        return false;
    }
}

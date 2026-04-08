using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WeatherBranchingEffect", menuName = "ScriptableObjects/SkillEffect/WeatherBranchingEffect")]
public class WeatherBranchingEffect : SkillEffect 
{
    [System.Serializable]
    public struct WeatherBranch
    {
        [Tooltip("The weather condition required to trigger these effects")]
        public WeatherType WeatherCondition;
        
        [Tooltip("The bonus effects to play when the weather matches (can safely be left empty)")]
        public List<SkillEffect> BonusEffects;
    }

    [Header("Weather Branches")]
    [Tooltip("Define which effects happen during which weather here.")]
    public List<WeatherBranch> Branches;

    public override bool Execute(Entity caster, Entity target, CombatActionLog log)
    {
        if (WeatherManager.Instance == null) return true;

        WeatherType currentWeather = WeatherManager.Instance.CurrentWeather;
// // Debug.Log($"[WeatherBranchingEffect] Checking branches for: {currentWeather}");

        bool allHit = true;

        foreach (var branch in Branches)
        {
            // Find the branch that matches the current weather
            if (branch.WeatherCondition == currentWeather)
            {
                if (branch.BonusEffects == null || branch.BonusEffects.Count == 0)
                    continue;

// // Debug.Log($"[WeatherBranchingEffect] Triggering {branch.BonusEffects.Count} bonus effect(s) for {currentWeather}.");

                // Execute all bonus effects for that weather
                foreach (var effect in branch.BonusEffects)
                {
                    bool hit = effect.Execute(caster, target, log);
                    if (!hit) 
                    {
                        allHit = false; 
                    }
                }

                // Stop checking other branches once we found the matching weather
                break;
            }
        }
        
        // Return whether the bonus effects hit or missed (usually returns true unless an accuracy-based damage effect missed)
        return allHit;
    }
}

using UnityEngine;

public enum WeatherType
{
    Sunny,
    Rainy,
    Windy
}

public class WeatherManager : Singleton<WeatherManager>
{
    public WeatherType CurrentWeather { get; private set; }
    public WeatherType NextWeather { get; private set; }
    public int WeatherStreak { get; private set; } = 1;

    public System.Action<WeatherType, WeatherType, int> OnWeatherChanged;

    protected override void Awake()
    {
        base.Awake();
        RandomizeInitialWeather();
    }

    private void RandomizeInitialWeather()
    {
        CurrentWeather = GetRandomWeather();
        NextWeather = GetRandomWeather();
    }

    public void AdvanceWeather()
    {
        if (CurrentWeather == NextWeather)
        {
            WeatherStreak++;
        }
        else
        {
            WeatherStreak = 1;
        }

        CurrentWeather = NextWeather;
        NextWeather = GetRandomWeather();
        
        OnWeatherChanged?.Invoke(CurrentWeather, NextWeather, WeatherStreak);
// // Debug.Log($"[WeatherManager] Weather changed to {CurrentWeather} (Streak x{WeatherStreak}). Next turn weather will be {NextWeather}.");
    }

    private WeatherType GetRandomWeather()
    {
        int totalWeathers = System.Enum.GetValues(typeof(WeatherType)).Length;
        return (WeatherType)Random.Range(0, totalWeathers);
    }
}

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
    private int lockDuration = 0;

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

    public void SetNextWeather(WeatherType next)
    {
        NextWeather = next;
        OnWeatherChanged?.Invoke(CurrentWeather, NextWeather, WeatherStreak);
    }

    public void LockWeather(int duration)
    {
        lockDuration = duration;
    }

    public void AdvanceWeather()
    {
        // 1. Always transition to the previously forecasted weather
        CurrentWeather = NextWeather;

        // 2. Determine the next forecast
        if (lockDuration > 0)
        {
            lockDuration--;
            // If locked, the next forecast is forced to stay the same as current
            NextWeather = CurrentWeather;
        }
        else
        {
            // Otherwise randomize normally
            NextWeather = GetRandomWeather();
        }

        // 3. Update streaks
        if (CurrentWeather == NextWeather)
        {
            WeatherStreak++;
        }
        else
        {
            WeatherStreak = 1;
        }
        
        OnWeatherChanged?.Invoke(CurrentWeather, NextWeather, WeatherStreak);
    }

    private WeatherType GetRandomWeather()
    {
        int totalWeathers = System.Enum.GetValues(typeof(WeatherType)).Length;
        return (WeatherType)Random.Range(0, totalWeathers);
    }
}

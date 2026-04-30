using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WeatherUI : MonoBehaviour
{
    [SerializeField] private Image currentWeatherIcon;
    [SerializeField] private Image nextWeatherIcon;
    [SerializeField] private TextMeshProUGUI streakText; // New text to show x2, x3, etc.
    
    [Header("Weather Icons")]
    [SerializeField] private Sprite sunnySprite;
    [SerializeField] private Sprite rainySprite;
    [SerializeField] private Sprite windySprite;

    private void Start()
    {
        if (WeatherManager.Instance != null)
        {
            WeatherManager.Instance.OnWeatherChanged += UpdateWeatherUI;
            // Initialize with starting weather
            UpdateWeatherUI(WeatherManager.Instance.CurrentWeather, WeatherManager.Instance.NextWeather, WeatherManager.Instance.WeatherStreak);
        }
    }

    private void OnEnable()
    {
        if (WeatherManager.Instance != null)
        {
            UpdateWeatherUI(WeatherManager.Instance.CurrentWeather, WeatherManager.Instance.NextWeather, WeatherManager.Instance.WeatherStreak);
        }
    }

    private void OnDestroy()
    {
        // Check HasInstance to safely see if the manager exists without re-creating or throwing errors
        if (WeatherManager.HasInstance)
        {
            WeatherManager.Instance.OnWeatherChanged -= UpdateWeatherUI;
        }
    }

    private void UpdateWeatherUI(WeatherType current, WeatherType next, int streak)
    {
        currentWeatherIcon.sprite = GetWeatherSprite(current);
        nextWeatherIcon.sprite = GetWeatherSprite(next);

        if (streakText != null)
        {
            if (streak > 1)
            {
                streakText.gameObject.SetActive(true);
                streakText.text = $"x{streak}"; // e.g., "x3" when getting the same weather 3 times
                
                // Optional: You can trigger a small animation or change text color (e.g., turn red at x3) here to make it funny!
            }
            else
            {
                streakText.gameObject.SetActive(false);
            }
        }
    }

    private Sprite GetWeatherSprite(WeatherType weatherType)
    {
        switch (weatherType)
        {
            case WeatherType.Sunny:
                return sunnySprite;
            case WeatherType.Rainy:
                return rainySprite;
            case WeatherType.Windy:
                return windySprite;
            default:
                return null;
        }
    }
}

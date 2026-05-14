using UnityEngine;
using UnityEngine.UI;

public class WaveProgressUI : MonoBehaviour
{
    [Header("Dots")]
    [SerializeField] private Image[] waveDots;
    [SerializeField] private Sprite notDoneDotSprite;
    [SerializeField] private Sprite doneDotSprite;
    [SerializeField] private Sprite currentDotSprite;

    [Header("Progress Bars (between dots)")]
    [SerializeField] private Image[] progressBars;
    [SerializeField] private Sprite notDoneProgressBarSprite;
    [SerializeField] private Sprite doneProgressBarSprite;

    [Header("Refresh")]
    [SerializeField] private bool autoRefresh = true;
    [SerializeField] private float refreshInterval = 0.2f;

    private int lastCurrentWave = -1;
    private int lastMaxWave = -1;
    private float refreshTimer;

    private void OnEnable()
    {
        refreshTimer = 0f;
        RefreshUI();
    }

    private void Update()
    {
        if (!autoRefresh)
        {
            return;
        }

        if (HasWaveChanged())
        {
            RefreshUI();
            return;
        }

        refreshTimer += Time.deltaTime;
        if (refreshTimer >= Mathf.Max(0.05f, refreshInterval))
        {
            refreshTimer = 0f;
            RefreshUI();
        }
    }

    public void RefreshUI()
    {
        TurnManager manager = TurnManager.Instance;
        if (manager == null)
        {
            int fallbackCurrent = 1;
            int fallbackMax = waveDots != null ? waveDots.Length : 0;
            lastCurrentWave = fallbackCurrent;
            lastMaxWave = fallbackMax;
            ApplyFallback(fallbackCurrent, fallbackMax);
            return;
        }

        int currentWave = Mathf.Max(1, manager.currentWave);
        int maxWave = Mathf.Max(1, Mathf.RoundToInt(manager.GetMaxWave()));

        if (currentWave == lastCurrentWave && maxWave == lastMaxWave)
        {
            return;
        }

        lastCurrentWave = currentWave;
        lastMaxWave = maxWave;

        UpdateDots(currentWave, maxWave);
        UpdateBars(currentWave, maxWave);
    }

    private bool HasWaveChanged()
    {
        TurnManager manager = TurnManager.Instance;
        if (manager == null)
        {
            int fallbackCurrent = 1;
            int fallbackMax = waveDots != null ? waveDots.Length : 0;
            return lastCurrentWave != fallbackCurrent || lastMaxWave != fallbackMax;
        }

        int currentWave = Mathf.Max(1, manager.currentWave);
        int maxWave = Mathf.Max(1, Mathf.RoundToInt(manager.GetMaxWave()));
        return currentWave != lastCurrentWave || maxWave != lastMaxWave;
    }

    private void UpdateDots(int currentWave, int maxWave)
    {
        if (waveDots == null) return;

        for (int i = 0; i < waveDots.Length; i++)
        {
            Image dot = waveDots[i];
            if (dot == null) continue;

            int waveIndex = i + 1;
            Sprite sprite = GetDotSpriteForWave(waveIndex, currentWave);
            if (sprite != null)
            {
                dot.sprite = sprite;
            }
        }
    }

    private Sprite GetDotSpriteForWave(int waveIndex, int currentWave)
    {
        if (currentWave > waveIndex) return doneDotSprite;
        if (currentWave == waveIndex) return currentDotSprite;
        return notDoneDotSprite;
    }

    private void UpdateBars(int currentWave, int maxWave)
    {
        if (progressBars == null) return;

        int maxBars = Mathf.Max(0, maxWave - 1);
        int doneBars = Mathf.Clamp(currentWave - 1, 0, maxBars);
        for (int i = 0; i < progressBars.Length; i++)
        {
            Image bar = progressBars[i];
            if (bar == null) continue;

            bool isDone = i < doneBars;
            Sprite sprite = isDone ? doneProgressBarSprite : notDoneProgressBarSprite;
            if (sprite != null)
            {
                bar.sprite = sprite;
            }
        }
    }

    private void ApplyFallback(int currentWave, int maxWave)
    {
        UpdateDots(currentWave, maxWave);
        UpdateBars(currentWave, maxWave);
    }
}

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class StatInfoSwitcherUI : MonoBehaviour
{
    [SerializeField] private StatInfoUI statInfoUI;
    [SerializeField] private Button playerOneButton;
    [SerializeField] private Button playerTwoButton;
    [SerializeField] private Button openStatInfoButton;
    [SerializeField] private PlayerEntity playerOne;
    [SerializeField] private PlayerEntity playerTwo;

    [Header("Auto Binding")]
    [SerializeField] private bool autoBindFromTeam = true;
    [SerializeField] private int playerOneIndex = 0;
    [SerializeField] private int playerTwoIndex = 1;
    [SerializeField] private float autoBindTimeoutSeconds = 3f;
    [SerializeField] private float rebindIntervalSeconds = 0.25f;

    private PlayerEntity lastSelectedPlayer;

    private void Start()
    {
        if (playerOneButton != null) playerOneButton.onClick.AddListener(OnPlayerOneClicked);
        if (playerTwoButton != null) playerTwoButton.onClick.AddListener(OnPlayerTwoClicked);
        if (openStatInfoButton != null) openStatInfoButton.onClick.AddListener(OnOpenStatInfoClicked);

        if (autoBindFromTeam && (playerOne == null || playerTwo == null))
        {
            StartCoroutine(BindPlayersFromTeamWhenReady());
        }
        else
        {
            UpdateButtonState();
        }
    }

    private IEnumerator BindPlayersFromTeamWhenReady()
    {
        float elapsed = 0f;
        bool warned = false;
        float interval = Mathf.Max(0.1f, rebindIntervalSeconds);

        while (true)
        {
            if (PlayerTeamManager.Instance != null)
            {
                if (playerOne == null && playerOneIndex >= 0)
                {
                    playerOne = PlayerTeamManager.Instance.GetMemberAt(playerOneIndex);
                }

                if (playerTwo == null && playerTwoIndex >= 0)
                {
                    playerTwo = PlayerTeamManager.Instance.GetMemberAt(playerTwoIndex);
                }
            }

            if (playerOne == null && playerOneIndex == 0 && PlayerCombat.instance != null)
            {
                playerOne = PlayerCombat.instance;
            }

            UpdateButtonState();

            if (playerOne != null && playerTwo != null)
            {
                yield break;
            }

            if (!warned && elapsed >= autoBindTimeoutSeconds)
            {
                warned = true;
                Debug.LogWarning("[StatInfoSwitcherUI] Waiting for team members to spawn. Will keep trying.");
            }

            elapsed += interval;
            yield return new WaitForSeconds(interval);
        }
    }

    private void OnDestroy()
    {
        if (playerOneButton != null) playerOneButton.onClick.RemoveListener(OnPlayerOneClicked);
        if (playerTwoButton != null) playerTwoButton.onClick.RemoveListener(OnPlayerTwoClicked);
        if (openStatInfoButton != null) openStatInfoButton.onClick.RemoveListener(OnOpenStatInfoClicked);
    }

    private void OnPlayerOneClicked()
    {
        lastSelectedPlayer = playerOne;
        ShowStatInfo(playerOne);
    }

    private void OnPlayerTwoClicked()
    {
        lastSelectedPlayer = playerTwo;
        ShowStatInfo(playerTwo);
    }

    private void OnOpenStatInfoClicked()
    {
        PlayerEntity target = lastSelectedPlayer != null ? lastSelectedPlayer : (playerOne != null ? playerOne : playerTwo);
        ShowStatInfo(target);
    }

    private void ShowStatInfo(PlayerEntity player)
    {
        if (statInfoUI == null || player == null) return;
        statInfoUI.SetEntity(player);
        statInfoUI.gameObject.SetActive(true);
    }

    private void UpdateButtonState()
    {
        if (playerOneButton != null) playerOneButton.interactable = playerOne != null;
        if (playerTwoButton != null) playerTwoButton.interactable = playerTwo != null;
        if (openStatInfoButton != null) openStatInfoButton.interactable = playerOne != null || playerTwo != null;
    }
}

using UnityEngine;
using UnityEngine.UI;

public class CombatResultUI : Singleton<CombatResultUI>
{
    [SerializeField] private Button ClaimButton;

    private void Start()
    {
        gameObject.SetActive(false);
        ClaimButton.onClick.AddListener(OnClaimButtonClick);
    }
    private void OnClaimButtonClick()
    {
        Loader.Load(Loader.Scenes.Overworld);
    }
}

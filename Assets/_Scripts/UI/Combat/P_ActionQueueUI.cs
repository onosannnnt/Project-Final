using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ActionQueuePrefab : MonoBehaviour
{
    [SerializeField] private Image Icon;
    [SerializeField] private TMP_Text NameText;

    public void Setup(Sprite icon, string name)
    {
        Icon.sprite = icon;
        NameText.text = name;
    }
}
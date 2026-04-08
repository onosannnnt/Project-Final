using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class BuffItemUI : MonoBehaviour
{
    [SerializeField] public Image buffIcon;
    [SerializeField] public TMP_Text headerText;
    [SerializeField] public TMP_Text descriptionText;
    [SerializeField] public TMP_Text buffStack;
    public void Setup(string name, string description, Sprite icon, int stackCount = 0)
    {
        headerText.text = name;
        descriptionText.text = description;
        buffIcon.sprite = icon;
        buffStack.text = $"{stackCount}";

        if (stackCount == 0)
        {
            buffStack.gameObject.SetActive(false);
        }
    }
}

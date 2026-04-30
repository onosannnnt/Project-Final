using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class BuffItemUI : MonoBehaviour
{
    [SerializeField] public Image buffIcon;
    [SerializeField] public TMP_Text headerText;
    [SerializeField] public TMP_Text descriptionText;
    [SerializeField] public TMP_Text buffStack;
    [SerializeField] public TMP_Text buffDuration;
    public void Setup(string name, string description, Sprite icon, int stackCount = 0, int duration = 0, bool isPermanent = false)
    {
        headerText.text = name;
        descriptionText.text = description;
        buffIcon.sprite = icon;
        buffStack.text = $"{stackCount}";
        buffDuration.text = $"{duration} turn(s) remaining";

        if (stackCount == 0)
        {
            buffStack.gameObject.SetActive(false);
        }
        else
        {
            buffStack.gameObject.SetActive(true);
        }

        if (duration == 0 || isPermanent)
        {
            buffDuration.gameObject.SetActive(false);
        }
        else
        {
            buffDuration.gameObject.SetActive(true);
        }
    }
}

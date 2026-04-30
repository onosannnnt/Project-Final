using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TransparentTMPDropdown : TMP_Dropdown
{
    [Header("Blocker Visual")]
    [SerializeField] private Color blockerColor = new Color(0f, 0f, 0f, 0f);

    protected override GameObject CreateBlocker(Canvas rootCanvas)
    {
        GameObject blocker = base.CreateBlocker(rootCanvas);
        if (blocker == null)
        {
            return null;
        }

        Image blockerImage = blocker.GetComponent<Image>();
        if (blockerImage != null)
        {
            blockerImage.color = blockerColor;
        }

        return blocker;
    }
}

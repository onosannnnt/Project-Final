using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class SkillHoverHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("UI References")]
    private Image bgImage;
    private Sprite originalBGSprite;
    private Image checkmarkImage;

    [Header("UI Elements")]
    public GameObject hoverBG;      
    public GameObject selectedBG;   
    public Image buttonIconImage;    
    public TextMeshProUGUI skillNameText;

    [Header("Checkmark Settings")]
    public Sprite unselectedCheckmark; // ติ๊กถูกเทา
    public Sprite selectedCheckmark;   // ติ๊กถูกม่วง

    [Header("Tooltip")]
    public GameObject infoBox;
    public TextMeshProUGUI infoText; 
    [TextArea] public string skillDescription; 
    public Vector3 offset = new Vector3(0, 0, 0); 

    [HideInInspector] public Skill skillData; 
    public bool isSelected = false; 

    void Awake()
    {
        bgImage = GetComponent<Image>();
        if (bgImage != null) originalBGSprite = bgImage.sprite;
        
        if (selectedBG != null)
            checkmarkImage = selectedBG.GetComponent<Image>();
    }

    void Start()
    {
        UpdateCheckmarkVisual();
    }

    public void SetupData(Skill data) 
    {
        skillData = data;
        if (buttonIconImage != null) buttonIconImage.sprite = data.skillIcon; 
        skillDescription = data.description;
        if (skillNameText != null) skillNameText.text = data.skillName;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (hoverBG != null) hoverBG.SetActive(true);
        if (infoBox != null)
        {
            infoText.text = skillDescription;
            infoBox.transform.position = transform.position - offset;
            infoBox.SetActive(true);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (hoverBG != null) hoverBG.SetActive(false);
        if (infoBox != null) infoBox.SetActive(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        SkillListManager manager = Object.FindFirstObjectByType<SkillListManager>();
        if (manager == null) return;

        if (isSelected) 
            manager.DeselectSkill(this);
        else 
            manager.TrySelectSkill(this, skillData);

        // หมายเหตุ: manager จะเป็นคนเรียก SetSelected เองเพื่อความแม่นยำ
    }

    public void SetSelected(bool state, bool triggerCheck = true)
    {
        isSelected = state;
        UpdateCheckmarkVisual();
        
        if (triggerCheck) 
        {
            SkillListManager manager = Object.FindFirstObjectByType<SkillListManager>();
        }
    }

    private void UpdateCheckmarkVisual()
    {
        if (checkmarkImage == null) return;
        checkmarkImage.sprite = isSelected ? selectedCheckmark : unselectedCheckmark;
        if (selectedBG != null) selectedBG.SetActive(true); 
    }

    void OnDisable()
    {
        if (hoverBG != null) hoverBG.SetActive(false);
        if (infoBox != null) infoBox.SetActive(false);
    }
}

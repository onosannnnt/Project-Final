using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class SkillHoverHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("UI Elements at Slot")]
    public GameObject hoverBG;      
    public GameObject selectedBG;   
    public Image buttonIconImage;    
    public TextMeshProUGUI skillNameText;

    [Header("Global Tooltip Settings")]
    public GameObject infoBox;
    public TextMeshProUGUI infoText; 
    [TextArea] public string skillDescription; 

    public Vector3 offset = new Vector3(0, 0, 0); 
    
    // เพิ่มตัวแปรเก็บข้อมูลจาก ScriptableObject
    [HideInInspector] public Skill skillData; 
    private bool isSelected = false; 

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
        {
            isSelected = false;
            manager.DeselectSkill(this);
        }
        else 
        {
            // ส่งข้อมูล skillData ไปให้ Manager ด้วย
            if (manager.TrySelectSkill(this, skillData)) 
            {
                isSelected = true;
            }
        }

        if (selectedBG != null) selectedBG.SetActive(isSelected);
    }

    public void SetupData(Skill data) 
    {
        skillData = data; // เก็บข้อมูลไว้ใช้ตอนถูกเลือก
        if (buttonIconImage != null) buttonIconImage.sprite = data.skillIcon; 
        skillDescription = data.description;
        if (skillNameText != null) skillNameText.text = data.skillName;
    }

    void OnDisable()
    {
        if (hoverBG != null) hoverBG.SetActive(false);
        if (infoBox != null) infoBox.SetActive(false);
    }
}
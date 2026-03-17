using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class SkillSetHandler : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    [Header("Data")]
    public SkillSet skillSetData; 

    [Header("UI Display (Set Info)")]
    public TextMeshProUGUI setNameText;      // ชื่อเซต
    public Image[] skillIcons;               // Array ของ Image 5 อันสำหรับไอคอนสกิล
    public TextMeshProUGUI[] skillNameTexts;  // Array ของ Text 5 อันสำหรับชื่อสกิล

    [Header("Visual Feedback")]
    public Image backgroundCanvas; 
    public Image checkmarkImage;  

    private SkillListManager manager;
    private bool isSelected = false;

    void Start() 
    { 
        manager = Object.FindFirstObjectByType<SkillListManager>(); 
        if (manager != null) manager.RegisterSetHandler(this);

        // บังคับ Reset สถานะทุกอย่างให้เป็น False ในเฟรมแรก
        isSelected = false; 
        if (checkmarkImage != null) checkmarkImage.enabled = false;
        
        UpdateUI();
    }

    public void UpdateUI()
    {
        if (skillSetData == null) return;

        // 1. แสดงชื่อเซต
        if (setNameText != null) setNameText.text = skillSetData.setName;

        // 2. แสดงข้อมูลสกิล 5 อย่างในเซต
        for (int i = 0; i < skillIcons.Length; i++)
        {
            if (i < skillSetData.selectedSkills.Count)
            {
                Skill currentSkill = skillSetData.selectedSkills[i];

                if (skillIcons[i] != null) 
                {
                    skillIcons[i].sprite = currentSkill.skillIcon;
                    skillIcons[i].gameObject.SetActive(true);
                }

                if (skillNameTexts != null && i < skillNameTexts.Length && skillNameTexts[i] != null)
                {
                    skillNameTexts[i].text = currentSkill.skillName;
                    skillNameTexts[i].gameObject.SetActive(true);
                }
            }
            else
            {
                // ถ้าสกิลไม่ครบ 5 ให้ปิด UI ส่วนที่เกิน
                if (skillIcons[i] != null) skillIcons[i].gameObject.SetActive(false);
                if (skillNameTexts != null && i < skillNameTexts.Length && skillNameTexts[i] != null)
                    skillNameTexts[i].gameObject.SetActive(false);
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isSelected && skillSetData != null && backgroundCanvas != null) 
            backgroundCanvas.sprite = skillSetData.setHoverBG;
    }

    // public void OnPointerExit(PointerEventData eventData)
    // {
    // }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (manager == null) return;
        
        if (!isSelected) 
        {
            // ถ้ายังไม่ได้เลือกเซตนี้ -> ให้เลือกทั้งเซต
            manager.SelectFullSet(this);
        }
        else 
        {
            // ถ้าเลือกเซตนี้อยู่แล้ว แล้วกดซ้ำ -> ให้ยกเลิกทั้งหมด
            manager.DeselectFullSet();
        }
    }

    public void SetState(bool selected)
    {
        isSelected = selected;
        
        // if (backgroundCanvas != null && skillSetData != null)
        // {
        //     backgroundCanvas.sprite = isSelected ? skillSetData.setSelectedBG : normalBG;
        // }
        
        if (checkmarkImage != null && skillSetData != null)
        {
            checkmarkImage.sprite = skillSetData.setCheckmark;
            checkmarkImage.enabled = isSelected;
        }
    }
}
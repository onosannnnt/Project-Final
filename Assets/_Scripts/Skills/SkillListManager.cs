using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class SkillListManager : MonoBehaviour
{
    public List<Skill> allSkills;      
    public GameObject skillSlotPrefab; 
    public Transform contentParent;    

    [Header("Loadout Settings")]
    public GameObject selectedSkillPrefab; // ลาก Prefab ที่จะใช้ใน 5 ช่องบนมาใส่
    public SelectedSlot[] selectedSlots;   // ช่อง UI ทั้ง 5 ช่อง

    [Header("Scene References")]
    public GameObject bubbleBoxInScene;      
    public TextMeshProUGUI bubbleTextInScene; 

    void Start()
    {
        GenerateSkillList();
    }

    void GenerateSkillList()
    {
        foreach (Transform child in contentParent) Destroy(child.gameObject);

        foreach (Skill data in allSkills)
        {
            GameObject newSlot = Instantiate(skillSlotPrefab, contentParent);
            SkillHoverHandler handler = newSlot.GetComponent<SkillHoverHandler>();
            
            if (handler != null)
            {
                handler.SetupData(data);
                handler.infoBox = bubbleBoxInScene;
                handler.infoText = bubbleTextInScene;
            }
        }
    }

    // แก้ไขให้รับทั้ง Handler และ SkillData
    public bool TrySelectSkill(SkillHoverHandler handler, Skill skillData)
    {
        foreach (var slot in selectedSlots)
        {
            if (!slot.isFull)
            {
                // ส่ง Prefab และข้อมูลไปให้ Slot จัดการสร้าง
                slot.SetSkill(handler, selectedSkillPrefab, skillData);
                return true;
            }
        }
        return false; 
    }

    public void DeselectSkill(SkillHoverHandler handler)
    {
        foreach (var slot in selectedSlots)
        {
            if (slot.currentSkillSource == handler)
            {
                slot.ClearSlot();
                break;
            }
        }
    }
}
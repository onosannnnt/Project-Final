using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using TMPro;

public class SkillListManager : MonoBehaviour
{
    public List<Skill> allSkills;

    [Header("Prefabs & Parents")]
    public GameObject skillSlotPrefab;
    public Transform contentParent;

    [Header("Loadout Settings")]
    public GameObject selectedSkillPrefab;
    public SelectedSlot[] selectedSlots; // ลาก 5 ช่องบนใน Scene มาใส่ที่นี่

    [Header("Scene References")]
    public GameObject bubbleBoxInScene;
    public TextMeshProUGUI bubbleTextInScene;

    [Header("Data Saving")]
    public SkillLoadout playerLoadout;

    void Start()
    {
        // ล้างทุกอย่างให้เกลี้ยงตั้งแต่วินาทีแรกที่เห็น
        ForceInitialCleanup();
        
        GenerateSkillList();
    }

    void ForceInitialCleanup()
    {
        foreach (var slot in selectedSlots)
        {
            if (slot != null)
            {
                // ใช้ฟังก์ชัน Hardcode ที่เราทำไว้
                slot.ClearSlot(); 
            }
        }
        // บังคับ UI ให้วาดพื้นที่ว่างๆ รอไว้เลย
        Canvas.ForceUpdateCanvases();
    }

    public void GenerateSkillList()
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

    public bool TrySelectSkill(SkillHoverHandler handler, Skill skillData)
    {
        foreach (var slot in selectedSlots)
        {
            if (slot != null && !slot.isOccupied) 
            {
                slot.SetSkill(handler, selectedSkillPrefab, skillData);
                SaveLoadout();
                return true;
            }
        }
        return false;
        
        
    }

    public void DeselectSkill(SkillHoverHandler handler)
    {
        foreach (var slot in selectedSlots)
        {
            if (slot != null && slot.currentHandler == handler)
            {
                slot.ClearSlot();
                SaveLoadout();
                break;
            }
        }
    }

    private SkillHoverHandler FindHandlerBySkill(Skill target)
    {
        foreach (Transform child in contentParent)
        {
            SkillHoverHandler h = child.GetComponent<SkillHoverHandler>();
            if (h != null && h.skillData == target) return h;
        }
        return null;
    }

    public void SaveLoadout()
    {
        if (playerLoadout == null) return;

        // ล้างข้อมูลเก่าใน ScriptableObject
        playerLoadout.EquippedSkills.Clear();

        // วนลูปเช็คช่องที่เลือกอยู่แล้วเพิ่มเข้าไปใน List
        foreach (var slot in selectedSlots)
        {
            if (slot != null && slot.isOccupied && slot.skillData != null)
            {
                playerLoadout.EquippedSkills.Add(slot.skillData);
            }
        }
        
        // บันทึกสถานะ (ใช้เฉพาะใน Editor เพื่อให้ค่าไม่หายเวลาปิดเกม)
        #if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(playerLoadout);
        #endif
    }

    public void LogSimpleLoadout()
    {
        string loadout = "<b><color=#00FFD5>[LOADOUT]</color></b>\n";

        for (int i = 0; i < selectedSlots.Length; i++)
        {
            if (selectedSlots[i] != null && selectedSlots[i].isOccupied && selectedSlots[i].skillData != null)
            {
                loadout += $"<color=#A78BFA>Slot {i + 1}</color>: <b>{selectedSlots[i].skillData.skillName}</b>\n";
            }
            else
            {
                loadout += $"<color=#A78BFA>Slot {i + 1}</color>: <color=#888888>None</color>\n";
            }
        }

        Debug.Log(loadout);
    }
}
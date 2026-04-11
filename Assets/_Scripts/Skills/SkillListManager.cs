using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using TMPro;
using UnityEngine.UI;

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
    [Tooltip("Assign loadout assets in player order (index 0 = player 1, index 1 = player 2).")]
    public SkillLoadout[] playerLoadouts;
    [SerializeField] private int activeLoadoutIndex = 0;

    [Header("Loadout Tab Colors")]
    [SerializeField] private Image player1TabImage;
    [SerializeField] private Image player2TabImage;
    [SerializeField] private Color activeTabColor = new Color(0.35f, 0.75f, 1f, 1f);
    [SerializeField] private Color inactiveTabColor = Color.white;

    void Start()
    {
        // ล้างทุกอย่างให้เกลี้ยงตั้งแต่วินาทีแรกที่เห็น
        ForceInitialCleanup();

        GenerateSkillList();
        LoadActiveLoadoutIntoSlots();
        UpdateLoadoutTabVisuals();
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
        if (GetActiveLoadout() == null) return false;

        // Check if the skill is already selected to prevent duplicates
        foreach (var slot in selectedSlots)
        {
            if (slot != null && slot.isOccupied && slot.skillData == skillData)
            {
                return false; 
            }
        }

        foreach (var slot in selectedSlots)
        {
            if (slot != null && !slot.isOccupied) 
            {
                slot.SetSkill(handler, selectedSkillPrefab, skillData);
                if (handler != null) handler.SetSelected(true);
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
                if (handler != null) handler.SetSelected(false);
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
        SkillLoadout activeLoadout = GetActiveLoadout();
        if (activeLoadout == null) return;

        // ล้างข้อมูลเก่าใน ScriptableObject
        activeLoadout.EquippedSkills.Clear();

        // วนลูปเช็คช่องที่เลือกอยู่แล้วเพิ่มเข้าไปใน List
        foreach (var slot in selectedSlots)
        {
            if (slot != null && slot.isOccupied && slot.skillData != null)
            {
                activeLoadout.EquippedSkills.Add(slot.skillData);
            }
        }
        
        // บันทึกสถานะ (ใช้เฉพาะใน Editor เพื่อให้ค่าไม่หายเวลาปิดเกม)
        #if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(activeLoadout);
        #endif
    }

    public void SetActiveLoadoutIndex(int index)
    {
        SaveLoadout();

        if (playerLoadouts == null || playerLoadouts.Length == 0)
        {
            activeLoadoutIndex = 0;
            ForceInitialCleanup();
            return;
        }

        activeLoadoutIndex = Mathf.Clamp(index, 0, playerLoadouts.Length - 1);
        LoadActiveLoadoutIntoSlots();
        UpdateLoadoutTabVisuals();
    }

    public void SelectPlayer1Loadout()
    {
        SetActiveLoadoutIndex(0);
    }

    public void SelectPlayer2Loadout()
    {
        SetActiveLoadoutIndex(1);
    }

    public int GetActiveLoadoutIndex()
    {
        return activeLoadoutIndex;
    }

    private SkillLoadout GetActiveLoadout()
    {
        if (playerLoadouts == null || playerLoadouts.Length == 0) return null;
        if (activeLoadoutIndex < 0 || activeLoadoutIndex >= playerLoadouts.Length) return null;
        return playerLoadouts[activeLoadoutIndex];
    }

    private void LoadActiveLoadoutIntoSlots()
    {
        ForceInitialCleanup();
        ResetSkillSelections();

        SkillLoadout activeLoadout = GetActiveLoadout();
        if (activeLoadout == null) return;

        int slotIndex = 0;
        foreach (var skill in activeLoadout.EquippedSkills)
        {
            if (skill == null) continue;
            if (slotIndex >= selectedSlots.Length) break;

            SelectedSlot targetSlot = selectedSlots[slotIndex];
            if (targetSlot == null)
            {
                slotIndex++;
                continue;
            }

            SkillHoverHandler handler = FindHandlerBySkill(skill);
            targetSlot.SetSkill(handler, selectedSkillPrefab, skill);
            if (handler != null)
            {
                handler.SetSelected(true);
            }

            slotIndex++;
        }
    }

    private void ResetSkillSelections()
    {
        foreach (Transform child in contentParent)
        {
            SkillHoverHandler handler = child.GetComponent<SkillHoverHandler>();
            if (handler != null)
            {
                handler.SetSelected(false, false);
            }
        }
    }

    private void UpdateLoadoutTabVisuals()
    {
        if (player1TabImage != null)
        {
            player1TabImage.color = activeLoadoutIndex == 0 ? activeTabColor : inactiveTabColor;
        }

        if (player2TabImage != null)
        {
            player2TabImage.color = activeLoadoutIndex == 1 ? activeTabColor : inactiveTabColor;
        }
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

// // Debug.Log(loadout);
    }
}

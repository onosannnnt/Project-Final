using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using TMPro;

public class SkillListManager : MonoBehaviour
{
    public List<Skill> allSkills;
    public List<SkillSet> availableSets;
    public List<SkillSetHandler> setHandlers = new List<SkillSetHandler>();
    private SkillSet currentActiveSet = null;

    [Header("Prefabs & Parents")]
    public GameObject skillSlotPrefab;
    public Transform contentParent;
    public GameObject skillSetPrefab;
    public Transform setContentParent;

    [Header("Loadout Settings")]
    public GameObject selectedSkillPrefab;
    public SelectedSlot[] selectedSlots; // ลาก 5 ช่องบนใน Scene มาใส่ที่นี่

    [Header("Scene References")]
    public GameObject bubbleBoxInScene;
    public TextMeshProUGUI bubbleTextInScene;

    void Start()
    {
        // ล้างทุกอย่างให้เกลี้ยงตั้งแต่วินาทีแรกที่เห็น
        ForceInitialCleanup();
        
        GenerateSkillList();
        GenerateSetList();
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

    public void GenerateSetList()
    {
        if (setContentParent != null)
            foreach (Transform child in setContentParent) Destroy(child.gameObject);

        foreach (SkillSet setData in availableSets)
        {
            if (setData == null) continue;
            GameObject newSetBtn = Instantiate(skillSetPrefab, setContentParent);
            SkillSetHandler handler = newSetBtn.GetComponent<SkillSetHandler>();
            if (handler != null)
            {
                handler.skillSetData = setData;
                handler.UpdateUI();
                RegisterSetHandler(handler);
            }
        }
    }

    public void SelectFullSet(SkillSetHandler handler)
    {
        // แทนที่จะรันโค้ดสดๆ ให้เรียกผ่าน Coroutine
        StartCoroutine(DoSelectFullSet(handler));
    }

    IEnumerator DoSelectFullSet(SkillSetHandler handler)
    {
        // 1. ล้างของเก่า
        DeselectFullSet();
        DeselectFullSet();
        
        // 2. รอ 1 เฟรม (เพื่อให้ Unity ลบ Object เก่าทิ้งจาก Memory จริงๆ)
        yield return new UnityEngine.WaitForEndOfFrame();
        
        // 3. ค่อยบรรจุสกิลใหม่
        foreach (Skill s in handler.skillSetData.selectedSkills)
        {
            SkillHoverHandler skillUI = FindHandlerBySkill(s);
            if (skillUI != null)
            {
                TrySelectSkill(skillUI, s);
            }
        }

        DeselectFullSet(); 
        DeselectFullSet();

        // 2. รอ 1 เฟรม (เพื่อให้ Unity ลบ Object เก่าทิ้งจาก Memory จริงๆ)
        yield return new UnityEngine.WaitForEndOfFrame();
        
        // 3. ค่อยบรรจุสกิลใหม่
        foreach (Skill s in handler.skillSetData.selectedSkills)
        {
            SkillHoverHandler skillUI = FindHandlerBySkill(s);
            if (skillUI != null)
            {
                TrySelectSkill(skillUI, s);
            }
        }
        
        ActivateSetUI(handler.skillSetData);
    }

    public void DeselectFullSet()
    {
        foreach (Transform child in contentParent)
        {
            SkillHoverHandler h = child.GetComponent<SkillHoverHandler>();
            if (h != null)
            {
                // บังคับ Reset เป็น false ทุกอย่าง
                h.SetSelected(false, false); 
                h.ApplySetVisuals(null);
                
                // เพิ่มบรรทัดนี้เพื่อความชัวร์ (ถ้า isSelected เป็น private ให้เปลี่ยนเป็น public)
                h.isSelected = false; 
            }
        }
        
        // 1. คืนค่าปุ่มสกิลทั้งหมดใน List
        foreach (Transform child in contentParent)
        {
            if (child == null) continue;
            SkillHoverHandler h = child.GetComponent<SkillHoverHandler>();
            if (h != null)
            {
                h.SetSelected(false, false);
                h.ApplySetVisuals(null);
            }
        }

        // 2. ล้าง 5 ช่องบน
        foreach (var slot in selectedSlots)
        {
            if (slot != null) slot.ClearSlot();
        }

        DeactivateSetUI();
        currentActiveSet = null;

        // 3. บังคับให้ UI คำนวณตำแหน่งและภาพใหม่ทันที
        Canvas.ForceUpdateCanvases();
    }

    public bool TrySelectSkill(SkillHoverHandler handler, Skill skillData)
    {
        foreach (var slot in selectedSlots)
        {
            // เปลี่ยนจาก isFull เป็น isOccupied ตามชื่อใหม่ใน SelectedSlot
            if (slot != null && !slot.isOccupied) 
            {
                slot.SetSkill(handler, selectedSkillPrefab, skillData);
                handler.SetSelected(true); // สั่งติ๊กถูกม่วงที่ปุ่มเล็ก
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
                handler.SetSelected(false);
                break;
            }
        }
    }

    public void CheckForSetMatch()
    {
        List<Skill> currentSkills = new List<Skill>();
        foreach (var slot in selectedSlots)
            if (slot != null && slot.isOccupied) currentSkills.Add(slot.skillData);

        foreach (var set in availableSets)
        {
            if (IsSetMatch(currentSkills, set.selectedSkills))
            {
                ActivateSetUI(set);
                return;
            }
        }
        DeactivateSetUI();
    }

    private bool IsSetMatch(List<Skill> current, List<Skill> setSkills)
    {
        if (current.Count != setSkills.Count) return false;
        foreach (var s in setSkills) if (!current.Contains(s)) return false;
        return true;
    }

    public void RegisterSetHandler(SkillSetHandler h) { if (!setHandlers.Contains(h)) setHandlers.Add(h); }
    private void ActivateSetUI(SkillSet set) { currentActiveSet = set; foreach (var h in setHandlers) if (h != null) h.SetState(h.skillSetData == set); }
    private void DeactivateSetUI() { currentActiveSet = null; foreach (var h in setHandlers) if (h != null) h.SetState(false); }

    private SkillHoverHandler FindHandlerBySkill(Skill target)
    {
        foreach (Transform child in contentParent)
        {
            SkillHoverHandler h = child.GetComponent<SkillHoverHandler>();
            if (h != null && h.skillData == target) return h;
        }
        return null;
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
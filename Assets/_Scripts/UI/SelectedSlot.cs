using UnityEngine;

public class SelectedSlot : MonoBehaviour
{
    public bool isOccupied = false; 
    public SkillHoverHandler currentHandler; 
    public Skill skillData; 
    private GameObject spawnedDisplay; 

    public void SetSkill(SkillHoverHandler source, GameObject prefab, Skill data)
    {
        // 1. กวาดล้างของเก่าก่อนสร้างเสมอ
        ClearSlot(); 

        currentHandler = source;
        skillData = data;
        spawnedDisplay = Instantiate(prefab, transform); 
        
        SkillListManager manager = Object.FindFirstObjectByType<SkillListManager>();
        var displayScript = spawnedDisplay.GetComponent<SelectedSkillDisplay>();
        if (displayScript != null)
        {
            displayScript.Setup(data, manager);
        }
        
        isOccupied = true;
    }

    public void ClearSlot()
    {
        currentHandler = null;
        skillData = null;
        isOccupied = false;

        ForceEmpty();
    }

    public void ForceEmpty()
    {
        // ลบแบบ Hardcode จนกว่าจะไม่มีลูกเหลืออยู่
        int safetyBreak = 0;
        while (transform.childCount > 0 && safetyBreak < 10)
        {
            Transform child = transform.GetChild(0);
            child.SetParent(null); // ตัดขาดทันทีเพื่อให้ภาพหายจาก Canvas
            DestroyImmediate(child.gameObject); // สั่งฆ่าทันที
            safetyBreak++;
        }
        
        spawnedDisplay = null;
    }
}
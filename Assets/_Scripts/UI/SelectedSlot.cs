using UnityEngine;
using UnityEngine.UI;

public class SelectedSlot : MonoBehaviour
{
    public bool isFull = false;
    public SkillHoverHandler currentSkillSource; // อ้างอิงกลับไปยังปุ่มในคลัง
    private GameObject spawnedDisplay; // เก็บตัว Prefab ที่ถูกสร้างขึ้น

    // ฟังก์ชันสร้าง Prefab และใส่ข้อมูลจาก ScriptableObject
    public void SetSkill(SkillHoverHandler source, GameObject prefab, Skill skillData)
{
    currentSkillSource = source;
    spawnedDisplay = Instantiate(prefab, transform); 
    
    // ดึง Manager มาจากตัวคุม
    SkillListManager manager = Object.FindFirstObjectByType<SkillListManager>();

    var displayScript = spawnedDisplay.GetComponent<SelectedSkillDisplay>();
    if (displayScript != null)
    {
        // ส่งทั้งข้อมูลสกิล และ Manager เข้าไป
        displayScript.Setup(skillData, manager);
    }
    
    isFull = true;
}

    public void ClearSlot()
    {
        currentSkillSource = null;
        if (spawnedDisplay != null) Destroy(spawnedDisplay);
        isFull = false;
    }

    
}


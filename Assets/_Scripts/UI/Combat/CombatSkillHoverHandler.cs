using UnityEngine;
using UnityEngine.EventSystems;

public class CombatSkillHoverHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Skill Data")]
    public Skill skillData; 

    [Header("Settings")]
    public float displayDelay = 0.3f; // หน่วงเวลาก่อนแสดง (วินาที)

    private float hoverTimer = 0f;
    private bool isHovering = false;
    private bool isShowing = false; // ป้องกันการสั่ง Show ซ้ำๆ ทุกเฟรม

    public void SetSkillData(Skill skill)
    {
        skillData = skill;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (skillData == null) return;
        
        isHovering = true;
        hoverTimer = 0f;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
        hoverTimer = 0f;
        
        if (isShowing)
        {
            CombatTooltipManager.instance?.HideTooltip();
            isShowing = false;
        }
    }

    private void Update()
    {
        // ถ้านับเวลาถึงกำหนด และยังไม่ได้เปิดกล่อง ให้สั่งเปิด
        if (isHovering && !isShowing && skillData != null)
        {
            hoverTimer += Time.deltaTime;
            
            if (hoverTimer >= displayDelay)
            {
                // เรียกใช้ Manager ให้แสดง Tooltip ตรงตำแหน่งของปุ่มนี้ (transform.position)
                CombatTooltipManager.instance?.ShowTooltip(skillData, transform.position);
                isShowing = true;
            }
        }
    }

    private void OnDisable()
    {
        // ป้องกันบักตอนปุ่มถูกทำลายหรือปิดไปขณะที่เมาส์ยังชี้อยู่
        isHovering = false;
        if (isShowing)
        {
            CombatTooltipManager.instance?.HideTooltip();
            isShowing = false;
        }
    }
}

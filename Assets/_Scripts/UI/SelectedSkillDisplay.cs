using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class SelectedSkillDisplay : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public Image iconImage;
    public TextMeshProUGUI skillNameText;
    private Skill skillData; // ตัวเก็บข้อมูลสกิล
    private SkillListManager manager; // ตัวเก็บการอ้างอิง Manager
    public Vector3 offset = new Vector3(0, 0, 0); 

    public void Setup(Skill data, SkillListManager mgr)
    {
        if (data == null) return;
        
        skillData = data;
        manager = mgr;
        
        // แสดงผลรูปไอคอน
        if (iconImage != null) iconImage.sprite = data.skillIcon;
        else {
            var img = GetComponent<Image>();
            if(img != null) img.sprite = data.skillIcon;
        }

        // แสดงผลชื่อสกิล
        if (skillNameText != null) skillNameText.text = data.skillName;
        else {
            var namee = GetComponent<TextMeshProUGUI>();
            if(namee != null) namee.text = data.skillName;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // ตอนนี้ skillData ไม่เป็น Null แล้ว ข้อมูลจะแสดงผลได้
        if (manager != null && manager.bubbleBoxInScene != null && skillData != null)
        {
            manager.bubbleTextInScene.text = skillData.description;
            manager.bubbleBoxInScene.transform.position = transform.position - offset;
            manager.bubbleBoxInScene.SetActive(true);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (manager != null && manager.bubbleBoxInScene != null)
            manager.bubbleBoxInScene.SetActive(false);
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        if (manager != null)
        {
            SelectedSlot parentSlot = GetComponentInParent<SelectedSlot>();
            if (parentSlot != null && parentSlot.currentHandler != null)
            {
                // แนะนำให้เรียกผ่าน Manager โดยตรงเพื่อความชัวร์ในการทำลาย Object
                manager.DeselectSkill(parentSlot.currentHandler);
                
                if (manager.bubbleBoxInScene != null)
                    manager.bubbleBoxInScene.SetActive(false);
            }
        }
    }
}

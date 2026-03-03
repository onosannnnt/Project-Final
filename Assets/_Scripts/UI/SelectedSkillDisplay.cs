// using UnityEngine;
// using UnityEngine.UI;
// using TMPro;

// public class SelectedSkillDisplay : MonoBehaviour
// {
//     public Image iconImage;
//     public TextMeshProUGUI nameText;

//     public void Setup(Skill data)
//     {
//         if (iconImage != null) iconImage.sprite = data.skillIcon;
//         if (nameText != null) nameText.text = data.skillName;
//     }
// }

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class SelectedSkillDisplay : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public Image iconImage;
    public TextMeshProUGUI skillNameText;
    private Skill skillData;
    private SkillListManager manager;

    public Vector3 offset = new Vector3(0, 0, 0); 

    // ฟังก์ชันรับข้อมูลมาโชว์
    public void Setup(Skill data, SkillListManager mgr)
    {
        skillData = data;
        manager = mgr;
        if (iconImage != null) iconImage.sprite = data.skillIcon;
        if (skillNameText != null) skillNameText.text = data.skillName;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // ส่งข้อมูลไปโชว์ที่ BubbleBox เดียวกัน
        if (manager != null && manager.bubbleBoxInScene != null)
        {
            manager.bubbleTextInScene.text = skillData.description;
            // ตั้งตำแหน่ง Tooltip (ปรับ offset ตามเหมาะสม)
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
        // 2. ทำให้กด Deselect ผ่านตรงนี้ได้
        if (manager != null)
        {
            // หาช่อง SelectedSlot ที่ตัวมันเองอาศัยอยู่
            SelectedSlot parentSlot = GetComponentInParent<SelectedSlot>();
            if (parentSlot != null && parentSlot.currentSkillSource != null)
            {
                // สั่ง Deselect ผ่าน Handler ต้นทาง (เพื่อให้ติ๊กถูกที่ MySkill หายไปด้วย)
                parentSlot.currentSkillSource.OnPointerClick(null); 
                // ปิด Tooltip ทันทีที่กดลบ
                OnPointerExit(null);
            }
        }
    }
}
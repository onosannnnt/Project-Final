// using UnityEngine;
// using System.Collections.Generic;

// public class ShopTabController : MonoBehaviour
// {
//     // สร้าง Enum สำหรับ Tab ต่างๆ (รวม All เข้าไปด้วย)
//     public enum TabType
//     {
//         All,
//         Attack,
//         Buff,
//         Debuff,
//         Sustain
//     }

//     // สร้าง Class สำหรับจัดกลุ่ม UI 3 ชิ้นเข้าด้วยกัน
//     [System.Serializable]
//     public class CategoryGroup
//     {
//         public TabType tabType;          // เลือกประเภท
//         public GameObject titleObj;      // ใส่ SkillType(type)
//         public GameObject listObj;       // ใส่ (type)List
//         public GameObject dividerObj;    // ใส่ Divider
//     }

//     [Header("Category Settings")]
//     // List สำหรับเก็บกลุ่ม UI ทั้งหมด
//     public List<CategoryGroup> categoryGroups;

//     [Header("Shop Generation")]
//     public GameObject sellBoxPrefab;       // ลาก Prefab "SellBox" ที่มี SkillShopItem ติดอยู่มาใส่
//     public List<Skill> shopSkillDatabase;  // ลาก ScriptableObject Skill ทั้งหมดที่ต้องการขายมาใส่

//     private void Start()
//     {
//         GenerateShop();

//         // เริ่มต้นมาให้โชว์หมวด All ก่อน (สามารถเปลี่ยนได้)
//         SelectTab("All");
//     }

//     public void GenerateShop()
//     {
//         // ล้างข้อมูลเก่าในทุกลิสต์ก่อน (AttackList, BuffList, etc.)
//         foreach (var group in categoryGroups)
//         {
//             if (group.listObj != null)
//             {
//                 foreach (Transform child in group.listObj.transform)
//                     Destroy(child.gameObject);
//             }
//         }

//         // วนลูปสร้างปุ่มสกิลตามฐานข้อมูล
//         foreach (Skill skill in shopSkillDatabase)
//         {
//             // ค้นหาว่าสกิลนี้ควรไปอยู่ที่ Group ไหน (เช็คตาม SkillType)
//             CategoryGroup targetGroup = categoryGroups.Find(g => g.tabType.ToString() == skill.skillType.ToString());

//             if (targetGroup != null && targetGroup.listObj != null)
//             {
//                 // สร้าง SellBox Prefab ลงในลิสต์ที่ถูกต้อง
//                 GameObject newBox = Instantiate(sellBoxPrefab, targetGroup.listObj.transform);

//                 // ส่งข้อมูลสกิลเข้าไปที่สคริปต์ SkillShopItem เพื่อตั้งค่า UI และราคา
//                 SkillShopItem itemScript = newBox.GetComponent<SkillShopItem>();
//                 if (itemScript != null)
//                 {
//                     itemScript.Setup(skill);
//                 }
//             }
//         }
//     }

//     // ฟังก์ชันนี้ไว้ใช้ผูกกับปุ่ม (Button OnClick)
//     // โดยรับค่าเป็น String ชื่อ Tab เช่น "Attack", "Buff", "All"
//     public void SelectTab(string tabName)
//     {
//         // แปลง String เป็น Enum TabType
//         if (System.Enum.TryParse(tabName, true, out TabType selectedTab))
//         {
//             UpdateUI(selectedTab);
//         }
//         else
//         {
//             Debug.LogWarning("พิมพ์ชื่อ Tab ไม่ถูกต้อง หรือไม่มีใน Enum: " + tabName);
//         }
//     }

//     // ระบบประมวลผลการเปิด/ปิด UI
//     private void UpdateUI(TabType selectedTab)
//     {
//         // เช็คว่ากดปุ่ม All หรือไม่
//         bool isAll = (selectedTab == TabType.All);

//         // วนลูปเช็คทุกกลุ่ม
//         foreach (var group in categoryGroups)
//         {
//             // เงื่อนไข: ให้ Active ก็ต่อเมื่อ กดปุ่ม All หรือ TabType ตรงกับที่เลือก
//             bool shouldBeActive = isAll || (group.tabType == selectedTab);

//             // สั่ง เปิด/ปิด
//             if (group.titleObj != null) group.titleObj.SetActive(shouldBeActive);
//             if (group.listObj != null) group.listObj.SetActive(shouldBeActive);
//             if (group.dividerObj != null) group.dividerObj.SetActive(shouldBeActive);
//         }
//     }
// }


using UnityEngine;
using System.Collections.Generic;
using System.Linq; // เพิ่มเพื่อใช้ค้นหา

public class ShopTabController : MonoBehaviour
{
    public enum TabType { All, Attack, Buff, Debuff, Sustain }

    [System.Serializable]
    public class CategoryGroup
    {
        public TabType tabType;
        public GameObject titleObj;
        public GameObject listObj;
        public GameObject dividerObj;
    }

    [Header("References")]
    [SerializeField] private SkillListManager skillListManager; // ลาก SkillListManager มาใส่
    [SerializeField] private GameObject sellBoxPrefab;

    [Header("Category Settings")]
    public List<CategoryGroup> categoryGroups;

    // เก็บรายการไอเทมทั้งหมดที่ถูกสร้างขึ้นในร้านค้า
    private List<SkillShopItem> spawnedItems = new List<SkillShopItem>();

    private void OnEnable()
    {
        GameInput.SetInputLock(true);
    }

    private void OnDisable()
    {
        GameInput.SetInputLock(false);
    }

    private void Start()
    {
        GenerateShop();
        SelectTab("All");
    }

    public void GenerateShop()
    {
        // ล้างข้อมูลเก่า
        spawnedItems.Clear();
        foreach (var group in categoryGroups)
        {
            if (group.listObj != null)
            {
                foreach (Transform child in group.listObj.transform) Destroy(child.gameObject);
            }
        }

        // ดึงข้อมูลจาก SkillListManager.allSkills
        if (skillListManager == null || skillListManager.allSkills == null) return;

        foreach (Skill skill in skillListManager.allSkills)
        {
            // หา Group ที่ตรงกับ SkillType ของสกิล
            var targetGroup = categoryGroups.FirstOrDefault(g => g.tabType.ToString() == skill.skillType.ToString());

            if (targetGroup != null && targetGroup.listObj != null)
            {
                GameObject newBox = Instantiate(sellBoxPrefab, targetGroup.listObj.transform);
                SkillShopItem itemScript = newBox.GetComponent<SkillShopItem>();

                if (itemScript != null)
                {
                    // ส่งข้อมูลสกิล และส่ง "ตัวเอง" (Controller) เข้าไปด้วยเพื่อให้ไอเทมสั่ง Refresh ได้
                    itemScript.Setup(skill, this);
                    spawnedItems.Add(itemScript);
                }
            }
        }
    }

    // ฟังก์ชันสำหรับสั่ง Refresh ทุกลูกในร้านค้า (เรียกใช้เมื่อเงินเปลี่ยนหรือซื้อสำเร็จ)
    public void RefreshAllItems()
    {
        foreach (var item in spawnedItems)
        {
            if (item != null) item.RefreshUI();
        }
    }

    public void SelectTab(string tabName)
    {
        if (System.Enum.TryParse(tabName, true, out TabType selectedTab))
        {
            UpdateTabVisibility(selectedTab);
        }
    }

    private void UpdateTabVisibility(TabType selectedTab)
    {
        bool isAll = (selectedTab == TabType.All);
        foreach (var group in categoryGroups)
        {
            bool shouldBeActive = isAll || (group.tabType == selectedTab);
            if (group.titleObj != null) group.titleObj.SetActive(shouldBeActive);
            if (group.listObj != null) group.listObj.SetActive(shouldBeActive);
            if (group.dividerObj != null) group.dividerObj.SetActive(shouldBeActive);
        }
    }
}
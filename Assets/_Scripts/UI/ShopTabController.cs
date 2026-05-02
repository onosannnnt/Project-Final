using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq; // เพิ่มเพื่อใช้ค้นหา
using TMPro;

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

    [System.Serializable]
    public class TabButton
    {
        public TabType tabType;
        public Image buttonImage;
        public TMP_Text buttonText;
    }

    [Header("References")]
    [SerializeField] private SkillListManager skillListManager; // ลาก SkillListManager มาใส่
    [SerializeField] private GameObject sellBoxPrefab;

    [Header("Category Settings")]
    public List<CategoryGroup> categoryGroups;

    [Header("Tab Visual Settings")]
    public List<TabButton> tabButtons;
    public Color activeImageColor = Color.white;
    public Color inactiveImageColor = Color.gray;
    public Color activeTextColor = Color.black;
    public Color inactiveTextColor = Color.white;

    [Header("UserData")]
    [SerializeField] private UserData userData;

    // ระบบ Refund ราย Session
    private List<ShopTransaction> sessionTransactions = new List<ShopTransaction>();

    // เก็บรายการไอเทมทั้งหมดที่ถูกสร้างขึ้นในร้านค้า
    private List<SkillShopItem> spawnedItems = new List<SkillShopItem>();

    private void OnEnable()
    {
        GameInput.SetInputLock(true);
        // เมื่อเปิดร้านค้า ให้เลือก Tab "All" เป็นค่าเริ่มต้นเสมอ
        SelectTab("All");
    }

    private void OnDisable()
    {
        GameInput.SetInputLock(false);
        // เมื่อปิดร้านค้า ให้เคลียร์รายการ Refund (Finalize transactions)
        FinalizeTransactions();
    }

    private void FinalizeTransactions()
    {
        sessionTransactions.Clear();
        RefreshAllItems();
    }

    public void RecordPurchase(Skill skill, int price, SkillShopItem uiItem)
    {
        sessionTransactions.Add(new ShopTransaction(skill, price, uiItem));
    }

    public bool IsSkillRefundable(Skill skill)
    {
        return sessionTransactions.Any(t => t.Skill == skill);
    }

    public void RefundSkill(Skill skill)
    {
        var transaction = sessionTransactions.FirstOrDefault(t => t.Skill == skill);
        if (transaction != null)
        {
            // 1. คืนเงิน
            if (userData != null)
            {
                userData.AddCoins(transaction.Price);
                // 2. ลบสกิลออกจาก Inventory
                userData.RemoveSkill(skill);
            }

            // 3. ลบ Transaction ออกจากลิสต์
            sessionTransactions.Remove(transaction);

            // 4. รีเฟรช UI
            RefreshAllItems();
            NotifyExternalManagers();
        }
    }

    private void NotifyExternalManagers()
    {
        SkillListManager[] allManagers = Resources.FindObjectsOfTypeAll<SkillListManager>();
        foreach (var manager in allManagers)
        {
            if (manager.gameObject.scene.name != null)
            {
                manager.RefreshAllData();
            }
        }

        ElementCustomizer[] allCustomizers = Resources.FindObjectsOfTypeAll<ElementCustomizer>();
        foreach (var customizer in allCustomizers)
        {
            if (customizer.gameObject.scene.name != null)
            {
                customizer.RefreshOptions();
            }
        }
    }

    private void Start()
    {
        GenerateShop();
        SelectTab("All");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            gameObject.SetActive(false);
        }
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
        Debug.Log($"[ShopTabController] SelectTab called with: {tabName}");
        if (System.Enum.TryParse(tabName, true, out TabType selectedTab))
        {
            UpdateTabVisibility(selectedTab);
        }
        else
        {
            Debug.LogError($"[ShopTabController] Invalid Tab Name: {tabName}. Make sure it matches the Enum names exactly.");
        }
    }

    private void UpdateTabVisibility(TabType selectedTab)
    {
        bool isAll = (selectedTab == TabType.All);
        
        // 1. จัดการการแสดงผลของรายการสกิลในแต่ละหมวดหมู่
        foreach (var group in categoryGroups)
        {
            bool shouldBeActive = isAll || (group.tabType == selectedTab);
            if (group.titleObj != null) group.titleObj.SetActive(shouldBeActive);
            if (group.listObj != null) group.listObj.SetActive(shouldBeActive);
            if (group.dividerObj != null) group.dividerObj.SetActive(shouldBeActive);
        }

        // 2. จัดการสีของปุ่ม Tab (Active / Inactive)
        foreach (var button in tabButtons)
        {
            bool isActive = (button.tabType == selectedTab);
            
            if (button.buttonImage != null)
            {
                button.buttonImage.color = isActive ? activeImageColor : inactiveImageColor;
            }

            if (button.buttonText != null)
            {
                button.buttonText.color = isActive ? activeTextColor : inactiveTextColor;
            }
        }
    }
}
// using UnityEngine;
// using UnityEngine.UI;
// using TMPro;

// public class SkillShopItem : MonoBehaviour
// {
//     [Header("Data References")]
//     [SerializeField] private UserData userData; // ลาก UserData (ScriptableObject) มาใส่
//     private Skill skillData;

//     [Header("UI References")]
//     [SerializeField] private TMP_Text skillNameText;
//     [SerializeField] private TMP_Text priceText;
//     [SerializeField] private Button buyButton;
//     [SerializeField] private GameObject ownedLayer;
//     [SerializeField] private GameObject lockLayer;

//     // ฟังก์ชันนี้ถูกเรียกจาก Shop Manager ตอนสร้างรายการสกิล
//     public void Setup(Skill skill)
//     {
//         skillData = skill;

//         // อัปเดตข้อมูลพื้นฐาน (ชื่อและราคา)
//         if (skillNameText != null) skillNameText.text = skill.skillName;
//         if (priceText != null) priceText.text = skill.GetSkillPrice().ToString();

//         // ตั้งค่าปุ่มกด
//         buyButton.onClick.RemoveAllListeners();
//         buyButton.onClick.AddListener(OnBuyButtonClicked);

//         // รีเฟรชสถานะ UI ทันที
//         RefreshUI();
//     }

//     // ฟังก์ชันสำหรับตรวจสอบและอัปเดตหน้าตา UI
//     public void RefreshUI()
//     {
//         if (skillData == null || userData == null) return;

//         bool isOwned = userData.HasSkill(skillData);
//         bool isLockedByPhase = userData.GamePhase < skillData.Tier; // ถ้า Phase น้อยกว่า Tier แปลว่าล็อค

//         if (isOwned)
//         {
//             // ซื้อแล้ว: ปิดปุ่มซื้อ, เปิด Owned, ปิด Lock
//             buyButton.gameObject.SetActive(false);
//             ownedLayer.SetActive(true);
//             lockLayer.SetActive(false);
//         }
//         else if (isLockedByPhase)
//         {
//             // ยังไม่ถึง Phase: เปิดปุ่มซื้อไว้ (แต่กดไม่ได้), ปิด Owned, เปิด Lock
//             buyButton.gameObject.SetActive(true);
//             buyButton.interactable = false;
//             ownedLayer.SetActive(false);
//             lockLayer.SetActive(true);
//         }
//         else
//         {
//             // ปลดล็อคแล้ว และยังไม่ได้ซื้อ
//             buyButton.gameObject.SetActive(true);

//             // ตรวจสอบว่าเงินพอไหม ถ้าเงินไม่พอให้ปุ่มกดไม่ได้
//             bool canAfford = userData.TotalCoins >= skillData.GetSkillPrice();
//             buyButton.interactable = canAfford;

//             ownedLayer.SetActive(false);
//             lockLayer.SetActive(false);
//         }
//     }

//     // เมื่อกดปุ่มซื้อ
//     private void OnBuyButtonClicked()
//     {
//         int price = skillData.GetSkillPrice();

//         // ลองหักเงินดู ถ้าหักสำเร็จ (เงินพอ) ค่อยปลดล็อคสกิล
//         if (userData.TrySpendCoins(price))
//         {
//             userData.UnlockSkill(skillData);

//             // อัปเดต UI ชิ้นนี้ให้เป็นสถานะ "Owned" ทันที
//             RefreshUI();

//             // หมายเหตุ: หากต้องการให้ปุ่มสกิลอื่นๆ อัปเดตตาม (เพราะเงินลดลง อาจจะซื้อสกิลอื่นไม่พอแล้ว)
//             // คุณอาจจะต้องมี Event หรือบอกให้ Shop Manager เรียก RefreshUI() ของทุกกล่องที่นี่
//         }
//         else
//         {
//             Debug.Log("Not enough coins!");
//         }
//     }
// }



using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillShopItem : MonoBehaviour
{
    [SerializeField] private UserData userData;
    private Skill skillData;
    private ShopTabController shopController; // เก็บตัวอ้างอิงร้านค้าหลัก

    [SerializeField] private TMP_Text skillNameText;
    [SerializeField] private TMP_Text skillDescriptionText;
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private Button buyButton;
    [SerializeField] private GameObject ownedLayer;
    [SerializeField] private GameObject lockLayer;

    // เพิ่ม Parameter shopTabController เข้ามา
    public void Setup(Skill skill, ShopTabController controller)
    {
        skillData = skill;
        shopController = controller;

        if (skillNameText != null) skillNameText.text = skill.skillName;
        if (skillDescriptionText != null) skillDescriptionText.text = skill.description;
        if (priceText != null) priceText.text = skill.GetSkillPrice().ToString();

        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(OnBuyButtonClicked);

        RefreshUI();
    }

    public void RefreshUI()
    {
        if (skillData == null || userData == null) return;

        bool isOwned = userData.HasSkill(skillData);
        bool isLockedByPhase = userData.GamePhase < skillData.Tier;

        if (isOwned)
        {
            buyButton.gameObject.SetActive(false);
            ownedLayer.SetActive(true);
            lockLayer.SetActive(false);
        }
        else
        {
            buyButton.gameObject.SetActive(true);
            ownedLayer.SetActive(false);
            lockLayer.SetActive(isLockedByPhase);

            // ถ้าล็อคด้วย Phase หรือเงินไม่พอ ปุ่มจะกดไม่ได้
            bool canAfford = userData.TotalCoins >= skillData.GetSkillPrice();
            buyButton.interactable = !isLockedByPhase && canAfford;
        }
    }

    private void OnBuyButtonClicked()
    {
        if (userData.TrySpendCoins(skillData.GetSkillPrice()))
        {
            userData.UnlockSkill(skillData);

            // สั่งให้ร้านค้าทั้งหมดรีเฟรช UI (เพราะเงินเราลดลง สกิลอื่นอาจจะซื้อไม่ได้แล้ว)
            if (shopController != null)
            {
                shopController.RefreshAllItems();
            }
        }
    }
}
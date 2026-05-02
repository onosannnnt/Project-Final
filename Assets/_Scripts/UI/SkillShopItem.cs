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
    [SerializeField] private Image styleImage;
    [SerializeField] private Button buyButton;
    [SerializeField] private Button refundButton;
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

        UpdateStyleColor();

        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(OnBuyButtonClicked);

        if (refundButton != null)
        {
            refundButton.onClick.RemoveAllListeners();
            refundButton.onClick.AddListener(OnRefundButtonClicked);
        }

        RefreshUI();
    }

    private void UpdateStyleColor()
    {
        if (styleImage == null || skillData == null) return;

        Color targetColor = Color.white;
        switch (skillData.skillStyle)
        {
            case SkillStyle.RV:
                ColorUtility.TryParseHtmlString("#D32F2F", out targetColor);
                break;
            case SkillStyle.EL:
                ColorUtility.TryParseHtmlString("#2E7D32", out targetColor);
                break;
            case SkillStyle.CE:
                ColorUtility.TryParseHtmlString("#1976D2", out targetColor);
                break;
            default:
                targetColor = Color.white;
                break;
        }
        styleImage.color = targetColor;
    }

    public void RefreshUI()
    {
        if (skillData == null || userData == null) return;

        bool isOwned = userData.HasSkill(skillData);
        bool isLockedByPhase = userData.GamePhase < skillData.Tier;
        bool isRefundable = shopController != null && shopController.IsSkillRefundable(skillData);

        if (isOwned)
        {
            buyButton.gameObject.SetActive(false);
            ownedLayer.SetActive(true);
            lockLayer.SetActive(false);
            if (refundButton != null) refundButton.gameObject.SetActive(isRefundable);
        }
        else
        {
            buyButton.gameObject.SetActive(true);
            ownedLayer.SetActive(false);
            lockLayer.SetActive(isLockedByPhase);
            if (refundButton != null) refundButton.gameObject.SetActive(false);

            // ถ้าล็อคด้วย Phase หรือเงินไม่พอ ปุ่มจะกดไม่ได้
            bool canAfford = userData.TotalCoins >= skillData.GetSkillPrice();
            buyButton.interactable = !isLockedByPhase && canAfford;
        }
    }

    private void OnBuyButtonClicked()
    {
        int price = skillData.GetSkillPrice();
        if (userData.TrySpendCoins(price))
        {
            userData.UnlockSkill(skillData);

            // บันทึก Transaction สำหรับ Refund
            if (shopController != null)
            {
                shopController.RecordPurchase(skillData, price, this);
                shopController.RefreshAllItems();
            }

            NotifyExternalManagers();
        }
    }

    private void OnRefundButtonClicked()
    {
        if (shopController != null)
        {
            shopController.RefundSkill(skillData);
        }
    }

    private void NotifyExternalManagers()
    {
        // ==========================================
        // สั่งให้หน้าสกิลรีเฟรชข้อมูล (หาแบบรวมที่ปิดอยู่ด้วย)
        // ==========================================
        SkillListManager[] allManagers = Resources.FindObjectsOfTypeAll<SkillListManager>();
        foreach (var manager in allManagers)
        {
            // ตรวจสอบว่าเป็น Object ใน Scene จริงๆ ไม่ใช่ Prefab ใน Project
            if (manager.gameObject.scene.name != null)
            {
                manager.RefreshAllData();
            }
        }

        // สั่งให้ ElementShop (ElementCustomizer) รีเฟรช Dropdown
        ElementCustomizer[] allCustomizers = Resources.FindObjectsOfTypeAll<ElementCustomizer>();
        foreach (var customizer in allCustomizers)
        {
            if (customizer.gameObject.scene.name != null)
            {
                customizer.RefreshOptions();
            }
        }
    }
    }
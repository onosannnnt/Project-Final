using UnityEngine;

public class InventoryTabController : MonoBehaviour
{
    [Header("Content Panels")]
    public GameObject itemList;  // ลาก ItemList มาใส่
    public GameObject skillList; // ลาก SkillList มาใส่
    public GameObject equipList; // ลาก EquipList มาใส่

    void Start()
    {
        // เริ่มเกมมาให้เปิดแค่หน้าแรก (เช่น ItemList)
        ShowItems();
    }

    public void ShowItems()
    {
        itemList.SetActive(true);
        skillList.SetActive(false);
        equipList.SetActive(false);
    }

    public void ShowSkills()
    {
        itemList.SetActive(false);
        skillList.SetActive(true);
        equipList.SetActive(false);
    }

    public void ShowEquip()
    {
        itemList.SetActive(false);
        skillList.SetActive(false);
        equipList.SetActive(true);
    }
}
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DropManager : Singleton<DropManager>
{

    //Drop UI
    [SerializeField] private GameObject DropUI;
    //button
    [SerializeField] private Button BackButton;
    // ItemFrame
    [SerializeField] private Transform ItemListParent;
    [SerializeField] private GameObject ItemFrame;
    private Sprite[] allIcons;
    private void Start()
    {
        allIcons = Resources.LoadAll<Sprite>("GUI_Parts/Icons");
        BackButton.onClick.AddListener(() =>
        {
            DropUI.SetActive(false);
            Loader.Load(Loader.Scenes.Overworld);
        });
    }

    public void OpenDropUI()
    {
        GameObject ItemInspect = DropUI.transform.Find("Canvas").Find("DropInspect").gameObject;

        DropUI.SetActive(true);
        ItemInspect.SetActive(false);
        Item droppedItem = RandomItem();

        GameObject itemFrame = Instantiate(ItemFrame, ItemListParent);
        itemFrame.transform.localPosition = new Vector3(-40, 30, 0);
        itemFrame.GetComponent<Button>().onClick.AddListener(() =>
        {
            ItemInspect.SetActive(true);
            ItemInspect.transform.Find("Name").GetComponent<TextMeshProUGUI>().text = droppedItem.name;
            ItemInspect.transform.Find("Description").GetComponent<TextMeshProUGUI>().text = droppedItem.description;
            ItemInspect.transform.Find("Rarity").GetComponent<TextMeshProUGUI>().text = droppedItem.rarity.ToString();
            Image inspectIcon = ItemInspect.transform.Find("ItemFrame").Find("ItemImage").GetComponent<Image>();
            inspectIcon.sprite = droppedItem.icon;
        });

        GameObject iconObject = itemFrame.transform.Find("ItemIcon").gameObject;
        Image iconImage = iconObject.GetComponent<Image>();
        iconImage.sprite = droppedItem.icon;
    }

    private Item RandomItem()
    {
        Item item = ScriptableObject.CreateInstance<Item>();
        item.name = "Sword of Testing";
        item.description = "A powerful sword used for testing purposes.";
        item.rarity = Rarity.Rare;
        item.icon = allIcons[1];
        return item;
    }
}
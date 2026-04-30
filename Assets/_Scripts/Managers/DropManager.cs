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

        GameObject itemFrame = Instantiate(ItemFrame, ItemListParent);
        itemFrame.transform.localPosition = new Vector3(-40, 30, 0);
        itemFrame.GetComponent<Button>().onClick.AddListener(() =>
        {
            ItemInspect.SetActive(true);
            Image inspectIcon = ItemInspect.transform.Find("ItemFrame").Find("ItemImage").GetComponent<Image>();
        });

        GameObject iconObject = itemFrame.transform.Find("ItemIcon").gameObject;
        Image iconImage = iconObject.GetComponent<Image>();
    }
}

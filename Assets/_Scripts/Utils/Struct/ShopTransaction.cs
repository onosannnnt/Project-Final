[System.Serializable]
public class ShopTransaction
{
    public Skill Skill;
    public int Price;
    public SkillShopItem UIItem;

    public ShopTransaction(Skill skill, int price, SkillShopItem uiItem)
    {
        Skill = skill;
        Price = price;
        UIItem = uiItem;
    }
}

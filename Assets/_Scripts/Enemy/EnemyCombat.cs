using UnityEngine;

public class EnemyCombat : Entity
{
    protected override void Die()
    {
        Destroy(gameObject);
    }
    public void highlight(bool highlight)
    {
        GetComponent<SpriteRenderer>().color = highlight ? Color.red : Color.white;
    }
    public void OnMouseDown()
    {

    }
}
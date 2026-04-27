using UnityEngine;
using System.Collections.Generic;

public class ElementalPillar : EnemyCombat
{
    [Header("Pattern Data")]
    [Tooltip("รายการธาตุที่วนลูปในเสาต้นนี้")]
    public List<DamageElement> currentPattern = new List<DamageElement>();

    [Header("Visuals")]
    public SpriteRenderer spriteRenderer;
    [Tooltip("ใส่รูปเรียงตามนี้: 0=Fire, 1=Ice, 2=Lightning, 3=Wind")]
    public Sprite fireSprite;
    public Sprite iceSprite;
    public Sprite lightningSprite;
    public Sprite windSprite;

    [SerializeField] private int currentIndex = 0;

    public DamageElement GetCurrentElement()
    {
        if (currentPattern == null || currentPattern.Count == 0) return DamageElement.None;
        return currentPattern[currentIndex];
    }

    // ฟังก์ชันสำหรับให้บอสเรียกใช้เพื่อตั้งค่า Pattern และธาตุเริ่มต้นใหม่
    public void ResetPillar(List<DamageElement> newPattern, int startIndex)
    {
        currentPattern = new List<DamageElement>(newPattern);
        currentIndex = startIndex;
        UpdateVisual();
    }

    public override void TakeDamage(Damage damage)
    {
        // เมื่อโดนโจมตี ให้เปลี่ยนธาตุไปอันถัดไป
        ChangeToNextElement();

        // แสดงเลขดาเมจ (หรือ 0) เพื่อให้ผู้เล่นรู้ว่าตีโดน
        ShowDamage((int)damage.Amount, Utils.GetDamageColor(damage.Element), damage.IsCriticalHit);

        // เสาเป็นอมตะ: เลือดไม่ลด
        currentHealth = GetStat(StatType.MaxHealth);
        TriggerHealthChanged();
    }

    public void ChangeToNextElement()
    {
        if (currentPattern == null || currentPattern.Count == 0) return;

        // เลื่อนไปยังธาตุถัดไปใน Pattern
        currentIndex = (currentIndex + 1) % currentPattern.Count;

        UpdateVisual();

        // สะกิดบอกบอสให้เช็คว่าเสาตรงกัน 4 ต้นหรือยัง
        FindObjectOfType<PuzzleBossCombat>()?.OnPillarHit();
    }

    private void UpdateVisual()
    {
        if (spriteRenderer == null) return;

        // เช็คว่าธาตุปัจจุบันคืออะไร แล้วเปลี่ยนภาพตามนั้น
        DamageElement current = GetCurrentElement();

        switch (current)
        {
            case DamageElement.Fire:
                spriteRenderer.sprite = fireSprite;
                break;
            case DamageElement.Frost: // ใช้ Frost แทน Ice ตามระบบของคุณ
                spriteRenderer.sprite = iceSprite;
                break;
            case DamageElement.Lightning:
                spriteRenderer.sprite = lightningSprite;
                break;
            case DamageElement.Wind:
                spriteRenderer.sprite = windSprite;
                break;
        }
    }
}
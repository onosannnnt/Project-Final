using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum BreakState
{
    Normal,
    JustBroken,
    Vulnerable
}

public class EnemyCombat : Entity
{
    [SerializeField] protected GameObject healthBarForeground;
    [SerializeField] protected TMPro.TextMeshProUGUI breakArmorText; // Add this for UI

    [Header("Break Status Visual")]
    [SerializeField] protected Image breakStatusImage;
    [SerializeField] private Sprite physicalBreakStatusSprite;
    [SerializeField] private Sprite fireBreakStatusSprite;
    [SerializeField] private Sprite frostBreakStatusSprite;
    [SerializeField] private Sprite lightningBreakStatusSprite;
    [SerializeField] private Sprite windBreakStatusSprite;
    [SerializeField] private bool hideBreakStatusWhenNone = true;

    [Header("Elemental Break Effects")]
    [SerializeField] private float vulnerableDamageBonus = 0.15f;
    [SerializeField] private float physicalDamageBonusPerStack = 0.05f;
    [SerializeField] private float fireBonusDamageRatio = 0.5f;
    [SerializeField] private float windSpreadDamageRatio = 0.3f;
    [SerializeField] private float lightningArmorRecoveryRatio = 0.7f;

    private float maxhealthBarForegroundWidth;
    private bool isDead;
    private DamageElement currentBreakElementStatus = DamageElement.None;
    private int physicalBreakStacks = 0;
    private int incomingSkillContextDepth = 0;
    private bool allowWindSpreadInCurrentSkillContext = true;

    public BreakState currentBreakState = BreakState.Normal;
    public int currentBreakArmor;

    private class BreakDamageModifier : IDamageModifier
    {
        private EnemyCombat owner;
        public BreakDamageModifier(EnemyCombat owner) { this.owner = owner; }
        public void Modify(DamageCtx ctx)
        {
            if (owner.currentBreakState == BreakState.Vulnerable && owner.currentBreakElementStatus != DamageElement.Frost)
            {
                ctx.Damage.Amount *= 1f + owner.vulnerableDamageBonus; // take bonus damage while vulnerable
            }

            if (owner.physicalBreakStacks > 0)
            {
                ctx.Damage.Amount *= 1f + (owner.physicalDamageBonusPerStack * owner.physicalBreakStacks);
            }
        }
    }

    private PlayerEntity GetActivePlayer()
    {
        if (TurnManager.Instance != null && TurnManager.Instance.CurrentActivePlayer != null)
            return TurnManager.Instance.CurrentActivePlayer;
            
        // Fallback for safety if CurrentActivePlayer is null
        if (PlayerTeamManager.Instance != null)
        {
            PlayerEntity firstAlive = PlayerTeamManager.Instance.GetFirstAliveMember();
            if (firstAlive != null) return firstAlive;
        }
        
        return PlayerCombat.instance;
    }

    protected override void Start()
    {
        base.Start();
        maxhealthBarForegroundWidth = healthBarForeground.GetComponent<RectTransform>().sizeDelta.x;
        if (GetActivePlayer() != null && GetActivePlayer().GetEnemyTarget() == this) TargetingPanel.instance.SetEnemyTargetPanel(this);

        if (breakStatusImage == null)
        {
            Transform statusTransform = transform.Find("HealthBar/Status");
            if (statusTransform != null)
            {
                breakStatusImage = statusTransform.GetComponent<Image>();
            }
        }

        currentBreakArmor = (int)GetStat(StatType.MaxBreakArmor);
        IncomingModifiers.Add(new BreakDamageModifier(this));
        
        UpdateArmorUI();
        UpdateBreakStatusVisual();
    }

    protected virtual void UpdateArmorUI()
    {
        if (breakArmorText != null)
        {
            if (currentBreakState == BreakState.JustBroken || currentBreakState == BreakState.Vulnerable)
            {
                breakArmorText.text = "BROKEN";
                breakArmorText.color = Color.red;
            }
            else
            {
                breakArmorText.text = $"{currentBreakArmor}/{GetStat(StatType.MaxBreakArmor)}";
                breakArmorText.color = Color.white;
            }
        }
    }
    private void Update()
    {
        PlayerEntity activePlayer = GetActivePlayer();
        if (activePlayer == null)
        {
            Highlight(Color.white);
            return;
        }

        if (activePlayer.GetPlayerState != PlayerActionState.Targeting || activePlayer.GetSelectedSkill == null)
        {
            Highlight(Color.white);
            return;
        }

        if (activePlayer.GetSelectedSkill.TargetType == TargetType.Self)
        {
            Highlight(Color.white);
            return;
        }

        // If the selected skill targets all enemies, highlight all in red
        if (activePlayer.GetSelectedSkill.TargetType == TargetType.Enemy)
        {
            if (activePlayer.GetSelectedSkill.TargetCount == TargetCount.All)
            {
                Highlight(Color.red);
            }
            // If the selected skill targets a single enemy
            else if (activePlayer.GetSelectedSkill.TargetCount == TargetCount.Single)
            {
                if (activePlayer.GetEnemyTarget() == this)
                    Highlight(Color.red);
                else
                    Highlight(Color.white); // Don't show yellow for non-targets
            }
        }
    }

    public virtual void ReduceArmor(int amount, DamageElement breakElement = DamageElement.None)
    {
        if (currentBreakState != BreakState.Normal) return; // Only reduce armor if normal
        
        currentBreakArmor -= amount;
// // Debug.Log($"{gameObject.name} armor reduced by {amount}. Remaining Armor: {currentBreakArmor}");

        if (currentBreakArmor <= 0)
        {
            currentBreakArmor = 0;
            TriggerBreak(breakElement);
        }
        
        UpdateArmorUI();
    }

    private void TriggerBreak(DamageElement breakElement)
    {
// // Debug.Log($"{gameObject.name} is BROKEN!");
        currentBreakState = BreakState.JustBroken;

        currentBreakElementStatus = NormalizeBreakElementForStatus(breakElement);
        if (currentBreakElementStatus == DamageElement.Physical)
        {
            physicalBreakStacks += 1;
        }

        UpdateBreakStatusVisual();
        // Animation, sound effect, or floating text could go here
    }

    public void AdvanceBreakState()
    {
        if (currentBreakState == BreakState.JustBroken)
        {
            currentBreakState = BreakState.Vulnerable;
// // Debug.Log($"{gameObject.name} is now Vulnerable and will take 15% more damage.");
        }
        else if (currentBreakState == BreakState.Vulnerable)
        {
            // Recover logic
            currentBreakState = BreakState.Normal;
            int maxBreakArmor = (int)GetStat(StatType.MaxBreakArmor);
            if (currentBreakElementStatus == DamageElement.Lightning)
            {
                int recoveredArmor = Mathf.RoundToInt(maxBreakArmor * lightningArmorRecoveryRatio);
                currentBreakArmor = Mathf.Clamp(recoveredArmor, 0, maxBreakArmor);
            }
            else
            {
                currentBreakArmor = maxBreakArmor;
            }

            // Elemental break statuses are temporary and disappear when armor recovers.
            currentBreakElementStatus = DamageElement.None;
// // Debug.Log($"{gameObject.name} has recovered. Armor reset to Max.");
            // NOTE: In the future, you can add conditional logic here to prevent recovery 
        }
        
        UpdateArmorUI();
        UpdateBreakStatusVisual();
    }

    public override bool CanAction()
    {
        // Skip action in the turn they are broken
        if (currentBreakState == BreakState.JustBroken)
        {
            return false;
        }

        // Frost break makes the enemy skip the next turn after being broken.
        if (currentBreakState == BreakState.Vulnerable && currentBreakElementStatus == DamageElement.Frost)
        {
            return false;
        }

        return base.CanAction();
    }

    public void BeginIncomingSkillDamageContext(bool allowWindSpread)
    {
        incomingSkillContextDepth += 1;
        allowWindSpreadInCurrentSkillContext = allowWindSpread;
    }

    public void EndIncomingSkillDamageContext()
    {
        incomingSkillContextDepth = Mathf.Max(0, incomingSkillContextDepth - 1);
        if (incomingSkillContextDepth == 0)
        {
            allowWindSpreadInCurrentSkillContext = true;
        }
    }

    public void ApplyElementalBreakOnHitEffects(float incomingDamage)
    {
        if (incomingDamage <= 0f) return;
        if (CurrentHealth <= 0f) return;
        if (currentBreakState == BreakState.Normal) return;

        switch (currentBreakElementStatus)
        {
            case DamageElement.Fire:
            {
                float extraFireDamage = incomingDamage * fireBonusDamageRatio;
                if (extraFireDamage > 0f)
                {
                    // Separate hit number as requested.
                    TakeDamage(new Damage(extraFireDamage, DamageElement.Fire));
                }
                break;
            }
            case DamageElement.Wind:
            {
                // In AoE scenarios, only the selected target should spread.
                if (!allowWindSpreadInCurrentSkillContext) return;

                float spreadDamage = incomingDamage * windSpreadDamageRatio;
                if (spreadDamage <= 0f) return;

                foreach (EnemyCombat adjacent in GetAdjacentAliveEnemies())
                {
                    if (adjacent == null || adjacent == this || adjacent.IsDead()) continue;

                    // Direct damage application prevents recursive bouncing across adjacent enemies.
                    adjacent.TakeDamage(new Damage(spreadDamage, DamageElement.Wind));
                }
                break;
            }
        }
    }

    private List<EnemyCombat> GetAdjacentAliveEnemies()
    {
        if (TurnManager.Instance == null)
        {
            return new List<EnemyCombat>();
        }

        List<EnemyCombat> aliveEnemies = TurnManager.Instance.GetAliveEnemiesInBattleOrder();
        int currentIndex = aliveEnemies.IndexOf(this);
        if (currentIndex < 0)
        {
            return new List<EnemyCombat>();
        }

        List<EnemyCombat> adjacent = new List<EnemyCombat>(2);
        if (currentIndex > 0)
        {
            adjacent.Add(aliveEnemies[currentIndex - 1]);
        }
        if (currentIndex < aliveEnemies.Count - 1)
        {
            adjacent.Add(aliveEnemies[currentIndex + 1]);
        }
        return adjacent;
    }

    private DamageElement NormalizeBreakElementForStatus(DamageElement element)
    {
        switch (element)
        {
            case DamageElement.Physical:
            case DamageElement.Fire:
            case DamageElement.Frost:
            case DamageElement.Lightning:
            case DamageElement.Wind:
                return element;
            default:
                return DamageElement.None;
        }
    }

    protected virtual void UpdateBreakStatusVisual()
    {
        if (breakStatusImage == null)
        {
            return;
        }

        Sprite sprite = GetBreakStatusSprite(currentBreakElementStatus);
        if (sprite == null && hideBreakStatusWhenNone)
        {
            breakStatusImage.gameObject.SetActive(false);
            return;
        }

        breakStatusImage.gameObject.SetActive(true);
        breakStatusImage.sprite = sprite;
    }

    private Sprite GetBreakStatusSprite(DamageElement element)
    {
        switch (element)
        {
            case DamageElement.Physical:
                return physicalBreakStatusSprite;
            case DamageElement.Fire:
                return fireBreakStatusSprite;
            case DamageElement.Frost:
                return frostBreakStatusSprite;
            case DamageElement.Lightning:
                return lightningBreakStatusSprite;
            case DamageElement.Wind:
                return windBreakStatusSprite;
            default:
                return null;
        }
    }

    protected override void Die()
    {
        gameObject.SetActive(false);
        isDead = true;
        TurnManager.Instance.RemoveActionQueue(this);
        Destroy(gameObject);
    }

    protected override void MarkAsDead()
    {
        isDead = true;
        TurnManager.Instance.RemoveActionQueue(this);
    }
    public override void TakeDamage(Damage damage)
    {
        base.TakeDamage(damage);
        var healthBar = healthBarForeground.GetComponent<UnityEngine.UI.Image>();
// // Debug.Log($"{gameObject.name} took {damage.Amount} damage, current health: {CurrentHealth / GetStat(StatType.MaxHealth)}");
        if (healthBar != null)
        {
            float healthPercent = CurrentHealth / GetStat(StatType.MaxHealth);
            healthBarForeground.GetComponent<RectTransform>().sizeDelta = new Vector2(maxhealthBarForegroundWidth * healthPercent, healthBarForeground.GetComponent<RectTransform>().sizeDelta.y);
        }
    }

    public void OnMouseDown()
    {
        PlayerEntity activePlayer = GetActivePlayer();
        if (activePlayer == null) return;
        if (activePlayer.GetPlayerState != PlayerActionState.Targeting) return;
        if (activePlayer.GetSelectedSkill == null) return;
        if (activePlayer.GetSelectedSkill.TargetType == TargetType.Self) return;
        
        if (activePlayer.GetEnemyTarget() == this)
        {
            // Submit the action on behalf of whichever player is currently active in TurnManager
            Entity currentActive = TurnManager.Instance.CurrentActivePlayer ?? activePlayer;
            TurnManager.Instance.SubmitPlayerAction(currentActive, this, activePlayer.GetSelectedSkill);
            return;
        }

        activePlayer.SetEnemyTarget(this);
        TargetingPanel.instance.SetEnemyTargetPanel(this);

    }
    public bool IsDead()
    {
        return isDead;
    }
    public void Highlight(Color color)
    {
        // Stop tinting the character sprite
        Transform visualTransform = transform.Find("EnemyVisual");
        if (visualTransform != null)
        {
            SpriteRenderer sr = visualTransform.GetComponent<SpriteRenderer>();
            if (sr != null) sr.color = Color.white;
        }

        if (targetIndicator != null)
        {
            bool isHighlighted = color != Color.white;
            targetIndicator.SetActive(isHighlighted);
            
            if (isHighlighted)
            {
                // Apply the highlight color (Red/Yellow) to the indicator instead of the character
                SpriteRenderer indicatorSR = targetIndicator.GetComponent<SpriteRenderer>();
                if (indicatorSR != null) indicatorSR.color = color;
            }
        }
    }
}

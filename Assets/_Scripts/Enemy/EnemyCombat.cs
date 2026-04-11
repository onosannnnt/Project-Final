using UnityEngine;

public enum BreakState
{
    Normal,
    JustBroken,
    Vulnerable
}

public class EnemyCombat : Entity
{
    [SerializeField] private GameObject healthBarForeground;
    [SerializeField] private TMPro.TextMeshProUGUI breakArmorText; // Add this for UI
    private float maxhealthBarForegroundWidth;
    private bool isDead;

    public BreakState currentBreakState = BreakState.Normal;
    public int currentBreakArmor;

    private class BreakDamageModifier : IDamageModifier
    {
        private EnemyCombat owner;
        public BreakDamageModifier(EnemyCombat owner) { this.owner = owner; }
        public void Modify(DamageCtx ctx)
        {
            if (owner.currentBreakState == BreakState.Vulnerable)
            {
                ctx.Damage.Amount *= 1.15f; // take 15% more damage
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

        currentBreakArmor = (int)GetStat(StatType.MaxBreakArmor);
        IncomingModifiers.Add(new BreakDamageModifier(this));
        
        UpdateArmorUI();
    }

    private void UpdateArmorUI()
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
                    Highlight(Color.yellow);
            }
        }
    }

    public void ReduceArmor(int amount)
    {
        if (currentBreakState != BreakState.Normal) return; // Only reduce armor if normal
        
        currentBreakArmor -= amount;
// // Debug.Log($"{gameObject.name} armor reduced by {amount}. Remaining Armor: {currentBreakArmor}");

        if (currentBreakArmor <= 0)
        {
            currentBreakArmor = 0;
            TriggerBreak();
        }
        
        UpdateArmorUI();
    }

    private void TriggerBreak()
    {
// // Debug.Log($"{gameObject.name} is BROKEN!");
        currentBreakState = BreakState.JustBroken;
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
            currentBreakArmor = (int)GetStat(StatType.MaxBreakArmor);
// // Debug.Log($"{gameObject.name} has recovered. Armor reset to Max.");
            // NOTE: In the future, you can add conditional logic here to prevent recovery 
        }
        
        UpdateArmorUI();
    }

    public override bool CanAction()
    {
        // Skip action in the turn they are broken
        if (currentBreakState == BreakState.JustBroken)
        {
            return false;
        }
        return base.CanAction();
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
        GameObject EnemyVisual = transform.Find("EnemyVisual").gameObject;
        EnemyVisual.GetComponent<SpriteRenderer>().color = color;
    }
}

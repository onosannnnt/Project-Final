using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TeamMemberSlot
{
    [Tooltip("Used for readability in the inspector only.")]
    public string memberId;

    [Tooltip("Character already placed in scene. If assigned, prefab is ignored.")]
    public PlayerEntity sceneMember;

    [Tooltip("Character prefab to instantiate when sceneMember is not assigned.")]
    public PlayerEntity prefab;

    [Tooltip("Optional loadout override for this member in combat.")]
    public SkillLoadout loadoutOverride;

    [Tooltip("Spawn point index. Use -1 to auto-assign by slot order.")]
    public int spawnPointIndex = -1;
}

public class PlayerTeamManager : Singleton<PlayerTeamManager>
{
    [SerializeField] private Transform worldParent;

    [Header("Team Setup")]
    [Tooltip("Add playable characters here to avoid code changes when expanding the party.")]
    [SerializeField] private List<TeamMemberSlot> teamSlots = new List<TeamMemberSlot>();
    [SerializeField] private bool autoSpawnOnStart = true;
    [SerializeField] private bool autoDiscoverScenePlayers = true;
    [SerializeField] private bool fallbackToPlayerCombatSingleton = true;

    [Header("Spawn Points")]
    [SerializeField] private Vector3[] spawnPositions = new Vector3[]
    {
        new Vector3(0.911f, -0.66f, 0f),
        new Vector3(1.341f, -0.696f, 0.72f),
        new Vector3(0.91f, -0.21f, 0.72f),
        new Vector3(1.5f, 0.03f, 0f),
        new Vector3(1f, 0.03f, 0.5f)
    };

    [SerializeField] private Vector3[] spawnRotations = new Vector3[]
    {
        new Vector3(0f, 180f, 0f),
        new Vector3(0f, 180f, 0f),
        new Vector3(0f, 180f, 0f),
        new Vector3(0f, 180f, 0f),
        new Vector3(0f, 180f, 0f)
    };

    public List<PlayerEntity> ActiveTeamMembers { get; } = new List<PlayerEntity>();

    private void Start()
    {
        if (autoSpawnOnStart)
        {
            SpawnTeam();
        }
    }

    public void SpawnTeam()
    {
        ActiveTeamMembers.Clear();

        if (worldParent == null)
        {
            worldParent = transform;
        }

        int autoSlotIndex = 0;

        foreach (var slot in teamSlots)
        {
            PlayerEntity member = ResolveSlotMember(slot);
            if (member == null) continue;

            int spawnIndex = slot != null && slot.spawnPointIndex >= 0 ? slot.spawnPointIndex : autoSlotIndex;
            PlaceMember(member, spawnIndex);

            if (slot != null && slot.loadoutOverride != null)
            {
                member.SetSkillLoadout(slot.loadoutOverride, true);
            }

            AddMember(member);
            autoSlotIndex++;
        }

        if (autoDiscoverScenePlayers && worldParent != null)
        {
            var discoveredMembers = worldParent.GetComponentsInChildren<PlayerEntity>(true);
            foreach (var member in discoveredMembers)
            {
                if (member == null || ActiveTeamMembers.Contains(member)) continue;
                PlaceMember(member, autoSlotIndex);
                AddMember(member);
                autoSlotIndex++;
            }
        }

        if (ActiveTeamMembers.Count == 0 && fallbackToPlayerCombatSingleton && PlayerCombat.instance != null)
        {
            PlaceMember(PlayerCombat.instance, 0);
            AddMember(PlayerCombat.instance);
        }
    }

    public List<PlayerEntity> GetAliveMembers()
    {
        List<PlayerEntity> aliveMembers = new List<PlayerEntity>();
        foreach (var member in ActiveTeamMembers)
        {
            if (member != null && member.CurrentHealth > 0)
            {
                aliveMembers.Add(member);
            }
        }
        return aliveMembers;
    }

    public PlayerEntity GetFirstAliveMember()
    {
        foreach (var member in ActiveTeamMembers)
        {
            if (member != null && member.CurrentHealth > 0)
            {
                return member;
            }
        }
        return null;
    }

    public PlayerEntity GetMemberAt(int index)
    {
        if (index < 0 || index >= ActiveTeamMembers.Count) return null;
        return ActiveTeamMembers[index];
    }

    public int IndexOfMember(PlayerEntity member)
    {
        return ActiveTeamMembers.IndexOf(member);
    }

    public bool HasAliveMembers()
    {
        return GetFirstAliveMember() != null;
    }

    private PlayerEntity ResolveSlotMember(TeamMemberSlot slot)
    {
        if (slot == null) return null;
        if (slot.sceneMember != null) return slot.sceneMember;

        if (slot.prefab != null)
        {
            Transform parent = worldParent != null ? worldParent : transform;
            return Instantiate(slot.prefab, parent);
        }

        return null;
    }

    private void AddMember(PlayerEntity member)
    {
        if (member != null && !ActiveTeamMembers.Contains(member))
        {
            ActiveTeamMembers.Add(member);
        }
    }

    private void PlaceMember(PlayerEntity member, int index)
    {
        if (member == null) return;

        int posIndex = ResolveIndex(index, spawnPositions.Length);
        int rotIndex = ResolveIndex(index, spawnRotations.Length);

        if (posIndex >= 0)
        {
            member.transform.position = spawnPositions[posIndex];
        }

        if (rotIndex >= 0)
        {
            member.transform.rotation = Quaternion.Euler(spawnRotations[rotIndex]);
        }

        if (worldParent != null && member.transform.parent != worldParent)
        {
            member.transform.SetParent(worldParent);
        }
    }

    private int ResolveIndex(int desiredIndex, int arrayLength)
    {
        if (arrayLength <= 0) return -1;
        return Mathf.Clamp(desiredIndex, 0, arrayLength - 1);
    }
}

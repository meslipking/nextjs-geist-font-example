using UnityEngine;
using System;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance { get; private set; }

    [System.Serializable]
    public class ResourceData
    {
        public int gold;
        public int gems;
        public int summoningScrolls;
        public int soulShards;
        public int evolutionStones;
    }

    [Header("Starting Resources")]
    [SerializeField] private int startingGold = 1000;
    [SerializeField] private int startingGems = 100;
    [SerializeField] private int startingSummoningScrolls = 10;
    [SerializeField] private int startingSoulShards = 50;
    [SerializeField] private int startingEvolutionStones = 5;

    [Header("Resource Limits")]
    [SerializeField] private int maxGold = 999999;
    [SerializeField] private int maxGems = 99999;
    [SerializeField] private int maxSummoningScrolls = 999;
    [SerializeField] private int maxSoulShards = 9999;
    [SerializeField] private int maxEvolutionStones = 999;

    private ResourceData resources;
    private const string SAVE_KEY = "GameResources";

    // Events
    public event Action<ResourceData> OnResourcesChanged;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Initialize();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Initialize()
    {
        LoadResources();
    }

    private void LoadResources()
    {
        string jsonData = PlayerPrefs.GetString(SAVE_KEY, "");
        if (string.IsNullOrEmpty(jsonData))
        {
            // Initialize with starting resources
            resources = new ResourceData
            {
                gold = startingGold,
                gems = startingGems,
                summoningScrolls = startingSummoningScrolls,
                soulShards = startingSoulShards,
                evolutionStones = startingEvolutionStones
            };
        }
        else
        {
            resources = JsonUtility.FromJson<ResourceData>(jsonData);
        }
        OnResourcesChanged?.Invoke(resources);
    }

    private void SaveResources()
    {
        string jsonData = JsonUtility.ToJson(resources);
        PlayerPrefs.SetString(SAVE_KEY, jsonData);
        PlayerPrefs.Save();
    }

    #region Resource Management

    public bool HasEnoughResources(int goldCost, int gemCost = 0, int scrollCost = 0, 
        int shardCost = 0, int stoneCost = 0)
    {
        return resources.gold >= goldCost &&
               resources.gems >= gemCost &&
               resources.summoningScrolls >= scrollCost &&
               resources.soulShards >= shardCost &&
               resources.evolutionStones >= stoneCost;
    }

    public bool SpendResources(int goldCost, int gemCost = 0, int scrollCost = 0, 
        int shardCost = 0, int stoneCost = 0)
    {
        if (!HasEnoughResources(goldCost, gemCost, scrollCost, shardCost, stoneCost))
            return false;

        resources.gold -= goldCost;
        resources.gems -= gemCost;
        resources.summoningScrolls -= scrollCost;
        resources.soulShards -= shardCost;
        resources.evolutionStones -= stoneCost;

        OnResourcesChanged?.Invoke(resources);
        SaveResources();
        return true;
    }

    public void AddResources(int gold = 0, int gems = 0, int scrolls = 0, 
        int shards = 0, int stones = 0)
    {
        resources.gold = Mathf.Min(resources.gold + gold, maxGold);
        resources.gems = Mathf.Min(resources.gems + gems, maxGems);
        resources.summoningScrolls = Mathf.Min(resources.summoningScrolls + scrolls, maxSummoningScrolls);
        resources.soulShards = Mathf.Min(resources.soulShards + shards, maxSoulShards);
        resources.evolutionStones = Mathf.Min(resources.evolutionStones + stones, maxEvolutionStones);

        OnResourcesChanged?.Invoke(resources);
        SaveResources();
    }

    #endregion

    #region Resource Getters

    public int GetGold() => resources.gold;
    public int GetGems() => resources.gems;
    public int GetSummoningScrolls() => resources.summoningScrolls;
    public int GetSoulShards() => resources.soulShards;
    public int GetEvolutionStones() => resources.evolutionStones;

    #endregion

    #region Resource Costs

    public static class Costs
    {
        // Summoning costs
        public static readonly int SingleSummon = 100;
        public static readonly int MultiSummon = 1000; // 10+1 summon
        public static readonly int GuaranteedRareSummon = 300;
        public static readonly int SpecialSummon = 500;

        // Evolution costs
        public static readonly int[] EvolutionCosts = {
            100,  // N to R
            300,  // R to SR
            1000, // SR to SSR
            3000, // SSR to SSS
            10000 // SSS to SSS+
        };

        // Skill upgrade costs
        public static readonly int[] SkillUpgradeCosts = {
            50,   // Level 1 to 2
            150,  // Level 2 to 3
            450,  // Level 3 to 4
            1350, // Level 4 to 5
            4050  // Level 5 to 6
        };
    }

    #endregion

    #region Daily Rewards

    public void ClaimDailyReward(int day)
    {
        // Daily rewards increase with consecutive logins
        int goldReward = 100 * day;
        int gemReward = 10 * day;
        int scrollReward = day / 2;
        int shardReward = day * 5;
        int stoneReward = day / 5;

        AddResources(goldReward, gemReward, scrollReward, shardReward, stoneReward);
    }

    #endregion

    #region Resource Conversion

    public bool ConvertGemsToGold(int gemAmount)
    {
        if (resources.gems >= gemAmount)
        {
            int goldGained = gemAmount * 1000; // 1 gem = 1000 gold
            if (SpendResources(0, gemAmount))
            {
                AddResources(goldGained);
                return true;
            }
        }
        return false;
    }

    public bool ConvertShardsToScrolls(int shardAmount)
    {
        if (resources.soulShards >= shardAmount)
        {
            int scrollsGained = shardAmount / 100; // 100 shards = 1 scroll
            if (scrollsGained > 0 && SpendResources(0, 0, 0, shardAmount))
            {
                AddResources(0, 0, scrollsGained);
                return true;
            }
        }
        return false;
    }

    #endregion

    private void OnApplicationQuit()
    {
        SaveResources();
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            SaveResources();
        }
    }
}

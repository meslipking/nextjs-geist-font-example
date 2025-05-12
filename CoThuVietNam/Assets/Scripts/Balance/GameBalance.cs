using UnityEngine;
using System.Collections.Generic;

public class GameBalance : MonoBehaviour
{
    public static GameBalance Instance { get; private set; }

    [System.Serializable]
    public class RarityBalance
    {
        public Rarity rarity;
        public float baseStatMultiplier;
        public float maxStatMultiplier;
        public int maxPerTeam;
        public float powerScore;
    }

    [System.Serializable]
    public class CounterSystem
    {
        public AnimalType attacker;
        public AnimalType defender;
        public float damageMultiplier;
        public float defenseMultiplier;
    }

    [Header("Team Balance")]
    [SerializeField] private float maxTeamPowerDifference = 1.5f;
    [SerializeField] private int maxSameTypePerTeam = 1;
    [SerializeField] private RarityBalance[] rarityBalances;

    [Header("Combat Balance")]
    [SerializeField] private float baseAttackMultiplier = 1.0f;
    [SerializeField] private float maxCriticalDamage = 2.0f;
    [SerializeField] private float maxDamageReduction = 0.75f;
    [SerializeField] private CounterSystem[] counterSystem;

    [Header("Skill Balance")]
    [SerializeField] private float maxSkillDamageMultiplier = 2.0f;
    [SerializeField] private float maxCrowdControlDuration = 3.0f;
    [SerializeField] private float maxHealingPercentage = 0.3f;
    [SerializeField] private int maxBuffStacks = 3;

    private Dictionary<Rarity, RarityBalance> rarityBalanceDict;
    private Dictionary<(AnimalType, AnimalType), CounterSystem> counterDict;

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
        // Initialize rarity balance dictionary
        rarityBalanceDict = new Dictionary<Rarity, RarityBalance>();
        foreach (var balance in rarityBalances)
        {
            rarityBalanceDict[balance.rarity] = balance;
        }

        // Initialize counter system dictionary
        counterDict = new Dictionary<(AnimalType, AnimalType), CounterSystem>();
        foreach (var counter in counterSystem)
        {
            counterDict[(counter.attacker, counter.defender)] = counter;
        }

        // Set default rarity balances if not configured
        if (rarityBalances.Length == 0)
        {
            SetDefaultRarityBalances();
        }
    }

    private void SetDefaultRarityBalances()
    {
        rarityBalances = new RarityBalance[]
        {
            new RarityBalance { 
                rarity = Rarity.N, 
                baseStatMultiplier = 1.0f,
                maxStatMultiplier = 1.5f,
                maxPerTeam = 4,
                powerScore = 1.0f
            },
            new RarityBalance { 
                rarity = Rarity.R, 
                baseStatMultiplier = 1.2f,
                maxStatMultiplier = 1.8f,
                maxPerTeam = 3,
                powerScore = 1.5f
            },
            new RarityBalance { 
                rarity = Rarity.SR, 
                baseStatMultiplier = 1.4f,
                maxStatMultiplier = 2.1f,
                maxPerTeam = 2,
                powerScore = 2.0f
            },
            new RarityBalance { 
                rarity = Rarity.SSR, 
                baseStatMultiplier = 1.6f,
                maxStatMultiplier = 2.4f,
                maxPerTeam = 2,
                powerScore = 2.5f
            },
            new RarityBalance { 
                rarity = Rarity.SSS, 
                baseStatMultiplier = 1.8f,
                maxStatMultiplier = 2.7f,
                maxPerTeam = 1,
                powerScore = 3.0f
            },
            new RarityBalance { 
                rarity = Rarity.SSSPlus, 
                baseStatMultiplier = 2.0f,
                maxStatMultiplier = 3.0f,
                maxPerTeam = 1,
                powerScore = 3.5f
            }
        };
    }

    public bool ValidateTeamComposition(List<Animal> team)
    {
        if (team == null || team.Count == 0) return false;

        // Check team power balance
        float teamPower = CalculateTeamPower(team);
        
        // Check rarity limits
        Dictionary<Rarity, int> rarityCount = new Dictionary<Rarity, int>();
        Dictionary<AnimalType, int> typeCount = new Dictionary<AnimalType, int>();

        foreach (var animal in team)
        {
            // Check rarity count
            if (!rarityCount.ContainsKey(animal.CurrentRarity))
                rarityCount[animal.CurrentRarity] = 0;
            rarityCount[animal.CurrentRarity]++;

            // Check type count
            if (!typeCount.ContainsKey(animal.type))
                typeCount[animal.type] = 0;
            typeCount[animal.type]++;

            // Validate against limits
            if (rarityCount[animal.CurrentRarity] > rarityBalanceDict[animal.CurrentRarity].maxPerTeam)
                return false;

            if (typeCount[animal.type] > maxSameTypePerTeam)
                return false;
        }

        return true;
    }

    public float CalculateTeamPower(List<Animal> team)
    {
        float totalPower = 0;
        foreach (var animal in team)
        {
            totalPower += CalculateAnimalPower(animal);
        }
        return totalPower;
    }

    public float CalculateAnimalPower(Animal animal)
    {
        if (!rarityBalanceDict.TryGetValue(animal.CurrentRarity, out RarityBalance balance))
            return 0;

        float basePower = balance.powerScore;
        float statPower = (animal.Stats.health / 100f) + 
                         (animal.Stats.attack / 10f) + 
                         (animal.Stats.defense / 10f);
        
        return basePower * statPower;
    }

    public float GetCounterMultiplier(Animal attacker, Animal defender)
    {
        if (counterDict.TryGetValue((attacker.type, defender.type), out CounterSystem counter))
        {
            return counter.damageMultiplier;
        }
        return 1.0f;
    }

    public float CalculateDamage(Animal attacker, Animal defender, bool isCritical)
    {
        float baseDamage = attacker.Stats.attack * baseAttackMultiplier;
        float counterMult = GetCounterMultiplier(attacker, defender);
        float critMult = isCritical ? Mathf.Min(attacker.Stats.criticalDamage, maxCriticalDamage) : 1.0f;
        
        float damage = baseDamage * counterMult * critMult;
        
        // Apply defense
        float damageReduction = Mathf.Min(defender.Stats.defense / (defender.Stats.defense + 100f), maxDamageReduction);
        damage *= (1 - damageReduction);

        return damage;
    }

    public float ValidateSkillDamage(float rawDamage)
    {
        return Mathf.Min(rawDamage, rawDamage * maxSkillDamageMultiplier);
    }

    public float ValidateCrowdControlDuration(float duration)
    {
        return Mathf.Min(duration, maxCrowdControlDuration);
    }

    public float ValidateHealingAmount(float healAmount, float maxHealth)
    {
        return Mathf.Min(healAmount, maxHealth * maxHealingPercentage);
    }

    public int ValidateBuffStacks(int stacks)
    {
        return Mathf.Min(stacks, maxBuffStacks);
    }

    public bool IsTeamPowerBalanced(List<Animal> team1, List<Animal> team2)
    {
        float team1Power = CalculateTeamPower(team1);
        float team2Power = CalculateTeamPower(team2);

        float powerRatio = Mathf.Max(team1Power, team2Power) / Mathf.Min(team1Power, team2Power);
        return powerRatio <= maxTeamPowerDifference;
    }

    public string GetTeamCompositionError(List<Animal> team)
    {
        Dictionary<Rarity, int> rarityCount = new Dictionary<Rarity, int>();
        Dictionary<AnimalType, int> typeCount = new Dictionary<AnimalType, int>();

        foreach (var animal in team)
        {
            if (!rarityCount.ContainsKey(animal.CurrentRarity))
                rarityCount[animal.CurrentRarity] = 0;
            rarityCount[animal.CurrentRarity]++;

            if (!typeCount.ContainsKey(animal.type))
                typeCount[animal.type] = 0;
            typeCount[animal.type]++;

            if (rarityCount[animal.CurrentRarity] > rarityBalanceDict[animal.CurrentRarity].maxPerTeam)
                return $"Too many {animal.CurrentRarity} rarity animals (max {rarityBalanceDict[animal.CurrentRarity].maxPerTeam})";

            if (typeCount[animal.type] > maxSameTypePerTeam)
                return $"Too many {animal.type} type animals (max {maxSameTypePerTeam})";
        }

        return "Valid team composition";
    }
}

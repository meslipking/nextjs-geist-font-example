using UnityEngine;
using System.Collections.Generic;

public class AnimalEvolution : MonoBehaviour
{
    public static AnimalEvolution Instance { get; private set; }

    [System.Serializable]
    public class EvolutionRequirement
    {
        public Rarity currentRarity;
        public int requiredLevel;
        public int goldCost;
        public int stoneCost;
        public int duplicatesNeeded;
    }

    [System.Serializable]
    public class StatBonus
    {
        public float healthMultiplier;
        public float attackMultiplier;
        public float defenseMultiplier;
        public float criticalChanceBonus;
        public float criticalDamageBonus;
    }

    [Header("Evolution Requirements")]
    [SerializeField] private EvolutionRequirement[] requirements;

    [Header("Evolution Bonuses")]
    [SerializeField] private StatBonus[] evolutionBonuses;

    [Header("Visual Effects")]
    [SerializeField] private ParticleSystem evolutionEffect;
    [SerializeField] private ParticleSystem successEffect;
    [SerializeField] private AudioClip evolutionSound;
    [SerializeField] private AudioClip successSound;

    private Dictionary<Rarity, EvolutionRequirement> requirementDict;
    private Dictionary<Rarity, StatBonus> bonusDict;

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
        requirementDict = new Dictionary<Rarity, EvolutionRequirement>();
        bonusDict = new Dictionary<Rarity, StatBonus>();

        foreach (var req in requirements)
        {
            requirementDict[req.currentRarity] = req;
        }

        // Initialize evolution bonuses for each rarity tier
        bonusDict[Rarity.R] = new StatBonus { 
            healthMultiplier = 1.2f, 
            attackMultiplier = 1.25f, 
            defenseMultiplier = 1.2f,
            criticalChanceBonus = 0.02f,
            criticalDamageBonus = 0.1f
        };

        bonusDict[Rarity.SR] = new StatBonus { 
            healthMultiplier = 1.4f, 
            attackMultiplier = 1.45f, 
            defenseMultiplier = 1.35f,
            criticalChanceBonus = 0.05f,
            criticalDamageBonus = 0.2f
        };

        bonusDict[Rarity.SSR] = new StatBonus { 
            healthMultiplier = 1.6f, 
            attackMultiplier = 1.7f, 
            defenseMultiplier = 1.5f,
            criticalChanceBonus = 0.08f,
            criticalDamageBonus = 0.3f
        };

        bonusDict[Rarity.SSS] = new StatBonus { 
            healthMultiplier = 2.0f, 
            attackMultiplier = 2.1f, 
            defenseMultiplier = 1.8f,
            criticalChanceBonus = 0.12f,
            criticalDamageBonus = 0.5f
        };

        bonusDict[Rarity.SSSPlus] = new StatBonus { 
            healthMultiplier = 2.5f, 
            attackMultiplier = 2.6f, 
            defenseMultiplier = 2.2f,
            criticalChanceBonus = 0.15f,
            criticalDamageBonus = 0.8f
        };
    }

    public bool CanEvolve(Animal animal)
    {
        if (!requirementDict.TryGetValue(animal.CurrentRarity, out EvolutionRequirement req))
            return false;

        // Check level requirement
        if (animal.Level < req.requiredLevel)
            return false;

        // Check resource requirements
        if (!ResourceManager.Instance.HasEnoughResources(
            req.goldCost, 0, 0, 0, req.stoneCost))
            return false;

        // Check duplicates
        if (GameManager.Instance.GetDuplicateCount(animal.type) < req.duplicatesNeeded)
            return false;

        return true;
    }

    public bool EvolveAnimal(Animal animal)
    {
        if (!CanEvolve(animal))
            return false;

        EvolutionRequirement req = requirementDict[animal.CurrentRarity];

        // Spend resources
        if (!ResourceManager.Instance.SpendResources(
            req.goldCost, 0, 0, 0, req.stoneCost))
            return false;

        // Consume duplicates
        GameManager.Instance.ConsumeDuplicates(animal.type, req.duplicatesNeeded);

        // Determine next rarity
        Rarity nextRarity = GetNextRarity(animal.CurrentRarity);
        
        // Apply evolution bonuses
        ApplyEvolutionBonuses(animal, nextRarity);

        // Update animal's rarity
        animal.CurrentRarity = nextRarity;

        // Play evolution effects
        PlayEvolutionEffects(animal);

        return true;
    }

    private Rarity GetNextRarity(Rarity currentRarity)
    {
        switch (currentRarity)
        {
            case Rarity.N:
                return Rarity.R;
            case Rarity.R:
                return Rarity.SR;
            case Rarity.SR:
                return Rarity.SSR;
            case Rarity.SSR:
                return Rarity.SSS;
            case Rarity.SSS:
                return Rarity.SSSPlus;
            default:
                return currentRarity;
        }
    }

    private void ApplyEvolutionBonuses(Animal animal, Rarity newRarity)
    {
        if (!bonusDict.TryGetValue(newRarity, out StatBonus bonus))
            return;

        // Apply stat bonuses
        animal.Stats.health *= bonus.healthMultiplier;
        animal.Stats.attack *= bonus.attackMultiplier;
        animal.Stats.defense *= bonus.defenseMultiplier;
        animal.Stats.criticalChance += bonus.criticalChanceBonus;
        animal.Stats.criticalDamage += bonus.criticalDamageBonus;

        // Unlock new skills or upgrade existing ones
        UpgradeSkills(animal, newRarity);
    }

    private void UpgradeSkills(Animal animal, Rarity newRarity)
    {
        // Get skills for the new rarity tier
        SkillData[] newSkills = AnimalSkillsDatabase.Instance.GetAnimalSkills(animal.type);
        
        foreach (var skill in newSkills)
        {
            // Check if this is a new skill or an upgrade
            SkillData existingSkill = System.Array.Find(animal.skills, s => s.skillName == skill.skillName);
            
            if (existingSkill != null)
            {
                // Upgrade existing skill
                existingSkill.damage *= 1.5f;
                existingSkill.cooldown *= 0.9f;
            }
            else if (newRarity >= skill.requiredRarity)
            {
                // Add new skill
                System.Array.Resize(ref animal.skills, animal.skills.Length + 1);
                animal.skills[animal.skills.Length - 1] = skill;
            }
        }
    }

    private void PlayEvolutionEffects(Animal animal)
    {
        // Play particle effects
        if (evolutionEffect != null)
        {
            ParticleSystem effect = Instantiate(evolutionEffect, animal.transform.position, Quaternion.identity);
            effect.Play();
            Destroy(effect.gameObject, effect.main.duration);
        }

        if (successEffect != null)
        {
            ParticleSystem effect = Instantiate(successEffect, animal.transform.position, Quaternion.identity);
            effect.Play();
            Destroy(effect.gameObject, effect.main.duration);
        }

        // Play sounds
        if (evolutionSound != null)
        {
            AudioManager.Instance.PlaySound(evolutionSound.name);
        }

        if (successSound != null)
        {
            AudioManager.Instance.PlaySound(successSound.name);
        }

        // Notify UI
        UIManager.Instance.ShowEvolutionSuccess(animal);
    }

    public EvolutionRequirement GetEvolutionRequirements(Rarity rarity)
    {
        requirementDict.TryGetValue(rarity, out EvolutionRequirement req);
        return req;
    }

    public StatBonus GetEvolutionBonuses(Rarity rarity)
    {
        bonusDict.TryGetValue(rarity, out StatBonus bonus);
        return bonus;
    }
}

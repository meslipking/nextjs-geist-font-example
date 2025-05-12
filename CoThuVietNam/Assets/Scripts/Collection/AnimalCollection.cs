using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class AnimalCollection : MonoBehaviour
{
    public static AnimalCollection Instance { get; private set; }

    [System.Serializable]
    public class CollectedAnimal
    {
        public AnimalType type;
        public Rarity rarity;
        public int level;
        public float experience;
        public AnimalStats stats;
        public List<SkillData> unlockedSkills;
        public bool isLocked;
        public int duplicateCount;
        public System.DateTime acquisitionDate;
    }

    [System.Serializable]
    public class CollectionData
    {
        public List<CollectedAnimal> collection = new List<CollectedAnimal>();
        public Dictionary<AnimalType, int> discoveryProgress = new Dictionary<AnimalType, int>();
        public int totalAnimalsCollected;
        public int totalEvolutions;
        public int maxRarityAchieved;
    }

    [Header("Collection Settings")]
    [SerializeField] private int maxInventorySize = 100;
    [SerializeField] private float experiencePerDuplicate = 100f;
    [SerializeField] private float rarityExperienceBonus = 0.5f;

    private CollectionData collectionData;
    private Dictionary<AnimalType, CollectedAnimal> activeAnimals;
    private const string SAVE_KEY = "AnimalCollection";

    // Events
    public System.Action<CollectedAnimal> OnAnimalCollected;
    public System.Action<CollectedAnimal> OnAnimalEvolved;
    public System.Action<CollectedAnimal> OnAnimalLevelUp;
    public System.Action<int> OnCollectionUpdated;

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
        LoadCollection();
        activeAnimals = new Dictionary<AnimalType, CollectedAnimal>();
    }

    #region Collection Management

    public bool AddAnimal(AnimalData animalData)
    {
        // Check inventory space
        if (collectionData.collection.Count >= maxInventorySize)
        {
            UIManager.Instance.ShowNotification("Inventory is full!");
            return false;
        }

        CollectedAnimal existingAnimal = collectionData.collection
            .FirstOrDefault(a => a.type == animalData.type && a.rarity == animalData.rarity);

        if (existingAnimal != null)
        {
            // Handle duplicate
            HandleDuplicate(existingAnimal, animalData);
            return true;
        }

        // Create new collected animal
        CollectedAnimal newAnimal = new CollectedAnimal
        {
            type = animalData.type,
            rarity = animalData.rarity,
            level = 1,
            experience = 0,
            stats = new AnimalStats
            {
                health = animalData.baseHealth,
                attack = animalData.baseAttack,
                defense = animalData.baseDefense,
                criticalChance = animalData.criticalChance,
                criticalDamage = animalData.criticalDamage
            },
            unlockedSkills = new List<SkillData>(animalData.skills),
            isLocked = false,
            duplicateCount = 0,
            acquisitionDate = System.DateTime.Now
        };

        collectionData.collection.Add(newAnimal);
        UpdateCollectionProgress(animalData.type);
        
        OnAnimalCollected?.Invoke(newAnimal);
        SaveCollection();

        return true;
    }

    private void HandleDuplicate(CollectedAnimal existing, AnimalData duplicate)
    {
        existing.duplicateCount++;
        
        // Add experience based on rarity
        float expGain = experiencePerDuplicate * (1 + ((int)duplicate.rarity * rarityExperienceBonus));
        AddExperience(existing, expGain);

        SaveCollection();
    }

    public void AddExperience(CollectedAnimal animal, float experience)
    {
        animal.experience += experience;
        
        // Check for level up
        while (animal.experience >= GetExperienceForNextLevel(animal.level))
        {
            animal.experience -= GetExperienceForNextLevel(animal.level);
            animal.level++;
            
            // Apply level up bonuses
            ApplyLevelUpBonuses(animal);
            OnAnimalLevelUp?.Invoke(animal);
        }

        SaveCollection();
    }

    private float GetExperienceForNextLevel(int currentLevel)
    {
        return currentLevel * 100f * (1 + (currentLevel * 0.1f));
    }

    private void ApplyLevelUpBonuses(CollectedAnimal animal)
    {
        // Increase stats
        animal.stats.health *= 1.1f;
        animal.stats.attack *= 1.08f;
        animal.stats.defense *= 1.07f;
        animal.stats.criticalChance += 0.01f;
        animal.stats.criticalDamage += 0.02f;

        // Check for skill unlocks
        CheckSkillUnlocks(animal);
    }

    private void CheckSkillUnlocks(CollectedAnimal animal)
    {
        SkillData[] availableSkills = AnimalSkillsDatabase.Instance.GetAnimalSkills(animal.type);
        
        foreach (var skill in availableSkills)
        {
            if (animal.level >= skill.requiredLevel && !animal.unlockedSkills.Contains(skill))
            {
                animal.unlockedSkills.Add(skill);
                UIManager.Instance.ShowNotification($"New skill unlocked: {skill.skillName}!");
            }
        }
    }

    #endregion

    #region Collection Progress

    private void UpdateCollectionProgress(AnimalType type)
    {
        if (!collectionData.discoveryProgress.ContainsKey(type))
        {
            collectionData.discoveryProgress[type] = 1;
            collectionData.totalAnimalsCollected++;
        }
        else
        {
            collectionData.discoveryProgress[type]++;
        }

        OnCollectionUpdated?.Invoke(collectionData.totalAnimalsCollected);
    }

    public float GetCollectionProgress()
    {
        int totalPossibleAnimals = System.Enum.GetValues(typeof(AnimalType)).Length;
        return (float)collectionData.totalAnimalsCollected / totalPossibleAnimals;
    }

    public int GetDiscoveryProgress(AnimalType type)
    {
        return collectionData.discoveryProgress.TryGetValue(type, out int progress) ? progress : 0;
    }

    #endregion

    #region Queries

    public CollectedAnimal GetAnimal(AnimalType type)
    {
        return collectionData.collection.FirstOrDefault(a => a.type == type);
    }

    public List<CollectedAnimal> GetAnimalsByRarity(Rarity rarity)
    {
        return collectionData.collection.Where(a => a.rarity == rarity).ToList();
    }

    public List<CollectedAnimal> GetAllAnimals()
    {
        return new List<CollectedAnimal>(collectionData.collection);
    }

    public int GetDuplicateCount(AnimalType type)
    {
        var animal = GetAnimal(type);
        return animal?.duplicateCount ?? 0;
    }

    #endregion

    #region Save/Load

    private void LoadCollection()
    {
        string jsonData = PlayerPrefs.GetString(SAVE_KEY, "");
        if (string.IsNullOrEmpty(jsonData))
        {
            collectionData = new CollectionData();
        }
        else
        {
            collectionData = JsonUtility.FromJson<CollectionData>(jsonData);
        }
    }

    private void SaveCollection()
    {
        string jsonData = JsonUtility.ToJson(collectionData);
        PlayerPrefs.SetString(SAVE_KEY, jsonData);
        PlayerPrefs.Save();
    }

    #endregion

    private void OnApplicationQuit()
    {
        SaveCollection();
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            SaveCollection();
        }
    }
}

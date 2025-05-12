using UnityEngine;
using System.Collections.Generic;

public class TerrainSystem : MonoBehaviour
{
    public static TerrainSystem Instance { get; private set; }

    public enum TerrainType
    {
        Plain,      // Basic terrain
        Mountain,   // High ground
        Water,      // Water bodies
        Forest,     // Dense vegetation
        Desert,     // Arid terrain
        Swamp      // Marshy terrain
    }

    [System.Serializable]
    public class TerrainEffect
    {
        public TerrainType terrain;
        public float movementModifier;
        public float attackBonus;
        public float defenseBonus;
        public bool blocksLineOfSight;
        public bool requiresSpecialMovement;
    }

    [System.Serializable]
    public class TerrainInteraction
    {
        public AnimalType animalType;
        public TerrainType terrain;
        public bool canPass;
        public float specialBonus;
        public string effectDescription;
    }

    [Header("Terrain Settings")]
    [SerializeField] private TerrainEffect[] terrainEffects;
    [SerializeField] private TerrainInteraction[] terrainInteractions;
    [SerializeField] private float highGroundBonus = 0.2f;
    [SerializeField] private float coverDefenseBonus = 0.15f;

    private Dictionary<TerrainType, TerrainEffect> effectsDict;
    private Dictionary<(AnimalType, TerrainType), TerrainInteraction> interactionsDict;

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
        InitializeTerrainEffects();
        InitializeTerrainInteractions();
    }

    private void InitializeTerrainEffects()
    {
        effectsDict = new Dictionary<TerrainType, TerrainEffect>();

        // Default terrain effects if none are set
        if (terrainEffects.Length == 0)
        {
            terrainEffects = new TerrainEffect[]
            {
                new TerrainEffect {
                    terrain = TerrainType.Plain,
                    movementModifier = 1.0f,
                    attackBonus = 0f,
                    defenseBonus = 0f,
                    blocksLineOfSight = false,
                    requiresSpecialMovement = false
                },
                new TerrainEffect {
                    terrain = TerrainType.Mountain,
                    movementModifier = 0.5f,
                    attackBonus = 0.2f,
                    defenseBonus = 0.2f,
                    blocksLineOfSight = true,
                    requiresSpecialMovement = true
                },
                new TerrainEffect {
                    terrain = TerrainType.Water,
                    movementModifier = 0.7f,
                    attackBonus = 0f,
                    defenseBonus = -0.1f,
                    blocksLineOfSight = false,
                    requiresSpecialMovement = true
                },
                new TerrainEffect {
                    terrain = TerrainType.Forest,
                    movementModifier = 0.8f,
                    attackBonus = 0f,
                    defenseBonus = 0.15f,
                    blocksLineOfSight = true,
                    requiresSpecialMovement = false
                },
                new TerrainEffect {
                    terrain = TerrainType.Desert,
                    movementModifier = 0.6f,
                    attackBonus = -0.1f,
                    defenseBonus = -0.1f,
                    blocksLineOfSight = false,
                    requiresSpecialMovement = false
                },
                new TerrainEffect {
                    terrain = TerrainType.Swamp,
                    movementModifier = 0.5f,
                    attackBonus = -0.15f,
                    defenseBonus = 0.1f,
                    blocksLineOfSight = false,
                    requiresSpecialMovement = true
                }
            };
        }

        foreach (var effect in terrainEffects)
        {
            effectsDict[effect.terrain] = effect;
        }
    }

    private void InitializeTerrainInteractions()
    {
        interactionsDict = new Dictionary<(AnimalType, TerrainType), TerrainInteraction>();

        // Default terrain interactions if none are set
        if (terrainInteractions.Length == 0)
        {
            terrainInteractions = new TerrainInteraction[]
            {
                // Flying creatures
                CreateInteraction(AnimalType.Dragon, TerrainType.Mountain, true, 0.2f, "Dragons gain power on mountains"),
                CreateInteraction(AnimalType.Phoenix, TerrainType.Mountain, true, 0.2f, "Phoenix thrives in high altitude"),
                CreateInteraction(AnimalType.Griffin, TerrainType.Mountain, true, 0.15f, "Griffin patrols mountains easily"),

                // Aquatic creatures
                CreateInteraction(AnimalType.Kraken, TerrainType.Water, true, 0.3f, "Kraken dominates in water"),
                CreateInteraction(AnimalType.Hydra, TerrainType.Water, true, 0.25f, "Hydra moves freely in water"),

                // Forest dwellers
                CreateInteraction(AnimalType.Tiger, TerrainType.Forest, true, 0.2f, "Tiger stalks in forests"),
                CreateInteraction(AnimalType.Wolf, TerrainType.Forest, true, 0.15f, "Wolf hunts in forests"),
                CreateInteraction(AnimalType.Fox, TerrainType.Forest, true, 0.1f, "Fox hides in forests"),

                // Desert adaptations
                CreateInteraction(AnimalType.Lion, TerrainType.Desert, true, 0.15f, "Lion endures desert heat"),
                CreateInteraction(AnimalType.Elephant, TerrainType.Desert, true, 0.1f, "Elephant traverses deserts"),

                // Swamp specialists
                CreateInteraction(AnimalType.Mouse, TerrainType.Swamp, true, 0.1f, "Mouse navigates swamps"),
                CreateInteraction(AnimalType.Hydra, TerrainType.Swamp, true, 0.2f, "Hydra lurks in swamps")
            };
        }

        foreach (var interaction in terrainInteractions)
        {
            interactionsDict[(interaction.animalType, interaction.terrain)] = interaction;
        }
    }

    private TerrainInteraction CreateInteraction(AnimalType type, TerrainType terrain, 
        bool canPass, float bonus, string description)
    {
        return new TerrainInteraction
        {
            animalType = type,
            terrain = terrain,
            canPass = canPass,
            specialBonus = bonus,
            effectDescription = description
        };
    }

    public bool CanTraverseTerrain(Animal animal, TerrainType terrain)
    {
        // Check if terrain requires special movement
        if (!effectsDict[terrain].requiresSpecialMovement)
            return true;

        // Check for special interaction
        if (interactionsDict.TryGetValue((animal.type, terrain), out TerrainInteraction interaction))
            return interaction.canPass;

        // Check habitat-based movement
        HabitatSystem.Habitat habitat = HabitatSystem.Instance.GetPrimaryHabitat(animal.type);
        switch (terrain)
        {
            case TerrainType.Water:
                return habitat == HabitatSystem.Habitat.Sea;
            case TerrainType.Mountain:
                return habitat == HabitatSystem.Habitat.Sky;
            default:
                return true;
        }
    }

    public float GetMovementModifier(Animal animal, TerrainType terrain)
    {
        float modifier = effectsDict[terrain].movementModifier;

        // Apply special interaction bonus
        if (interactionsDict.TryGetValue((animal.type, terrain), out TerrainInteraction interaction))
        {
            modifier += interaction.specialBonus;
        }

        return Mathf.Max(modifier, 0.1f); // Minimum 10% movement speed
    }

    public float GetAttackBonus(Animal attacker, TerrainType terrain)
    {
        float bonus = effectsDict[terrain].attackBonus;

        // Apply special interaction bonus
        if (interactionsDict.TryGetValue((attacker.type, terrain), out TerrainInteraction interaction))
        {
            bonus += interaction.specialBonus;
        }

        // Apply high ground bonus if applicable
        if (terrain == TerrainType.Mountain)
        {
            bonus += highGroundBonus;
        }

        return bonus;
    }

    public float GetDefenseBonus(Animal defender, TerrainType terrain)
    {
        float bonus = effectsDict[terrain].defenseBonus;

        // Apply special interaction bonus
        if (interactionsDict.TryGetValue((defender.type, terrain), out TerrainInteraction interaction))
        {
            bonus += interaction.specialBonus;
        }

        // Apply cover bonus in forests
        if (terrain == TerrainType.Forest)
        {
            bonus += coverDefenseBonus;
        }

        return bonus;
    }

    public bool BlocksLineOfSight(TerrainType terrain)
    {
        return effectsDict[terrain].blocksLineOfSight;
    }

    public string GetTerrainEffect(Animal animal, TerrainType terrain)
    {
        if (interactionsDict.TryGetValue((animal.type, terrain), out TerrainInteraction interaction))
        {
            return interaction.effectDescription;
        }

        return "No special effect";
    }

    public List<TerrainType> GetFavorableTerrains(Animal animal)
    {
        List<TerrainType> favorable = new List<TerrainType>();

        foreach (var pair in interactionsDict)
        {
            if (pair.Key.Item1 == animal.type && pair.Value.specialBonus > 0)
            {
                favorable.Add(pair.Key.Item2);
            }
        }

        return favorable;
    }
}

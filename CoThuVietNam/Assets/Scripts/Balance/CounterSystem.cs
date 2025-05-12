using UnityEngine;
using System.Collections.Generic;

public class CounterSystem : MonoBehaviour
{
    public static CounterSystem Instance { get; private set; }

    [System.Serializable]
    public class TypeAdvantage
    {
        public AnimalType attacker;
        public AnimalType defender;
        public float damageBonus;
        public float defenseReduction;
        public string description;
    }

    [System.Serializable]
    public class ElementalAdvantage
    {
        public ElementType attacker;
        public ElementType defender;
        public float multiplier;
    }

    public enum ElementType
    {
        None,
        Fire,
        Water,
        Earth,
        Air,
        Light,
        Dark
    }

    [Header("Type Advantages")]
    [SerializeField] private TypeAdvantage[] typeAdvantages;
    [SerializeField] private float maxTypeAdvantageBonus = 0.5f; // 50% max bonus

    [Header("Elemental System")]
    [SerializeField] private ElementalAdvantage[] elementalAdvantages;
    [SerializeField] private float maxElementalBonus = 0.3f; // 30% max bonus

    private Dictionary<(AnimalType, AnimalType), TypeAdvantage> typeAdvantageDict;
    private Dictionary<(ElementType, ElementType), float> elementalAdvantageDict;

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
        InitializeTypeAdvantages();
        InitializeElementalAdvantages();
    }

    private void InitializeTypeAdvantages()
    {
        typeAdvantageDict = new Dictionary<(AnimalType, AnimalType), TypeAdvantage>();

        // Default type advantages if none are set
        if (typeAdvantages.Length == 0)
        {
            typeAdvantages = new TypeAdvantage[]
            {
                // Traditional counters
                CreateAdvantage(AnimalType.Tiger, AnimalType.Dog, 0.3f, 0.2f, "Tigers naturally dominate dogs"),
                CreateAdvantage(AnimalType.Dog, AnimalType.Mouse, 0.3f, 0.2f, "Dogs are natural mouse hunters"),
                CreateAdvantage(AnimalType.Mouse, AnimalType.Elephant, 0.3f, 0.2f, "Mice can outmaneuver elephants"),
                CreateAdvantage(AnimalType.Elephant, AnimalType.Tiger, 0.3f, 0.2f, "Elephants overpower tigers"),

                // Mythical counters
                CreateAdvantage(AnimalType.Dragon, AnimalType.Phoenix, 0.2f, 0.1f, "Dragon's wisdom counters Phoenix"),
                CreateAdvantage(AnimalType.Phoenix, AnimalType.Unicorn, 0.2f, 0.1f, "Phoenix fire affects Unicorn"),
                CreateAdvantage(AnimalType.Unicorn, AnimalType.Dragon, 0.2f, 0.1f, "Unicorn's purity affects Dragon"),

                // Strategic counters
                CreateAdvantage(AnimalType.Griffin, AnimalType.Hydra, 0.25f, 0.15f, "Aerial advantage"),
                CreateAdvantage(AnimalType.Hydra, AnimalType.Chimera, 0.25f, 0.15f, "Multiple heads advantage"),
                CreateAdvantage(AnimalType.Chimera, AnimalType.Griffin, 0.25f, 0.15f, "Versatile attack patterns"),

                // Balanced counters
                CreateAdvantage(AnimalType.Lion, AnimalType.Wolf, 0.2f, 0.1f, "Pride leader advantage"),
                CreateAdvantage(AnimalType.Wolf, AnimalType.Fox, 0.2f, 0.1f, "Pack hunter advantage"),
                CreateAdvantage(AnimalType.Fox, AnimalType.Lion, 0.2f, 0.1f, "Cunning over strength")
            };
        }

        foreach (var advantage in typeAdvantages)
        {
            typeAdvantageDict[(advantage.attacker, advantage.defender)] = advantage;
        }
    }

    private void InitializeElementalAdvantages()
    {
        elementalAdvantageDict = new Dictionary<(ElementType, ElementType), float>();

        // Default elemental advantages
        if (elementalAdvantages.Length == 0)
        {
            elementalAdvantages = new ElementalAdvantage[]
            {
                new ElementalAdvantage { attacker = ElementType.Fire, defender = ElementType.Earth, multiplier = 1.3f },
                new ElementalAdvantage { attacker = ElementType.Earth, defender = ElementType.Air, multiplier = 1.3f },
                new ElementalAdvantage { attacker = ElementType.Air, defender = ElementType.Water, multiplier = 1.3f },
                new ElementalAdvantage { attacker = ElementType.Water, defender = ElementType.Fire, multiplier = 1.3f },
                new ElementalAdvantage { attacker = ElementType.Light, defender = ElementType.Dark, multiplier = 1.5f },
                new ElementalAdvantage { attacker = ElementType.Dark, defender = ElementType.Light, multiplier = 1.5f }
            };
        }

        foreach (var advantage in elementalAdvantages)
        {
            elementalAdvantageDict[(advantage.attacker, advantage.defender)] = advantage.multiplier;
        }
    }

    private TypeAdvantage CreateAdvantage(AnimalType attacker, AnimalType defender, 
        float damageBonus, float defenseReduction, string description)
    {
        return new TypeAdvantage
        {
            attacker = attacker,
            defender = defender,
            damageBonus = Mathf.Min(damageBonus, maxTypeAdvantageBonus),
            defenseReduction = Mathf.Min(defenseReduction, maxTypeAdvantageBonus),
            description = description
        };
    }

    public float CalculateDamageMultiplier(Animal attacker, Animal defender)
    {
        float multiplier = 1f;

        // Check type advantage
        if (typeAdvantageDict.TryGetValue((attacker.type, defender.type), out TypeAdvantage typeAdvantage))
        {
            multiplier += typeAdvantage.damageBonus;
        }

        // Check elemental advantage
        ElementType attackerElement = GetAnimalElement(attacker);
        ElementType defenderElement = GetAnimalElement(defender);
        
        if (elementalAdvantageDict.TryGetValue((attackerElement, defenderElement), out float elementalMultiplier))
        {
            multiplier *= elementalMultiplier;
        }

        return multiplier;
    }

    public float CalculateDefenseMultiplier(Animal defender, Animal attacker)
    {
        float multiplier = 1f;

        // Check type disadvantage
        if (typeAdvantageDict.TryGetValue((attacker.type, defender.type), out TypeAdvantage typeAdvantage))
        {
            multiplier -= typeAdvantage.defenseReduction;
        }

        return Mathf.Max(multiplier, 0.5f); // Minimum 50% defense
    }

    public string GetAdvantageDescription(Animal attacker, Animal defender)
    {
        if (typeAdvantageDict.TryGetValue((attacker.type, defender.type), out TypeAdvantage advantage))
        {
            return advantage.description;
        }
        return "No specific advantage";
    }

    private ElementType GetAnimalElement(Animal animal)
    {
        // Map animals to elements based on their characteristics
        switch (animal.type)
        {
            case AnimalType.Dragon:
            case AnimalType.Phoenix:
                return ElementType.Fire;

            case AnimalType.Mouse:
            case AnimalType.Kraken:
                return ElementType.Water;

            case AnimalType.Elephant:
            case AnimalType.Lion:
                return ElementType.Earth;

            case AnimalType.Griffin:
            case AnimalType.Pegasus:
                return ElementType.Air;

            case AnimalType.Unicorn:
                return ElementType.Light;

            case AnimalType.Hydra:
            case AnimalType.Cerberus:
                return ElementType.Dark;

            default:
                return ElementType.None;
        }
    }

    public List<AnimalType> GetStrongAgainst(AnimalType type)
    {
        List<AnimalType> strongAgainst = new List<AnimalType>();
        foreach (var advantage in typeAdvantages)
        {
            if (advantage.attacker == type)
            {
                strongAgainst.Add(advantage.defender);
            }
        }
        return strongAgainst;
    }

    public List<AnimalType> GetWeakAgainst(AnimalType type)
    {
        List<AnimalType> weakAgainst = new List<AnimalType>();
        foreach (var advantage in typeAdvantages)
        {
            if (advantage.defender == type)
            {
                weakAgainst.Add(advantage.attacker);
            }
        }
        return weakAgainst;
    }

    public string GetElementalAdvantageDescription(Animal attacker, Animal defender)
    {
        ElementType attackerElement = GetAnimalElement(attacker);
        ElementType defenderElement = GetAnimalElement(defender);

        if (attackerElement == ElementType.None || defenderElement == ElementType.None)
            return "";

        if (elementalAdvantageDict.TryGetValue((attackerElement, defenderElement), out float multiplier))
        {
            if (multiplier > 1)
                return $"{attackerElement} is strong against {defenderElement}";
            else if (multiplier < 1)
                return $"{attackerElement} is weak against {defenderElement}";
        }

        return "No elemental advantage";
    }
}

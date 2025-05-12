using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "AnimalDatabase", menuName = "CoThuVietNam/AnimalDatabase")]
public partial class AnimalDatabase
{
    [System.Serializable]
    public class AnimalEntry
    {
        public AnimalData data;
        public string description;
        public string specialAbility;
        public string comboPotential;
        public string counterPlay;
    }

    public List<AnimalEntry> animals = new List<AnimalEntry>
    {
        // SSS+ Tier
        new AnimalEntry {
            data = new AnimalData {
                animalName = "Divine Dragon",
                type = AnimalType.Dragon,
                rarity = Rarity.SSSPlus,
                baseHealth = 1000,
                baseAttack = 150,
                baseDefense = 120,
                moveRange = 3,
                criticalChance = 0.15f,
                criticalDamage = 2.0f,
                summoningCost = 1000,
                summoningChance = 0.001f
            },
            description = "Ancient dragon with divine powers",
            specialAbility = "Dragon's Breath: Deals massive AoE damage",
            comboPotential = "Works well with Phoenix for ultimate destruction",
            counterPlay = "Vulnerable to coordinated attacks from multiple smaller units"
        },

        // SSS Tier
        new AnimalEntry {
            data = new AnimalData {
                animalName = "Phoenix",
                type = AnimalType.Phoenix,
                rarity = Rarity.SSS,
                baseHealth = 800,
                baseAttack = 130,
                baseDefense = 90,
                moveRange = 4,
                criticalChance = 0.12f,
                criticalDamage = 1.8f,
                summoningCost = 800,
                summoningChance = 0.003f
            },
            description = "Immortal fire bird",
            specialAbility = "Rebirth: Revives once with 50% HP",
            comboPotential = "Synergizes with fire-based abilities",
            counterPlay = "Water-based attacks deal extra damage"
        },

        // SSR Tier
        new AnimalEntry {
            data = new AnimalData {
                animalName = "Unicorn",
                type = AnimalType.Unicorn,
                rarity = Rarity.SSR,
                baseHealth = 600,
                baseAttack = 100,
                baseDefense = 80,
                moveRange = 3,
                criticalChance = 0.1f,
                criticalDamage = 1.6f,
                summoningCost = 600,
                summoningChance = 0.01f
            },
            description = "Majestic healing unicorn",
            specialAbility = "Healing Light: Restores HP to nearby allies",
            comboPotential = "Great support for high-damage units",
            counterPlay = "Focus fire to prevent healing"
        },

        // SR Tier
        new AnimalEntry {
            data = new AnimalData {
                animalName = "Griffin",
                type = AnimalType.Griffin,
                rarity = Rarity.SR,
                baseHealth = 500,
                baseAttack = 90,
                baseDefense = 70,
                moveRange = 4,
                criticalChance = 0.08f,
                criticalDamage = 1.5f,
                summoningCost = 400,
                summoningChance = 0.03f
            },
            description = "Noble aerial predator",
            specialAbility = "Sky Strike: Can attack from above",
            comboPotential = "Works well with ground units",
            counterPlay = "Ranged attacks are effective"
        },

        // More animals with their unique attributes...
        // (I'll continue with more animals in the next message due to length)
    };

    // Helper method to get animal data by type
    public AnimalData GetAnimalData(AnimalType type)
    {
        return animals.Find(a => a.data.type == type)?.data;
    }

    // Helper method to get animals by rarity
    public List<AnimalData> GetAnimalsByRarity(Rarity rarity)
    {
        return animals
            .Where(a => a.data.rarity == rarity)
            .Select(a => a.data)
            .ToList();
    }

    // Helper method to get animal description
    public string GetAnimalDescription(AnimalType type)
    {
        return animals.Find(a => a.data.type == type)?.description;
    }

    // Helper method to get animal special ability description
    public string GetSpecialAbilityDescription(AnimalType type)
    {
        return animals.Find(a => a.data.type == type)?.specialAbility;
    }

    // Helper method to get animal combo potential
    public string GetComboPotential(AnimalType type)
    {
        return animals.Find(a => a.data.type == type)?.comboPotential;
    }

    // Helper method to get animal counter play
    public string GetCounterPlay(AnimalType type)
    {
        return animals.Find(a => a.data.type == type)?.counterPlay;
    }
}

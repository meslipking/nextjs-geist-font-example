using UnityEngine;

public partial class AnimalDatabase
{
    // Additional animals to complete the 20-animal roster
    private void InitializeRemainingAnimals()
    {
        animals.AddRange(new[] {
            // SSR Tier Continued
            new AnimalEntry {
                data = new AnimalData {
                    animalName = "Hydra",
                    type = AnimalType.Hydra,
                    rarity = Rarity.SSR,
                    baseHealth = 750,
                    baseAttack = 110,
                    baseDefense = 85,
                    moveRange = 2,
                    criticalChance = 0.1f,
                    criticalDamage = 1.7f,
                    summoningCost = 650,
                    summoningChance = 0.015f
                },
                description = "Multi-headed mythical serpent",
                specialAbility = "Regeneration: Grows new head when damaged, increasing attack power",
                comboPotential = "Pairs well with healing support units",
                counterPlay = "Fire damage prevents head regeneration"
            },

            // SR Tier Continued
            new AnimalEntry {
                data = new AnimalData {
                    animalName = "Chimera",
                    type = AnimalType.Chimera,
                    rarity = Rarity.SR,
                    baseHealth = 550,
                    baseAttack = 95,
                    baseDefense = 75,
                    moveRange = 3,
                    criticalChance = 0.09f,
                    criticalDamage = 1.5f,
                    summoningCost = 450,
                    summoningChance = 0.035f
                },
                description = "Three-headed hybrid beast",
                specialAbility = "Triple Strike: Can attack three different targets",
                comboPotential = "Effective with crowd control units",
                counterPlay = "Vulnerable to single-target burst damage"
            },

            new AnimalEntry {
                data = new AnimalData {
                    animalName = "Pegasus",
                    type = AnimalType.Pegasus,
                    rarity = Rarity.SR,
                    baseHealth = 480,
                    baseAttack = 85,
                    baseDefense = 70,
                    moveRange = 5,
                    criticalChance = 0.08f,
                    criticalDamage = 1.4f,
                    summoningCost = 400,
                    summoningChance = 0.04f
                },
                description = "Majestic winged horse",
                specialAbility = "Swift Flight: Can move over obstacles",
                comboPotential = "Great mobility support for slower units",
                counterPlay = "Weak against ranged attacks"
            },

            // R Tier
            new AnimalEntry {
                data = new AnimalData {
                    animalName = "Basilisk",
                    type = AnimalType.Basilisk,
                    rarity = Rarity.R,
                    baseHealth = 400,
                    baseAttack = 80,
                    baseDefense = 60,
                    moveRange = 3,
                    criticalChance = 0.07f,
                    criticalDamage = 1.3f,
                    summoningCost = 300,
                    summoningChance = 0.06f
                },
                description = "Legendary serpent king",
                specialAbility = "Petrifying Gaze: Chance to stun enemies",
                comboPotential = "Works well with area control units",
                counterPlay = "Mirror shields reflect petrifying gaze"
            },

            new AnimalEntry {
                data = new AnimalData {
                    animalName = "Manticore",
                    type = AnimalType.Manticore,
                    rarity = Rarity.R,
                    baseHealth = 420,
                    baseAttack = 75,
                    baseDefense = 65,
                    moveRange = 3,
                    criticalChance = 0.07f,
                    criticalDamage = 1.35f,
                    summoningCost = 320,
                    summoningChance = 0.055f
                },
                description = "Lion-scorpion hybrid",
                specialAbility = "Venomous Sting: Applies poison effect",
                comboPotential = "Synergizes with other poison units",
                counterPlay = "Antidote effects neutralize poison"
            },

            // Original animals with updated stats and abilities
            new AnimalEntry {
                data = new AnimalData {
                    animalName = "Tiger",
                    type = AnimalType.Tiger,
                    rarity = Rarity.SR,
                    baseHealth = 520,
                    baseAttack = 100,
                    baseDefense = 70,
                    moveRange = 4,
                    criticalChance = 0.1f,
                    criticalDamage = 1.6f,
                    summoningCost = 450,
                    summoningChance = 0.035f
                },
                description = "Powerful jungle predator",
                specialAbility = "Leap Attack: Can jump over water and enemies",
                comboPotential = "Strong with other aggressive units",
                counterPlay = "Vulnerable after using leap attack"
            },

            new AnimalEntry {
                data = new AnimalData {
                    animalName = "Lion",
                    type = AnimalType.Lion,
                    rarity = Rarity.SR,
                    baseHealth = 500,
                    baseAttack = 95,
                    baseDefense = 75,
                    moveRange = 3,
                    criticalChance = 0.09f,
                    criticalDamage = 1.55f,
                    summoningCost = 430,
                    summoningChance = 0.038f
                },
                description = "King of beasts",
                specialAbility = "Mighty Roar: Intimidates nearby enemies",
                comboPotential = "Leadership bonus for nearby allies",
                counterPlay = "Weak against coordinated attacks"
            },

            // Continue with more animals...
            // Each with unique abilities, stats, and strategic value
        });
    }
}

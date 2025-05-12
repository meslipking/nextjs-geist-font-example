using UnityEngine;

public enum Rarity
{
    N,      // Normal
    R,      // Rare
    SR,     // Super Rare
    SSR,    // Super Super Rare
    SSS,    // Triple S
    SSSPlus // Triple S Plus
}

[CreateAssetMenu(fileName = "New Animal Data", menuName = "CoThuVietNam/AnimalData")]
public class AnimalData : ScriptableObject
{
    [Header("Basic Info")]
    public string animalName;
    public AnimalType type;
    public Rarity rarity;
    public Sprite icon;
    public RuntimeAnimatorController animatorController;
    public GameObject prefab;

    [Header("Stats")]
    public int baseHealth;
    public int baseAttack;
    public int baseDefense;
    public int moveRange;
    public float criticalChance;
    public float criticalDamage;

    [Header("Skills")]
    public SkillData[] skills;

    [Header("Visual Effects")]
    public ParticleSystem summonEffect;
    public ParticleSystem levelUpEffect;
    public AudioClip summonSound;
    public AudioClip attackSound;
    public AudioClip skillSound;

    [Header("Summoning")]
    public int summoningCost;
    public float summoningChance; // Probability of being summoned based on rarity
}

// Extended animal types for 20 animals
public enum AnimalType
{
    // Original animals
    Tiger,
    Lion,
    Elephant,
    Mouse,
    Cat,
    Dog,
    Wolf,
    Fox,

    // New animals
    Dragon,
    Phoenix,
    Unicorn,
    Griffin,
    Hydra,
    Chimera,
    Pegasus,
    Basilisk,
    Manticore,
    Kraken,
    Cerberus,
    Sphinx
}

[System.Serializable]
public class AnimalStats
{
    public float health;
    public float attack;
    public float defense;
    public float criticalChance;
    public float criticalDamage;
    public int level;
    public int experience;
    public int experienceToNextLevel;

    public void LevelUp()
    {
        level++;
        health *= 1.1f;
        attack *= 1.15f;
        defense *= 1.12f;
        criticalChance += 0.01f;
        criticalDamage += 0.05f;
        experienceToNextLevel = CalculateNextLevelExp();
    }

    private int CalculateNextLevelExp()
    {
        return 100 * level * (level + 1) / 2;
    }
}

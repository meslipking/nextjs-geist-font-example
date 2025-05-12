using UnityEngine;

[CreateAssetMenu(fileName = "GameConfig", menuName = "CoThuVietNam/GameConfig")]
public class GameConfig : ScriptableObject
{
    [Header("Board Settings")]
    public int boardWidth = 8;
    public int boardHeight = 8;
    public float cellSize = 1f;
    public Color normalCellColor = Color.white;
    public Color waterCellColor = Color.blue;
    public Color highlightedCellColor = Color.yellow;
    public Color selectedCellColor = Color.green;

    [Header("Game Rules")]
    public int minAnimalsPerPlayer = 2;
    public int maxAnimalsPerPlayer = 4;
    public int maxTurns = 100;
    public float turnTimeLimit = 60f;
    public float waterMovementPenalty = 0.5f;

    [Header("Animal Base Stats")]
    [System.Serializable]
    public class AnimalStats
    {
        public AnimalType type;
        public int moveRange;
        public int attackPower;
        public float health;
        public SkillData[] defaultSkills;
    }
    public AnimalStats[] animalBaseStats;

    [Header("Combat Settings")]
    public float baseDamage = 10f;
    public float criticalHitChance = 0.1f;
    public float criticalHitMultiplier = 1.5f;
    public float waterCombatPenalty = 0.75f;

    [Header("Animation Settings")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 720f;
    public float jumpHeight = 1f;
    public float attackAnimationDuration = 0.5f;
    public float deathAnimationDuration = 1f;
    public float skillAnimationDuration = 1f;

    [Header("Audio Settings")]
    public float masterVolume = 1f;
    public float musicVolume = 0.7f;
    public float sfxVolume = 1f;
    public float uiVolume = 0.8f;

    [Header("UI Settings")]
    public float skillCooldownDisplayScale = 1f;
    public Color skillReadyColor = Color.green;
    public Color skillCooldownColor = Color.gray;
    public float tooltipDelay = 0.5f;
    public float notificationDuration = 2f;

    [Header("Special Abilities")]
    [System.Serializable]
    public class SpecialAbility
    {
        public AnimalType animalType;
        public string abilityName;
        public string description;
        public float cooldown;
        public float effectDuration;
        public float effectStrength;
    }
    public SpecialAbility[] specialAbilities;

    [Header("Starting Positions")]
    public Vector2Int[] player1DefaultPositions = new Vector2Int[]
    {
        new Vector2Int(0, 0),
        new Vector2Int(0, 1),
        new Vector2Int(0, 2),
        new Vector2Int(0, 3)
    };

    public Vector2Int[] player2DefaultPositions = new Vector2Int[]
    {
        new Vector2Int(7, 7),
        new Vector2Int(7, 6),
        new Vector2Int(7, 5),
        new Vector2Int(7, 4)
    };

    [Header("Water Tiles")]
    public Vector2Int[] waterTilePositions = new Vector2Int[]
    {
        new Vector2Int(3, 3),
        new Vector2Int(3, 4),
        new Vector2Int(4, 3),
        new Vector2Int(4, 4)
    };

    // Singleton instance
    private static GameConfig instance;
    public static GameConfig Instance
    {
        get
        {
            if (instance == null)
            {
                instance = Resources.Load<GameConfig>("GameConfig");
                if (instance == null)
                {
                    Debug.LogError("GameConfig not found in Resources folder!");
                    instance = CreateInstance<GameConfig>();
                }
            }
            return instance;
        }
    }

    // Helper methods to get animal stats
    public AnimalStats GetAnimalStats(AnimalType type)
    {
        return System.Array.Find(animalBaseStats, stats => stats.type == type);
    }

    // Helper methods to get special abilities
    public SpecialAbility GetSpecialAbility(AnimalType type)
    {
        return System.Array.Find(specialAbilities, ability => ability.animalType == type);
    }

    // Helper method to check if a position is a water tile
    public bool IsWaterTile(Vector2Int position)
    {
        return System.Array.Exists(waterTilePositions, pos => pos == position);
    }

    // Helper method to get movement cost for a tile
    public float GetMovementCost(Vector2Int position)
    {
        return IsWaterTile(position) ? waterMovementPenalty : 1f;
    }

    // Helper method to get combat modifier for a position
    public float GetCombatModifier(Vector2Int position)
    {
        return IsWaterTile(position) ? waterCombatPenalty : 1f;
    }

    // Helper method to validate board position
    public bool IsValidBoardPosition(Vector2Int position)
    {
        return position.x >= 0 && position.x < boardWidth &&
               position.y >= 0 && position.y < boardHeight;
    }

    // Helper method to get starting positions for a player
    public Vector2Int[] GetStartingPositions(bool isPlayer1)
    {
        return isPlayer1 ? player1DefaultPositions : player2DefaultPositions;
    }

    // Helper method to calculate damage
    public float CalculateDamage(float baseAttack, bool isCritical, bool inWater)
    {
        float damage = baseAttack * baseDamage;
        if (isCritical) damage *= criticalHitMultiplier;
        if (inWater) damage *= waterCombatPenalty;
        return damage;
    }
}

using UnityEngine;
using System.Collections.Generic;

public class GameRules : MonoBehaviour
{
    public static GameRules Instance { get; private set; }

    [Header("Game Rules")]
    [SerializeField] private int maxTurns = 100;
    [SerializeField] private float turnTimeLimit = 60f;
    [SerializeField] private int minAnimalsToStart = 2;
    [SerializeField] private int maxAnimalsPerPlayer = 4;

    [Header("Combat Rules")]
    [SerializeField] private bool allowFriendlyFire = false;
    [SerializeField] private bool allowRevenge = true;
    [SerializeField] private float waterPenalty = 0.5f;

    private Dictionary<AnimalType, List<AnimalType>> strengthChart;
    private int currentTurn = 1;
    private float currentTurnTime;
    private bool isGameOver = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializeStrengthChart();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeStrengthChart()
    {
        strengthChart = new Dictionary<AnimalType, List<AnimalType>>
        {
            // Tiger is strong against most animals except Elephant
            { AnimalType.Tiger, new List<AnimalType> { 
                AnimalType.Lion, AnimalType.Wolf, AnimalType.Dog, AnimalType.Cat, AnimalType.Mouse 
            }},
            
            // Lion is strong against medium and small animals
            { AnimalType.Lion, new List<AnimalType> { 
                AnimalType.Wolf, AnimalType.Dog, AnimalType.Cat, AnimalType.Mouse 
            }},
            
            // Elephant is strong against large animals but weak against Mouse
            { AnimalType.Elephant, new List<AnimalType> { 
                AnimalType.Tiger, AnimalType.Lion, AnimalType.Wolf 
            }},
            
            // Mouse is only strong against Elephant (can sneak in and disturb)
            { AnimalType.Mouse, new List<AnimalType> { 
                AnimalType.Elephant 
            }},
            
            // Cat is strong against Mouse and can catch small animals
            { AnimalType.Cat, new List<AnimalType> { 
                AnimalType.Mouse 
            }},
            
            // Dog is strong against Cat and Mouse
            { AnimalType.Dog, new List<AnimalType> { 
                AnimalType.Cat, AnimalType.Mouse 
            }},
            
            // Wolf is strong against Dog and smaller animals
            { AnimalType.Wolf, new List<AnimalType> { 
                AnimalType.Dog, AnimalType.Cat, AnimalType.Mouse 
            }},
            
            // Fox is tricky and can outsmart some animals
            { AnimalType.Fox, new List<AnimalType> { 
                AnimalType.Cat, AnimalType.Mouse, AnimalType.Dog 
            }}
        };
    }

    public bool IsValidMove(Animal animal, Vector2Int from, Vector2Int to, bool isWaterTile)
    {
        // Check if it's the animal's turn
        if (!IsCorrectPlayerTurn(animal))
            return false;

        // Check if the move is within the animal's range
        if (!IsWithinMoveRange(animal, from, to))
            return false;

        // Special movement rules for different animals
        switch (animal.type)
        {
            case AnimalType.Mouse:
                // Mouse can move through water
                return true;
            
            case AnimalType.Tiger:
                // Tiger can jump over water
                if (isWaterTile && IsValidTigerJump(from, to))
                    return true;
                break;
            
            default:
                // Other animals cannot move through water
                if (isWaterTile)
                    return false;
                break;
        }

        return true;
    }

    private bool IsCorrectPlayerTurn(Animal animal)
    {
        // Check if it's the correct player's turn
        return GameManager.Instance.IsPlayer1Turn == (animal.gameObject.CompareTag("Player1"));
    }

    private bool IsWithinMoveRange(Animal animal, Vector2Int from, Vector2Int to)
    {
        int distance = Mathf.Abs(to.x - from.x) + Mathf.Abs(to.y - from.y);
        return distance <= animal.moveRange;
    }

    private bool IsValidTigerJump(Vector2Int from, Vector2Int to)
    {
        // Check if the jump is exactly 2 cells in any direction
        return (Mathf.Abs(to.x - from.x) == 2 && from.y == to.y) ||
               (Mathf.Abs(to.y - from.y) == 2 && from.x == to.x);
    }

    public bool CanAttack(Animal attacker, Animal defender)
    {
        // Check if friendly fire is allowed
        if (!allowFriendlyFire && IsSameTeam(attacker, defender))
            return false;

        // Check if the attacker can defeat the defender based on the strength chart
        if (strengthChart.TryGetValue(attacker.type, out List<AnimalType> strongAgainst))
        {
            return strongAgainst.Contains(defender.type);
        }

        return false;
    }

    private bool IsSameTeam(Animal a1, Animal a2)
    {
        return a1.gameObject.CompareTag(a2.gameObject.tag);
    }

    public float CalculateDamage(Animal attacker, Animal defender, bool isInWater)
    {
        float baseDamage = attacker.attackPower;

        // Apply water penalty if attacker is in water
        if (isInWater)
        {
            baseDamage *= waterPenalty;
        }

        // Check strength advantages
        if (strengthChart.TryGetValue(attacker.type, out List<AnimalType> strongAgainst))
        {
            if (strongAgainst.Contains(defender.type))
            {
                baseDamage *= 1.5f; // 50% bonus damage
            }
        }

        return baseDamage;
    }

    public void StartTurn()
    {
        currentTurnTime = turnTimeLimit;
        currentTurn++;

        // Check for game over conditions
        if (currentTurn > maxTurns)
        {
            EndGame("Draw - Max turns reached");
        }
    }

    public void UpdateTurn()
    {
        if (isGameOver) return;

        currentTurnTime -= Time.deltaTime;
        if (currentTurnTime <= 0)
        {
            // Time's up - switch turns
            GameManager.Instance.SwitchTurns();
        }
    }

    public bool CheckVictoryCondition()
    {
        // Count remaining animals for each player
        int player1Animals = GameObject.FindGameObjectsWithTag("Player1").Length;
        int player2Animals = GameObject.FindGameObjectsWithTag("Player2").Length;

        if (player1Animals == 0)
        {
            EndGame("Player 2 Wins!");
            return true;
        }
        else if (player2Animals == 0)
        {
            EndGame("Player 1 Wins!");
            return true;
        }

        return false;
    }

    private void EndGame(string result)
    {
        isGameOver = true;
        GameManager.Instance.EndGame(result);
    }

    public bool IsValidAnimalSelection(List<AnimalType> selectedAnimals)
    {
        if (selectedAnimals.Count < minAnimalsToStart)
            return false;

        if (selectedAnimals.Count > maxAnimalsPerPlayer)
            return false;

        return true;
    }

    public float GetTurnTimeRemaining()
    {
        return currentTurnTime;
    }

    public int GetCurrentTurn()
    {
        return currentTurn;
    }

    public bool IsGameOver()
    {
        return isGameOver;
    }
}

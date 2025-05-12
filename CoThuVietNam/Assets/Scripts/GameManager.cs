using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Components")]
    [SerializeField] private GameBoard gameBoard;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private AnimationController animationController;
    [SerializeField] private GameRules gameRules;

    [Header("Game Settings")]
    [SerializeField] private GameObject[] animalPrefabs;
    [SerializeField] private Vector2Int[] player1StartPositions;
    [SerializeField] private Vector2Int[] player2StartPositions;

    // Game state
    private GameState currentState;
    public bool IsPlayer1Turn { get; private set; } = true;
    public int Player1Score { get; private set; }
    public int Player2Score { get; private set; }
    private List<Animal> player1Animals = new List<Animal>();
    private List<Animal> player2Animals = new List<Animal>();
    private Animal selectedAnimal;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeGame();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Start with main menu
        currentState = GameState.MainMenu;
        uiManager.ShowMainMenu();
        AudioManager.Instance.PlayMenuMusic();
    }

    private void Update()
    {
        if (currentState == GameState.Playing)
        {
            gameRules.UpdateTurn();
        }
    }

    private void InitializeGame()
    {
        // Initialize components if not set
        if (gameBoard == null) gameBoard = FindObjectOfType<GameBoard>();
        if (uiManager == null) uiManager = FindObjectOfType<UIManager>();
        if (animationController == null) animationController = FindObjectOfType<AnimationController>();
        if (gameRules == null) gameRules = FindObjectOfType<GameRules>();
    }

    public void StartGame(List<AnimalType> selectedAnimals)
    {
        if (!gameRules.IsValidAnimalSelection(selectedAnimals))
        {
            Debug.LogError("Invalid animal selection!");
            return;
        }

        currentState = GameState.Playing;
        IsPlayer1Turn = true;
        Player1Score = 0;
        Player2Score = 0;

        // Clear existing animals
        ClearBoard();

        // Spawn selected animals
        SpawnAnimals(selectedAnimals);

        // Start first turn
        gameRules.StartTurn();
        AudioManager.Instance.PlayGameplayMusic();
        uiManager.ShowGameplay();
    }

    private void ClearBoard()
    {
        foreach (var animal in player1Animals)
        {
            if (animal != null) Destroy(animal.gameObject);
        }
        foreach (var animal in player2Animals)
        {
            if (animal != null) Destroy(animal.gameObject);
        }
        player1Animals.Clear();
        player2Animals.Clear();
    }

    private void SpawnAnimals(List<AnimalType> selectedTypes)
    {
        // Spawn Player 1 animals
        for (int i = 0; i < selectedTypes.Count && i < player1StartPositions.Length; i++)
        {
            SpawnAnimal(selectedTypes[i], player1StartPositions[i], true);
        }

        // Spawn Player 2 animals (mirror of Player 1's selection)
        for (int i = 0; i < selectedTypes.Count && i < player2StartPositions.Length; i++)
        {
            SpawnAnimal(selectedTypes[i], player2StartPositions[i], false);
        }
    }

    private void SpawnAnimal(AnimalType type, Vector2Int position, bool isPlayer1)
    {
        // Find the prefab for this animal type
        GameObject prefab = System.Array.Find(animalPrefabs, p => p.GetComponent<Animal>().type == type);
        if (prefab == null)
        {
            Debug.LogError($"No prefab found for animal type: {type}");
            return;
        }

        // Spawn the animal
        Vector3 worldPos = new Vector3(position.x, 0, position.y);
        GameObject animalObj = Instantiate(prefab, worldPos, Quaternion.identity);
        Animal animal = animalObj.GetComponent<Animal>();

        // Set up the animal
        animal.gameObject.tag = isPlayer1 ? "Player1" : "Player2";
        animationController.SetupAnimalAnimator(animal);

        // Add to appropriate list
        if (isPlayer1)
            player1Animals.Add(animal);
        else
            player2Animals.Add(animal);
    }

    public void SelectAnimal(Animal animal)
    {
        if (currentState != GameState.Playing) return;
        
        // Check if it's the correct player's turn
        if ((IsPlayer1Turn && !animal.gameObject.CompareTag("Player1")) ||
            (!IsPlayer1Turn && !animal.gameObject.CompareTag("Player2")))
            return;

        selectedAnimal = animal;
        gameBoard.SelectAnimal(animal);
        uiManager.ShowSkillPanel(animal);
        AudioManager.Instance.PlaySelectSound();
    }

    public void TryMove(Vector2Int targetPosition)
    {
        if (selectedAnimal == null || currentState != GameState.Playing) return;

        Vector2Int currentPos = gameBoard.GetAnimalPosition(selectedAnimal);
        if (gameRules.IsValidMove(selectedAnimal, currentPos, targetPosition, gameBoard.IsWaterTile(targetPosition)))
        {
            // Check if there's a target at the position
            Animal targetAnimal = gameBoard.GetAnimalAt(targetPosition);
            if (targetAnimal != null)
            {
                if (gameRules.CanAttack(selectedAnimal, targetAnimal))
                {
                    // Perform attack
                    PerformAttack(selectedAnimal, targetAnimal);
                }
            }
            else
            {
                // Perform movement
                PerformMove(selectedAnimal, targetPosition);
            }
        }
    }

    private void PerformMove(Animal animal, Vector2Int targetPosition)
    {
        Vector3 worldPos = new Vector3(targetPosition.x, 0, targetPosition.y);
        animationController.PlayMoveAnimation(animal, worldPos);
        gameBoard.MoveAnimal(animal, targetPosition);
        AudioManager.Instance.PlayMoveSound();

        // End turn after movement
        EndTurn();
    }

    private void PerformAttack(Animal attacker, Animal defender)
    {
        // Calculate damage
        float damage = gameRules.CalculateDamage(attacker, defender, gameBoard.IsWaterTile(gameBoard.GetAnimalPosition(attacker)));

        // Play attack animation
        animationController.PlayAttackAnimation(attacker, defender);
        AudioManager.Instance.PlayAttackSound();

        // Apply damage
        if (damage > 0)
        {
            // Remove defeated animal
            RemoveAnimal(defender);
            
            // Update score
            if (defender.CompareTag("Player1"))
                Player2Score++;
            else
                Player1Score++;
        }

        // End turn after attack
        EndTurn();
    }

    private void RemoveAnimal(Animal animal)
    {
        // Play death animation
        animationController.PlayDeathAnimation(animal);

        // Remove from lists
        if (animal.CompareTag("Player1"))
            player1Animals.Remove(animal);
        else
            player2Animals.Remove(animal);

        // Destroy after animation
        Destroy(animal.gameObject, 1f);

        // Check victory condition
        if (gameRules.CheckVictoryCondition())
        {
            EndGame(player1Animals.Count > 0 ? "Player 1" : "Player 2");
        }
    }

    public void OnSkillSelected(Animal animal, SkillData skill)
    {
        if (currentState != GameState.Playing || animal != selectedAnimal) return;

        // Execute skill
        skill.Execute(animal, null); // Target-less skill execution
        AudioManager.Instance.PlaySkillSound(skill.skillName);

        // End turn after skill use
        EndTurn();
    }

    public void SwitchTurns()
    {
        IsPlayer1Turn = !IsPlayer1Turn;
        selectedAnimal = null;
        gameBoard.ClearHighlights();
        uiManager.ShowSkillPanel(null);
        gameRules.StartTurn();
        uiManager.UpdateGameplayUI();
    }

    private void EndTurn()
    {
        selectedAnimal = null;
        gameBoard.ClearHighlights();
        uiManager.ShowSkillPanel(null);
        SwitchTurns();
    }

    public void EndGame(string winner)
    {
        currentState = GameState.GameOver;
        AudioManager.Instance.PlayVictoryMusic();
        uiManager.ShowGameOver(winner);
    }

    public void RestartGame()
    {
        StartGame(new List<AnimalType>()); // Will show character selection
    }

    public void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}

public enum GameState
{
    MainMenu,
    CharacterSelection,
    Playing,
    GameOver
}

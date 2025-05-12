using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject characterSelectionPanel;
    [SerializeField] private GameObject gameplayPanel;
    [SerializeField] private GameObject gameOverPanel;

    [Header("Character Selection")]
    [SerializeField] private Transform characterContainer;
    [SerializeField] private GameObject characterCardPrefab;
    [SerializeField] private Button startGameButton;

    [Header("Gameplay UI")]
    [SerializeField] private TextMeshProUGUI turnText;
    [SerializeField] private TextMeshProUGUI playerScoreText;
    [SerializeField] private GameObject skillPanel;
    [SerializeField] private Transform skillButtonContainer;
    [SerializeField] private GameObject skillButtonPrefab;

    [Header("Game Over")]
    [SerializeField] private TextMeshProUGUI winnerText;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;

    private List<AnimalType> selectedAnimals = new List<AnimalType>();
    private Dictionary<AnimalType, GameObject> characterCards = new Dictionary<AnimalType, GameObject>();

    private void Start()
    {
        InitializeUI();
        ShowMainMenu();
    }

    private void InitializeUI()
    {
        // Initialize character selection
        foreach (AnimalType type in System.Enum.GetValues(typeof(AnimalType)))
        {
            CreateCharacterCard(type);
        }

        // Button listeners
        startGameButton.onClick.AddListener(OnStartGameClick);
        restartButton.onClick.AddListener(OnRestartClick);
        mainMenuButton.onClick.AddListener(ShowMainMenu);

        // Initially disable start game button
        startGameButton.interactable = false;
    }

    private void CreateCharacterCard(AnimalType type)
    {
        GameObject card = Instantiate(characterCardPrefab, characterContainer);
        CharacterCard cardScript = card.GetComponent<CharacterCard>();
        cardScript.Initialize(type, OnCharacterSelected);
        characterCards[type] = card;
    }

    public void ShowMainMenu()
    {
        SetActivePanel(mainMenuPanel);
        selectedAnimals.Clear();
        UpdateStartGameButton();
    }

    public void ShowCharacterSelection()
    {
        SetActivePanel(characterSelectionPanel);
    }

    public void ShowGameplay()
    {
        SetActivePanel(gameplayPanel);
        UpdateGameplayUI();
    }

    public void ShowGameOver(string winner)
    {
        SetActivePanel(gameOverPanel);
        winnerText.text = $"{winner} Wins!";
    }

    private void SetActivePanel(GameObject panel)
    {
        mainMenuPanel.SetActive(panel == mainMenuPanel);
        characterSelectionPanel.SetActive(panel == characterSelectionPanel);
        gameplayPanel.SetActive(panel == gameplayPanel);
        gameOverPanel.SetActive(panel == gameOverPanel);
    }

    private void OnCharacterSelected(AnimalType type, bool selected)
    {
        if (selected)
        {
            if (!selectedAnimals.Contains(type))
            {
                selectedAnimals.Add(type);
            }
        }
        else
        {
            selectedAnimals.Remove(type);
        }

        UpdateStartGameButton();
    }

    private void UpdateStartGameButton()
    {
        startGameButton.interactable = selectedAnimals.Count >= 2;
    }

    private void OnStartGameClick()
    {
        GameManager.Instance.StartGame(selectedAnimals);
        ShowGameplay();
    }

    private void OnRestartClick()
    {
        ShowCharacterSelection();
    }

    public void UpdateGameplayUI()
    {
        // Update turn text
        turnText.text = $"Turn: Player {(GameManager.Instance.IsPlayer1Turn ? "1" : "2")}";

        // Update score
        playerScoreText.text = $"Score - P1: {GameManager.Instance.Player1Score} | P2: {GameManager.Instance.Player2Score}";
    }

    public void ShowSkillPanel(Animal selectedAnimal)
    {
        if (selectedAnimal == null)
        {
            skillPanel.SetActive(false);
            return;
        }

        skillPanel.SetActive(true);
        UpdateSkillButtons(selectedAnimal);
    }

    private void UpdateSkillButtons(Animal animal)
    {
        // Clear existing buttons
        foreach (Transform child in skillButtonContainer)
        {
            Destroy(child.gameObject);
        }

        // Create new skill buttons
        foreach (SkillData skill in animal.skills)
        {
            GameObject buttonObj = Instantiate(skillButtonPrefab, skillButtonContainer);
            SkillButton skillButton = buttonObj.GetComponent<SkillButton>();
            skillButton.Initialize(skill, () => OnSkillSelected(animal, skill));
        }
    }

    private void OnSkillSelected(Animal animal, SkillData skill)
    {
        // Notify GameManager or relevant system about skill selection
        GameManager.Instance.OnSkillSelected(animal, skill);
    }
}

// Helper class for character selection cards
public class CharacterCard : MonoBehaviour
{
    [SerializeField] private Image characterImage;
    [SerializeField] private TextMeshProUGUI characterName;
    [SerializeField] private TextMeshProUGUI characterDescription;
    [SerializeField] private Button selectButton;
    [SerializeField] private Image selectedOverlay;

    private AnimalType animalType;
    private System.Action<AnimalType, bool> onSelected;
    private bool isSelected;

    public void Initialize(AnimalType type, System.Action<AnimalType, bool> callback)
    {
        animalType = type;
        onSelected = callback;
        characterName.text = type.ToString();
        characterDescription.text = GetAnimalDescription(type);
        selectButton.onClick.AddListener(ToggleSelection);
        selectedOverlay.gameObject.SetActive(false);
    }

    private void ToggleSelection()
    {
        isSelected = !isSelected;
        selectedOverlay.gameObject.SetActive(isSelected);
        onSelected?.Invoke(animalType, isSelected);
    }

    private string GetAnimalDescription(AnimalType type)
    {
        switch (type)
        {
            case AnimalType.Tiger:
                return "Powerful jumper, can leap over water";
            case AnimalType.Lion:
                return "Strong attacker with high damage";
            case AnimalType.Elephant:
                return "Tank with high defense";
            case AnimalType.Mouse:
                return "Can move through water";
            case AnimalType.Cat:
                return "Quick and agile movements";
            case AnimalType.Dog:
                return "Loyal defender with pack bonus";
            case AnimalType.Wolf:
                return "Pack hunter with group tactics";
            case AnimalType.Fox:
                return "Tricky movement patterns";
            default:
                return "No description available";
        }
    }
}

// Helper class for skill buttons
public class SkillButton : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI skillName;
    [SerializeField] private TextMeshProUGUI cooldownText;
    [SerializeField] private Image cooldownOverlay;
    [SerializeField] private Button button;

    private SkillData skill;
    private System.Action onClick;

    public void Initialize(SkillData skillData, System.Action callback)
    {
        skill = skillData;
        onClick = callback;
        skillName.text = skill.skillName;
        button.onClick.AddListener(() => onClick?.Invoke());
        UpdateCooldown();
    }

    private void Update()
    {
        UpdateCooldown();
    }

    private void UpdateCooldown()
    {
        float progress = skill.GetCooldownProgress();
        cooldownOverlay.fillAmount = progress;
        cooldownText.text = progress > 0 ? $"{Mathf.Ceil(progress * skill.cooldown)}s" : "";
        button.interactable = progress <= 0;
    }
}

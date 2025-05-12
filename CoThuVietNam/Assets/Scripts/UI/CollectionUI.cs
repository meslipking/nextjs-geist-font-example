using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;

public class CollectionUI : MonoBehaviour
{
    [Header("Main Panel")]
    [SerializeField] private GameObject collectionPanel;
    [SerializeField] private Transform gridContainer;
    [SerializeField] private GameObject animalCardPrefab;
    [SerializeField] private Button closeButton;

    [Header("Filters")]
    [SerializeField] private TMP_Dropdown rarityFilter;
    [SerializeField] private TMP_Dropdown sortFilter;
    [SerializeField] private Toggle showLockedToggle;
    [SerializeField] private TMP_InputField searchField;

    [Header("Stats Panel")]
    [SerializeField] private GameObject statsPanel;
    [SerializeField] private Image animalPortrait;
    [SerializeField] private TextMeshProUGUI animalNameText;
    [SerializeField] private TextMeshProUGUI rarityText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI statsText;
    [SerializeField] private TextMeshProUGUI skillsText;
    [SerializeField] private Slider experienceSlider;
    [SerializeField] private TextMeshProUGUI experienceText;
    [SerializeField] private TextMeshProUGUI duplicateText;

    [Header("Collection Progress")]
    [SerializeField] private Slider progressBar;
    [SerializeField] private TextMeshProUGUI progressText;
    [SerializeField] private TextMeshProUGUI totalAnimalsText;

    [Header("Card Animation")]
    [SerializeField] private float cardScaleDuration = 0.2f;
    [SerializeField] private float cardHoverScale = 1.1f;

    private List<AnimalCollection.CollectedAnimal> displayedAnimals;
    private Dictionary<AnimalType, GameObject> animalCards;
    private AnimalCollection.CollectedAnimal selectedAnimal;

    private void Start()
    {
        InitializeUI();
    }

    private void InitializeUI()
    {
        animalCards = new Dictionary<AnimalType, GameObject>();

        // Set up button listeners
        closeButton.onClick.AddListener(Hide);
        
        // Set up filters
        InitializeFilters();
        
        // Set up search
        searchField.onValueChanged.AddListener(OnSearchChanged);
        
        // Subscribe to collection events
        AnimalCollection.Instance.OnAnimalCollected += OnAnimalCollected;
        AnimalCollection.Instance.OnAnimalEvolved += OnAnimalEvolved;
        AnimalCollection.Instance.OnAnimalLevelUp += OnAnimalLevelUp;
        AnimalCollection.Instance.OnCollectionUpdated += UpdateProgress;
    }

    private void InitializeFilters()
    {
        // Rarity filter options
        List<string> rarityOptions = new List<string> { "All" };
        rarityOptions.AddRange(System.Enum.GetNames(typeof(Rarity)));
        rarityFilter.ClearOptions();
        rarityFilter.AddOptions(rarityOptions);
        rarityFilter.onValueChanged.AddListener(OnFilterChanged);

        // Sort filter options
        sortFilter.ClearOptions();
        sortFilter.AddOptions(new List<string> {
            "Rarity",
            "Level",
            "Recent",
            "Name"
        });
        sortFilter.onValueChanged.AddListener(OnFilterChanged);

        // Locked toggle
        showLockedToggle.onValueChanged.AddListener(OnFilterChanged);
    }

    public void Show()
    {
        collectionPanel.SetActive(true);
        RefreshCollection();
        UpdateProgress(AnimalCollection.Instance.GetAllAnimals().Count);
    }

    public void Hide()
    {
        collectionPanel.SetActive(false);
        statsPanel.SetActive(false);
    }

    private void RefreshCollection()
    {
        // Clear existing cards
        foreach (var card in animalCards.Values)
        {
            Destroy(card);
        }
        animalCards.Clear();

        // Get filtered and sorted animals
        displayedAnimals = GetFilteredAnimals();
        
        // Create cards for each animal
        foreach (var animal in displayedAnimals)
        {
            CreateAnimalCard(animal);
        }
    }

    private List<AnimalCollection.CollectedAnimal> GetFilteredAnimals()
    {
        var animals = AnimalCollection.Instance.GetAllAnimals();

        // Apply rarity filter
        if (rarityFilter.value > 0)
        {
            Rarity selectedRarity = (Rarity)(rarityFilter.value - 1);
            animals = animals.Where(a => a.rarity == selectedRarity).ToList();
        }

        // Apply locked filter
        if (!showLockedToggle.isOn)
        {
            animals = animals.Where(a => !a.isLocked).ToList();
        }

        // Apply search filter
        if (!string.IsNullOrEmpty(searchField.text))
        {
            string search = searchField.text.ToLower();
            animals = animals.Where(a => a.type.ToString().ToLower().Contains(search)).ToList();
        }

        // Apply sorting
        switch (sortFilter.value)
        {
            case 0: // Rarity
                animals = animals.OrderByDescending(a => a.rarity)
                                .ThenBy(a => a.type.ToString())
                                .ToList();
                break;
            case 1: // Level
                animals = animals.OrderByDescending(a => a.level)
                                .ThenBy(a => a.type.ToString())
                                .ToList();
                break;
            case 2: // Recent
                animals = animals.OrderByDescending(a => a.acquisitionDate)
                                .ToList();
                break;
            case 3: // Name
                animals = animals.OrderBy(a => a.type.ToString())
                                .ToList();
                break;
        }

        return animals;
    }

    private void CreateAnimalCard(AnimalCollection.CollectedAnimal animal)
    {
        GameObject card = Instantiate(animalCardPrefab, gridContainer);
        animalCards[animal.type] = card;

        // Set up card UI
        Image portrait = card.GetComponentInChildren<Image>();
        TextMeshProUGUI nameText = card.GetComponentInChildren<TextMeshProUGUI>();
        TextMeshProUGUI levelText = card.transform.Find("LevelText").GetComponent<TextMeshProUGUI>();

        // Get animal data
        AnimalData data = AnimalDatabase.Instance.GetAnimalData(animal.type);
        portrait.sprite = data.icon;
        nameText.text = data.animalName;
        levelText.text = $"Lv.{animal.level}";

        // Set up rarity color
        Image background = card.GetComponent<Image>();
        background.color = GetRarityColor(animal.rarity);

        // Add hover effects
        AddCardHoverEffects(card.GetComponent<Button>(), animal);
    }

    private void AddCardHoverEffects(Button cardButton, AnimalCollection.CollectedAnimal animal)
    {
        cardButton.onClick.AddListener(() => ShowAnimalStats(animal));

        // Hover animations
        EventTrigger trigger = cardButton.gameObject.AddComponent<EventTrigger>();

        EventTrigger.Entry enterEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        enterEntry.callback.AddListener((data) => {
            cardButton.transform.DOScale(cardHoverScale, cardScaleDuration);
        });
        trigger.triggers.Add(enterEntry);

        EventTrigger.Entry exitEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        exitEntry.callback.AddListener((data) => {
            cardButton.transform.DOScale(1f, cardScaleDuration);
        });
        trigger.triggers.Add(exitEntry);
    }

    private void ShowAnimalStats(AnimalCollection.CollectedAnimal animal)
    {
        selectedAnimal = animal;
        statsPanel.SetActive(true);

        // Update stats panel
        AnimalData data = AnimalDatabase.Instance.GetAnimalData(animal.type);
        animalPortrait.sprite = data.icon;
        animalNameText.text = data.animalName;
        rarityText.text = animal.rarity.ToString();
        rarityText.color = GetRarityColor(animal.rarity);
        levelText.text = $"Level {animal.level}";

        // Stats
        statsText.text = $"HP: {animal.stats.health:N0}\n" +
                        $"Attack: {animal.stats.attack:N0}\n" +
                        $"Defense: {animal.stats.defense:N0}\n" +
                        $"Crit Rate: {animal.stats.criticalChance:P0}\n" +
                        $"Crit Damage: {animal.stats.criticalDamage:P0}";

        // Skills
        skillsText.text = "Skills:\n";
        foreach (var skill in animal.unlockedSkills)
        {
            skillsText.text += $"• {skill.skillName}: {skill.description}\n";
        }

        // Experience
        float nextLevelExp = AnimalCollection.Instance.GetExperienceForNextLevel(animal.level);
        experienceSlider.value = animal.experience / nextLevelExp;
        experienceText.text = $"{animal.experience:N0} / {nextLevelExp:N0}";

        // Duplicates
        duplicateText.text = $"Duplicates: {animal.duplicateCount}";
    }

    private void OnFilterChanged(int _)
    {
        RefreshCollection();
    }

    private void OnFilterChanged(bool _)
    {
        RefreshCollection();
    }

    private void OnSearchChanged(string _)
    {
        RefreshCollection();
    }

    private void UpdateProgress(int totalAnimals)
    {
        float progress = AnimalCollection.Instance.GetCollectionProgress();
        progressBar.value = progress;
        progressText.text = $"{(progress * 100):N0}%";
        totalAnimalsText.text = $"Total Animals: {totalAnimals}";
    }

    private void OnAnimalCollected(AnimalCollection.CollectedAnimal animal)
    {
        RefreshCollection();
        ShowAnimalStats(animal);
    }

    private void OnAnimalEvolved(AnimalCollection.CollectedAnimal animal)
    {
        if (animalCards.TryGetValue(animal.type, out GameObject card))
        {
            // Update card visuals
            Image background = card.GetComponent<Image>();
            background.color = GetRarityColor(animal.rarity);

            // Update level text
            TextMeshProUGUI levelText = card.transform.Find("LevelText").GetComponent<TextMeshProUGUI>();
            levelText.text = $"Lv.{animal.level}";
        }

        if (selectedAnimal == animal)
        {
            ShowAnimalStats(animal);
        }
    }

    private void OnAnimalLevelUp(AnimalCollection.CollectedAnimal animal)
    {
        if (animalCards.TryGetValue(animal.type, out GameObject card))
        {
            // Update level text
            TextMeshProUGUI levelText = card.transform.Find("LevelText").GetComponent<TextMeshProUGUI>();
            levelText.text = $"Lv.{animal.level}";

            // Play level up effect
            PlayLevelUpEffect(card);
        }

        if (selectedAnimal == animal)
        {
            ShowAnimalStats(animal);
        }
    }

    private void PlayLevelUpEffect(GameObject card)
    {
        // Scale animation
        Sequence levelUpSequence = DOTween.Sequence();
        levelUpSequence.Append(card.transform.DOScale(1.2f, 0.2f));
        levelUpSequence.Append(card.transform.DOScale(1f, 0.2f));

        // Particle effect
        VFXManager.Instance.PlayEffect("LevelUp", card.transform.position);

        // Sound effect
        AudioManager.Instance.PlaySound("LevelUp");
    }

    private Color GetRarityColor(Rarity rarity)
    {
        return rarity switch
        {
            Rarity.N => Color.white,
            Rarity.R => Color.blue,
            Rarity.SR => Color.magenta,
            Rarity.SSR => Color.yellow,
            Rarity.SSS => Color.red,
            Rarity.SSSPlus => new Color(1f, 0f, 1f),
            _ => Color.white
        };
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        if (AnimalCollection.Instance != null)
        {
            AnimalCollection.Instance.OnAnimalCollected -= OnAnimalCollected;
            AnimalCollection.Instance.OnAnimalEvolved -= OnAnimalEvolved;
            AnimalCollection.Instance.OnAnimalLevelUp -= OnAnimalLevelUp;
            AnimalCollection.Instance.OnCollectionUpdated -= UpdateProgress;
        }
    }
}

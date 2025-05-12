using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class HabitatUI : MonoBehaviour
{
    [Header("Main Panel")]
    [SerializeField] private GameObject habitatPanel;
    [SerializeField] private Button closeButton;

    [Header("Habitat Sections")]
    [SerializeField] private Transform skySection;
    [SerializeField] private Transform landSection;
    [SerializeField] private Transform seaSection;
    [SerializeField] private Transform hybridSection;

    [Header("UI Elements")]
    [SerializeField] private GameObject animalCardPrefab;
    [SerializeField] private Image habitatAdvantageChart;
    [SerializeField] private TextMeshProUGUI habitatDescriptionText;

    [Header("Tooltips")]
    [SerializeField] private GameObject tooltipPanel;
    [SerializeField] private TextMeshProUGUI tooltipText;

    [Header("Visual Elements")]
    [SerializeField] private Sprite skyIcon;
    [SerializeField] private Sprite landIcon;
    [SerializeField] private Sprite seaIcon;
    [SerializeField] private Color skyColor = new Color(0.5f, 0.8f, 1f);
    [SerializeField] private Color landColor = new Color(0.6f, 0.8f, 0.3f);
    [SerializeField] private Color seaColor = new Color(0.3f, 0.5f, 0.9f);

    private Dictionary<HabitatSystem.Habitat, Transform> habitatSections;
    private Dictionary<AnimalType, GameObject> animalCards;

    private void Start()
    {
        InitializeUI();
    }

    private void InitializeUI()
    {
        closeButton.onClick.AddListener(Hide);

        habitatSections = new Dictionary<HabitatSystem.Habitat, Transform>
        {
            { HabitatSystem.Habitat.Sky, skySection },
            { HabitatSystem.Habitat.Land, landSection },
            { HabitatSystem.Habitat.Sea, seaSection },
            { HabitatSystem.Habitat.Hybrid, hybridSection }
        };

        animalCards = new Dictionary<AnimalType, GameObject>();

        // Set up habitat description
        UpdateHabitatDescription();
    }

    public void Show()
    {
        habitatPanel.SetActive(true);
        RefreshHabitatDisplay();
    }

    public void Hide()
    {
        habitatPanel.SetActive(false);
        tooltipPanel.SetActive(false);
    }

    private void RefreshHabitatDisplay()
    {
        // Clear existing cards
        foreach (var card in animalCards.Values)
        {
            Destroy(card);
        }
        animalCards.Clear();

        // Populate each habitat section
        foreach (HabitatSystem.Habitat habitat in System.Enum.GetValues(typeof(HabitatSystem.Habitat)))
        {
            PopulateHabitatSection(habitat);
        }
    }

    private void PopulateHabitatSection(HabitatSystem.Habitat habitat)
    {
        List<AnimalType> animals = HabitatSystem.Instance.GetAnimalsInHabitat(habitat);
        Transform section = habitatSections[habitat];

        foreach (var animalType in animals)
        {
            CreateAnimalCard(animalType, section, habitat);
        }
    }

    private void CreateAnimalCard(AnimalType animalType, Transform parent, HabitatSystem.Habitat habitat)
    {
        GameObject card = Instantiate(animalCardPrefab, parent);
        animalCards[animalType] = card;

        // Set up card visuals
        Image portrait = card.GetComponentInChildren<Image>();
        TextMeshProUGUI nameText = card.GetComponentInChildren<TextMeshProUGUI>();
        Image habitatIcon = card.transform.Find("HabitatIcon").GetComponent<Image>();

        // Get animal data
        AnimalData data = AnimalDatabase.Instance.GetAnimalData(animalType);
        portrait.sprite = data.icon;
        nameText.text = data.animalName;

        // Set habitat icon and color
        SetHabitatVisuals(habitatIcon, habitat);

        // Add hover effects and tooltip
        AddCardInteraction(card, animalType);
    }

    private void SetHabitatVisuals(Image icon, HabitatSystem.Habitat habitat)
    {
        switch (habitat)
        {
            case HabitatSystem.Habitat.Sky:
                icon.sprite = skyIcon;
                icon.color = skyColor;
                break;
            case HabitatSystem.Habitat.Land:
                icon.sprite = landIcon;
                icon.color = landColor;
                break;
            case HabitatSystem.Habitat.Sea:
                icon.sprite = seaIcon;
                icon.color = seaColor;
                break;
            case HabitatSystem.Habitat.Hybrid:
                icon.sprite = landIcon; // Default icon
                icon.color = Color.gray;
                break;
        }
    }

    private void AddCardInteraction(GameObject card, AnimalType animalType)
    {
        EventTrigger trigger = card.AddComponent<EventTrigger>();

        // Mouse enter
        EventTrigger.Entry enterEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        enterEntry.callback.AddListener((data) => {
            ShowTooltip(animalType, card.transform.position);
        });
        trigger.triggers.Add(enterEntry);

        // Mouse exit
        EventTrigger.Entry exitEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        exitEntry.callback.AddListener((data) => {
            HideTooltip();
        });
        trigger.triggers.Add(exitEntry);

        // Click
        Button button = card.GetComponent<Button>();
        button.onClick.AddListener(() => {
            ShowAnimalDetails(animalType);
        });
    }

    private void ShowTooltip(AnimalType animalType, Vector3 position)
    {
        string description = HabitatSystem.Instance.GetHabitatDescription(animalType);
        tooltipText.text = description;
        tooltipPanel.transform.position = position + Vector3.up * 50f;
        tooltipPanel.SetActive(true);
    }

    private void HideTooltip()
    {
        tooltipPanel.SetActive(false);
    }

    private void ShowAnimalDetails(AnimalType animalType)
    {
        // Get habitat information
        string habitatInfo = HabitatSystem.Instance.GetHabitatDescription(animalType);
        
        // Get advantages
        List<AnimalType> strongAgainst = CounterSystem.Instance.GetStrongAgainst(animalType);
        List<AnimalType> weakAgainst = CounterSystem.Instance.GetWeakAgainst(animalType);

        // Update description text
        string details = $"<b>{animalType}</b>\n\n";
        details += $"Habitat Information:\n{habitatInfo}\n\n";
        details += "Strong Against:\n";
        details += string.Join(", ", strongAgainst) + "\n\n";
        details += "Weak Against:\n";
        details += string.Join(", ", weakAgainst);

        habitatDescriptionText.text = details;
    }

    private void UpdateHabitatDescription()
    {
        string description = "Habitat Advantages:\n\n" +
                           "• Sky creatures have advantage over Land creatures\n" +
                           "• Land creatures have advantage over Sea creatures\n" +
                           "• Sea creatures have advantage over Sky creatures\n\n" +
                           "Advantages grant:\n" +
                           "• 30% increased damage\n" +
                           "• 20% increased movement\n" +
                           "• Special terrain bonuses";

        habitatDescriptionText.text = description;
    }

    private void OnDestroy()
    {
        // Clean up
        foreach (var card in animalCards.Values)
        {
            if (card != null)
                Destroy(card);
        }
    }
}

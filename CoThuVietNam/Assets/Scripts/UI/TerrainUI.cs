using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class TerrainUI : MonoBehaviour
{
    [Header("Main Panel")]
    [SerializeField] private GameObject terrainPanel;
    [SerializeField] private Button closeButton;

    [Header("Terrain Display")]
    [SerializeField] private Transform terrainGridContainer;
    [SerializeField] private GameObject terrainTilePrefab;
    [SerializeField] private float tileSize = 60f;
    [SerializeField] private int gridSize = 8;

    [Header("Terrain Info")]
    [SerializeField] private GameObject infoPanel;
    [SerializeField] private Image terrainIcon;
    [SerializeField] private TextMeshProUGUI terrainNameText;
    [SerializeField] private TextMeshProUGUI terrainEffectsText;
    [SerializeField] private TextMeshProUGUI specialInteractionsText;

    [Header("Terrain Icons")]
    [SerializeField] private Sprite plainSprite;
    [SerializeField] private Sprite mountainSprite;
    [SerializeField] private Sprite waterSprite;
    [SerializeField] private Sprite forestSprite;
    [SerializeField] private Sprite desertSprite;
    [SerializeField] private Sprite swampSprite;

    [Header("Terrain Colors")]
    [SerializeField] private Color plainColor = new Color(0.7f, 0.9f, 0.5f);
    [SerializeField] private Color mountainColor = new Color(0.6f, 0.6f, 0.6f);
    [SerializeField] private Color waterColor = new Color(0.4f, 0.6f, 1f);
    [SerializeField] private Color forestColor = new Color(0.3f, 0.7f, 0.3f);
    [SerializeField] private Color desertColor = new Color(1f, 0.9f, 0.6f);
    [SerializeField] private Color swampColor = new Color(0.5f, 0.5f, 0.3f);

    private Dictionary<TerrainSystem.TerrainType, Sprite> terrainSprites;
    private Dictionary<TerrainSystem.TerrainType, Color> terrainColors;
    private TerrainSystem.TerrainType[,] demoGrid;

    private void Start()
    {
        InitializeUI();
    }

    private void InitializeUI()
    {
        closeButton.onClick.AddListener(Hide);

        InitializeTerrainMappings();
        GenerateDemoGrid();
        CreateTerrainGrid();
    }

    private void InitializeTerrainMappings()
    {
        terrainSprites = new Dictionary<TerrainSystem.TerrainType, Sprite>
        {
            { TerrainSystem.TerrainType.Plain, plainSprite },
            { TerrainSystem.TerrainType.Mountain, mountainSprite },
            { TerrainSystem.TerrainType.Water, waterSprite },
            { TerrainSystem.TerrainType.Forest, forestSprite },
            { TerrainSystem.TerrainType.Desert, desertSprite },
            { TerrainSystem.TerrainType.Swamp, swampSprite }
        };

        terrainColors = new Dictionary<TerrainSystem.TerrainType, Color>
        {
            { TerrainSystem.TerrainType.Plain, plainColor },
            { TerrainSystem.TerrainType.Mountain, mountainColor },
            { TerrainSystem.TerrainType.Water, waterColor },
            { TerrainSystem.TerrainType.Forest, forestColor },
            { TerrainSystem.TerrainType.Desert, desertColor },
            { TerrainSystem.TerrainType.Swamp, swampColor }
        };
    }

    private void GenerateDemoGrid()
    {
        demoGrid = new TerrainSystem.TerrainType[gridSize, gridSize];
        
        // Create an interesting demo layout
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                // Create a varied terrain pattern
                if (x == 0 || x == gridSize - 1 || y == 0 || y == gridSize - 1)
                    demoGrid[x, y] = TerrainSystem.TerrainType.Mountain;
                else if ((x + y) % 3 == 0)
                    demoGrid[x, y] = TerrainSystem.TerrainType.Forest;
                else if ((x + y) % 4 == 0)
                    demoGrid[x, y] = TerrainSystem.TerrainType.Water;
                else if ((x + y) % 5 == 0)
                    demoGrid[x, y] = TerrainSystem.TerrainType.Desert;
                else if ((x + y) % 6 == 0)
                    demoGrid[x, y] = TerrainSystem.TerrainType.Swamp;
                else
                    demoGrid[x, y] = TerrainSystem.TerrainType.Plain;
            }
        }
    }

    private void CreateTerrainGrid()
    {
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                CreateTerrainTile(x, y, demoGrid[x, y]);
            }
        }
    }

    private void CreateTerrainTile(int x, int y, TerrainSystem.TerrainType terrainType)
    {
        GameObject tile = Instantiate(terrainTilePrefab, terrainGridContainer);
        RectTransform rectTransform = tile.GetComponent<RectTransform>();
        
        // Position the tile
        rectTransform.anchoredPosition = new Vector2(x * tileSize, y * tileSize);
        rectTransform.sizeDelta = new Vector2(tileSize, tileSize);

        // Set up visuals
        Image tileImage = tile.GetComponent<Image>();
        tileImage.sprite = terrainSprites[terrainType];
        tileImage.color = terrainColors[terrainType];

        // Add click handler
        Button tileButton = tile.GetComponent<Button>();
        tileButton.onClick.AddListener(() => ShowTerrainInfo(terrainType));

        // Add hover effect
        EventTrigger trigger = tile.AddComponent<EventTrigger>();
        
        EventTrigger.Entry enterEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        enterEntry.callback.AddListener((data) => {
            tileImage.color = Color.Lerp(terrainColors[terrainType], Color.white, 0.3f);
        });
        trigger.triggers.Add(enterEntry);

        EventTrigger.Entry exitEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        exitEntry.callback.AddListener((data) => {
            tileImage.color = terrainColors[terrainType];
        });
        trigger.triggers.Add(exitEntry);
    }

    private void ShowTerrainInfo(TerrainSystem.TerrainType terrainType)
    {
        infoPanel.SetActive(true);
        
        // Update terrain icon and name
        terrainIcon.sprite = terrainSprites[terrainType];
        terrainIcon.color = terrainColors[terrainType];
        terrainNameText.text = terrainType.ToString();

        // Get terrain effects
        var effect = TerrainSystem.Instance.GetTerrainEffect(terrainType);
        terrainEffectsText.text = FormatTerrainEffects(effect);

        // Get special interactions
        List<string> interactions = new List<string>();
        foreach (AnimalType animalType in System.Enum.GetValues(typeof(AnimalType)))
        {
            string interaction = TerrainSystem.Instance.GetTerrainEffect(null, terrainType);
            if (!string.IsNullOrEmpty(interaction) && interaction != "No special effect")
            {
                interactions.Add(interaction);
            }
        }

        specialInteractionsText.text = "Special Interactions:\n" + string.Join("\n", interactions);
    }

    private string FormatTerrainEffects(TerrainSystem.TerrainEffect effect)
    {
        string effects = "Terrain Effects:\n\n";
        
        effects += $"Movement: {(effect.movementModifier >= 1f ? "Normal" : $"{effect.movementModifier:P0} speed")}\n";
        
        if (effect.attackBonus != 0)
            effects += $"Attack: {(effect.attackBonus > 0 ? "+" : "")}{effect.attackBonus:P0}\n";
        
        if (effect.defenseBonus != 0)
            effects += $"Defense: {(effect.defenseBonus > 0 ? "+" : "")}{effect.defenseBonus:P0}\n";
        
        if (effect.blocksLineOfSight)
            effects += "Blocks Line of Sight\n";
        
        if (effect.requiresSpecialMovement)
            effects += "Requires Special Movement\n";

        return effects;
    }

    public void Show()
    {
        terrainPanel.SetActive(true);
        infoPanel.SetActive(false);
    }

    public void Hide()
    {
        terrainPanel.SetActive(false);
        infoPanel.SetActive(false);
    }

    private void OnDestroy()
    {
        // Clean up
        foreach (Transform child in terrainGridContainer)
        {
            Destroy(child.gameObject);
        }
    }
}

using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class SceneInitializer : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private PrefabConfig prefabConfig;
    [SerializeField] private GameConfig gameConfig;
    
    [Header("Loading Settings")]
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private float minimumLoadTime = 1f;
    [SerializeField] private bool autoInitialize = true;

    private SceneSetup sceneSetup;
    private GameHierarchyBuilder hierarchyBuilder;

    private void Start()
    {
        if (autoInitialize)
            StartCoroutine(InitializeScene());
    }

    public IEnumerator InitializeScene()
    {
        if (loadingScreen != null)
            loadingScreen.SetActive(true);

        float startTime = Time.time;

        // Step 1: Validate configurations
        if (!ValidateConfigurations())
        {
            Debug.LogError("Configuration validation failed!");
            yield break;
        }

        // Step 2: Create scene hierarchy
        yield return CreateSceneHierarchy();

        // Step 3: Initialize core systems
        yield return InitializeCoreSystems();

        // Step 4: Load and instantiate prefabs
        yield return InstantiatePrefabs();

        // Step 5: Initialize game state
        yield return InitializeGameState();

        // Ensure minimum loading time
        float elapsedTime = Time.time - startTime;
        if (elapsedTime < minimumLoadTime)
            yield return new WaitForSeconds(minimumLoadTime - elapsedTime);

        // Complete initialization
        if (loadingScreen != null)
            loadingScreen.SetActive(false);

        OnInitializationComplete();
    }

    private bool ValidateConfigurations()
    {
        if (prefabConfig == null)
        {
            Debug.LogError("PrefabConfig not assigned!");
            return false;
        }

        if (gameConfig == null)
        {
            Debug.LogError("GameConfig not assigned!");
            return false;
        }

        prefabConfig.ValidateConfig();
        return true;
    }

    private IEnumerator CreateSceneHierarchy()
    {
        // Create scene setup
        GameObject setupObj = new GameObject("SceneSetup");
        sceneSetup = setupObj.AddComponent<SceneSetup>();
        yield return null;

        // Get hierarchy builder reference
        hierarchyBuilder = FindObjectOfType<GameHierarchyBuilder>();
        if (hierarchyBuilder == null)
        {
            Debug.LogError("GameHierarchyBuilder not found after scene setup!");
            yield break;
        }

        // Apply scene settings
        prefabConfig.ApplySceneSettings();
        yield return null;
    }

    private IEnumerator InitializeCoreSystems()
    {
        // Initialize managers
        InstantiateManagerPrefabs(prefabConfig.managers);
        yield return null;

        // Initialize systems
        InstantiateSystemPrefabs(prefabConfig.systems);
        yield return null;

        // Initialize UI
        InstantiateUIPrefabs(prefabConfig.ui);
        yield return null;
    }

    private IEnumerator InstantiatePrefabs()
    {
        // Instantiate sky domain animals
        foreach (var animalSet in prefabConfig.skyAnimals)
        {
            InstantiateAnimal(animalSet, hierarchyBuilder.skyParent);
            yield return null;
        }

        // Instantiate land domain animals
        foreach (var animalSet in prefabConfig.landAnimals)
        {
            InstantiateAnimal(animalSet, hierarchyBuilder.landParent);
            yield return null;
        }

        // Instantiate sea domain animals
        foreach (var animalSet in prefabConfig.seaAnimals)
        {
            InstantiateAnimal(animalSet, hierarchyBuilder.seaParent);
            yield return null;
        }
    }

    private void InstantiateManagerPrefabs(PrefabConfig.ManagerPrefabs managers)
    {
        if (hierarchyBuilder.managersParent == null) return;

        InstantiatePrefab(managers.gameManager, "GameManager");
        InstantiatePrefab(managers.uiManager, "UIManager");
        InstantiatePrefab(managers.audioManager, "AudioManager");
        InstantiatePrefab(managers.vfxManager, "VFXManager");
        InstantiatePrefab(managers.inputManager, "InputManager");
        InstantiatePrefab(managers.networkManager, "NetworkManager");
        InstantiatePrefab(managers.saveSystem, "SaveSystem");
        InstantiatePrefab(managers.resourceManager, "ResourceManager");
        InstantiatePrefab(managers.tutorialManager, "TutorialManager");
    }

    private void InstantiateSystemPrefabs(PrefabConfig.SystemPrefabs systems)
    {
        if (hierarchyBuilder.managersParent == null) return;

        InstantiatePrefab(systems.terrainSystem, "TerrainSystem");
        InstantiatePrefab(systems.habitatSystem, "HabitatSystem");
        InstantiatePrefab(systems.counterSystem, "CounterSystem");
        InstantiatePrefab(systems.gameBalance, "GameBalance");
        InstantiatePrefab(systems.evolutionSystem, "EvolutionSystem");
        InstantiatePrefab(systems.summoningSystem, "SummoningSystem");
        InstantiatePrefab(systems.collectionSystem, "CollectionSystem");
    }

    private void InstantiateUIPrefabs(PrefabConfig.UIPrefabs ui)
    {
        GameObject uiRoot = GameObject.Find("UISpace");
        if (uiRoot == null) return;

        InstantiatePrefab(ui.mainMenu, "MainMenu", uiRoot.transform);
        InstantiatePrefab(ui.gameHUD, "GameHUD", uiRoot.transform);
        InstantiatePrefab(ui.summoningUI, "SummoningUI", uiRoot.transform);
        InstantiatePrefab(ui.evolutionUI, "EvolutionUI", uiRoot.transform);
        InstantiatePrefab(ui.collectionUI, "CollectionUI", uiRoot.transform);
        InstantiatePrefab(ui.habitatUI, "HabitatUI", uiRoot.transform);
        InstantiatePrefab(ui.terrainUI, "TerrainUI", uiRoot.transform);
        InstantiatePrefab(ui.popupSystem, "PopupSystem", uiRoot.transform);
    }

    private void InstantiateAnimal(PrefabConfig.AnimalPrefabSet animalSet, Transform parent)
    {
        if (animalSet.prefab == null || parent == null) return;

        GameObject instance = Instantiate(animalSet.prefab, parent);
        instance.name = animalSet.type.ToString();

        // Set up components
        Animal animal = instance.GetComponent<Animal>();
        if (animal != null)
        {
            animal.type = animalSet.type;
        }

        Animator animator = instance.GetComponent<Animator>();
        if (animator != null && animalSet.animator != null)
        {
            animator.runtimeAnimatorController = animalSet.animator;
        }
    }

    private GameObject InstantiatePrefab(GameObject prefab, string name, Transform parent = null)
    {
        if (prefab == null) return null;

        GameObject instance = Instantiate(prefab, parent ?? hierarchyBuilder.managersParent);
        instance.name = name;
        return instance;
    }

    private IEnumerator InitializeGameState()
    {
        // Initialize game state from GameConfig
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            gameManager.Initialize(gameConfig);
        }

        yield return null;
    }

    private void OnInitializationComplete()
    {
        // Notify systems that initialization is complete
        GameManager.Instance?.OnSceneInitializationComplete();
        UIManager.Instance?.OnSceneInitializationComplete();
        
        // Enable input
        InputManager.Instance?.EnableInput();
    }
}

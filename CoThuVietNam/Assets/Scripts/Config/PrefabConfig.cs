using UnityEngine;

[CreateAssetMenu(fileName = "PrefabConfig", menuName = "CoThuVietNam/Prefab Configuration")]
public class PrefabConfig : ScriptableObject
{
    [System.Serializable]
    public class ManagerPrefabs
    {
        public GameObject gameManager;
        public GameObject uiManager;
        public GameObject audioManager;
        public GameObject vfxManager;
        public GameObject inputManager;
        public GameObject networkManager;
        public GameObject saveSystem;
        public GameObject resourceManager;
        public GameObject tutorialManager;
    }

    [System.Serializable]
    public class SystemPrefabs
    {
        public GameObject terrainSystem;
        public GameObject habitatSystem;
        public GameObject counterSystem;
        public GameObject gameBalance;
        public GameObject evolutionSystem;
        public GameObject summoningSystem;
        public GameObject collectionSystem;
    }

    [System.Serializable]
    public class UIPrefabs
    {
        public GameObject mainMenu;
        public GameObject gameHUD;
        public GameObject summoningUI;
        public GameObject evolutionUI;
        public GameObject collectionUI;
        public GameObject habitatUI;
        public GameObject terrainUI;
        public GameObject popupSystem;
    }

    [System.Serializable]
    public class AnimalPrefabSet
    {
        public AnimalType type;
        public GameObject prefab;
        public RuntimeAnimatorController animator;
        public ParticleSystem[] effects;
        public AudioClip[] sounds;
    }

    [Header("Core Systems")]
    public ManagerPrefabs managers;
    public SystemPrefabs systems;
    public UIPrefabs ui;

    [Header("Animal Prefabs")]
    public AnimalPrefabSet[] skyAnimals;
    public AnimalPrefabSet[] landAnimals;
    public AnimalPrefabSet[] seaAnimals;

    [Header("Common Prefabs")]
    public GameObject boardTilePrefab;
    public GameObject effectsPrefab;
    public GameObject soundEmitterPrefab;

    [Header("Scene Settings")]
    public Material skyboxMaterial;
    public Color ambientLight = Color.white;
    public float ambientIntensity = 1f;

    public AnimalPrefabSet GetAnimalPrefabSet(AnimalType type)
    {
        // Search in all habitat arrays
        foreach (var animal in skyAnimals)
            if (animal.type == type) return animal;
        foreach (var animal in landAnimals)
            if (animal.type == type) return animal;
        foreach (var animal in seaAnimals)
            if (animal.type == type) return animal;
        
        return null;
    }

    public void ApplySceneSettings()
    {
        // Apply skybox
        if (skyboxMaterial != null)
            RenderSettings.skybox = skyboxMaterial;

        // Apply lighting settings
        RenderSettings.ambientLight = ambientLight;
        RenderSettings.ambientIntensity = ambientIntensity;
    }

    public void ValidateConfig()
    {
        // Validate manager prefabs
        ValidateRequiredPrefab(managers.gameManager, "GameManager");
        ValidateRequiredPrefab(managers.uiManager, "UIManager");
        ValidateRequiredPrefab(managers.audioManager, "AudioManager");
        ValidateRequiredPrefab(managers.vfxManager, "VFXManager");

        // Validate system prefabs
        ValidateRequiredPrefab(systems.terrainSystem, "TerrainSystem");
        ValidateRequiredPrefab(systems.habitatSystem, "HabitatSystem");
        ValidateRequiredPrefab(systems.counterSystem, "CounterSystem");

        // Validate UI prefabs
        ValidateRequiredPrefab(ui.mainMenu, "MainMenu");
        ValidateRequiredPrefab(ui.gameHUD, "GameHUD");

        // Validate common prefabs
        ValidateRequiredPrefab(boardTilePrefab, "BoardTile");
        ValidateRequiredPrefab(effectsPrefab, "Effects");
        ValidateRequiredPrefab(soundEmitterPrefab, "SoundEmitter");

        // Validate animal prefabs
        ValidateAnimalPrefabSets(skyAnimals, "Sky");
        ValidateAnimalPrefabSets(landAnimals, "Land");
        ValidateAnimalPrefabSets(seaAnimals, "Sea");
    }

    private void ValidateRequiredPrefab(GameObject prefab, string name)
    {
        if (prefab == null)
            Debug.LogError($"Missing required prefab: {name}");
    }

    private void ValidateAnimalPrefabSets(AnimalPrefabSet[] sets, string domain)
    {
        if (sets == null || sets.Length == 0)
        {
            Debug.LogError($"No animal prefabs defined for {domain} domain");
            return;
        }

        foreach (var set in sets)
        {
            if (set.prefab == null)
                Debug.LogError($"Missing prefab for {set.type} in {domain} domain");
            if (set.animator == null)
                Debug.LogError($"Missing animator for {set.type} in {domain} domain");
        }
    }

    #if UNITY_EDITOR
    private void OnValidate()
    {
        ValidateConfig();
    }
    #endif
}

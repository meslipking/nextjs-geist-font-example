using UnityEngine;
using System.Collections.Generic;

public class GameHierarchyBuilder : MonoBehaviour
{
    [Header("Parents")]
    public Transform skyParent;
    public Transform landParent;
    public Transform seaParent;
    public Transform managersParent;

    [Header("Prefabs")]
    public GameObject uiManagerPrefab;
    public GameObject gameManagerPrefab;
    public GameObject audioManagerPrefab;
    public GameObject vfxManagerPrefab;
    public GameObject terrainSystemPrefab;
    public GameObject habitatSystemPrefab;
    public GameObject counterSystemPrefab;
    public GameObject gameBalancePrefab;

    private Dictionary<HabitatSystem.Habitat, Transform> habitatParents;

    private void Awake()
    {
        BuildHierarchy();
    }

    public void BuildHierarchy()
    {
        // Create parent dictionary
        habitatParents = new Dictionary<HabitatSystem.Habitat, Transform>
        {
            { HabitatSystem.Habitat.Sky, skyParent },
            { HabitatSystem.Habitat.Land, landParent },
            { HabitatSystem.Habitat.Sea, seaParent }
        };

        // Instantiate core managers under managersParent
        InstantiateManager(uiManagerPrefab, "UIManager");
        InstantiateManager(gameManagerPrefab, "GameManager");
        InstantiateManager(audioManagerPrefab, "AudioManager");
        InstantiateManager(vfxManagerPrefab, "VFXManager");
        InstantiateManager(terrainSystemPrefab, "TerrainSystem");
        InstantiateManager(habitatSystemPrefab, "HabitatSystem");
        InstantiateManager(counterSystemPrefab, "CounterSystem");
        InstantiateManager(gameBalancePrefab, "GameBalance");

        // Instantiate animals grouped by habitat
        foreach (AnimalType animalType in System.Enum.GetValues(typeof(AnimalType)))
        {
            HabitatSystem.Habitat habitat = HabitatSystem.Instance.GetPrimaryHabitat(animalType);
            if (habitatParents.TryGetValue(habitat, out Transform parent))
            {
                GameObject prefab = AssetManager.Instance.GetAnimalAssets(animalType)?.prefab;
                if (prefab != null)
                {
                    GameObject animalGO = Instantiate(prefab, parent);
                    animalGO.name = animalType.ToString();
                }
            }
        }
    }

    private void InstantiateManager(GameObject prefab, string name)
    {
        if (prefab == null || managersParent == null) return;

        GameObject go = Instantiate(prefab, managersParent);
        go.name = name;
    }
}

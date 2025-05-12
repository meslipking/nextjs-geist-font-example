using UnityEngine;
using UnityEditor;
using System.IO;

[CustomEditor(typeof(ConfigurationManager))]
public class ConfigurationManagerEditor : Editor
{
    private ConfigurationManager manager;
    private bool showVersionInfo = true;
    private bool showProjectSettings = true;
    private bool showPrefabSettings = true;
    private bool showGameSettings = true;
    private Vector2 scrollPosition;

    private void OnEnable()
    {
        manager = (ConfigurationManager)target;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.Space(10);
        DrawHeader("Configuration Manager");

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        // Draw default inspector
        DrawDefaultInspector();

        EditorGUILayout.Space(20);

        // Version Information
        showVersionInfo = EditorGUILayout.Foldout(showVersionInfo, "Unity Version Information", true);
        if (showVersionInfo)
        {
            EditorGUI.indentLevel++;
            DrawVersionInfo();
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space(10);

        // Project Settings
        showProjectSettings = EditorGUILayout.Foldout(showProjectSettings, "Project Settings", true);
        if (showProjectSettings)
        {
            EditorGUI.indentLevel++;
            DrawProjectSettings();
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space(10);

        // Prefab Settings
        showPrefabSettings = EditorGUILayout.Foldout(showPrefabSettings, "Prefab Configuration", true);
        if (showPrefabSettings)
        {
            EditorGUI.indentLevel++;
            DrawPrefabSettings();
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space(10);

        // Game Settings
        showGameSettings = EditorGUILayout.Foldout(showGameSettings, "Game Configuration", true);
        if (showGameSettings)
        {
            EditorGUI.indentLevel++;
            DrawGameSettings();
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space(20);

        // Action Buttons
        DrawActionButtons();

        EditorGUILayout.EndScrollView();

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawHeader(string title)
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
    }

    private void DrawVersionInfo()
    {
        EditorGUILayout.LabelField("Current Unity Version:", Application.unityVersion);
        
        var versionConfig = manager.GetConfig<UnityVersionConfig>();
        if (versionConfig != null)
        {
            EditorGUILayout.LabelField("Target Version:", UnityVersionConfig.UNITY_VERSION);
            EditorGUILayout.LabelField("Minimum Version:", UnityVersionConfig.MIN_UNITY_VERSION);
            EditorGUILayout.LabelField("Maximum Version:", UnityVersionConfig.MAX_UNITY_VERSION);
        }
        else
        {
            EditorGUILayout.HelpBox("UnityVersionConfig not assigned!", MessageType.Warning);
        }
    }

    private void DrawProjectSettings()
    {
        var projectConfig = manager.GetConfig<ProjectConfig>();
        if (projectConfig != null)
        {
            EditorGUILayout.LabelField("Product Name:", projectConfig.productName);
            EditorGUILayout.LabelField("Bundle ID:", projectConfig.bundleIdentifier);
            EditorGUILayout.LabelField("Version:", projectConfig.version);

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Graphics Settings:", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            EditorGUILayout.LabelField("Color Space:", projectConfig.graphics.colorSpace.ToString());
            EditorGUILayout.LabelField("Anti-Aliasing:", projectConfig.graphics.antiAliasing + "x");
            EditorGUI.indentLevel--;
        }
        else
        {
            EditorGUILayout.HelpBox("ProjectConfig not assigned!", MessageType.Warning);
        }
    }

    private void DrawPrefabSettings()
    {
        var prefabConfig = manager.GetConfig<PrefabConfig>();
        if (prefabConfig != null)
        {
            // Display prefab statistics
            int totalPrefabs = CountPrefabs(prefabConfig);
            EditorGUILayout.LabelField("Total Prefabs:", totalPrefabs.ToString());
        }
        else
        {
            EditorGUILayout.HelpBox("PrefabConfig not assigned!", MessageType.Warning);
        }
    }

    private void DrawGameSettings()
    {
        var gameConfig = manager.GetConfig<GameConfig>();
        if (gameConfig != null)
        {
            EditorGUILayout.LabelField("Game Configuration Found");
        }
        else
        {
            EditorGUILayout.HelpBox("GameConfig not assigned!", MessageType.Warning);
        }
    }

    private void DrawActionButtons()
    {
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Validate All", GUILayout.Height(30)))
        {
            ValidateAllConfigurations();
        }

        if (GUILayout.Button("Apply Settings", GUILayout.Height(30)))
        {
            ApplyAllSettings();
        }

        if (GUILayout.Button("Create Missing Configs", GUILayout.Height(30)))
        {
            CreateMissingConfigs();
        }

        EditorGUILayout.EndHorizontal();
    }

    private void ValidateAllConfigurations()
    {
        Debug.Log("Starting configuration validation...");

        var versionConfig = manager.GetConfig<UnityVersionConfig>();
        if (versionConfig != null)
            versionConfig.ValidateUnityVersion();

        var projectConfig = manager.GetConfig<ProjectConfig>();
        if (projectConfig != null)
            projectConfig.ApplySettings();

        var prefabConfig = manager.GetConfig<PrefabConfig>();
        if (prefabConfig != null)
            prefabConfig.ValidateConfig();

        Debug.Log("Configuration validation complete.");
    }

    private void ApplyAllSettings()
    {
        Debug.Log("Applying all settings...");
        
        // Force Unity to save the project
        EditorApplication.ExecuteMenuItem("File/Save Project");
        
        Debug.Log("All settings applied and saved.");
    }

    private void CreateMissingConfigs()
    {
        string configPath = "Assets/Resources/Configs";
        
        // Create directory if it doesn't exist
        if (!Directory.Exists(configPath))
        {
            Directory.CreateDirectory(configPath);
        }

        // Create UnityVersionConfig if missing
        if (manager.GetConfig<UnityVersionConfig>() == null)
        {
            CreateScriptableObject<UnityVersionConfig>("UnityVersionConfig");
        }

        // Create ProjectConfig if missing
        if (manager.GetConfig<ProjectConfig>() == null)
        {
            CreateScriptableObject<ProjectConfig>("ProjectConfig");
        }

        // Create PrefabConfig if missing
        if (manager.GetConfig<PrefabConfig>() == null)
        {
            CreateScriptableObject<PrefabConfig>("PrefabConfig");
        }

        // Create GameConfig if missing
        if (manager.GetConfig<GameConfig>() == null)
        {
            CreateScriptableObject<GameConfig>("GameConfig");
        }

        AssetDatabase.Refresh();
    }

    private void CreateScriptableObject<T>(string name) where T : ScriptableObject
    {
        var asset = ScriptableObject.CreateInstance<T>();
        AssetDatabase.CreateAsset(asset, $"Assets/Resources/Configs/{name}.asset");
    }

    private int CountPrefabs(PrefabConfig config)
    {
        int count = 0;
        
        // Count manager prefabs
        if (config.managers.gameManager) count++;
        if (config.managers.uiManager) count++;
        if (config.managers.audioManager) count++;
        if (config.managers.vfxManager) count++;
        if (config.managers.inputManager) count++;
        if (config.managers.networkManager) count++;
        if (config.managers.saveSystem) count++;
        if (config.managers.resourceManager) count++;
        if (config.managers.tutorialManager) count++;

        // Count system prefabs
        if (config.systems.terrainSystem) count++;
        if (config.systems.habitatSystem) count++;
        if (config.systems.counterSystem) count++;
        if (config.systems.gameBalance) count++;
        if (config.systems.evolutionSystem) count++;
        if (config.systems.summoningSystem) count++;
        if (config.systems.collectionSystem) count++;

        // Count UI prefabs
        if (config.ui.mainMenu) count++;
        if (config.ui.gameHUD) count++;
        if (config.ui.summoningUI) count++;
        if (config.ui.evolutionUI) count++;
        if (config.ui.collectionUI) count++;
        if (config.ui.habitatUI) count++;
        if (config.ui.terrainUI) count++;
        if (config.ui.popupSystem) count++;

        // Count animal prefabs
        if (config.skyAnimals != null) count += config.skyAnimals.Length;
        if (config.landAnimals != null) count += config.landAnimals.Length;
        if (config.seaAnimals != null) count += config.seaAnimals.Length;

        return count;
    }
}

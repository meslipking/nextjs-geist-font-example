using UnityEngine;
using System.Collections;

public class ConfigurationManager : MonoBehaviour
{
    public static ConfigurationManager Instance { get; private set; }

    [Header("Configuration Assets")]
    [SerializeField] private UnityVersionConfig versionConfig;
    [SerializeField] private ProjectConfig projectConfig;
    [SerializeField] private PrefabConfig prefabConfig;
    [SerializeField] private GameConfig gameConfig;

    [Header("Initialization")]
    [SerializeField] private bool validateOnStart = true;
    [SerializeField] private bool applySettingsOnStart = true;
    [SerializeField] private bool showDebugLogs = true;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Initialize();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Initialize()
    {
        LoadConfigurations();
        
        if (validateOnStart)
        {
            StartCoroutine(ValidateConfigurations());
        }

        if (applySettingsOnStart)
        {
            ApplyAllSettings();
        }
    }

    private void LoadConfigurations()
    {
        if (versionConfig == null)
            versionConfig = Resources.Load<UnityVersionConfig>("Configs/UnityVersionConfig");
        
        if (projectConfig == null)
            projectConfig = Resources.Load<ProjectConfig>("Configs/ProjectConfig");
        
        if (prefabConfig == null)
            prefabConfig = Resources.Load<PrefabConfig>("Configs/PrefabConfig");
        
        if (gameConfig == null)
            gameConfig = Resources.Load<GameConfig>("Configs/GameConfig");

        ValidateConfigurationAssets();
    }

    private void ValidateConfigurationAssets()
    {
        if (versionConfig == null)
            Debug.LogError("UnityVersionConfig asset not found!");
        
        if (projectConfig == null)
            Debug.LogError("ProjectConfig asset not found!");
        
        if (prefabConfig == null)
            Debug.LogError("PrefabConfig asset not found!");
        
        if (gameConfig == null)
            Debug.LogError("GameConfig asset not found!");
    }

    private IEnumerator ValidateConfigurations()
    {
        if (showDebugLogs)
            Debug.Log("Starting configuration validation...");

        // Version validation
        if (versionConfig != null)
        {
            versionConfig.ValidateUnityVersion();
            yield return null;
        }

        // Project settings validation
        if (projectConfig != null)
        {
            projectConfig.ApplySettings();
            yield return null;
        }

        // Prefab validation
        if (prefabConfig != null)
        {
            prefabConfig.ValidateConfig();
            yield return null;
        }

        if (showDebugLogs)
            Debug.Log("Configuration validation complete.");
    }

    private void ApplyAllSettings()
    {
        if (showDebugLogs)
            Debug.Log("Applying all configuration settings...");

        // Apply project settings
        if (projectConfig != null)
        {
            ApplyGraphicsSettings();
            ApplyAudioSettings();
            ApplyInputSettings();
            ApplyPhysicsSettings();
        }

        // Apply game-specific settings
        if (gameConfig != null)
        {
            ApplyGameSettings();
        }

        if (showDebugLogs)
            Debug.Log("All settings applied successfully.");
    }

    private void ApplyGraphicsSettings()
    {
        if (projectConfig.graphics != null)
        {
            // Quality level settings
            QualitySettings.vSyncCount = projectConfig.quality.vSyncCount;
            QualitySettings.antiAliasing = projectConfig.graphics.antiAliasing;

            // Set color space
            #if UNITY_EDITOR
            UnityEditor.PlayerSettings.colorSpace = projectConfig.graphics.colorSpace;
            #endif

            // Apply dynamic batching
            if (projectConfig.graphics.useDynamicBatching)
            {
                #if UNITY_EDITOR
                UnityEditor.PlayerSettings.enableDynamicBatching = true;
                #endif
            }
        }
    }

    private void ApplyAudioSettings()
    {
        if (projectConfig.audio != null)
        {
            AudioSettings.Reset(projectConfig.audio.audioConfig);
            AudioListener.volume = projectConfig.audio.globalVolume;
        }
    }

    private void ApplyInputSettings()
    {
        if (projectConfig.input != null)
        {
            // Input system settings are mostly handled through the Input System package settings
            #if UNITY_EDITOR
            if (projectConfig.input.useNewInputSystem)
            {
                UnityEditor.PlayerSettings.enableNativePlatformBackendsForNewInputSystem = true;
                UnityEditor.PlayerSettings.disableOldInputManagerSupport = projectConfig.input.disableLegacyInput;
            }
            #endif
        }
    }

    private void ApplyPhysicsSettings()
    {
        if (projectConfig.physics != null)
        {
            Physics2D.autoSimulation = projectConfig.physics.autoSimulation;
            Physics2D.velocityIterations = projectConfig.physics.velocityIterations;
            Physics2D.positionIterations = projectConfig.physics.positionIterations;
            Time.fixedDeltaTime = projectConfig.physics.fixedTimestep;
        }
    }

    private void ApplyGameSettings()
    {
        // Apply game-specific settings from GameConfig
        // This will vary based on your GameConfig implementation
    }

    public T GetConfig<T>() where T : ScriptableObject
    {
        if (typeof(T) == typeof(UnityVersionConfig))
            return versionConfig as T;
        if (typeof(T) == typeof(ProjectConfig))
            return projectConfig as T;
        if (typeof(T) == typeof(PrefabConfig))
            return prefabConfig as T;
        if (typeof(T) == typeof(GameConfig))
            return gameConfig as T;
        
        return null;
    }

    #if UNITY_EDITOR
    private void OnValidate()
    {
        LoadConfigurations();
    }
    #endif
}

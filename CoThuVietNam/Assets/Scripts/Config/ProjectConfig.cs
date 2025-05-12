using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(fileName = "ProjectConfig", menuName = "CoThuVietNam/Project Configuration")]
public class ProjectConfig : ScriptableObject
{
    [System.Serializable]
    public class GraphicsSettings
    {
        public ColorSpace colorSpace = ColorSpace.Linear;
        public bool useSRP = true;
        public bool useHDR = true;
        public bool usePostProcessing = true;
        public int antiAliasing = 2; // 0=Off, 2=2x, 4=4x, 8=8x
        public bool useDynamicBatching = true;
        public bool useInstancing = true;
    }

    [System.Serializable]
    public class QualitySettings
    {
        public TextureQuality textureQuality = TextureQuality.Full;
        public AnisotropicFiltering anisotropicFiltering = AnisotropicFiltering.ForceEnable;
        public bool softParticles = true;
        public bool softVegetation = true;
        public RealtimeReflectionProbes realtimeReflections = RealtimeReflectionProbes.Simple;
        public bool billboardsFaceCameraPosition = true;
        public int vSyncCount = 1;
        public int maxQueuedFrames = 2;
        public ResolutionScalingFixedDPIFactor resolutionScalingFactor = ResolutionScalingFixedDPIFactor.Disabled;
    }

    [System.Serializable]
    public class InputSettings
    {
        public bool useNewInputSystem = true;
        public bool disableLegacyInput = true;
        public float defaultDeadzone = 0.1f;
        public bool enableTouchInput = true;
        public bool enableGamepadInput = true;
    }

    [System.Serializable]
    public class AudioSettings
    {
        public AudioConfiguration audioConfig = new AudioConfiguration
        {
            sampleRate = 48000,
            speakerMode = AudioSpeakerMode.Stereo,
            dspBufferSize = 0,
            numRealVoices = 32,
            numVirtualVoices = 64
        };
        public bool enableSpatialization = true;
        public bool enableReverb = true;
        public float globalVolume = 1f;
    }

    [System.Serializable]
    public class PhysicsSettings
    {
        public bool use2DPhysics = true;
        public bool use3DPhysics = false;
        public float fixedTimestep = 0.02f;
        public int velocityIterations = 8;
        public int positionIterations = 3;
        public bool autoSimulation = true;
        public bool enableAdaptiveForce = true;
    }

    [Header("Project Settings")]
    public GraphicsSettings graphics;
    public QualitySettings quality;
    public InputSettings input;
    public AudioSettings audio;
    public PhysicsSettings physics;

    [Header("Build Settings")]
    public string productName = "Cờ Thú Việt Nam";
    public string bundleIdentifier = "com.company.cothuvietnam";
    public string version = "1.0.0";
    public bool developmentBuild = true;
    public bool autoconnectProfiler = true;
    public bool enableDeepProfiling = true;

    public void ApplySettings()
    {
        #if UNITY_EDITOR
        ApplyGraphicsSettings();
        ApplyQualitySettings();
        ApplyInputSettings();
        ApplyAudioSettings();
        ApplyPhysicsSettings();
        ApplyBuildSettings();
        #endif
    }

    #if UNITY_EDITOR
    private void ApplyGraphicsSettings()
    {
        UnityEditor.PlayerSettings.colorSpace = graphics.colorSpace;
        
        // SRP settings through GraphicsSettings asset
        var graphicsSettings = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Rendering.GraphicsSettings>(
            "ProjectSettings/GraphicsSettings.asset");
        if (graphicsSettings != null)
        {
            // Set up SRP if needed
            if (graphics.useSRP)
            {
                // Load and assign URP asset
                var urpAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset>(
                    "Assets/Settings/UniversalRP.asset");
                if (urpAsset != null)
                {
                    graphicsSettings.renderPipelineAsset = urpAsset;
                }
            }
        }
    }

    private void ApplyQualitySettings()
    {
        UnityEngine.QualitySettings.anisotropicFiltering = quality.anisotropicFiltering;
        UnityEngine.QualitySettings.softParticles = quality.softParticles;
        UnityEngine.QualitySettings.softVegetation = quality.softVegetation;
        UnityEngine.QualitySettings.billboardsFaceCameraPosition = quality.billboardsFaceCameraPosition;
        UnityEngine.QualitySettings.vSyncCount = quality.vSyncCount;
        UnityEngine.QualitySettings.maxQueuedFrames = quality.maxQueuedFrames;
    }

    private void ApplyInputSettings()
    {
        // Input System settings through ProjectSettings
        var inputSettings = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.InputSystem.InputSettings>(
            "ProjectSettings/InputSystem.asset");
        if (inputSettings != null)
        {
            inputSettings.updateMode = UnityEngine.InputSystem.InputSettings.UpdateMode.ProcessEventsInFixedUpdate;
            inputSettings.compensateForScreenOrientation = true;
        }
    }

    private void ApplyAudioSettings()
    {
        UnityEngine.AudioSettings.Reset(audio.audioConfig);
        UnityEngine.AudioListener.volume = audio.globalVolume;
    }

    private void ApplyPhysicsSettings()
    {
        if (physics.use2DPhysics)
        {
            Physics2D.autoSimulation = physics.autoSimulation;
            Physics2D.velocityIterations = physics.velocityIterations;
            Physics2D.positionIterations = physics.positionIterations;
        }
        
        Time.fixedDeltaTime = physics.fixedTimestep;
    }

    private void ApplyBuildSettings()
    {
        UnityEditor.PlayerSettings.productName = productName;
        UnityEditor.PlayerSettings.bundleVersion = version;
        UnityEditor.PlayerSettings.companyName = "Company";

        var buildTargetGroup = UnityEditor.BuildTargetGroup.Standalone;
        UnityEditor.PlayerSettings.SetApplicationIdentifier(buildTargetGroup, bundleIdentifier);

        // Development build settings
        if (developmentBuild)
        {
            UnityEditor.EditorUserBuildSettings.development = true;
            UnityEditor.EditorUserBuildSettings.connectProfiler = autoconnectProfiler;
            UnityEditor.PlayerSettings.enableDynamicBatching = graphics.useDynamicBatching;
            UnityEditor.PlayerSettings.gpuSkinning = true;
        }
    }

    private void OnValidate()
    {
        ApplySettings();
    }
    #endif

    public enum TextureQuality
    {
        Full = 0,
        Half = 1,
        Quarter = 2,
        Eighth = 3
    }

    public enum RealtimeReflectionProbes
    {
        Off = 0,
        Simple = 1,
        Full = 2
    }

    public enum ResolutionScalingFixedDPIFactor
    {
        Disabled = 0,
        DPI_720p = 1,
        DPI_1080p = 2,
        DPI_1440p = 3,
        DPI_2160p = 4
    }
}

using UnityEngine;

public class UnityVersionConfig : ScriptableObject
{
    public const string UNITY_VERSION = "2022.3.61f1";
    public const string MIN_UNITY_VERSION = "2022.3.0f1";
    public const string MAX_UNITY_VERSION = "2022.3.99f1";

    [Header("Version Information")]
    [SerializeField] private string targetUnityVersion = UNITY_VERSION;
    [SerializeField] private string minimumUnityVersion = MIN_UNITY_VERSION;
    [SerializeField] private string maximumUnityVersion = MAX_UNITY_VERSION;

    [Header("Required Packages")]
    public PackageRequirement[] requiredPackages = new PackageRequirement[]
    {
        new PackageRequirement("com.unity.render-pipelines.universal", "14.0.8"),  // URP
        new PackageRequirement("com.unity.inputsystem", "1.7.0"),                  // New Input System
        new PackageRequirement("com.unity.textmeshpro", "3.0.6"),                 // TextMeshPro
        new PackageRequirement("com.unity.addressables", "1.21.19"),              // Addressables
        new PackageRequirement("com.unity.visualscripting", "1.9.0"),            // Visual Scripting
        new PackageRequirement("com.unity.mathematics", "1.2.6"),                // Mathematics
        new PackageRequirement("com.unity.burst", "1.8.8"),                      // Burst Compiler
        new PackageRequirement("com.unity.collections", "1.2.4"),               // Collections
    };

    [System.Serializable]
    public class PackageRequirement
    {
        public string packageId;
        public string minimumVersion;
        public bool isRequired = true;

        public PackageRequirement(string id, string version, bool required = true)
        {
            packageId = id;
            minimumVersion = version;
            isRequired = required;
        }
    }

    [Header("Build Settings")]
    public BuildRequirements buildRequirements = new BuildRequirements
    {
        scriptingBackend = ScriptingBackend.IL2CPP,
        apiCompatibility = ApiCompatibilityLevel.NET_Standard_2_1,
        allowUnsafeCode = true,
        optimizeMesh = true,
        stripEngineCode = true
    };

    [System.Serializable]
    public class BuildRequirements
    {
        public enum ScriptingBackend { Mono, IL2CPP }
        public enum ApiCompatibilityLevel { NET_Standard_2_0, NET_Standard_2_1 }

        public ScriptingBackend scriptingBackend;
        public ApiCompatibilityLevel apiCompatibility;
        public bool allowUnsafeCode;
        public bool optimizeMesh;
        public bool stripEngineCode;
    }

    public void ValidateUnityVersion()
    {
        #if UNITY_EDITOR
        string currentVersion = Application.unityVersion;
        Debug.Log($"Current Unity Version: {currentVersion}");
        Debug.Log($"Target Unity Version: {targetUnityVersion}");
        
        // Version compatibility check
        if (CompareVersions(currentVersion, minimumUnityVersion) < 0)
        {
            Debug.LogError($"Unity version {currentVersion} is below minimum required version {minimumUnityVersion}");
        }
        
        if (CompareVersions(currentVersion, maximumUnityVersion) > 0)
        {
            Debug.LogWarning($"Unity version {currentVersion} is above maximum tested version {maximumUnityVersion}");
        }

        // Package validation
        ValidatePackages();
        
        // Build settings validation
        ValidateBuildSettings();
        #endif
    }

    #if UNITY_EDITOR
    private void ValidatePackages()
    {
        foreach (var package in requiredPackages)
        {
            UnityEditor.PackageManager.PackageInfo packageInfo = 
                UnityEditor.PackageManager.PackageInfo.FindForAssetPath(package.packageId);
                
            if (packageInfo == null)
            {
                if (package.isRequired)
                {
                    Debug.LogError($"Required package {package.packageId} is not installed");
                }
                else
                {
                    Debug.LogWarning($"Recommended package {package.packageId} is not installed");
                }
            }
            else if (CompareVersions(packageInfo.version, package.minimumVersion) < 0)
            {
                Debug.LogWarning($"Package {package.packageId} version {packageInfo.version} is below recommended version {package.minimumVersion}");
            }
        }
    }

    private void ValidateBuildSettings()
    {
        // Validate Scripting Backend
        if (UnityEditor.PlayerSettings.GetScriptingBackend(UnityEditor.BuildTargetGroup.Standalone) 
            != UnityEditor.ScriptingImplementation.IL2CPP && 
            buildRequirements.scriptingBackend == BuildRequirements.ScriptingBackend.IL2CPP)
        {
            Debug.LogError("Project requires IL2CPP scripting backend");
        }

        // Validate API Compatibility Level
        if (UnityEditor.PlayerSettings.GetApiCompatibilityLevel(UnityEditor.BuildTargetGroup.Standalone) 
            != UnityEditor.ApiCompatibilityLevel.NET_Standard_2_1 &&
            buildRequirements.apiCompatibility == BuildRequirements.ApiCompatibilityLevel.NET_Standard_2_1)
        {
            Debug.LogError("Project requires .NET Standard 2.1 API Compatibility Level");
        }

        // Validate other build settings
        if (buildRequirements.allowUnsafeCode && !UnityEditor.PlayerSettings.allowUnsafeCode)
        {
            Debug.LogError("Project requires unsafe code to be enabled");
        }
    }
    #endif

    private int CompareVersions(string version1, string version2)
    {
        string[] v1Parts = version1.Split('.');
        string[] v2Parts = version2.Split('.');

        int length = Mathf.Min(v1Parts.Length, v2Parts.Length);

        for (int i = 0; i < length; i++)
        {
            int v1Part = int.Parse(v1Parts[i]);
            int v2Part = int.Parse(v2Parts[i]);

            if (v1Part < v2Part) return -1;
            if (v1Part > v2Part) return 1;
        }

        return v1Parts.Length.CompareTo(v2Parts.Length);
    }

    #if UNITY_EDITOR
    private void OnValidate()
    {
        ValidateUnityVersion();
    }
    #endif
}

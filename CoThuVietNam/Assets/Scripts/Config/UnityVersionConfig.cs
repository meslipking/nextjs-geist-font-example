using UnityEngine;

public class UnityVersionConfig : ScriptableObject
{
    public const string UNITY_VERSION = "2022.3 LTS"; // Unity 2022 Long Term Support
    public const string MIN_UNITY_VERSION = "2021.3";
    public const string MAX_UNITY_VERSION = "2023.1";

    [Header("Version Information")]
    [SerializeField] private string targetUnityVersion = UNITY_VERSION;
    [SerializeField] private string minimumUnityVersion = MIN_UNITY_VERSION;
    [SerializeField] private string maximumUnityVersion = MAX_UNITY_VERSION;

    [Header("Feature Requirements")]
    public bool requiresURP = true;           // Universal Render Pipeline
    public bool requiresNewInputSystem = true; // Input System Package
    public bool requiresTextMeshPro = true;   // TextMeshPro Package
    public bool requiresDOTween = true;       // DOTween Asset
    public bool requiresAddressables = true;  // Addressables System

    [Header("Package Versions")]
    public string urpVersion = "14.0.8";
    public string inputSystemVersion = "1.7.0";
    public string tmpVersion = "3.0.6";
    public string doTweenVersion = "1.2.705";
    public string addressablesVersion = "1.21.19";

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

        // Feature compatibility checks
        ValidateRequiredFeatures();
        #endif
    }

    private void ValidateRequiredFeatures()
    {
        #if UNITY_EDITOR
        if (requiresURP)
        {
            ValidatePackage("com.unity.render-pipelines.universal", urpVersion);
        }

        if (requiresNewInputSystem)
        {
            ValidatePackage("com.unity.inputsystem", inputSystemVersion);
        }

        if (requiresTextMeshPro)
        {
            ValidatePackage("com.unity.textmeshpro", tmpVersion);
        }

        if (requiresAddressables)
        {
            ValidatePackage("com.unity.addressables", addressablesVersion);
        }

        // DOTween check (Asset Store package)
        if (requiresDOTween)
        {
            if (!UnityEditorInternal.InternalEditorUtility.HasPro())
            {
                Debug.LogWarning("Unity Pro/Plus license recommended for DOTween Pro usage");
            }
        }
        #endif
    }

    private void ValidatePackage(string packageId, string requiredVersion)
    {
        #if UNITY_EDITOR
        UnityEditor.PackageManager.PackageInfo packageInfo = UnityEditor.PackageManager.PackageInfo.FindForAssetPath(packageId);
        if (packageInfo == null)
        {
            Debug.LogError($"Required package {packageId} is not installed");
        }
        else if (CompareVersions(packageInfo.version, requiredVersion) < 0)
        {
            Debug.LogWarning($"Package {packageId} version {packageInfo.version} is below recommended version {requiredVersion}");
        }
        #endif
    }

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

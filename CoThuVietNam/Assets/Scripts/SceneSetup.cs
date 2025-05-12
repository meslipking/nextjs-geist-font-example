using UnityEngine;
using UnityEngine.UI;

public class SceneSetup : MonoBehaviour
{
    private void Awake()
    {
        CreateSceneHierarchy();
    }

    private void CreateSceneHierarchy()
    {
        // Create main parent objects
        GameObject worldSpace = CreateParent("WorldSpace");
        GameObject uiSpace = CreateParent("UISpace");
        GameObject managers = CreateParent("Managers");

        // Create habitat parents under WorldSpace
        GameObject skyDomain = CreateParent("SkyDomain", worldSpace.transform);
        GameObject landDomain = CreateParent("LandDomain", worldSpace.transform);
        GameObject seaDomain = CreateParent("SeaDomain", worldSpace.transform);

        // Create UI structure
        CreateUIStructure(uiSpace);

        // Create GameHierarchyBuilder and assign references
        GameObject hierarchyBuilder = new GameObject("GameHierarchyBuilder");
        GameHierarchyBuilder builder = hierarchyBuilder.AddComponent<GameHierarchyBuilder>();
        
        // Assign parent references
        builder.skyParent = skyDomain.transform;
        builder.landParent = landDomain.transform;
        builder.seaParent = seaDomain.transform;
        builder.managersParent = managers.transform;

        // Create camera setup
        CreateCameraSetup();
    }

    private GameObject CreateParent(string name, Transform parent = null)
    {
        GameObject go = new GameObject(name);
        if (parent != null)
            go.transform.SetParent(parent);
        return go;
    }

    private void CreateUIStructure(GameObject uiSpace)
    {
        // Add Canvas
        Canvas canvas = uiSpace.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        
        // Add CanvasScaler
        CanvasScaler scaler = uiSpace.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        // Add GraphicRaycaster
        uiSpace.AddComponent<GraphicRaycaster>();

        // Create UI sections
        CreateParent("GameUI", uiSpace.transform);
        CreateParent("MenuUI", uiSpace.transform);
        CreateParent("OverlayUI", uiSpace.transform);
        CreateParent("PopupUI", uiSpace.transform);
    }

    private void CreateCameraSetup()
    {
        // Main Camera
        GameObject mainCam = new GameObject("MainCamera");
        Camera camera = mainCam.AddComponent<Camera>();
        camera.tag = "MainCamera";
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = Color.black;
        camera.orthographic = true;
        camera.orthographicSize = 5;
        
        // UI Camera
        GameObject uiCam = new GameObject("UICamera");
        Camera uiCamera = uiCam.AddComponent<Camera>();
        uiCamera.clearFlags = CameraClearFlags.Depth;
        uiCamera.cullingMask = LayerMask.GetMask("UI");
        uiCamera.orthographic = true;
        uiCamera.depth = 1;
    }
}

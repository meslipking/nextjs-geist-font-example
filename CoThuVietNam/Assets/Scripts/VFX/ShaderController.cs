using UnityEngine;
using System.Collections.Generic;

public class ShaderController : MonoBehaviour
{
    public static ShaderController Instance { get; private set; }

    [System.Serializable]
    public class ShaderPreset
    {
        public string presetName;
        public Material material;
        public Color primaryColor = Color.white;
        public Color secondaryColor = Color.white;
        public float intensity = 1f;
        public float speed = 1f;
        public Vector4 customParams;
    }

    [Header("Shader Presets")]
    [SerializeField] private ShaderPreset[] rarityShaders;
    [SerializeField] private ShaderPreset[] elementalShaders;
    [SerializeField] private ShaderPreset[] effectShaders;

    [Header("Common Effects")]
    [SerializeField] private Material outlineMaterial;
    [SerializeField] private Material glowMaterial;
    [SerializeField] private Material dissolveMaterial;
    [SerializeField] private Material hologramMaterial;

    private Dictionary<string, ShaderPreset> presetDict;
    private Dictionary<GameObject, Material> originalMaterials;
    private Dictionary<GameObject, Material> activeMaterials;

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
        presetDict = new Dictionary<string, ShaderPreset>();
        originalMaterials = new Dictionary<GameObject, Material>();
        activeMaterials = new Dictionary<GameObject, Material>();

        // Initialize shader presets
        foreach (var preset in rarityShaders)
            presetDict[preset.presetName] = preset;
        foreach (var preset in elementalShaders)
            presetDict[preset.presetName] = preset;
        foreach (var preset in effectShaders)
            presetDict[preset.presetName] = preset;
    }

    #region Rarity Effects

    public void ApplyRarityShader(GameObject target, Rarity rarity)
    {
        string presetName = $"Rarity_{rarity}";
        if (presetDict.TryGetValue(presetName, out ShaderPreset preset))
        {
            ApplyShaderPreset(target, preset);
        }
    }

    public void ApplyElementalShader(GameObject target, HabitatSystem.ElementType elementType)
    {
        string presetName = $"Element_{elementType}";
        if (presetDict.TryGetValue(presetName, out ShaderPreset preset))
        {
            ApplyShaderPreset(target, preset);
        }
    }

    #endregion

    #region Special Effects

    public void ApplyOutlineEffect(GameObject target, Color color, float width = 1f)
    {
        if (outlineMaterial == null) return;

        Material material = new Material(outlineMaterial);
        material.SetColor("_OutlineColor", color);
        material.SetFloat("_OutlineWidth", width);

        ApplyMaterial(target, material);
    }

    public void ApplyGlowEffect(GameObject target, Color color, float intensity = 1f)
    {
        if (glowMaterial == null) return;

        Material material = new Material(glowMaterial);
        material.SetColor("_GlowColor", color);
        material.SetFloat("_GlowIntensity", intensity);

        ApplyMaterial(target, material);
    }

    public void ApplyDissolveEffect(GameObject target, Color edgeColor, float dissolveAmount)
    {
        if (dissolveMaterial == null) return;

        Material material = new Material(dissolveMaterial);
        material.SetColor("_EdgeColor", edgeColor);
        material.SetFloat("_DissolveAmount", dissolveAmount);

        ApplyMaterial(target, material);
    }

    public void ApplyHologramEffect(GameObject target, Color hologramColor)
    {
        if (hologramMaterial == null) return;

        Material material = new Material(hologramMaterial);
        material.SetColor("_HologramColor", hologramColor);

        ApplyMaterial(target, material);
    }

    #endregion

    #region Utility Methods

    private void ApplyShaderPreset(GameObject target, ShaderPreset preset)
    {
        if (preset.material == null) return;

        Material material = new Material(preset.material);
        material.SetColor("_PrimaryColor", preset.primaryColor);
        material.SetColor("_SecondaryColor", preset.secondaryColor);
        material.SetFloat("_Intensity", preset.intensity);
        material.SetFloat("_Speed", preset.speed);
        material.SetVector("_CustomParams", preset.customParams);

        ApplyMaterial(target, material);
    }

    private void ApplyMaterial(GameObject target, Material material)
    {
        SpriteRenderer renderer = target.GetComponent<SpriteRenderer>();
        if (renderer == null) return;

        // Store original material if not already stored
        if (!originalMaterials.ContainsKey(target))
        {
            originalMaterials[target] = renderer.material;
        }

        // Clean up previous active material
        if (activeMaterials.TryGetValue(target, out Material oldMaterial))
        {
            Destroy(oldMaterial);
        }

        renderer.material = material;
        activeMaterials[target] = material;
    }

    public void ResetMaterial(GameObject target)
    {
        if (originalMaterials.TryGetValue(target, out Material originalMaterial))
        {
            SpriteRenderer renderer = target.GetComponent<SpriteRenderer>();
            if (renderer != null)
            {
                renderer.material = originalMaterial;
            }

            if (activeMaterials.TryGetValue(target, out Material activeMaterial))
            {
                Destroy(activeMaterial);
                activeMaterials.Remove(target);
            }
        }
    }

    public void UpdateShaderProperty(GameObject target, string propertyName, float value)
    {
        if (activeMaterials.TryGetValue(target, out Material material))
        {
            material.SetFloat(propertyName, value);
        }
    }

    public void UpdateShaderProperty(GameObject target, string propertyName, Color value)
    {
        if (activeMaterials.TryGetValue(target, out Material material))
        {
            material.SetColor(propertyName, value);
        }
    }

    public void UpdateShaderProperty(GameObject target, string propertyName, Vector4 value)
    {
        if (activeMaterials.TryGetValue(target, out Material material))
        {
            material.SetVector(propertyName, value);
        }
    }

    #endregion

    private void OnDestroy()
    {
        // Clean up materials
        foreach (var material in activeMaterials.Values)
        {
            Destroy(material);
        }
        activeMaterials.Clear();
        originalMaterials.Clear();
    }
}

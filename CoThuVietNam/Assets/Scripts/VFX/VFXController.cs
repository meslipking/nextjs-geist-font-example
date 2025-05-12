using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

public class VFXController : MonoBehaviour
{
    public static VFXController Instance { get; private set; }

    [System.Serializable]
    public class EffectSet
    {
        public ParticleSystem summonEffect;
        public ParticleSystem evolutionEffect;
        public ParticleSystem levelUpEffect;
        public ParticleSystem victoryEffect;
        public ParticleSystem defeatEffect;
        public ParticleSystem healEffect;
        public ParticleSystem shieldEffect;
        public ParticleSystem stunEffect;
    }

    [System.Serializable]
    public class ElementalEffects
    {
        public ParticleSystem fireEffect;
        public ParticleSystem waterEffect;
        public ParticleSystem earthEffect;
        public ParticleSystem airEffect;
        public ParticleSystem lightEffect;
        public ParticleSystem darkEffect;
    }

    [System.Serializable]
    public class TerrainEffects
    {
        public ParticleSystem mountainEffect;
        public ParticleSystem waterSplash;
        public ParticleSystem forestLeaves;
        public ParticleSystem desertSand;
        public ParticleSystem swampBubbles;
    }

    [Header("Effect Sets")]
    [SerializeField] private EffectSet commonEffects;
    [SerializeField] private ElementalEffects elementalEffects;
    [SerializeField] private TerrainEffects terrainEffects;

    [Header("Screen Effects")]
    [SerializeField] private Material screenFlashMaterial;
    [SerializeField] private Material screenShakeMaterial;
    [SerializeField] private Material screenBlurMaterial;

    [Header("Effect Settings")]
    [SerializeField] private float defaultEffectDuration = 1f;
    [SerializeField] private float screenEffectDuration = 0.5f;
    [SerializeField] private float shakeIntensity = 0.5f;
    [SerializeField] private int shakeVibrato = 10;

    private Dictionary<GameObject, List<ParticleSystem>> activeEffects;
    private Camera mainCamera;

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
        activeEffects = new Dictionary<GameObject, List<ParticleSystem>>();
        mainCamera = Camera.main;
    }

    #region Common Effects

    public void PlaySummonEffect(Vector3 position)
    {
        SpawnEffect(commonEffects.summonEffect, position, defaultEffectDuration);
        PlayScreenFlash(Color.white, screenEffectDuration * 0.5f);
    }

    public void PlayEvolutionEffect(GameObject target)
    {
        var effect = SpawnEffect(commonEffects.evolutionEffect, target.transform.position, defaultEffectDuration * 2f);
        effect.transform.SetParent(target.transform);
        
        // Screen effects
        PlayScreenFlash(Color.yellow, screenEffectDuration);
        PlayScreenShake(shakeIntensity * 0.5f);
    }

    public void PlayLevelUpEffect(GameObject target)
    {
        var effect = SpawnEffect(commonEffects.levelUpEffect, target.transform.position + Vector3.up, defaultEffectDuration);
        effect.transform.SetParent(target.transform);
    }

    public void PlayVictoryEffect(Vector3 position)
    {
        SpawnEffect(commonEffects.victoryEffect, position, defaultEffectDuration);
        PlayScreenFlash(Color.green, screenEffectDuration);
    }

    public void PlayDefeatEffect(Vector3 position)
    {
        SpawnEffect(commonEffects.defeatEffect, position, defaultEffectDuration);
        PlayScreenFlash(Color.red, screenEffectDuration);
        PlayScreenShake(shakeIntensity);
    }

    #endregion

    #region Combat Effects

    public void PlayHealEffect(GameObject target)
    {
        var effect = SpawnEffect(commonEffects.healEffect, target.transform.position, defaultEffectDuration);
        effect.transform.SetParent(target.transform);
    }

    public void PlayShieldEffect(GameObject target, float duration)
    {
        var effect = SpawnEffect(commonEffects.shieldEffect, target.transform.position, duration);
        effect.transform.SetParent(target.transform);
        TrackEffect(target, effect);
    }

    public void PlayStunEffect(GameObject target, float duration)
    {
        var effect = SpawnEffect(commonEffects.stunEffect, target.transform.position + Vector3.up, duration);
        effect.transform.SetParent(target.transform);
        TrackEffect(target, effect);
    }

    #endregion

    #region Elemental Effects

    public void PlayElementalEffect(HabitatSystem.ElementType elementType, Vector3 position)
    {
        ParticleSystem effectPrefab = null;

        switch (elementType)
        {
            case HabitatSystem.ElementType.Fire:
                effectPrefab = elementalEffects.fireEffect;
                break;
            case HabitatSystem.ElementType.Water:
                effectPrefab = elementalEffects.waterEffect;
                break;
            case HabitatSystem.ElementType.Earth:
                effectPrefab = elementalEffects.earthEffect;
                break;
            case HabitatSystem.ElementType.Air:
                effectPrefab = elementalEffects.airEffect;
                break;
            case HabitatSystem.ElementType.Light:
                effectPrefab = elementalEffects.lightEffect;
                break;
            case HabitatSystem.ElementType.Dark:
                effectPrefab = elementalEffects.darkEffect;
                break;
        }

        if (effectPrefab != null)
        {
            SpawnEffect(effectPrefab, position, defaultEffectDuration);
        }
    }

    #endregion

    #region Terrain Effects

    public void PlayTerrainEffect(TerrainSystem.TerrainType terrainType, Vector3 position)
    {
        ParticleSystem effectPrefab = null;

        switch (terrainType)
        {
            case TerrainSystem.TerrainType.Mountain:
                effectPrefab = terrainEffects.mountainEffect;
                break;
            case TerrainSystem.TerrainType.Water:
                effectPrefab = terrainEffects.waterSplash;
                break;
            case TerrainSystem.TerrainType.Forest:
                effectPrefab = terrainEffects.forestLeaves;
                break;
            case TerrainSystem.TerrainType.Desert:
                effectPrefab = terrainEffects.desertSand;
                break;
            case TerrainSystem.TerrainType.Swamp:
                effectPrefab = terrainEffects.swampBubbles;
                break;
        }

        if (effectPrefab != null)
        {
            SpawnEffect(effectPrefab, position, defaultEffectDuration);
        }
    }

    #endregion

    #region Screen Effects

    public void PlayScreenFlash(Color color, float duration)
    {
        if (screenFlashMaterial != null)
        {
            screenFlashMaterial.DOColor(color, "_FlashColor", duration * 0.5f)
                .SetLoops(2, LoopType.Yoyo);
        }
    }

    public void PlayScreenShake(float intensity = 1f)
    {
        mainCamera.transform.DOShakePosition(screenEffectDuration, intensity * shakeIntensity, shakeVibrato)
            .SetEase(Ease.OutQuad);
    }

    public void PlayScreenBlur(float intensity, float duration)
    {
        if (screenBlurMaterial != null)
        {
            screenBlurMaterial.DOFloat(intensity, "_BlurAmount", duration * 0.5f)
                .SetLoops(2, LoopType.Yoyo);
        }
    }

    #endregion

    #region Utility Methods

    private ParticleSystem SpawnEffect(ParticleSystem prefab, Vector3 position, float duration)
    {
        if (prefab == null) return null;

        ParticleSystem effect = Instantiate(prefab, position, Quaternion.identity);
        float actualDuration = duration > 0 ? duration : effect.main.duration;
        Destroy(effect.gameObject, actualDuration);
        return effect;
    }

    private void TrackEffect(GameObject target, ParticleSystem effect)
    {
        if (!activeEffects.ContainsKey(target))
        {
            activeEffects[target] = new List<ParticleSystem>();
        }
        activeEffects[target].Add(effect);
    }

    public void StopEffects(GameObject target)
    {
        if (activeEffects.TryGetValue(target, out List<ParticleSystem> effects))
        {
            foreach (var effect in effects)
            {
                if (effect != null)
                {
                    effect.Stop();
                    Destroy(effect.gameObject, effect.main.duration);
                }
            }
            effects.Clear();
        }
    }

    private void OnDestroy()
    {
        // Clean up all active effects
        foreach (var effectsList in activeEffects.Values)
        {
            foreach (var effect in effectsList)
            {
                if (effect != null)
                {
                    Destroy(effect.gameObject);
                }
            }
        }
        activeEffects.Clear();
    }

    #endregion
}

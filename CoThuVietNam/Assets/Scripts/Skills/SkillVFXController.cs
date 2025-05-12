using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class SkillVFXController : MonoBehaviour
{
    [System.Serializable]
    public class SkillVFX
    {
        public string skillName;
        public ParticleSystem[] castEffects;
        public ParticleSystem[] projectileEffects;
        public ParticleSystem[] impactEffects;
        public TrailRenderer projectileTrail;
        public GameObject skillIndicator;
        public AudioClip[] skillSounds;
        public Color effectColor = Color.white;
        public float effectScale = 1f;
        public bool useScreenShake = true;
    }

    [System.Serializable]
    public class ElementalVFX
    {
        public HabitatSystem.ElementType elementType;
        public Color elementColor;
        public ParticleSystem elementalAura;
        public ParticleSystem elementalBurst;
        public Material elementalMaterial;
    }

    [Header("Skill Effects")]
    [SerializeField] private SkillVFX[] skillEffects;
    [SerializeField] private ElementalVFX[] elementalEffects;

    [Header("Common Settings")]
    [SerializeField] private float projectileSpeed = 10f;
    [SerializeField] private float indicatorDuration = 0.5f;
    [SerializeField] private float screenShakeIntensity = 0.3f;
    [SerializeField] private AnimationCurve scaleCurve;

    private Dictionary<string, SkillVFX> skillVFXDict;
    private Dictionary<HabitatSystem.ElementType, ElementalVFX> elementalVFXDict;
    private List<ParticleSystem> activeEffects;
    private Camera mainCamera;

    private void Awake()
    {
        Initialize();
    }

    private void Initialize()
    {
        mainCamera = Camera.main;
        activeEffects = new List<ParticleSystem>();

        // Initialize dictionaries
        skillVFXDict = new Dictionary<string, SkillVFX>();
        foreach (var effect in skillEffects)
        {
            skillVFXDict[effect.skillName] = effect;
        }

        elementalVFXDict = new Dictionary<HabitatSystem.ElementType, ElementalVFX>();
        foreach (var effect in elementalEffects)
        {
            elementalVFXDict[effect.elementType] = effect;
        }
    }

    public IEnumerator PlaySkillVFX(string skillName, Transform caster, Vector3 targetPosition, 
        HabitatSystem.ElementType elementType = HabitatSystem.ElementType.None)
    {
        if (!skillVFXDict.TryGetValue(skillName, out SkillVFX skillVFX))
            yield break;

        // Show skill indicator if available
        if (skillVFX.skillIndicator != null)
        {
            yield return ShowSkillIndicator(skillVFX.skillIndicator, targetPosition, skillVFX.effectColor);
        }

        // Play cast effects
        PlayCastEffects(skillVFX, caster, elementType);

        // Play skill sound
        PlaySkillSound(skillVFX);

        // Launch projectile if available
        if (skillVFX.projectileEffects.Length > 0)
        {
            yield return LaunchProjectile(skillVFX, caster.position, targetPosition, elementType);
        }

        // Play impact effects
        PlayImpactEffects(skillVFX, targetPosition, elementType);

        // Screen shake if enabled
        if (skillVFX.useScreenShake)
        {
            mainCamera.transform.DOShakePosition(0.3f, screenShakeIntensity);
        }
    }

    private void PlayCastEffects(SkillVFX skillVFX, Transform caster, HabitatSystem.ElementType elementType)
    {
        foreach (var effectPrefab in skillVFX.castEffects)
        {
            ParticleSystem effect = Instantiate(effectPrefab, caster.position, Quaternion.identity);
            effect.transform.SetParent(caster);
            
            // Apply elemental color if applicable
            if (elementType != HabitatSystem.ElementType.None && 
                elementalVFXDict.TryGetValue(elementType, out ElementalVFX elementalVFX))
            {
                SetParticleColor(effect, elementalVFX.elementColor);
            }
            else
            {
                SetParticleColor(effect, skillVFX.effectColor);
            }

            effect.transform.localScale *= skillVFX.effectScale;
            activeEffects.Add(effect);
            
            float duration = effect.main.duration;
            Destroy(effect.gameObject, duration);
        }
    }

    private IEnumerator LaunchProjectile(SkillVFX skillVFX, Vector3 startPos, Vector3 targetPos, 
        HabitatSystem.ElementType elementType)
    {
        foreach (var projectilePrefab in skillVFX.projectileEffects)
        {
            GameObject projectile = new GameObject("Projectile");
            projectile.transform.position = startPos;

            // Add projectile effect
            ParticleSystem projectileEffect = Instantiate(projectilePrefab, projectile.transform);
            
            // Add trail if available
            if (skillVFX.projectileTrail != null)
            {
                TrailRenderer trail = projectile.AddComponent<TrailRenderer>();
                trail.colorGradient = skillVFX.projectileTrail.colorGradient;
                trail.time = skillVFX.projectileTrail.time;
                trail.startWidth = skillVFX.projectileTrail.startWidth * skillVFX.effectScale;
                trail.endWidth = skillVFX.projectileTrail.endWidth * skillVFX.effectScale;
            }

            // Apply elemental effects
            if (elementType != HabitatSystem.ElementType.None && 
                elementalVFXDict.TryGetValue(elementType, out ElementalVFX elementalVFX))
            {
                SetParticleColor(projectileEffect, elementalVFX.elementColor);
            }
            else
            {
                SetParticleColor(projectileEffect, skillVFX.effectColor);
            }

            // Move projectile
            Vector3 direction = (targetPos - startPos).normalized;
            float distance = Vector3.Distance(startPos, targetPos);
            float duration = distance / projectileSpeed;

            projectile.transform.DOMove(targetPos, duration).SetEase(Ease.Linear);
            projectile.transform.up = direction;

            yield return new WaitForSeconds(duration);

            Destroy(projectile.gameObject);
        }
    }

    private void PlayImpactEffects(SkillVFX skillVFX, Vector3 position, HabitatSystem.ElementType elementType)
    {
        foreach (var impactPrefab in skillVFX.impactEffects)
        {
            ParticleSystem impact = Instantiate(impactPrefab, position, Quaternion.identity);
            
            if (elementType != HabitatSystem.ElementType.None && 
                elementalVFXDict.TryGetValue(elementType, out ElementalVFX elementalVFX))
            {
                SetParticleColor(impact, elementalVFX.elementColor);
            }
            else
            {
                SetParticleColor(impact, skillVFX.effectColor);
            }

            impact.transform.localScale *= skillVFX.effectScale;
            activeEffects.Add(impact);
            
            float duration = impact.main.duration;
            Destroy(impact.gameObject, duration);
        }
    }

    private IEnumerator ShowSkillIndicator(GameObject indicatorPrefab, Vector3 position, Color color)
    {
        GameObject indicator = Instantiate(indicatorPrefab, position, Quaternion.identity);
        SpriteRenderer spriteRenderer = indicator.GetComponent<SpriteRenderer>();
        
        if (spriteRenderer != null)
        {
            spriteRenderer.color = color with { a = 0 };
            spriteRenderer.DOFade(0.5f, indicatorDuration * 0.5f);
        }

        indicator.transform.localScale = Vector3.zero;
        indicator.transform.DOScale(Vector3.one, indicatorDuration)
            .SetEase(scaleCurve);

        yield return new WaitForSeconds(indicatorDuration);

        if (spriteRenderer != null)
        {
            spriteRenderer.DOFade(0f, indicatorDuration * 0.2f);
        }

        Destroy(indicator, indicatorDuration * 0.2f);
    }

    private void PlaySkillSound(SkillVFX skillVFX)
    {
        if (skillVFX.skillSounds != null && skillVFX.skillSounds.Length > 0)
        {
            AudioClip sound = skillVFX.skillSounds[Random.Range(0, skillVFX.skillSounds.Length)];
            AudioManager.Instance.PlaySound(sound);
        }
    }

    private void SetParticleColor(ParticleSystem ps, Color color)
    {
        var main = ps.main;
        main.startColor = color;

        // Apply color to child particle systems
        foreach (ParticleSystem child in ps.GetComponentsInChildren<ParticleSystem>())
        {
            if (child != ps)
            {
                var childMain = child.main;
                childMain.startColor = color;
            }
        }
    }

    public void PlayElementalAura(Transform target, HabitatSystem.ElementType elementType)
    {
        if (elementalVFXDict.TryGetValue(elementType, out ElementalVFX elementalVFX))
        {
            ParticleSystem aura = Instantiate(elementalVFX.elementalAura, target.position, Quaternion.identity);
            aura.transform.SetParent(target);
            activeEffects.Add(aura);
        }
    }

    public void StopAllEffects()
    {
        foreach (var effect in activeEffects)
        {
            if (effect != null)
            {
                effect.Stop();
                Destroy(effect.gameObject, effect.main.duration);
            }
        }
        activeEffects.Clear();
    }

    private void OnDestroy()
    {
        StopAllEffects();
    }
}

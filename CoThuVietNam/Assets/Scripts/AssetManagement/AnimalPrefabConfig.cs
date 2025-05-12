using UnityEngine;

[CreateAssetMenu(fileName = "AnimalPrefabConfig", menuName = "CoThuVietNam/Animal Prefab Config")]
public class AnimalPrefabConfig : ScriptableObject
{
    [System.Serializable]
    public class VisualConfig
    {
        [Header("Basic Visuals")]
        public Sprite defaultSprite;
        public RuntimeAnimatorController animatorController;
        public Color tintColor = Color.white;
        public Vector2 spriteScale = Vector2.one;
        public Vector2 colliderSize = Vector2.one;
        public Vector2 shadowOffset = new Vector2(0, -0.5f);
        
        [Header("Rarity Variations")]
        public Color rarityGlowColor;
        public float glowIntensity = 1f;
        public float outlineWidth = 1f;
        public bool useCustomShader;
        public Material customMaterial;
    }

    [System.Serializable]
    public class AnimationConfig
    {
        [Header("Animation Clips")]
        public AnimationClip idleAnimation;
        public AnimationClip walkAnimation;
        public AnimationClip attackAnimation;
        public AnimationClip skillAnimation;
        public AnimationClip hurtAnimation;
        public AnimationClip deathAnimation;
        public AnimationClip victoryAnimation;
        public AnimationClip evolutionAnimation;

        [Header("Animation Settings")]
        public float idleSpeed = 1f;
        public float walkSpeed = 1f;
        public float attackSpeed = 1f;
        public float skillSpeed = 1f;
        public bool useRootMotion;
        public bool mirrorAnimations;
    }

    [System.Serializable]
    public class EffectConfig
    {
        [Header("Common Effects")]
        public ParticleSystem spawnEffect;
        public ParticleSystem levelUpEffect;
        public ParticleSystem evolutionEffect;
        public ParticleSystem deathEffect;

        [Header("Combat Effects")]
        public ParticleSystem[] attackEffects;
        public ParticleSystem[] skillEffects;
        public ParticleSystem hitEffect;
        public ParticleSystem criticalHitEffect;
        public ParticleSystem healEffect;
        public ParticleSystem shieldEffect;

        [Header("Status Effects")]
        public ParticleSystem stunEffect;
        public ParticleSystem poisonEffect;
        public ParticleSystem burnEffect;
        public ParticleSystem freezeEffect;

        [Header("Trail Effects")]
        public TrailRenderer movementTrail;
        public TrailRenderer attackTrail;
        public Color trailColor;
    }

    [System.Serializable]
    public class AudioConfig
    {
        [Header("Basic Sounds")]
        public AudioClip spawnSound;
        public AudioClip moveSound;
        public AudioClip attackSound;
        public AudioClip hurtSound;
        public AudioClip deathSound;

        [Header("Special Sounds")]
        public AudioClip[] skillSounds;
        public AudioClip evolutionSound;
        public AudioClip victorySound;
        public AudioClip levelUpSound;

        [Header("Sound Settings")]
        public float volumeMultiplier = 1f;
        public float pitchVariation = 0.1f;
        public bool usePositionalAudio = true;
    }

    [Header("Animal Information")]
    public AnimalType animalType;
    public HabitatSystem.Habitat habitat;
    public string displayName;
    public string description;

    [Header("Configurations")]
    public VisualConfig visuals;
    public AnimationConfig animations;
    public EffectConfig effects;
    public AudioConfig audio;

    [Header("Prefab Settings")]
    public GameObject basePrefab;
    public bool useCustomPrefab;
    public Vector3 spawnOffset = Vector3.zero;
    public Vector3 effectOffset = Vector3.zero;
    public bool orientToMovement = true;

    public GameObject CreatePrefab()
    {
        GameObject prefab = useCustomPrefab ? basePrefab : CreateDefaultPrefab();
        SetupPrefabComponents(prefab);
        return prefab;
    }

    private GameObject CreateDefaultPrefab()
    {
        GameObject prefab = new GameObject(displayName);

        // Add basic components
        SpriteRenderer spriteRenderer = prefab.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = visuals.defaultSprite;
        spriteRenderer.color = visuals.tintColor;
        spriteRenderer.sortingOrder = 1;

        // Add collider
        BoxCollider2D collider = prefab.AddComponent<BoxCollider2D>();
        collider.size = visuals.colliderSize;
        collider.isTrigger = true;

        // Add shadow
        GameObject shadow = new GameObject("Shadow");
        shadow.transform.SetParent(prefab.transform);
        shadow.transform.localPosition = visuals.shadowOffset;
        SpriteRenderer shadowRenderer = shadow.AddComponent<SpriteRenderer>();
        shadowRenderer.sprite = visuals.defaultSprite;
        shadowRenderer.color = new Color(0, 0, 0, 0.3f);
        shadowRenderer.sortingOrder = 0;

        return prefab;
    }

    private void SetupPrefabComponents(GameObject prefab)
    {
        // Add required components
        Animator animator = prefab.GetComponent<Animator>() ?? prefab.AddComponent<Animator>();
        AnimalAnimationController animController = prefab.GetComponent<AnimalAnimationController>() ?? 
                                                 prefab.AddComponent<AnimalAnimationController>();
        AudioSource audioSource = prefab.GetComponent<AudioSource>() ?? prefab.AddComponent<AudioSource>();

        // Configure animator
        animator.runtimeAnimatorController = visuals.animatorController;
        animator.applyRootMotion = animations.useRootMotion;

        // Configure sprite renderer
        SpriteRenderer spriteRenderer = prefab.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            if (visuals.useCustomShader && visuals.customMaterial != null)
            {
                spriteRenderer.material = visuals.customMaterial;
            }
            spriteRenderer.transform.localScale = visuals.spriteScale;
        }

        // Configure audio source
        audioSource.spatialBlend = audio.usePositionalAudio ? 1f : 0f;
        audioSource.volume = audio.volumeMultiplier;
        audioSource.playOnAwake = false;

        // Add trail renderer if specified
        if (effects.movementTrail != null)
        {
            TrailRenderer trail = prefab.AddComponent<TrailRenderer>();
            trail.colorGradient = new Gradient()
            {
                colorKeys = new GradientColorKey[] 
                {
                    new GradientColorKey(effects.trailColor, 0f),
                    new GradientColorKey(effects.trailColor with { a = 0 }, 1f)
                }
            };
            trail.time = 0.5f;
            trail.startWidth = 0.2f;
            trail.endWidth = 0f;
        }

        // Add particle systems
        if (effects.spawnEffect != null)
        {
            ParticleSystem spawn = Instantiate(effects.spawnEffect, prefab.transform);
            spawn.transform.localPosition = effectOffset;
        }

        // Set up rarity effects if needed
        if (visuals.glowIntensity > 0)
        {
            GameObject glowObj = new GameObject("Glow");
            glowObj.transform.SetParent(prefab.transform);
            glowObj.transform.localPosition = Vector3.zero;
            SpriteRenderer glowRenderer = glowObj.AddComponent<SpriteRenderer>();
            glowRenderer.sprite = visuals.defaultSprite;
            glowRenderer.color = visuals.rarityGlowColor * visuals.glowIntensity;
            glowRenderer.sortingOrder = -1;
        }
    }
}

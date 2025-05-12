using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class VFXManager : MonoBehaviour
{
    public static VFXManager Instance { get; private set; }

    [System.Serializable]
    public class VFXData
    {
        public string effectName;
        public ParticleSystem particlePrefab;
        public float duration = 2f;
        public bool autoDestroy = true;
        public AudioClip sound;
    }

    [Header("Effect Prefabs")]
    [SerializeField] private VFXData[] effects;
    [SerializeField] private int poolSize = 10;

    [Header("Trail Effects")]
    [SerializeField] private TrailRenderer moveTrailPrefab;
    [SerializeField] private TrailRenderer attackTrailPrefab;
    [SerializeField] private float trailDuration = 0.5f;

    private Dictionary<string, Queue<ParticleSystem>> particlePool;
    private Dictionary<string, VFXData> effectDictionary;
    private List<ParticleSystem> activeEffects;
    private Transform poolContainer;

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
        // Create containers
        poolContainer = new GameObject("VFX_Pool").transform;
        poolContainer.SetParent(transform);
        
        particlePool = new Dictionary<string, Queue<ParticleSystem>>();
        effectDictionary = new Dictionary<string, VFXData>();
        activeEffects = new List<ParticleSystem>();

        // Initialize effect dictionary
        foreach (var effect in effects)
        {
            effectDictionary[effect.effectName] = effect;
            InitializePool(effect);
        }
    }

    private void InitializePool(VFXData effect)
    {
        Queue<ParticleSystem> queue = new Queue<ParticleSystem>();
        for (int i = 0; i < poolSize; i++)
        {
            ParticleSystem instance = CreateParticleInstance(effect);
            queue.Enqueue(instance);
        }
        particlePool[effect.effectName] = queue;
    }

    private ParticleSystem CreateParticleInstance(VFXData effect)
    {
        ParticleSystem instance = Instantiate(effect.particlePrefab, poolContainer);
        instance.gameObject.SetActive(false);
        return instance;
    }

    #region Public Effect Methods

    public void PlayEffect(string effectName, Vector3 position, Quaternion rotation = default)
    {
        if (!effectDictionary.ContainsKey(effectName))
        {
            Debug.LogWarning($"Effect {effectName} not found!");
            return;
        }

        VFXData effectData = effectDictionary[effectName];
        ParticleSystem particleSystem = GetParticleFromPool(effectName);
        
        if (particleSystem != null)
        {
            particleSystem.transform.position = position;
            particleSystem.transform.rotation = rotation;
            particleSystem.gameObject.SetActive(true);
            particleSystem.Play();

            // Play sound if available
            if (effectData.sound != null)
            {
                AudioManager.Instance.PlaySound(effectData.sound.name);
            }

            activeEffects.Add(particleSystem);

            if (effectData.autoDestroy)
            {
                StartCoroutine(ReturnToPool(particleSystem, effectName, effectData.duration));
            }
        }
    }

    public void PlayMoveTrail(Vector3 start, Vector3 end, float duration = -1)
    {
        TrailRenderer trail = Instantiate(moveTrailPrefab);
        trail.transform.position = start;
        StartCoroutine(AnimateTrail(trail, start, end, duration < 0 ? trailDuration : duration));
    }

    public void PlayAttackTrail(Vector3 start, Vector3 end, float duration = -1)
    {
        TrailRenderer trail = Instantiate(attackTrailPrefab);
        trail.transform.position = start;
        StartCoroutine(AnimateTrail(trail, start, end, duration < 0 ? trailDuration : duration));
    }

    public void PlaySkillEffect(string effectName, Vector3 position, Vector3 targetPosition, float duration)
    {
        if (!effectDictionary.ContainsKey(effectName))
        {
            Debug.LogWarning($"Skill effect {effectName} not found!");
            return;
        }

        StartCoroutine(AnimateSkillEffect(effectName, position, targetPosition, duration));
    }

    public void StopEffect(string effectName)
    {
        for (int i = activeEffects.Count - 1; i >= 0; i--)
        {
            ParticleSystem effect = activeEffects[i];
            if (effect.gameObject.name.Contains(effectName))
            {
                effect.Stop();
                ReturnParticleToPool(effect, effectName);
                activeEffects.RemoveAt(i);
            }
        }
    }

    public void StopAllEffects()
    {
        for (int i = activeEffects.Count - 1; i >= 0; i--)
        {
            ParticleSystem effect = activeEffects[i];
            effect.Stop();
            string effectName = effect.gameObject.name.Split('_')[0];
            ReturnParticleToPool(effect, effectName);
        }
        activeEffects.Clear();
    }

    #endregion

    #region Pool Management

    private ParticleSystem GetParticleFromPool(string effectName)
    {
        if (particlePool.TryGetValue(effectName, out Queue<ParticleSystem> queue))
        {
            if (queue.Count == 0)
            {
                // Create new instance if pool is empty
                return CreateParticleInstance(effectDictionary[effectName]);
            }
            return queue.Dequeue();
        }
        return null;
    }

    private void ReturnParticleToPool(ParticleSystem particle, string effectName)
    {
        if (particlePool.TryGetValue(effectName, out Queue<ParticleSystem> queue))
        {
            particle.gameObject.SetActive(false);
            particle.transform.SetParent(poolContainer);
            queue.Enqueue(particle);
        }
    }

    #endregion

    #region Coroutines

    private IEnumerator ReturnToPool(ParticleSystem particle, string effectName, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (particle != null)
        {
            activeEffects.Remove(particle);
            ReturnParticleToPool(particle, effectName);
        }
    }

    private IEnumerator AnimateTrail(TrailRenderer trail, Vector3 start, Vector3 end, float duration)
    {
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            trail.transform.position = Vector3.Lerp(start, end, t);
            yield return null;
        }

        // Wait for trail to fade
        yield return new WaitForSeconds(trail.time);
        Destroy(trail.gameObject);
    }

    private IEnumerator AnimateSkillEffect(string effectName, Vector3 start, Vector3 target, float duration)
    {
        ParticleSystem effect = GetParticleFromPool(effectName);
        if (effect != null)
        {
            effect.gameObject.SetActive(true);
            activeEffects.Add(effect);

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                effect.transform.position = Vector3.Lerp(start, target, t);
                yield return null;
            }

            effect.Stop();
            yield return new WaitForSeconds(effect.main.duration);
            
            activeEffects.Remove(effect);
            ReturnParticleToPool(effect, effectName);
        }
    }

    #endregion

    private void OnDestroy()
    {
        if (Instance == this)
        {
            StopAllEffects();
            Instance = null;
        }
    }
}

using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class AssetManager : MonoBehaviour
{
    public static AssetManager Instance { get; private set; }

    [System.Serializable]
    public class AnimalAssets
    {
        public Sprite icon;
        public Sprite portrait;
        public RuntimeAnimatorController animator;
        public GameObject prefab;
        public ParticleSystem[] skillEffects;
        public AudioClip[] soundEffects;
    }

    [System.Serializable]
    public class AnimationSet
    {
        public AnimationClip idle;
        public AnimationClip walk;
        public AnimationClip attack;
        public AnimationClip skill;
        public AnimationClip hurt;
        public AnimationClip death;
        public AnimationClip victory;
        public AnimationClip evolution;
    }

    private Dictionary<AnimalType, AnimalAssets> animalAssets;
    private Dictionary<string, Object> cachedAssets;
    private Dictionary<AnimalType, AnimationSet> animalAnimations;

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
        animalAssets = new Dictionary<AnimalType, AnimalAssets>();
        cachedAssets = new Dictionary<string, Object>();
        animalAnimations = new Dictionary<AnimalType, AnimationSet>();

        // Create required directories
        CreateDirectoryStructure();
        
        // Load all animal assets
        LoadAnimalAssets();
    }

    private void CreateDirectoryStructure()
    {
        CreateDirectory(AssetPaths.ANIMALS_ROOT);
        CreateDirectory(AssetPaths.EFFECTS_ROOT);
        CreateDirectory(AssetPaths.UI_ROOT);
        CreateDirectory(AssetPaths.ANIMATIONS_ROOT);
        CreateDirectory(AssetPaths.AUDIO_ROOT);

        CreateAnimalDomainDirectories();
        CreateEffectDirectories();
        CreateUIDirectories();
        CreateAudioDirectories();
    }

    private void CreateDirectory(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }

    private void LoadAnimalAssets()
    {
        foreach (AnimalType type in System.Enum.GetValues(typeof(AnimalType)))
        {
            string basePath = GetAnimalPath(type);
            if (Directory.Exists(basePath))
            {
                LoadAnimalAsset(type, basePath);
            }
        }
    }

    private void LoadAnimalAsset(AnimalType type, string basePath)
    {
        AnimalAssets assets = new AnimalAssets();

        // Load sprites
        assets.icon = LoadSprite($"{basePath}/icon.png");
        assets.portrait = LoadSprite($"{basePath}/portrait.png");

        // Load prefab
        assets.prefab = LoadPrefab($"{basePath}/prefab.prefab");

        // Load animations
        string animPath = $"{basePath}/Animations";
        AnimationSet animations = LoadAnimationSet(animPath);
        animalAnimations[type] = animations;

        // Create animator controller
        assets.animator = CreateAnimatorController(type, animations);

        // Load effects
        string effectsPath = $"{basePath}/Effects";
        assets.skillEffects = LoadSkillEffects(effectsPath);

        // Load sounds
        string soundsPath = $"{basePath}/Sounds";
        assets.soundEffects = LoadSoundEffects(soundsPath);

        animalAssets[type] = assets;
    }

    private Sprite LoadSprite(string path)
    {
        if (cachedAssets.TryGetValue(path, out Object cached))
            return cached as Sprite;

        if (File.Exists(path))
        {
            byte[] fileData = File.ReadAllBytes(path);
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(fileData);
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), 
                new Vector2(0.5f, 0.5f));
            cachedAssets[path] = sprite;
            return sprite;
        }
        return null;
    }

    private GameObject LoadPrefab(string path)
    {
        if (cachedAssets.TryGetValue(path, out Object cached))
            return cached as GameObject;

        GameObject prefab = Resources.Load<GameObject>(path);
        if (prefab != null)
            cachedAssets[path] = prefab;
        return prefab;
    }

    private AnimationSet LoadAnimationSet(string path)
    {
        AnimationSet set = new AnimationSet();
        set.idle = LoadAnimation($"{path}/{AssetPaths.Animations.IDLE}.anim");
        set.walk = LoadAnimation($"{path}/{AssetPaths.Animations.WALK}.anim");
        set.attack = LoadAnimation($"{path}/{AssetPaths.Animations.ATTACK}.anim");
        set.skill = LoadAnimation($"{path}/{AssetPaths.Animations.SKILL}.anim");
        set.hurt = LoadAnimation($"{path}/{AssetPaths.Animations.HURT}.anim");
        set.death = LoadAnimation($"{path}/{AssetPaths.Animations.DEATH}.anim");
        set.victory = LoadAnimation($"{path}/{AssetPaths.Animations.VICTORY}.anim");
        set.evolution = LoadAnimation($"{path}/{AssetPaths.Animations.EVOLUTION}.anim");
        return set;
    }

    private AnimationClip LoadAnimation(string path)
    {
        if (cachedAssets.TryGetValue(path, out Object cached))
            return cached as AnimationClip;

        AnimationClip clip = Resources.Load<AnimationClip>(path);
        if (clip != null)
            cachedAssets[path] = clip;
        return clip;
    }

    private RuntimeAnimatorController CreateAnimatorController(AnimalType type, AnimationSet animations)
    {
        AnimatorController controller = new AnimatorController();
        controller.name = $"{type}Controller";

        // Add parameters
        controller.AddParameter("IsMoving", AnimatorControllerParameterType.Bool);
        controller.AddParameter("Attack", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("Skill", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("Hurt", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("Die", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("Victory", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("Evolve", AnimatorControllerParameterType.Trigger);

        // Add states and transitions
        var rootStateMachine = controller.layers[0].stateMachine;

        var idleState = rootStateMachine.AddState("Idle");
        idleState.motion = animations.idle;

        var walkState = rootStateMachine.AddState("Walk");
        walkState.motion = animations.walk;

        var attackState = rootStateMachine.AddState("Attack");
        attackState.motion = animations.attack;

        var skillState = rootStateMachine.AddState("Skill");
        skillState.motion = animations.skill;

        var hurtState = rootStateMachine.AddState("Hurt");
        hurtState.motion = animations.hurt;

        var deathState = rootStateMachine.AddState("Death");
        deathState.motion = animations.death;

        var victoryState = rootStateMachine.AddState("Victory");
        victoryState.motion = animations.victory;

        var evolutionState = rootStateMachine.AddState("Evolution");
        evolutionState.motion = animations.evolution;

        // Add transitions
        CreateTransition(idleState, walkState, "IsMoving");
        CreateTransition(walkState, idleState, "IsMoving", false);
        CreateTransition(idleState, attackState, "Attack");
        CreateTransition(idleState, skillState, "Skill");
        CreateTransition(idleState, hurtState, "Hurt");
        CreateTransition(idleState, deathState, "Die");
        CreateTransition(idleState, victoryState, "Victory");
        CreateTransition(idleState, evolutionState, "Evolve");

        return controller;
    }

    private void CreateTransition(AnimatorState from, AnimatorState to, string parameter, bool value = true)
    {
        var transition = from.AddTransition(to);
        transition.hasExitTime = false;
        transition.duration = 0.25f;
        transition.AddCondition(value ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot, 0, parameter);
    }

    private ParticleSystem[] LoadSkillEffects(string path)
    {
        List<ParticleSystem> effects = new List<ParticleSystem>();
        if (Directory.Exists(path))
        {
            string[] files = Directory.GetFiles(path, "*.prefab");
            foreach (string file in files)
            {
                GameObject effectPrefab = LoadPrefab(file);
                if (effectPrefab != null)
                {
                    ParticleSystem particleSystem = effectPrefab.GetComponent<ParticleSystem>();
                    if (particleSystem != null)
                        effects.Add(particleSystem);
                }
            }
        }
        return effects.ToArray();
    }

    private AudioClip[] LoadSoundEffects(string path)
    {
        List<AudioClip> sounds = new List<AudioClip>();
        if (Directory.Exists(path))
        {
            string[] files = Directory.GetFiles(path, "*.wav");
            foreach (string file in files)
            {
                AudioClip clip = Resources.Load<AudioClip>(file);
                if (clip != null)
                    sounds.Add(clip);
            }
        }
        return sounds.ToArray();
    }

    public AnimalAssets GetAnimalAssets(AnimalType type)
    {
        return animalAssets.TryGetValue(type, out AnimalAssets assets) ? assets : null;
    }

    public AnimationSet GetAnimalAnimations(AnimalType type)
    {
        return animalAnimations.TryGetValue(type, out AnimationSet animations) ? animations : null;
    }

    private string GetAnimalPath(AnimalType type)
    {
        switch (HabitatSystem.Instance.GetPrimaryHabitat(type))
        {
            case HabitatSystem.Habitat.Sky:
                return AssetPaths.Animals.Sky.GetType().GetField(type.ToString())?.GetValue(null) as string;
            case HabitatSystem.Habitat.Land:
                return AssetPaths.Animals.Land.GetType().GetField(type.ToString())?.GetValue(null) as string;
            case HabitatSystem.Habitat.Sea:
                return AssetPaths.Animals.Sea.GetType().GetField(type.ToString())?.GetValue(null) as string;
            default:
                return null;
        }
    }

    private void OnDestroy()
    {
        // Clean up cached assets
        foreach (var asset in cachedAssets.Values)
        {
            if (asset is UnityEngine.Object obj)
                Destroy(obj);
        }
        cachedAssets.Clear();
    }
}

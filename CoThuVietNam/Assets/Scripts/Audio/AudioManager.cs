using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [System.Serializable]
    public class Sound
    {
        public string name;
        public AudioClip clip;
        [Range(0f, 1f)]
        public float volume = 1f;
        [Range(0.1f, 3f)]
        public float pitch = 1f;
        public bool loop;
        public AudioMixerGroup mixerGroup;

        [HideInInspector]
        public AudioSource source;
    }

    [Header("Audio Settings")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private Sound[] sounds;
    
    [Header("Volume Settings")]
    [SerializeField] private float masterVolume = 1f;
    [SerializeField] private float musicVolume = 1f;
    [SerializeField] private float sfxVolume = 1f;

    private Dictionary<string, Sound> soundDictionary;
    private Sound currentMusic;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudio();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeAudio()
    {
        soundDictionary = new Dictionary<string, Sound>();

        foreach (Sound s in sounds)
        {
            GameObject soundObject = new GameObject($"Sound_{s.name}");
            soundObject.transform.SetParent(transform);
            
            s.source = soundObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
            
            if (s.mixerGroup != null)
                s.source.outputAudioMixerGroup = s.mixerGroup;

            soundDictionary.Add(s.name, s);
        }
    }

    #region Sound Control Methods

    public void PlaySound(string name)
    {
        if (soundDictionary.TryGetValue(name, out Sound sound))
        {
            sound.source.Play();
        }
        else
        {
            Debug.LogWarning($"Sound {name} not found!");
        }
    }

    public void PlayMusic(string name)
    {
        if (currentMusic != null)
        {
            currentMusic.source.Stop();
        }

        if (soundDictionary.TryGetValue(name, out Sound music))
        {
            currentMusic = music;
            music.source.Play();
        }
        else
        {
            Debug.LogWarning($"Music {name} not found!");
        }
    }

    public void StopSound(string name)
    {
        if (soundDictionary.TryGetValue(name, out Sound sound))
        {
            sound.source.Stop();
        }
    }

    public void PauseSound(string name)
    {
        if (soundDictionary.TryGetValue(name, out Sound sound))
        {
            sound.source.Pause();
        }
    }

    public void ResumeSound(string name)
    {
        if (soundDictionary.TryGetValue(name, out Sound sound))
        {
            sound.source.UnPause();
        }
    }

    #endregion

    #region Volume Control Methods

    public void SetMasterVolume(float volume)
    {
        masterVolume = volume;
        audioMixer.SetFloat("MasterVolume", Mathf.Log10(volume) * 20);
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = volume;
        audioMixer.SetFloat("MusicVolume", Mathf.Log10(volume) * 20);
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = volume;
        audioMixer.SetFloat("SFXVolume", Mathf.Log10(volume) * 20);
    }

    #endregion

    #region Game-Specific Sound Methods

    public void PlayAnimalSound(AnimalType type)
    {
        string soundName = $"Animal_{type}";
        PlaySound(soundName);
    }

    public void PlaySkillSound(string skillName)
    {
        string soundName = $"Skill_{skillName}";
        PlaySound(soundName);
    }

    public void PlayUISound(string uiElement)
    {
        string soundName = $"UI_{uiElement}";
        PlaySound(soundName);
    }

    public void PlayVictoryMusic()
    {
        PlayMusic("Victory");
    }

    public void PlayGameplayMusic()
    {
        PlayMusic("Gameplay");
    }

    public void PlayMenuMusic()
    {
        PlayMusic("Menu");
    }

    public void PlayMoveSound()
    {
        PlaySound("Move");
    }

    public void PlayAttackSound()
    {
        PlaySound("Attack");
    }

    public void PlaySelectSound()
    {
        PlaySound("Select");
    }

    public void PlayButtonClickSound()
    {
        PlaySound("ButtonClick");
    }

    #endregion

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}

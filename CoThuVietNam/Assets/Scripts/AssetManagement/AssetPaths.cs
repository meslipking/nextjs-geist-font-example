using UnityEngine;

public static class AssetPaths
{
    // Root paths
    public const string ANIMALS_ROOT = "Assets/Resources/Animals/";
    public const string EFFECTS_ROOT = "Assets/Resources/Effects/";
    public const string UI_ROOT = "Assets/Resources/UI/";
    public const string ANIMATIONS_ROOT = "Assets/Resources/Animations/";
    public const string AUDIO_ROOT = "Assets/Resources/Audio/";

    // Animal assets
    public static class Animals
    {
        // Sky Domain
        public static class Sky
        {
            public const string DRAGON = ANIMALS_ROOT + "Sky/Dragon/";
            public const string PHOENIX = ANIMALS_ROOT + "Sky/Phoenix/";
            public const string GRIFFIN = ANIMALS_ROOT + "Sky/Griffin/";
            public const string PEGASUS = ANIMALS_ROOT + "Sky/Pegasus/";
        }

        // Land Domain
        public static class Land
        {
            public const string TIGER = ANIMALS_ROOT + "Land/Tiger/";
            public const string LION = ANIMALS_ROOT + "Land/Lion/";
            public const string ELEPHANT = ANIMALS_ROOT + "Land/Elephant/";
            public const string WOLF = ANIMALS_ROOT + "Land/Wolf/";
            public const string DOG = ANIMALS_ROOT + "Land/Dog/";
            public const string FOX = ANIMALS_ROOT + "Land/Fox/";
            public const string UNICORN = ANIMALS_ROOT + "Land/Unicorn/";
        }

        // Sea Domain
        public static class Sea
        {
            public const string KRAKEN = ANIMALS_ROOT + "Sea/Kraken/";
            public const string HYDRA = ANIMALS_ROOT + "Sea/Hydra/";
            public const string MOUSE = ANIMALS_ROOT + "Sea/Mouse/";
        }
    }

    // Animation paths
    public static class Animations
    {
        public const string IDLE = "Idle";
        public const string WALK = "Walk";
        public const string ATTACK = "Attack";
        public const string SKILL = "Skill";
        public const string HURT = "Hurt";
        public const string DEATH = "Death";
        public const string VICTORY = "Victory";
        public const string EVOLUTION = "Evolution";
    }

    // Effect paths
    public static class Effects
    {
        public const string SUMMON = EFFECTS_ROOT + "Summon/";
        public const string EVOLUTION = EFFECTS_ROOT + "Evolution/";
        public const string COMBAT = EFFECTS_ROOT + "Combat/";
        public const string SKILLS = EFFECTS_ROOT + "Skills/";
        public const string ENVIRONMENT = EFFECTS_ROOT + "Environment/";
    }

    // UI paths
    public static class UI
    {
        public const string ICONS = UI_ROOT + "Icons/";
        public const string FRAMES = UI_ROOT + "Frames/";
        public const string BACKGROUNDS = UI_ROOT + "Backgrounds/";
        public const string BUTTONS = UI_ROOT + "Buttons/";
        public const string POPUPS = UI_ROOT + "Popups/";
    }

    // Audio paths
    public static class Audio
    {
        public const string BGM = AUDIO_ROOT + "BGM/";
        public const string SFX = AUDIO_ROOT + "SFX/";
        public const string VOICE = AUDIO_ROOT + "Voice/";
    }
}

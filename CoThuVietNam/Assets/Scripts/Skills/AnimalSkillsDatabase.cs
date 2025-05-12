using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "AnimalSkillsDatabase", menuName = "CoThuVietNam/SkillsDatabase")]
public class AnimalSkillsDatabase : ScriptableObject
{
    [System.Serializable]
    public class SkillEntry
    {
        public AnimalType animalType;
        public SkillData[] skills;
        public string[] skillDescriptions;
        public string[] animationTriggers;
        public ParticleSystem[] skillEffects;
        public AudioClip[] skillSounds;
    }

    public List<SkillEntry> skillEntries = new List<SkillEntry>
    {
        // SSS+ Tier Skills
        new SkillEntry {
            animalType = AnimalType.Dragon,
            skills = new SkillData[] {
                CreateSkill("Dragon's Breath", "Unleashes a devastating flame attack in a line", 15f, 
                    SkillType.Attack, 3f, 150f),
                CreateSkill("Ancient Wisdom", "Enhances all allies' abilities", 20f, 
                    SkillType.Support, 5f, 0f),
                CreateSkill("Dragon Scale", "Creates an impenetrable shield", 12f, 
                    SkillType.Defense, 4f, 0f)
            }
        },

        // SSS Tier Skills
        new SkillEntry {
            animalType = AnimalType.Phoenix,
            skills = new SkillData[] {
                CreateSkill("Rebirth Flame", "Revives with 50% HP upon death", 60f, 
                    SkillType.Special, 0f, 0f),
                CreateSkill("Solar Flare", "Blinds and damages nearby enemies", 12f, 
                    SkillType.Attack, 2f, 80f),
                CreateSkill("Healing Embers", "Regenerates HP over time", 15f, 
                    SkillType.Support, 5f, 0f)
            }
        },

        // SSR Tier Skills
        new SkillEntry {
            animalType = AnimalType.Unicorn,
            skills = new SkillData[] {
                CreateSkill("Holy Light", "Heals all nearby allies", 10f, 
                    SkillType.Support, 3f, 0f),
                CreateSkill("Purifying Ray", "Removes debuffs and deals damage", 15f, 
                    SkillType.Special, 2f, 60f),
                CreateSkill("Blessing", "Increases allies' defense", 18f, 
                    SkillType.Defense, 4f, 0f)
            }
        },

        // SR Tier Skills
        new SkillEntry {
            animalType = AnimalType.Griffin,
            skills = new SkillData[] {
                CreateSkill("Aerial Strike", "Attacks from above, ignoring obstacles", 8f, 
                    SkillType.Attack, 1f, 70f),
                CreateSkill("Wind Gust", "Pushes enemies back", 12f, 
                    SkillType.Special, 2f, 40f),
                CreateSkill("Sky Watch", "Reveals hidden enemies", 15f, 
                    SkillType.Support, 3f, 0f)
            }
        },

        // More skill entries for other animals...
        new SkillEntry {
            animalType = AnimalType.Hydra,
            skills = new SkillData[] {
                CreateSkill("Multi-Head Strike", "Attacks multiple targets", 10f, 
                    SkillType.Attack, 2f, 65f),
                CreateSkill("Head Regeneration", "Recovers HP and increases attack", 20f, 
                    SkillType.Special, 4f, 0f),
                CreateSkill("Poison Breath", "Applies poison to enemies", 15f, 
                    SkillType.Attack, 3f, 45f)
            }
        },

        new SkillEntry {
            animalType = AnimalType.Tiger,
            skills = new SkillData[] {
                CreateSkill("Pounce", "Leaps to target location and attacks", 8f, 
                    SkillType.Movement, 1f, 80f),
                CreateSkill("Savage Roar", "Intimidates nearby enemies", 12f, 
                    SkillType.Special, 2f, 0f),
                CreateSkill("Predator's Mark", "Marks target for bonus damage", 15f, 
                    SkillType.Attack, 3f, 40f)
            }
        }

        // Additional skill entries will be added in the extension file...
    };

    private static SkillData CreateSkill(string name, string description, float cooldown, 
        SkillType type, float castTime, float damage)
    {
        var skill = ScriptableObject.CreateInstance<SkillData>();
        skill.skillName = name;
        skill.description = description;
        skill.cooldown = cooldown;
        skill.type = type;
        skill.castTime = castTime;
        skill.damage = damage;
        return skill;
    }

    // Helper methods
    public SkillData[] GetAnimalSkills(AnimalType type)
    {
        return skillEntries.Find(entry => entry.animalType == type)?.skills ?? new SkillData[0];
    }

    public string[] GetSkillDescriptions(AnimalType type)
    {
        return skillEntries.Find(entry => entry.animalType == type)?.skillDescriptions ?? new string[0];
    }

    public ParticleSystem[] GetSkillEffects(AnimalType type)
    {
        return skillEntries.Find(entry => entry.animalType == type)?.skillEffects ?? new ParticleSystem[0];
    }

    public AudioClip[] GetSkillSounds(AnimalType type)
    {
        return skillEntries.Find(entry => entry.animalType == type)?.skillSounds ?? new AudioClip[0];
    }

    public string[] GetAnimationTriggers(AnimalType type)
    {
        return skillEntries.Find(entry => entry.animalType == type)?.animationTriggers ?? new string[0];
    }
}

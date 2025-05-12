using UnityEngine;
using System;

[CreateAssetMenu(fileName = "New Skill", menuName = "CoThuVietNam/Skill")]
public class SkillData : ScriptableObject
{
    [Header("Basic Properties")]
    public string skillName;
    public string description;
    public float cooldown;
    public SkillType type;
    public float range;
    public float damage;

    [Header("Animation")]
    public string animationTrigger;
    public float castTime;
    public ParticleSystem effectPrefab;

    [Header("Special Effects")]
    public bool hasStatusEffect;
    public StatusEffectType statusEffect;
    public float statusEffectDuration;

    private float currentCooldown;

    public virtual void Execute(Animal user, Animal target)
    {
        if (currentCooldown <= 0)
        {
            ApplySkillEffect(user, target);
            currentCooldown = cooldown;
        }
    }

    protected virtual void ApplySkillEffect(Animal user, Animal target)
    {
        // Base implementation
        if (effectPrefab != null)
        {
            var effect = Instantiate(effectPrefab, target.transform.position, Quaternion.identity);
            Destroy(effect.gameObject, effect.main.duration);
        }

        // Apply damage
        if (damage > 0)
        {
            // Apply damage logic here
        }

        // Apply status effect
        if (hasStatusEffect)
        {
            ApplyStatusEffect(target);
        }
    }

    private void ApplyStatusEffect(Animal target)
    {
        // Implementation for status effects
        switch (statusEffect)
        {
            case StatusEffectType.Stun:
                // Implement stun logic
                break;
            case StatusEffectType.Slow:
                // Implement slow logic
                break;
            case StatusEffectType.Buff:
                // Implement buff logic
                break;
            case StatusEffectType.Debuff:
                // Implement debuff logic
                break;
        }
    }

    public void UpdateCooldown()
    {
        if (currentCooldown > 0)
        {
            currentCooldown -= Time.deltaTime;
        }
    }

    public float GetCooldownProgress()
    {
        return Mathf.Clamp01(currentCooldown / cooldown);
    }
}

public enum SkillType
{
    Attack,    // Direct damage skills
    Defense,   // Protective abilities
    Support,   // Buffs and healing
    Special,   // Unique abilities
    Movement   // Special movement skills
}

public enum StatusEffectType
{
    None,
    Stun,    // Cannot move or act
    Slow,    // Reduced movement speed
    Buff,    // Increased stats
    Debuff   // Decreased stats
}

// Example skill implementations
[CreateAssetMenu(fileName = "TigerLeap", menuName = "CoThuVietNam/Skills/TigerLeap")]
public class TigerLeap : SkillData
{
    public float leapDistance = 2f;

    protected override void ApplySkillEffect(Animal user, Animal target)
    {
        base.ApplySkillEffect(user, target);
        // Implement tiger's leap logic
    }
}

[CreateAssetMenu(fileName = "ElephantStomp", menuName = "CoThuVietNam/Skills/ElephantStomp")]
public class ElephantStomp : SkillData
{
    public float stunDuration = 1.5f;
    public float areaOfEffect = 2f;

    protected override void ApplySkillEffect(Animal user, Animal target)
    {
        base.ApplySkillEffect(user, target);
        // Implement elephant's stomp logic
    }
}

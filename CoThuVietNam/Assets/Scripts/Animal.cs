using UnityEngine;

public class Animal : MonoBehaviour
{
    [Header("Animal Properties")]
    public string animalName;
    public int moveRange;
    public int attackPower;
    public AnimalType type;

    [Header("Skills")]
    public SkillData[] skills;

    private Animator animator;
    private bool isSelected;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void Move(Vector2 destination)
    {
        // Implement movement logic
        animator.SetTrigger("Move");
    }

    public void Attack(Animal target)
    {
        // Implement attack logic
        animator.SetTrigger("Attack");
    }

    public void UseSkill(int skillIndex, Animal target)
    {
        if (skillIndex < skills.Length)
        {
            skills[skillIndex].Execute(this, target);
            animator.SetTrigger("UseSkill");
        }
    }

    // Animation event handlers
    public void OnMoveComplete()
    {
        // Called when move animation completes
    }

    public void OnAttackComplete()
    {
        // Called when attack animation completes
    }

    public void OnSkillComplete()
    {
        // Called when skill animation completes
    }
}

public enum AnimalType
{
    Tiger,    // Can jump over other pieces
    Lion,     // Strong attack power
    Elephant, // High defense
    Mouse,    // Can move through water
    Cat,      // Quick movement
    Dog,      // Loyal defender
    Wolf,     // Pack tactics
    Fox       // Tricky movement patterns
}

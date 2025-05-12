using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AnimationController : MonoBehaviour
{
    [System.Serializable]
    public class AnimationData
    {
        public AnimalType animalType;
        public RuntimeAnimatorController animatorController;
        public ParticleSystem[] skillEffects;
    }

    [Header("Animation Settings")]
    [SerializeField] private AnimationData[] animalAnimations;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 720f;
    [SerializeField] private float jumpHeight = 1f;
    [SerializeField] private AnimationCurve jumpCurve;

    private Dictionary<AnimalType, AnimationData> animationDictionary;
    private Dictionary<Animal, Coroutine> activeAnimations;

    private void Awake()
    {
        InitializeDictionaries();
    }

    private void InitializeDictionaries()
    {
        animationDictionary = new Dictionary<AnimalType, AnimationData>();
        foreach (var data in animalAnimations)
        {
            animationDictionary[data.animalType] = data;
        }

        activeAnimations = new Dictionary<Animal, Coroutine>();
    }

    public void SetupAnimalAnimator(Animal animal)
    {
        if (animationDictionary.TryGetValue(animal.type, out AnimationData data))
        {
            Animator animator = animal.GetComponent<Animator>();
            if (animator != null)
            {
                animator.runtimeAnimatorController = data.animatorController;
            }
        }
    }

    #region Movement Animations

    public void PlayMoveAnimation(Animal animal, Vector3 targetPosition)
    {
        if (activeAnimations.ContainsKey(animal))
        {
            StopCoroutine(activeAnimations[animal]);
        }

        Coroutine moveCoroutine = StartCoroutine(MoveAnimationCoroutine(animal, targetPosition));
        activeAnimations[animal] = moveCoroutine;
    }

    private IEnumerator MoveAnimationCoroutine(Animal animal, Vector3 targetPosition)
    {
        Animator animator = animal.GetComponent<Animator>();
        animator.SetBool("IsMoving", true);

        Vector3 startPosition = animal.transform.position;
        float distance = Vector3.Distance(startPosition, targetPosition);
        float duration = distance / moveSpeed;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Move
            animal.transform.position = Vector3.Lerp(startPosition, targetPosition, t);

            // Rotate towards movement direction
            Vector3 direction = (targetPosition - startPosition).normalized;
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                animal.transform.rotation = Quaternion.RotateTowards(
                    animal.transform.rotation,
                    targetRotation,
                    rotationSpeed * Time.deltaTime
                );
            }

            yield return null;
        }

        animal.transform.position = targetPosition;
        animator.SetBool("IsMoving", false);
        activeAnimations.Remove(animal);
    }

    public void PlayJumpAnimation(Animal animal, Vector3 targetPosition)
    {
        if (activeAnimations.ContainsKey(animal))
        {
            StopCoroutine(activeAnimations[animal]);
        }

        Coroutine jumpCoroutine = StartCoroutine(JumpAnimationCoroutine(animal, targetPosition));
        activeAnimations[animal] = jumpCoroutine;
    }

    private IEnumerator JumpAnimationCoroutine(Animal animal, Vector3 targetPosition)
    {
        Animator animator = animal.GetComponent<Animator>();
        animator.SetTrigger("Jump");

        Vector3 startPosition = animal.transform.position;
        float distance = Vector3.Distance(startPosition, targetPosition);
        float duration = distance / moveSpeed;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Calculate position with jump arc
            Vector3 currentPos = Vector3.Lerp(startPosition, targetPosition, t);
            float heightOffset = jumpCurve.Evaluate(t) * jumpHeight;
            currentPos.y += heightOffset;

            animal.transform.position = currentPos;

            // Rotate towards movement direction
            Vector3 direction = (targetPosition - startPosition).normalized;
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                animal.transform.rotation = Quaternion.RotateTowards(
                    animal.transform.rotation,
                    targetRotation,
                    rotationSpeed * Time.deltaTime
                );
            }

            yield return null;
        }

        animal.transform.position = targetPosition;
        activeAnimations.Remove(animal);
    }

    #endregion

    #region Combat Animations

    public void PlayAttackAnimation(Animal attacker, Animal target)
    {
        if (activeAnimations.ContainsKey(attacker))
        {
            StopCoroutine(activeAnimations[attacker]);
        }

        Coroutine attackCoroutine = StartCoroutine(AttackAnimationCoroutine(attacker, target));
        activeAnimations[attacker] = attackCoroutine;
    }

    private IEnumerator AttackAnimationCoroutine(Animal attacker, Animal target)
    {
        Animator animator = attacker.GetComponent<Animator>();
        animator.SetTrigger("Attack");

        // Face the target
        Vector3 direction = (target.transform.position - attacker.transform.position).normalized;
        attacker.transform.rotation = Quaternion.LookRotation(direction);

        // Wait for attack animation to complete
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);

        activeAnimations.Remove(attacker);
    }

    public void PlayHitAnimation(Animal target)
    {
        if (target != null)
        {
            Animator animator = target.GetComponent<Animator>();
            animator.SetTrigger("Hit");
        }
    }

    public void PlayDeathAnimation(Animal target)
    {
        if (target != null)
        {
            Animator animator = target.GetComponent<Animator>();
            animator.SetTrigger("Death");
        }
    }

    #endregion

    #region Skill Animations

    public void PlaySkillAnimation(Animal animal, SkillData skill, Vector3 targetPosition)
    {
        if (activeAnimations.ContainsKey(animal))
        {
            StopCoroutine(activeAnimations[animal]);
        }

        Coroutine skillCoroutine = StartCoroutine(SkillAnimationCoroutine(animal, skill, targetPosition));
        activeAnimations[animal] = skillCoroutine;
    }

    private IEnumerator SkillAnimationCoroutine(Animal animal, SkillData skill, Vector3 targetPosition)
    {
        Animator animator = animal.GetComponent<Animator>();
        animator.SetTrigger(skill.animationTrigger);

        // Face the target
        Vector3 direction = (targetPosition - animal.transform.position).normalized;
        animal.transform.rotation = Quaternion.LookRotation(direction);

        // Play skill effects
        if (animationDictionary.TryGetValue(animal.type, out AnimationData data))
        {
            foreach (var effect in data.skillEffects)
            {
                ParticleSystem skillEffect = Instantiate(effect, targetPosition, Quaternion.identity);
                skillEffect.Play();
                Destroy(skillEffect.gameObject, skillEffect.main.duration);
            }
        }

        // Wait for skill animation to complete
        yield return new WaitForSeconds(skill.castTime);

        activeAnimations.Remove(animal);
    }

    #endregion

    public void StopAllAnimations(Animal animal)
    {
        if (activeAnimations.ContainsKey(animal))
        {
            StopCoroutine(activeAnimations[animal]);
            activeAnimations.Remove(animal);
        }

        Animator animator = animal.GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetBool("IsMoving", false);
            animator.ResetTrigger("Attack");
            animator.ResetTrigger("Jump");
            animator.ResetTrigger("Hit");
            animator.ResetTrigger("Death");
        }
    }
}

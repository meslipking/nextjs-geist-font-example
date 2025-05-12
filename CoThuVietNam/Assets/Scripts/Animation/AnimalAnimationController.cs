using UnityEngine;
using System.Collections;
using DG.Tweening;

[RequireComponent(typeof(Animator), typeof(SpriteRenderer))]
public class AnimalAnimationController : MonoBehaviour
{
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private ParticleSystem[] activeEffects;
    private AssetManager.AnimationSet animations;

    [Header("Animation Settings")]
    [SerializeField] private float attackDuration = 0.5f;
    [SerializeField] private float skillDuration = 1.0f;
    [SerializeField] private float hurtDuration = 0.3f;
    [SerializeField] private float deathDuration = 1.0f;
    [SerializeField] private float evolutionDuration = 2.0f;

    [Header("Visual Effects")]
    [SerializeField] private float flashDuration = 0.1f;
    [SerializeField] private Color hurtFlashColor = Color.red;
    [SerializeField] private Color evolutionFlashColor = Color.yellow;
    [SerializeField] private float shakeIntensity = 0.5f;
    [SerializeField] private int shakeVibrato = 10;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Initialize(AnimalType type)
    {
        // Get animal assets
        var assets = AssetManager.Instance.GetAnimalAssets(type);
        if (assets != null)
        {
            animator.runtimeAnimatorController = assets.animator;
            activeEffects = assets.skillEffects;
        }

        // Get animation set
        animations = AssetManager.Instance.GetAnimalAnimations(type);
    }

    #region Movement Animations

    public void SetMoving(bool isMoving)
    {
        animator.SetBool("IsMoving", isMoving);
    }

    public void SetDirection(Vector2 direction)
    {
        if (direction.x != 0)
        {
            spriteRenderer.flipX = direction.x < 0;
        }
    }

    #endregion

    #region Combat Animations

    public IEnumerator PlayAttackAnimation(Vector3 targetPosition)
    {
        // Face target
        SetDirection(targetPosition - transform.position);

        // Play attack animation
        animator.SetTrigger("Attack");

        // Quick forward movement
        Vector3 originalPos = transform.position;
        Vector3 attackPos = Vector3.Lerp(originalPos, targetPosition, 0.3f);

        transform.DOMove(attackPos, attackDuration * 0.3f)
            .SetEase(Ease.OutQuad);
        yield return new WaitForSeconds(attackDuration * 0.3f);

        // Return to original position
        transform.DOMove(originalPos, attackDuration * 0.2f)
            .SetEase(Ease.InQuad);
        yield return new WaitForSeconds(attackDuration * 0.7f);
    }

    public IEnumerator PlaySkillAnimation(Vector3 targetPosition)
    {
        // Face target
        SetDirection(targetPosition - transform.position);

        // Play skill animation
        animator.SetTrigger("Skill");

        // Play skill effect
        if (activeEffects != null && activeEffects.Length > 0)
        {
            foreach (var effect in activeEffects)
            {
                ParticleSystem skillEffect = Instantiate(effect, transform.position, Quaternion.identity);
                skillEffect.transform.LookAt(targetPosition);
                Destroy(skillEffect.gameObject, skillDuration);
            }
        }

        yield return new WaitForSeconds(skillDuration);
    }

    public IEnumerator PlayHurtAnimation(Vector3 attackerPosition)
    {
        // Face attacker
        SetDirection(attackerPosition - transform.position);

        // Play hurt animation
        animator.SetTrigger("Hurt");

        // Flash effect
        StartCoroutine(FlashSprite(hurtFlashColor));

        // Shake effect
        transform.DOShakePosition(hurtDuration, shakeIntensity, shakeVibrato)
            .SetEase(Ease.OutQuad);

        yield return new WaitForSeconds(hurtDuration);
    }

    public IEnumerator PlayDeathAnimation()
    {
        // Play death animation
        animator.SetTrigger("Die");

        // Fade out
        spriteRenderer.DOFade(0f, deathDuration)
            .SetEase(Ease.InQuad);

        // Fall effect
        transform.DORotate(new Vector3(0, 0, 90), deathDuration)
            .SetEase(Ease.InQuad);

        yield return new WaitForSeconds(deathDuration);
    }

    #endregion

    #region Special Animations

    public IEnumerator PlayEvolutionAnimation()
    {
        // Play evolution animation
        animator.SetTrigger("Evolve");

        // Initial effects
        StartCoroutine(FlashSprite(evolutionFlashColor));
        
        // Scale up effect
        transform.DOScale(transform.localScale * 1.2f, evolutionDuration * 0.5f)
            .SetEase(Ease.OutQuad);

        // Rotation effect
        transform.DORotate(new Vector3(0, 360, 0), evolutionDuration, RotateMode.FastBeyond360)
            .SetEase(Ease.InOutQuad);

        yield return new WaitForSeconds(evolutionDuration * 0.5f);

        // Evolution flash
        for (int i = 0; i < 3; i++)
        {
            spriteRenderer.DOFade(0.2f, 0.1f);
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.DOFade(1f, 0.1f);
            yield return new WaitForSeconds(0.1f);
        }

        // Return to normal scale with bounce
        transform.DOScale(transform.localScale, evolutionDuration * 0.5f)
            .SetEase(Ease.OutBounce);

        yield return new WaitForSeconds(evolutionDuration * 0.5f);
    }

    public IEnumerator PlayVictoryAnimation()
    {
        animator.SetTrigger("Victory");

        // Jump effect
        transform.DOJump(transform.position + Vector3.up * 0.5f, 1f, 1, 1f)
            .SetEase(Ease.OutQuad);

        // Spin effect
        transform.DORotate(new Vector3(0, 360, 0), 1f, RotateMode.FastBeyond360)
            .SetEase(Ease.OutQuad);

        yield return new WaitForSeconds(1f);
    }

    public IEnumerator PlaySummonAnimation()
    {
        // Start invisible
        spriteRenderer.color = new Color(1f, 1f, 1f, 0f);

        // Appear effect
        spriteRenderer.DOFade(1f, 0.5f)
            .SetEase(Ease.OutQuad);

        // Scale up from nothing
        transform.localScale = Vector3.zero;
        transform.DOScale(Vector3.one, 0.5f)
            .SetEase(Ease.OutBack);

        yield return new WaitForSeconds(0.5f);
    }

    #endregion

    #region Visual Effects

    private IEnumerator FlashSprite(Color flashColor)
    {
        Color originalColor = spriteRenderer.color;
        
        spriteRenderer.DOColor(flashColor, flashDuration * 0.5f);
        yield return new WaitForSeconds(flashDuration * 0.5f);
        
        spriteRenderer.DOColor(originalColor, flashDuration * 0.5f);
        yield return new WaitForSeconds(flashDuration * 0.5f);
    }

    public void PlayEffect(ParticleSystem effect, Vector3 position)
    {
        if (effect != null)
        {
            ParticleSystem instance = Instantiate(effect, position, Quaternion.identity);
            float duration = instance.main.duration + instance.main.startLifetime.constantMax;
            Destroy(instance.gameObject, duration);
        }
    }

    #endregion

    private void OnDestroy()
    {
        // Clean up DOTween animations
        transform.DOKill();
        spriteRenderer.DOKill();
    }
}

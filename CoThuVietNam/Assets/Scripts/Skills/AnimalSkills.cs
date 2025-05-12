using UnityEngine;
using System.Collections;

namespace AnimalSkills
{
    [CreateAssetMenu(fileName = "TigerLeap", menuName = "CoThuVietNam/Skills/TigerLeap")]
    public class TigerLeap : SkillData
    {
        public float leapDistance = 2f;
        public float stunDuration = 1f;

        public override void Execute(Animal user, Animal target)
        {
            base.Execute(user, target);
            
            // Calculate leap position
            Vector3 direction = (target.transform.position - user.transform.position).normalized;
            Vector3 leapPosition = target.transform.position + direction * leapDistance;
            
            // Perform leap animation
            AnimationController.Instance.PlaySkillAnimation(user, this, leapPosition);
            
            // Apply stun effect to target
            StartCoroutine(ApplyStunEffect(target));
        }

        private IEnumerator ApplyStunEffect(Animal target)
        {
            target.IsStunned = true;
            yield return new WaitForSeconds(stunDuration);
            target.IsStunned = false;
        }
    }

    [CreateAssetMenu(fileName = "ElephantStomp", menuName = "CoThuVietNam/Skills/ElephantStomp")]
    public class ElephantStomp : SkillData
    {
        public float stunDuration = 1.5f;
        public float areaOfEffect = 2f;
        public float knockbackForce = 5f;

        public override void Execute(Animal user, Animal target)
        {
            base.Execute(user, target);
            
            // Play stomp animation and effect
            AnimationController.Instance.PlaySkillAnimation(user, this, user.transform.position);
            
            // Find all animals in area of effect
            Collider[] hitColliders = Physics.OverlapSphere(user.transform.position, areaOfEffect);
            foreach (var hitCollider in hitColliders)
            {
                Animal hitAnimal = hitCollider.GetComponent<Animal>();
                if (hitAnimal != null && hitAnimal != user)
                {
                    // Apply knockback and stun
                    Vector3 direction = (hitAnimal.transform.position - user.transform.position).normalized;
                    StartCoroutine(ApplyKnockbackAndStun(hitAnimal, direction));
                }
            }
        }

        private IEnumerator ApplyKnockbackAndStun(Animal target, Vector3 direction)
        {
            target.IsStunned = true;
            
            // Apply knockback
            float elapsedTime = 0f;
            Vector3 startPos = target.transform.position;
            Vector3 endPos = startPos + direction * knockbackForce;
            
            while (elapsedTime < stunDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / stunDuration;
                target.transform.position = Vector3.Lerp(startPos, endPos, t);
                yield return null;
            }
            
            target.IsStunned = false;
        }
    }

    [CreateAssetMenu(fileName = "MouseStealth", menuName = "CoThuVietNam/Skills/MouseStealth")]
    public class MouseStealth : SkillData
    {
        public float stealthDuration = 3f;
        public float speedBoost = 1.5f;

        public override void Execute(Animal user, Animal target)
        {
            base.Execute(user, target);
            
            // Play stealth animation
            AnimationController.Instance.PlaySkillAnimation(user, this, user.transform.position);
            
            // Apply stealth effect
            StartCoroutine(ApplyStealthEffect(user));
        }

        private IEnumerator ApplyStealthEffect(Animal user)
        {
            // Make semi-transparent
            SpriteRenderer renderer = user.GetComponent<SpriteRenderer>();
            Color originalColor = renderer.color;
            renderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0.5f);
            
            // Increase speed
            float originalSpeed = user.moveRange;
            user.moveRange = Mathf.RoundToInt(originalSpeed * speedBoost);
            
            // Wait for duration
            yield return new WaitForSeconds(stealthDuration);
            
            // Restore original state
            renderer.color = originalColor;
            user.moveRange = Mathf.RoundToInt(originalSpeed);
        }
    }

    [CreateAssetMenu(fileName = "LionRoar", menuName = "CoThuVietNam/Skills/LionRoar")]
    public class LionRoar : SkillData
    {
        public float fearRadius = 3f;
        public float fearDuration = 2f;
        public int pushbackDistance = 1;

        public override void Execute(Animal user, Animal target)
        {
            base.Execute(user, target);
            
            // Play roar animation and sound
            AnimationController.Instance.PlaySkillAnimation(user, this, user.transform.position);
            AudioManager.Instance.PlaySound("LionRoar");
            
            // Find all animals in fear radius
            Collider[] hitColliders = Physics.OverlapSphere(user.transform.position, fearRadius);
            foreach (var hitCollider in hitColliders)
            {
                Animal hitAnimal = hitCollider.GetComponent<Animal>();
                if (hitAnimal != null && hitAnimal != user)
                {
                    // Apply fear effect
                    StartCoroutine(ApplyFearEffect(hitAnimal, user.transform.position));
                }
            }
        }

        private IEnumerator ApplyFearEffect(Animal target, Vector3 roarSource)
        {
            // Calculate pushback direction
            Vector3 pushDirection = (target.transform.position - roarSource).normalized;
            Vector3 pushbackPosition = target.transform.position + pushDirection * pushbackDistance;
            
            // Apply fear status
            target.IsFeared = true;
            
            // Move target away
            float elapsedTime = 0f;
            Vector3 startPos = target.transform.position;
            
            while (elapsedTime < fearDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / fearDuration;
                target.transform.position = Vector3.Lerp(startPos, pushbackPosition, t);
                yield return null;
            }
            
            target.IsFeared = false;
        }
    }

    [CreateAssetMenu(fileName = "WolfPack", menuName = "CoThuVietNam/Skills/WolfPack")]
    public class WolfPack : SkillData
    {
        public float packRadius = 3f;
        public float damageBonus = 1.5f;
        public float duration = 5f;

        public override void Execute(Animal user, Animal target)
        {
            base.Execute(user, target);
            
            // Play pack howl animation
            AnimationController.Instance.PlaySkillAnimation(user, this, user.transform.position);
            
            // Find nearby wolf allies
            Collider[] hitColliders = Physics.OverlapSphere(user.transform.position, packRadius);
            foreach (var hitCollider in hitColliders)
            {
                Animal hitAnimal = hitCollider.GetComponent<Animal>();
                if (hitAnimal != null && hitAnimal.type == AnimalType.Wolf)
                {
                    // Apply pack bonus
                    StartCoroutine(ApplyPackBonus(hitAnimal));
                }
            }
        }

        private IEnumerator ApplyPackBonus(Animal wolf)
        {
            // Increase attack power
            int originalAttack = wolf.attackPower;
            wolf.attackPower = Mathf.RoundToInt(originalAttack * damageBonus);
            
            // Visual effect
            ParticleSystem packEffect = wolf.GetComponentInChildren<ParticleSystem>();
            if (packEffect != null)
            {
                packEffect.Play();
            }
            
            yield return new WaitForSeconds(duration);
            
            // Restore original attack power
            wolf.attackPower = originalAttack;
            
            if (packEffect != null)
            {
                packEffect.Stop();
            }
        }
    }

    [CreateAssetMenu(fileName = "FoxTrick", menuName = "CoThuVietNam/Skills/FoxTrick")]
    public class FoxTrick : SkillData
    {
        public float illusionDuration = 3f;
        public int maxIllusions = 2;

        public override void Execute(Animal user, Animal target)
        {
            base.Execute(user, target);
            
            // Create illusions
            StartCoroutine(CreateIllusions(user));
        }

        private IEnumerator CreateIllusions(Animal user)
        {
            GameObject[] illusions = new GameObject[maxIllusions];
            
            // Create illusions
            for (int i = 0; i < maxIllusions; i++)
            {
                // Create illusion at random position around user
                Vector2 randomOffset = Random.insideUnitCircle * 2f;
                Vector3 illusionPos = user.transform.position + new Vector3(randomOffset.x, 0, randomOffset.y);
                
                illusions[i] = Instantiate(user.gameObject, illusionPos, user.transform.rotation);
                
                // Make illusion non-interactive
                Destroy(illusions[i].GetComponent<Collider>());
                
                // Add fade effect
                StartCoroutine(FadeIllusion(illusions[i]));
            }
            
            yield return new WaitForSeconds(illusionDuration);
            
            // Remove illusions
            foreach (var illusion in illusions)
            {
                if (illusion != null)
                {
                    Destroy(illusion);
                }
            }
        }

        private IEnumerator FadeIllusion(GameObject illusion)
        {
            SpriteRenderer renderer = illusion.GetComponent<SpriteRenderer>();
            Color originalColor = renderer.color;
            
            float elapsedTime = 0f;
            while (elapsedTime < illusionDuration)
            {
                elapsedTime += Time.deltaTime;
                float alpha = Mathf.PingPong(elapsedTime * 2f, 1f);
                renderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
                yield return null;
            }
        }
    }
}

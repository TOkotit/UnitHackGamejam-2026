using System.Collections;
using UnityEngine;

namespace Script.Character
{
    public class CharacterHealth : MonoBehaviour
    {
        [SerializeField] private int maxHealth = 3;
        private int currentHealth;

        [SerializeField] private float invincibilityDuration = 1.5f; 
        [SerializeField] private float blinkInterval = 0.1f;      
        [SerializeField] private Color blinkColor = new (1f, 0f, 0f, 0.5f);
        private Color originalColor;
        
        [SerializeField] private SpriteRenderer spriteRenderer;
        
        private bool isInvincible = false;
        
        private void Awake()
        {
            currentHealth = maxHealth;

            if (spriteRenderer == null)
            {
                originalColor = spriteRenderer.color;
            }
        }
        
        public void TakeDamage(int damage = 1)
        {
            if (isInvincible) return;

            currentHealth -= damage;
            Debug.Log($"Игрок получил урон! Осталось ХП: {currentHealth}");

            if (currentHealth <= 0)
            {
                Die();
            }
            else
            {
                StartCoroutine(InvincibilityRoutine());
            }
        }
        
        private IEnumerator InvincibilityRoutine()
        {
            isInvincible = true; 
            
            var timer = 0f;
            var useBlinkColor = true;

            while (timer < invincibilityDuration)
            {
                if (spriteRenderer)
                {
                    spriteRenderer.color = useBlinkColor ? blinkColor : originalColor;
                }

                useBlinkColor = !useBlinkColor;
                timer += blinkInterval;
        
                yield return new WaitForSeconds(blinkInterval);
            }
            
            spriteRenderer.color = originalColor;
            isInvincible = false; 
        }

        private void Die()
        {
            GameSceneManager.ReloadCurrentScene();
        }
    }
}
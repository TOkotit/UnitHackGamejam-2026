using System;
using Script.Character;
using UnityEngine;

namespace Script.Enemy
{
    public class ShooterAttack : MonoBehaviour
    {
        
        [SerializeField] private float speed = 12f;
        
        [SerializeField]private Rigidbody2D _rb;
        private bool isParried = false; 

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
        }
        
        public void SetDirection(float direction)
        {
            if (_rb == null) _rb = GetComponent<Rigidbody2D>();
            _rb.linearVelocityX = Mathf.Sign(direction) * speed;
        }
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            HandleCollision(other);
        }
        
        
        private void OnTriggerStay2D(Collider2D other)
        {
            HandleCollision(other);
        }
        
        private void HandleCollision(Collider2D other)
        {
            if (other.CompareTag("Slash"))
            {
                if (!isParried)
                {
                    isParried = true;
                    
                    var currentDirX = _rb.linearVelocityX;
                    var newDirection = -Mathf.Sign(currentDirX);
                    speed *= 1.5f;
                    SetDirection(newDirection);
                }
                return;
            }

            if (!isParried && other.CompareTag("Player"))
            {
                AttackPlayer(other);
                Destroy(gameObject);
                return;
            }
            
            if (isParried && other.CompareTag("Enemy"))
            {
                AttackEnemy(other);
                Destroy(gameObject);
                return;
            }

            if (!other.isTrigger && !other.CompareTag("Player") && !other.CompareTag("Enemy"))
            {
                Destroy(gameObject);
            }
        }
        
        private void AttackPlayer(Collider2D other)
        {
            var playerHealth = other.GetComponent<CharacterHealth>();
            playerHealth.TakeDamage();
        }

        private void AttackEnemy(Collider2D other)
        {
            var enemy =  other.GetComponent<IEnemy>();
            enemy.Die();
        }
        
    }
}
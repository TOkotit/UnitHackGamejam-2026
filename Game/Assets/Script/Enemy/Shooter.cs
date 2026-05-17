using UnityEngine;

namespace Script.Enemy
{
    public class Shooter : MonoBehaviour, IEnemy
    {
        public ShooterState currentState = ShooterState.Patrol;
        [SerializeField] private SpriteRenderer spriteRenderer;
        // [SerializeField] private Animator animator;
        
        [Header("Настройки стрельбы")]
        [SerializeField] private GameObject bulletPrefab;
        [SerializeField] private Transform firePoint; 
        [SerializeField] private float fireInterval = 1.75f; 
        [SerializeField] private float visionRange = 8f;
        
        [Header("Настройки патруля")]
        [SerializeField] private float patrolSpeed = 2f;
        [SerializeField] private Transform[] patrolPoints; 
        
        [SerializeField] private GameObject player;
        private Rigidbody2D _rb;
        
        private int _currentPatrolIndex = 0;
        private float _fireTimer;

        private void Start()
        {
            _rb = GetComponent<Rigidbody2D>();
            _fireTimer = fireInterval; 
        }
        
        void Update()
        {
            if (!player) return;

            var distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);

            if (distanceToPlayer <= visionRange)
            {
                currentState = ShooterState.Attack; 
                StopAndAttack();
            }
            else
            {
                currentState = ShooterState.Patrol;
                Patrol();
            }
        }

        private void Patrol()
        {
            if (patrolPoints.Length == 0) return;

            var targetPoint = patrolPoints[_currentPatrolIndex];
            var directionX = Mathf.Sign(targetPoint.position.x - transform.position.x);
        
            _rb.linearVelocity = new Vector2(directionX * patrolSpeed, _rb.linearVelocity.y);
            Flip(directionX);

            if (Mathf.Abs(transform.position.x - targetPoint.position.x) < 0.3f)
            {
                _currentPatrolIndex = (_currentPatrolIndex + 1) % patrolPoints.Length;
            }
        }
        
        private void StopAndAttack()
        {
            _rb.linearVelocity = new Vector2(0, _rb.linearVelocity.y);
            
            var directionToPlayer = Mathf.Sign(player.transform.position.x - transform.position.x);
            Flip(directionToPlayer);

            _fireTimer += Time.deltaTime;
            if (!(_fireTimer >= fireInterval)) return;
            Shoot();
            _fireTimer = 0f; 
        }

        // ReSharper disable Unity.PerformanceAnalysis
        private void Shoot()
        {
            // if (animator != null)
            // {
            //     // animator.SetTrigger("attack");
            // }

            if (bulletPrefab != null && firePoint != null)
            {
                var bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
                var shootDirection = (player.transform.position - firePoint.position).normalized;
                
                var bulletScript = bullet.GetComponent<ShooterAttack>();
                if (bulletScript != null)
                {
                    bulletScript.SetDirection(shootDirection.normalized.x);
                }
                
            }
        }
        
        private void Flip(float moveX)
        {
            if (moveX == 0) return;

            if (moveX > 0) 
                transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            else if (moveX < 0) 
                transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
        
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, visionRange);
        }
        public void Die()
        {
            Destroy(gameObject);
        }
    }
}
using System;
using System.Collections;
using UnityEngine;

namespace Script.Enemy
{
    public class Charger : MonoBehaviour, IEnemy
    {
        
        public ChargeState currentState = ChargeState.Patrol;
        [SerializeField] SpriteRenderer spriteRenderer;
        [SerializeField] Animator animator;
        [SerializeField] private GameObject attackZone;
        [SerializeField] Transform spriteTransform;
        [SerializeField] private float visionRange = 8f;
        [SerializeField] private float chargeDistance = 4f;
        
        [SerializeField] private float patrolSpeed = 2f;
        [SerializeField] private float approachSpeed = 4f;
        [SerializeField] private float chargeSpeed = 12f;
        
        [SerializeField] private float windUpTime = 0.6f;
        [SerializeField] private float chargeDuration = 0.5f;
        
        [SerializeField] private Transform[] patrolPoints; 
        
        [SerializeField] private GameObject player;
        private Rigidbody2D _rb;
        
        private int _currentPatrolIndex = 0;
        private Vector2 _chargeDirection;
        private bool _isChargingSequence = false;

        private void Start()
        {
            _rb = GetComponent<Rigidbody2D>();
            attackZone.SetActive(false);
        }
        
        void Update()
        {
            if (_isChargingSequence) return;

            if (!player) return;

            var distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);

            if (distanceToPlayer <= chargeDistance)
            {
                StartCoroutine(ChargeRoutine());
                return;
            }
            currentState = distanceToPlayer <= visionRange ? ChargeState.Approach : ChargeState.Patrol;

            switch (currentState)
            {
                case ChargeState.Patrol:
                    Patrol();
                    break;
                case ChargeState.Approach:
                    ApproachPlayer();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void Patrol()
        {
            if (patrolPoints.Length == 0) return;

            var targetPoint = patrolPoints[_currentPatrolIndex];
            var directionX = Mathf.Sign(targetPoint.position.x - transform.position.x);
        
            _rb.linearVelocity = new Vector2(directionX * patrolSpeed, _rb.linearVelocityY);
            Flip(directionX);

            if (Mathf.Abs(transform.position.x - targetPoint.position.x) < 0.3f)
            {
                _currentPatrolIndex = (_currentPatrolIndex + 1) % patrolPoints.Length;
            }
            
        }
        
        private void ApproachPlayer()
        {
            var directionX = Mathf.Sign(player.transform.position.x - transform.position.x);
            _rb.linearVelocity = new Vector2(directionX * approachSpeed, _rb.linearVelocityY);
            Flip(directionX);
        }
        
        private IEnumerator ChargeRoutine()
        {
            _isChargingSequence = true;
            
            currentState = ChargeState.WindUp;
            animator.SetTrigger("startWait");
            _rb.linearVelocity = new Vector2(0, _rb.linearVelocityY); 
        
            
            
            _chargeDirection = new Vector2(Mathf.Sign(player.transform.position.x - transform.position.x), 0).normalized;
            Flip(_chargeDirection.x);

            yield return new WaitForSeconds(windUpTime);

            currentState = ChargeState.Charge;
            animator.SetTrigger("startCharge");
            attackZone.SetActive(true);
            var timer = 0f;

            while (timer < chargeDuration)
            {
                _rb.linearVelocity = new Vector2(_chargeDirection.x * chargeSpeed, _rb.linearVelocityY);
                timer += Time.deltaTime;
                yield return null;
            }

            _rb.linearVelocity = new Vector2(0, _rb.linearVelocityY);
            yield return new WaitForSeconds(0.3f); 

            _isChargingSequence = false;
            currentState = ChargeState.Patrol;
            attackZone.SetActive(false);
            animator.SetTrigger("startWalk");
        }
        
        private void Flip(float moveX)
        {
            if (moveX > 0) 
                transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            else if (moveX < 0) 
                transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
        
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, visionRange);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, chargeDistance);
        }

        public void Die()
        {
            Destroy(gameObject);
        }
        
    }
}
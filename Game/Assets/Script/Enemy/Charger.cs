using System;
using System.Collections;
using UnityEngine;

namespace Script.Enemy
{
    public class Charger : MonoBehaviour
    {
        
        public EnemyState currentState = EnemyState.Patrol;
        
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
            currentState = distanceToPlayer <= visionRange ? EnemyState.Approach : EnemyState.Patrol;

            switch (currentState)
            {
                case EnemyState.Patrol:
                    Patrol();
                    break;
                case EnemyState.Approach:
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
        
            currentState = EnemyState.WindUp;
            _rb.linearVelocity = new Vector2(0, _rb.linearVelocityY); 
        
            _chargeDirection = new Vector2(Mathf.Sign(player.transform.position.x - transform.position.x), 0).normalized;
            Flip(_chargeDirection.x);

            yield return new WaitForSeconds(windUpTime);

            currentState = EnemyState.Charge;
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
            currentState = EnemyState.Patrol;
        }
        
        private void Flip(float moveX)
        {
            if (moveX > 0) transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            else if (moveX < 0) transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
        
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, visionRange);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, chargeDistance);
        }
        
    }
}
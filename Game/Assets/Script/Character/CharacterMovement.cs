using System;
using System.Collections;
using UnityEngine;

namespace Script.Character
{
    public class CharacterMovement :  MonoBehaviour
    {
        [SerializeField] private GameObject SlashPrefab;
        [SerializeField] private float WalikingSpeed;
        [SerializeField] private float DashSpeed = 15;
        [SerializeField] private float jumpForce = 10f;
        [SerializeField] private Rigidbody2D characterRB;
        [SerializeField] private Transform groundCheck;
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private float checkRadius = 0.2f;
        
        
        private bool isAttacking = false;
        private bool isGrounded;
        public void Awake()
        {
            if(!characterRB)
                characterRB = GetComponent<Rigidbody2D>();
        }

        public void Jump()
        {
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);
            
            if(isGrounded)
                characterRB.linearVelocity = new Vector2(0, jumpForce);
        }

        public void Move(Vector2 moveDirection)
        {
            if(!isAttacking)
                transform.Translate(moveDirection * (WalikingSpeed * Time.fixedDeltaTime));
            
        }

        public void Attack(Vector2 mouseScreenPosition)
        {
            isAttacking = true;
            if (Camera.main == null) return;
            var mouseWorldPosition = Camera.main.ScreenToWorldPoint(mouseScreenPosition);

            var attackDirectionX = mouseWorldPosition.x > transform.position.x ? 1f : -1f;
            DashWithAttack(attackDirectionX);
            var spawnPosition = transform.position + new Vector3(attackDirectionX * 2f, 0f, 0f);
            var splash = Instantiate(SlashPrefab, spawnPosition, Quaternion.identity);
            splash.transform.SetParent(transform);
            var currentScale = splash.transform.localScale;
            if (attackDirectionX < 0)
                currentScale.x = -Mathf.Abs(currentScale.x);
            else
                currentScale.x = Mathf.Abs(currentScale.x);
            splash.transform.localScale = currentScale;
            StartCoroutine(AttackAnimtation(splash));
        }
        
        private IEnumerator AttackAnimtation(GameObject splash)
        {
            yield return new WaitForSeconds(0.2f);
            Destroy(splash);
            isAttacking = false;
        }

        private void DashWithAttack(float attackDirection)
        {
            if (attackDirection > 0)
            {
                transform.position += new Vector3(DashSpeed, 0f, 0f);
            }
            else
            {
                transform.position += new Vector3(-DashSpeed, 0f, 0f);

            }
            
        }
        
        private void FixedUpdate()
        {
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);
        }
        
    }
}
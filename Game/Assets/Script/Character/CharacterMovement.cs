using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

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
        
        
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip[] footstepClips;
        [SerializeField] private float footstepInterval = 0.4f;
        
        [SerializeField] private AudioClip[] attackClips;
        
        [SerializeField] private Transform spriteTransform;
        
        private float footstepTimer;
        private bool isAttacking = false;
        private bool isGrounded;
        public void Awake()
        {
            if(!characterRB)
                characterRB = GetComponent<Rigidbody2D>();
            if (!audioSource)
                audioSource = GetComponent<AudioSource>();
            if (!spriteTransform)
            {
                var animator = GetComponentInChildren<Animator>();
                if (animator != null) spriteTransform = animator.transform;
            }
        }

        public void Jump()
        {
            if(isGrounded)
                characterRB.linearVelocity = new Vector2(characterRB.linearVelocity.x, jumpForce);        }

        public void Move(Vector2 moveDirection)
        {
            if(!isAttacking)
                characterRB.linearVelocity = new Vector2(moveDirection.x * WalikingSpeed, characterRB.linearVelocity.y);
           if (Mathf.Abs(moveDirection.x) > 0.05f)
            {
                var directionX = moveDirection.x > 0 ? 1f : -1f;
                FlipSprite(directionX);
            }
            HandleFootsteps(moveDirection);
        }
        
        private void HandleFootsteps(Vector2 moveDirection)
        {
            if (isGrounded && !isAttacking && Mathf.Abs(moveDirection.x) > 0.1f)
            {
                footstepTimer += Time.deltaTime;

                if (!(footstepTimer >= footstepInterval)) return;
                PlayFootstepSound();
                footstepTimer = 0f;
            }
            else
            {
                footstepTimer = footstepInterval; 
            }
        }
        
        private void PlayFootstepSound()
        {
            if (!audioSource || footstepClips == null || footstepClips.Length == 0) return;

            var randomIndex = UnityEngine.Random.Range(0, footstepClips.Length);
            var clip = footstepClips[randomIndex];

            audioSource.PlayOneShot(clip);
        }

        public void Attack(Vector2 mouseScreenPosition)
        {
            isAttacking = true;
            if (Camera.main == null) return;
            var mouseWorldPosition = Camera.main.ScreenToWorldPoint(mouseScreenPosition);

            var attackDirectionX = mouseWorldPosition.x > transform.position.x ? 1f : -1f;
            FlipSprite(attackDirectionX);
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
            PlayAttackSound();
            StartCoroutine(AttackAnimtation(splash));
        }
        private void FlipSprite(float directionX)
        {
            if (!spriteTransform) return;

            var scale = spriteTransform.localScale;
            scale.x = Mathf.Abs(scale.x) * directionX;
            
            var pos = spriteTransform.position;
            pos.x = Mathf.Abs(pos.x) * directionX;
            spriteTransform.localScale = scale;
            spriteTransform.position = pos;
        }
        
        private IEnumerator AttackAnimtation(GameObject splash)
        {
            yield return new WaitForSeconds(0.2f);
            Destroy(splash);
            isAttacking = false;
        }

        private void PlayAttackSound()
        {
            if (!audioSource || attackClips == null || attackClips.Length == 0) return;

            var randomIndex = UnityEngine.Random.Range(0, attackClips.Length);
            var clip = attackClips[randomIndex];

            audioSource.PlayOneShot(clip);
        }


        private void DashWithAttack(float attackDirection)
        {
            characterRB.linearVelocityX += attackDirection * DashSpeed;
        }
        
        private void FixedUpdate()
        {
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);
        }
        
    }
}
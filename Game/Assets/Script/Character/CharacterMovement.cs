using System.Collections;
using UnityEngine;

namespace Script.Character
{
    public class CharacterMovement :  MonoBehaviour
    {
        [SerializeField] private GameObject SlashPrefab;
        [SerializeField] private float WalikingSpeed;
        
        private Rigidbody2D chara;

        
        public void Jump()
        {
        }

        public void Move(Vector2 moveDirection)
        {
            transform.Translate(moveDirection * (WalikingSpeed * Time.fixedDeltaTime));
            
        }

        public void Attack(Vector2 mouseScreenPosition)
        {

            if (Camera.main == null) return;
            var mouseWorldPosition = Camera.main.ScreenToWorldPoint(mouseScreenPosition);

            var attackDirectionX = mouseWorldPosition.x > transform.position.x ? 1f : -1f;

            var spawnPosition = transform.position + new Vector3(attackDirectionX * 2f, 0f, 0f);
            var splash = Instantiate(SlashPrefab, spawnPosition, Quaternion.identity);
            var currentScale = splash.transform.localScale;
            if (attackDirectionX < 0)
                currentScale.x = -Mathf.Abs(currentScale.x);
            else
                currentScale.x = Mathf.Abs(currentScale.x);
            splash.transform.localScale = currentScale;
            DashWithAttack(attackDirectionX);
            StartCoroutine(AttackAnimtation(splash));
        }
        
        private IEnumerator AttackAnimtation(GameObject splash)
        {
            yield return new WaitForSeconds(0.2f);
            Destroy(splash);
        }
        
        public void DashWithAttack(float attackDirection)
        {
            if (attackDirection > 0)
            {
                transform.position += new Vector3(WalikingSpeed * Time.fixedDeltaTime * 5, 0f, 0f);
            }
            else
            {
                transform.position += new Vector3(-WalikingSpeed * Time.fixedDeltaTime * 5, 0f, 0f);

            }
            
        }
        
        private void FixedUpdate()
        {
            
        }
        
    }
}
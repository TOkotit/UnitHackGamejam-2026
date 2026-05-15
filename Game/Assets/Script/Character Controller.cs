using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;


public class CharacterController : MonoBehaviour
{
        private GameInput _gameInput;
        private Vector2 moveDirection;
        [SerializeField] private GameObject SlashPrefab;

        private void Awake()
        {
                _gameInput = new GameInput();
        }

        private void OnEnable()
        {
                _gameInput.Gameplay.Enable();
                _gameInput.Gameplay.Attack.performed += Attack;
        }

        private void OnDisable()
        {
                _gameInput.Gameplay.Disable();
                _gameInput.Gameplay.Attack.performed -= Attack;

        }

        private void Update()
        {
                moveDirection = _gameInput.Gameplay.Move.ReadValue<Vector2>();
                Debug.Log($"moveInput: {moveDirection}");
        }

        private void FixedUpdate()
        {
                transform.Translate(moveDirection * (4 * Time.fixedDeltaTime));
        }

        private void Attack(InputAction.CallbackContext context)
        {
                var mouseScreenPosition = _gameInput.Gameplay.Cursore.ReadValue<Vector2>();

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

                StartCoroutine(AttackAnimtation(splash));


        }

        private IEnumerator AttackAnimtation(GameObject splash)
        {
                yield return new WaitForSeconds(0.2f);
                Destroy(splash);
        }

}

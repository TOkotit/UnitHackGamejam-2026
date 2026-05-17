using UnityEngine;
using UnityEngine.InputSystem;

namespace Script.Character
{
        public class CharacterController : MonoBehaviour
        {
                private Vector2 moveDirection;
                [SerializeField] private CharacterHealth health;
                [SerializeField] CharacterMovement _characterMovement;
                [SerializeField] InputManager _inputManager;


                private void OnEnable()
                {
                        _inputManager.GameInput.Gameplay.Attack.performed += OnAttack;
                        _inputManager.GameInput.Gameplay.Jump.performed += OnJump;
                        _inputManager.GameInput.Gameplay.Restart.performed += OnRestart;
                }

                private void OnDisable()
                {
                        _inputManager.GameInput.Gameplay.Disable();
                        _inputManager.GameInput.Gameplay.Attack.performed -= OnAttack;
                        _inputManager.GameInput.Gameplay.Restart.performed -= OnRestart;
                }

                private void FixedUpdate()
                {
                        moveDirection = _inputManager.GameInput.Gameplay.Move.ReadValue<Vector2>(); 
                        _characterMovement.Move(moveDirection);
                }



                private void OnJump(InputAction.CallbackContext context)
                {
                        _characterMovement.Jump();
                }

                private void OnAttack(InputAction.CallbackContext context)
                {
                        var mouseScreenPosition = _inputManager.GameInput.Gameplay.Cursore.ReadValue<Vector2>();
                        _characterMovement.Attack(mouseScreenPosition);
                }

                private void OnRestart(InputAction.CallbackContext context)
                {
                        if (health != null)
                                health.Die();
                }

        }
}
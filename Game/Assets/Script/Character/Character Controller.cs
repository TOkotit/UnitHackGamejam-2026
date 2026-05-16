using UnityEngine;
using UnityEngine.InputSystem;

namespace Script.Character
{
        public class CharacterController : MonoBehaviour
        {
                private GameInput _gameInput;
                private Vector2 moveDirection;
                [SerializeField] CharacterMovement _characterMovement;

                private void Awake()
                {
                        _gameInput = new GameInput();
                }

                private void OnEnable()
                {
                        _gameInput.Gameplay.Enable();
                        _gameInput.Gameplay.Attack.performed += OnAttack;
                        _gameInput.Gameplay.Jump.performed += OnJump;
                }

                private void OnDisable()
                {
                        _gameInput.Gameplay.Disable();
                        _gameInput.Gameplay.Attack.performed -= OnAttack;

                }

                private void FixedUpdate()
                {
                        moveDirection = _gameInput.Gameplay.Move.ReadValue<Vector2>(); 
                        _characterMovement.Move(moveDirection);
                }



                private void OnJump(InputAction.CallbackContext context)
                {
                        _characterMovement.Jump();
                }

                private void OnAttack(InputAction.CallbackContext context)
                {
                        var mouseScreenPosition = _gameInput.Gameplay.Cursore.ReadValue<Vector2>();
                        _characterMovement.Attack(mouseScreenPosition);
                }



        }
}
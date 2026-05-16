using UnityEngine;
using UnityEngine.Events;

namespace Script
{
    public class InputManager : MonoBehaviour
    {
        private GameInput gameInput;
        public GameInput GameInput => gameInput;

        public void SetGameplay()
        {
            gameInput.Disable();
            gameInput.Gameplay.Enable();
        }

        public void SetUI()
        {
            gameInput.Disable();
            gameInput.UI.Enable();
        }
        private void Awake()
        {
            gameInput = new GameInput();
        }
        public bool IsUIMode => gameInput.UI.enabled;

        public GameInput GetGameInput() => gameInput;
    }
}
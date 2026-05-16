using UnityEngine;
using UnityEngine.Events;

namespace Script
{
    public class InputManager : MonoBehaviour
    {
        private GameInput2 gameInput;

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
            gameInput = new GameInput2();
        }
        public bool IsUIMode => gameInput.UI.enabled;

        public GameInput2 GetGameInput() => gameInput;
    }
}
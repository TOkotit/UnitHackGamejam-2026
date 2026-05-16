using UnityEngine;
using UnityEngine.Events;

namespace Script
{
    public class InputManager : MonoBehaviour
    {
        private GameInput gameInput;
        public GameInput GameInput
        {
            get
            {
                if (gameInput == null)
                {
                    gameInput = new GameInput();
                }
                return gameInput;
            }
        }

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
        public bool IsUIMode => gameInput.UI.enabled;

        public GameInput GetGameInput() => gameInput;
    }
}
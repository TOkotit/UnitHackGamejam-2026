using System;
using UnityEngine;
using UnityEngine.UI;

namespace Script.UI
{
    public class ExitButton :  MonoBehaviour
    {
        [SerializeField] private Button button;

        private void Awake()
        {
            button.onClick.AddListener(ExitGame);
        }

        private void ExitGame()
        {
            Application.Quit();
            #if UNITY_EDITOR
                        UnityEditor.EditorApplication.isPlaying = false;
            #endif
        }
    }
}
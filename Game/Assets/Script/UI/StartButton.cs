using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Script.UI
{
    public class StartButton : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer screen1; 
        [SerializeField] private GameObject globalVolume;
        [SerializeField] private CameraZoomTransition zoomTransition;
        [SerializeField] private Button startButton;
        private bool isStarting = false;

        private void Awake()
        {
            startButton.onClick.AddListener(StartGame);
        }

        private void StartGame()
        {
            isStarting = true;

            if (screen1 != null)
                screen1.enabled = false;

            if (globalVolume != null)
            {
                var volume = Instantiate(globalVolume);
                DontDestroyOnLoad(volume);
            }
            if (zoomTransition != null)
            {
                zoomTransition.StartZoomTransition();
            }
        }

        
    }
}
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Script.UI
{
    public class CameraZoomTransition : MonoBehaviour
    {
        [Header("Ссылки на объекты")]
        [SerializeField] private GameObject mainCanvas;   
        [SerializeField] private Transform targetObject;    

        [Header("Настройки анимации")]
        [SerializeField] private float targetSize = 1.5f;  
        [SerializeField] private float duration = 2.0f;     

        private Camera mainCamera;
        private bool isRunning = false;

        private void Start()
        {
            mainCamera = Camera.main;
        }

        public void StartZoomTransition()
        {
            if (isRunning) return;

            if (mainCanvas != null)
            {
                mainCanvas.SetActive(false);
            }

            StartCoroutine(ZoomRoutine());
        }

        private IEnumerator ZoomRoutine()
        {
            isRunning = true;

            var startPosition = mainCamera.transform.position;
            var startSize = mainCamera.orthographicSize;

            var targetPosition = new Vector3(targetObject.position.x, targetObject.position.y, startPosition.z);

            var elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                var t = elapsedTime / duration;

                t = Mathf.SmoothStep(0f, 1f, t);

                mainCamera.transform.position = Vector3.Lerp(startPosition, targetPosition, t);
                mainCamera.orthographicSize = Mathf.Lerp(startSize, targetSize, t);

                yield return null;
            }

            mainCamera.transform.position = targetPosition;
            mainCamera.orthographicSize = targetSize;

            var nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
            if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
            {
                SceneManager.LoadScene(nextSceneIndex);
            }
            else
            {
                Debug.LogError("Следующей сцены нет в Build Settings!");
            }
        }
    }
}
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Script.UI
{
    public class MenuButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        public enum ButtonAction { StartGame, ExitGame }

        [SerializeField] private ButtonAction actionType;

        [SerializeField] private float normalAlpha = 0f;
        [SerializeField] private float hoverAlpha = 0.1f;

        [SerializeField] private Image screen1; // Картинка, которую скрываем
        [SerializeField] private float delayBeforeLoad = 2f; // Задержка в секундах перед сменой сцены

        
        [SerializeField] private GameObject GlobalVolume;

        private bool isStarting = false;
        private Image buttonImage;

        private void Start()
        {
            buttonImage = GetComponent<Image>();
            SetAlpha(normalAlpha);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (isStarting) return;
            SetAlpha(hoverAlpha);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (isStarting) return;
            SetAlpha(normalAlpha);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (isStarting) return;

            if (actionType == ButtonAction.StartGame)
            {
                StartGame();
            }
            else if (actionType == ButtonAction.ExitGame)
            {
                ExitGame();
            }
        }

        private void StartGame()
        {
            isStarting = true;
            SetAlpha(normalAlpha);

            // Мгновенно выключаем картинку
            if (screen1 != null)
                screen1.enabled = false;
            var volume = Instantiate(GlobalVolume);
            DontDestroyOnLoad(volume);
            // Запускаем обычный таймер ожидания
            StartCoroutine(WaitAndLoadRoutine());
        }

        private IEnumerator WaitAndLoadRoutine()
        {
            // Тупо ждем указанные секунды (например, 2 секунды)
            yield return new WaitForSeconds(delayBeforeLoad);

            // Грузим следующую по списку сцену
            int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
            if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
            {
                SceneManager.LoadScene(nextSceneIndex);
            }
            else
            {
                Debug.LogError("Сцена не переключилась, потому что следующего уровня нет в Build Settings!");
            }
        }

        private void ExitGame()
        {
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }

        private void SetAlpha(float alpha)
        {
            if (buttonImage == null) return;

            Color color = buttonImage.color;
            color.a = alpha;
            buttonImage.color = color;
        }
    }
}
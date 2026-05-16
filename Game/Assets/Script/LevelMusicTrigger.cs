using UnityEngine;
using Script; // Подключаем пространство имен твоего AudioManager

public class LevelMusicTrigger : MonoBehaviour
{
    [SerializeField] private AudioClip levelMusicClip;

    [SerializeField] private float fadeDuration = 1.5f;

    private void Start()
    {
        if (AudioManager.Instance != null)
        {
            if (levelMusicClip != null)
            {
                AudioManager.Instance.ChangeMusicWithFade(levelMusicClip, fadeDuration);
            }
            else
            {
                AudioManager.Instance.StopMusic(fadeDuration);
            }
        }
        else
        {
            Debug.LogWarning("AudioManager не найден на сцене! Музыка не включится.");
        }
    }
}
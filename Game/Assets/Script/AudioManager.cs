using System.Collections;
using UnityEngine;

namespace Script
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }
        
        [SerializeField] private AudioSource musicSource;
        
        [SerializeField] private float defaultFadeDuration = 1.0f;
        
        [SerializeField] private float fadeDuration = 1.0f;
        
        private Coroutine fadeCoroutine;
        
        private void Awake()
        {
            if (!Instance)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            if (!musicSource)
            {
                musicSource = GetComponent<AudioSource>();
            }
            
            if (musicSource != null)
            {
                musicSource.loop = true;
            }
        }
        
        public void PlayMusic(AudioClip clip)
        {
            if (musicSource.clip == clip) return;

            if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);

            musicSource.clip = clip;
            musicSource.volume = 1f;
            musicSource.Play();
        }
        
        public void ChangeMusicWithFade(AudioClip newClip, float duration = -1f)
        {
            if (musicSource.clip == newClip) return;

            var finalDuration = duration < 0 ? defaultFadeDuration : duration;

            if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(FadeMusicRoutine(newClip, finalDuration));
        }
        
        private IEnumerator FadeMusicRoutine(AudioClip newClip, float duration)
        {
            var startVolume = musicSource.volume;

            if (musicSource.isPlaying)
            {
                for (float t = 0; t < duration / 2; t += Time.deltaTime)
                {
                    musicSource.volume = Mathf.Lerp(startVolume, 0f, t / (duration / 2));
                    yield return null;
                }
                musicSource.volume = 0f;
                musicSource.Stop();
            }

            musicSource.clip = newClip;

            if (!newClip) yield break;
            {
                musicSource.Play();
                for (float t = 0; t < duration / 2; t += Time.deltaTime)
                {
                    musicSource.volume = Mathf.Lerp(0f, 1f, t / (duration / 2));
                    yield return null;
                }
                musicSource.volume = 1f;
            }
        }
        
        public void StopMusic(float duration = 0.5f)
        {
            ChangeMusicWithFade(null, duration);
        }
    }
}
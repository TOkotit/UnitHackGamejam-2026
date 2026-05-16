using UnityEngine;

public class ExitDoor : MonoBehaviour
{
    [Header("Настройки двери")] 
    [SerializeField] private float delay = 0f;  
    private bool isExiting = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isExiting && other.CompareTag("Player"))
        {
            isExiting = true;
            StartExit();
        }
    }

    private void StartExit()
    {
        if (delay > 0f)
            Invoke(nameof(LoadNextScene), delay);
        else
            LoadNextScene();
    }

    private void LoadNextScene()
    {
        GameSceneManager.LoadNextScene();
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitDoor : MonoBehaviour
{
    [Header("Настройки двери")] 
    [SerializeField] private float delay = 0f;  
    
    [SerializeField] private Sprite openDoorSprite;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private List<GameObject> enemiesToKill;
    private bool isExiting;
    private bool isOpen;

    
    private void Start()
    {
        if (!spriteRenderer)
            spriteRenderer = GetComponent<SpriteRenderer>();
    }
    
    private void Update()
    {
        if (!isOpen)
        {
            CheckEnemiesList();
        }
    }
    private void CheckEnemiesList()
    {

        enemiesToKill.RemoveAll(enemy => !enemy);
        
        if (enemiesToKill.Count == 0)
        {
            OpenDoor();
        }
    }
    
    private void OpenDoor()
    {
        isOpen = true; 
        if (spriteRenderer && openDoorSprite)
        {
            spriteRenderer.sprite = openDoorSprite;
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isOpen && !isExiting && other.CompareTag("Player"))
        {
            isExiting = true;
            GameSceneManager.LoadNextScene(this);
            
        }
    }
}
using Script.Character;
using UnityEngine;

namespace Script.Enemy
{
    public class ChargerAttack : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D other)
        {
            Debug.Log($"!!! ФИЗИКА СРАБОТАЛА !!! Триггер задел: {other.gameObject.name} (Тег: {other.gameObject.tag})");
            if (other.CompareTag("Player"))
            {
                AttackPlayer(other);
            }
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            Debug.Log(other.gameObject.name);
            if (other.CompareTag("Player"))
            {
                AttackPlayer(other);
            }
        }

        private void AttackPlayer(Collider2D other)
        {
            var playerHealth = other.GetComponent<CharacterHealth>();
            playerHealth.TakeDamage();
        }
    }
}
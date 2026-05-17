using Script.Enemy;
using UnityEngine;

namespace Script.Character
{
    public class SplashAttack :  MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Enemy"))
            {
                Debug.Log("Entered splash");
                KillEnemy(collision.gameObject);
            }
        }

        private void OnTriggerStay2D(Collider2D collision)
        {
            if (collision.CompareTag("Enemy"))
            {
                Debug.Log("Stayed splash");
                KillEnemy(collision.gameObject);
            }
        }

        private void KillEnemy(GameObject enemy)
        {
            var component = enemy.GetComponent<IEnemy>();
            component.Die();
        }
    }
}
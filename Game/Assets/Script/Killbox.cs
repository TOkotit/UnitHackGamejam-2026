using System;
using Script.Character;
using Script.Enemy;
using UnityEngine;

namespace Script
{
    public class Killbox : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            { 
                other.GetComponent<CharacterHealth>().Die();
            }
            else if (other.CompareTag("Enemy"))
            {
                other.GetComponent<IEnemy>().Die();
            }
        }
    }
}
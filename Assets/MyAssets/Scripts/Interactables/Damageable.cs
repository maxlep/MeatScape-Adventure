using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Damageable : MonoBehaviour
{
    [SerializeField] private HealthBar HealthBar;
    [SerializeField] private int MaxHealth;
    [SerializeField] private UnityEvent OnDamage;
    [SerializeField] private UnityEvent OnDeath;

    private int currentHealth;
    private bool isAlive;

    private void Awake()
    {
        isAlive = true;
        currentHealth = MaxHealth;
        HealthBar?.SetMaxHealth(MaxHealth);
    }

    public void TakeDamage(int damage)
    {
        if (!isAlive) return;

        currentHealth -= damage;
        HealthBar?.UpdateHealth(currentHealth);
        OnDamage.Invoke();
        
        if (currentHealth <= 0) Death();
    }

    private void Death()
    {
        isAlive = false;
        OnDeath.Invoke();
    }
}

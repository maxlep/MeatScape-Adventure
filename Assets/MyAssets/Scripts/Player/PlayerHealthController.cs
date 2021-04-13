using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Den.Tools;
using MyAssets.ScriptableObjects.Variables;
using Shapes;
using Sirenix.Utilities;
using UnityEngine;

public class PlayerHealthController : MonoBehaviour
{
    [SerializeField] private bool isHorizontal;
    [SerializeField] private IntReference currentHungerLevel;
    [SerializeField] private IntReference maxHungerLevel;
    [SerializeField] private GameObject healthTickPrefab;
    [SerializeField] private RectTransform healthBarRect;
    [SerializeField] private Gradient healthGradient;

    private List<PlayerHealthTickController> healthTickList = new List<PlayerHealthTickController>();
    private int _displayedHealth = 0;
    private float tickHeight;

    private void Awake()
    {
        healthTickList.Clear();
        InitMaxHealth();
        currentHungerLevel.Subscribe(UpdateHealthBar);

        //Trigger update to get latest.
        currentHungerLevel.Value = currentHungerLevel.Value;
    }

    private void OnDisable()
    {
        currentHungerLevel.Unsubscribe(UpdateHealthBar);
    }

    private void InitMaxHealth()
    {
        for (int i = 0; i < maxHungerLevel.Value; i++)
        {
            GameObject healthTick = Instantiate(healthTickPrefab, healthBarRect);
            healthTickList.Add(healthTick.GetComponent<PlayerHealthTickController>());
        }
    }

    private void UpdateHealthBar(int prev, int current)
    {
        var delta = current - _displayedHealth;
        
        healthTickList.ForEach((t, i) =>
        {
            healthTickList[i].SetFill(i < currentHungerLevel.Value);
        });

        // if (delta > 0)  IncrementHealth(delta);
        // else if (delta < 0) DecrementHealth(delta);

        _displayedHealth = current;
        UpdateHealthTickColor();
    }

    private void IncrementHealth(int amount)
    {
        // for (int i = 0; i < amount; i++)
        // {
        //     GameObject healthTick = Instantiate(healthTickPrefab, healthBarRect);
        //     currentHealthList.Add(healthTick);
        // }

        // for (int i = Mathf.Max(currentHungerLevel.Value - amount, 0); i < currentHungerLevel.Value; i++)
        // {
        //     healthTickList[i].GetComponent<PlayerHealthTickController>().SetFill(true);
        // }

        healthTickList.ForEach((t, i) =>
        {
            healthTickList[i].SetFill(true);
        });
    }

    private void DecrementHealth(int amount)
    {
        if (healthTickList.Count < 1) return;
        
        // for (int i = 0; i < Mathf.Abs(amount); i++)
        // {
        //     int index = currentHealthList.Count - 1;
        //     Destroy(currentHealthList[index]);
        //     currentHealthList.RemoveAt(index);
        // }
        
        for (int i = currentHungerLevel.Value; i < Mathf.Min(currentHungerLevel.Value + Mathf.Abs(amount), maxHungerLevel.Value - 1); i++)
        {
            healthTickList[i].GetComponent<PlayerHealthTickController>().SetFill(false);
        }
    }

    private void UpdateHealthTickColor()
    {
        foreach (var healthTick in healthTickList)
        {
            float percentHealth = Mathf.InverseLerp(0f, maxHungerLevel.Value, currentHungerLevel.Value);
            PlayerHealthTickController healthTickController = healthTick.GetComponent<PlayerHealthTickController>();
            healthTickController.SetColor(healthGradient.Evaluate(percentHealth));
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Den.Tools;
using MyAssets.ScriptableObjects.Variables;
using Shapes;
using UnityEngine;

public class PlayerHealthController : MonoBehaviour
{
    [SerializeField] private IntReference currentHungerLevel;
    [SerializeField] private IntReference maxHungerLevel;
    [SerializeField] private GameObject healthTickPrefab;
    [SerializeField] private GameObject healthEmptyTickPrefab;
    [SerializeField] private RectTransform healthBarRect;
    [SerializeField] private float tickSpacing = 5f;
    [SerializeField] private Gradient healthGradient;

    private List<GameObject> currentHealthList = new List<GameObject>();
    private float tickHeight;

    private void Awake()
    {
        InitMaxHealth();
        currentHungerLevel.Subscribe(UpdateHealthBar);
        
        //Trigger update to get latest.
        currentHungerLevel.Value = currentHungerLevel.Value;
    }

    private void InitMaxHealth()
    {
        for (int i = 0; i < maxHungerLevel.Value; i++)
        {
            GameObject healthTick = Instantiate(healthEmptyTickPrefab, healthBarRect);
            healthTick.SetActive(true);
            RectTransform healthTickRect = healthTick.GetComponent<RectTransform>();
            tickHeight = healthTickRect.rect.height;
            healthTickRect.position = healthBarRect.position +
                                      i * (tickHeight + tickSpacing) * Vector3.up; 
        }

    }

    private void UpdateHealthBar(int prev, int current)
    {
        var delta = current - currentHealthList.Count;
        
        if (delta > 0)  IncrementHealth(delta);
        else if (delta < 0) DecrementHealth(delta);

        UpdateHealthTickColor();
    }

    private void IncrementHealth(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            GameObject healthTick = Instantiate(healthTickPrefab, healthBarRect);
            RectTransform healthTickRect = healthTick.GetComponent<RectTransform>();
            tickHeight = healthTickRect.rect.height;
            healthTickRect.position = healthBarRect.position +
                                      currentHealthList.Count * (tickHeight + tickSpacing) * Vector3.up;
            currentHealthList.Add(healthTick);
        }
    }

    private void DecrementHealth(int amount)
    {
        if (currentHealthList.Count < 1) return;
        
        for (int i = 0; i < Mathf.Abs(amount); i++)
        {
            int index = currentHealthList.Count - 1;
            Destroy(currentHealthList[index]);
            currentHealthList.RemoveAt(index);
        }
    }

    private void UpdateHealthTickColor()
    {
        foreach (var healthTick in currentHealthList)
        {
            float percentHealth = Mathf.InverseLerp(0f, maxHungerLevel.Value, currentHungerLevel.Value);
            PlayerHealthTickController healthTickController = healthTick.GetComponent<PlayerHealthTickController>();
            healthTickController.SetColor(healthGradient.Evaluate(percentHealth));
        }
    }
}

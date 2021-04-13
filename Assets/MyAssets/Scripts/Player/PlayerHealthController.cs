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
        healthTickList.ForEach((t, i) =>
        {
            healthTickList[i].SetFill(i < currentHungerLevel.Value);
        });
        
        UpdateHealthTickColor();
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

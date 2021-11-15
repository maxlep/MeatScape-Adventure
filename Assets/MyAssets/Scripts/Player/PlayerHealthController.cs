using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Linq;
using Den.Tools;
using MyAssets.ScriptableObjects.Variables;
using Shapes;
using Sirenix.Utilities;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class PlayerHealthController : MonoBehaviour
{
    [SerializeField] private bool isHorizontal;
    [SerializeField] private IntReference currentHungerLevel;
    [SerializeField] private IntReference maxHungerLevel;
    [SerializeField] private GameObject healthTickPrefab;
    [SerializeField] private RectTransform healthBarRect;
    [SerializeField] private Gradient healthGradient;
    [SerializeField] private Color overchargeOutlineColor;
    [SerializeField] private Color defaultOutlineColor;

    private List<PlayerHealthTickController> healthTickList = new List<PlayerHealthTickController>();

    private Color _originalOutlineColor;

    private void Awake()
    {
        healthTickList.Clear();
        InitMaxHealth();
        currentHungerLevel.Subscribe(UpdateHealthBar);

        //Trigger update to get latest.
        currentHungerLevel.Value = currentHungerLevel.Value;

        _originalOutlineColor = (healthTickList.FirstOrDefault()?.GetOutlineColor()).GetValueOrDefault();
    }

    private void OnDestroy()
    {
        currentHungerLevel.Unsubscribe(UpdateHealthBar);
    }

    private void InitMaxHealth()
    {
        for (int i = 0; i < healthBarRect.childCount; i++)
        {
            GameObject child = healthBarRect.GetChild(i).gameObject;
            Destroy(child);
        }
        for (int i = 0; i < maxHungerLevel.Value; i++)
        {
            GameObject healthTick = Instantiate(healthTickPrefab, healthBarRect);
            healthTickList.Add(healthTick.GetComponent<PlayerHealthTickController>());
        }

        healthTickList.Reverse();
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
        float percentHealth = (float) currentHungerLevel.Value / (float) maxHungerLevel.Value;
        float percentToOne = percentHealth % 1;
        bool isOvercharged = percentHealth > 1;
        Color healthColor = healthGradient.Evaluate(percentHealth);

        for (int i = 0; i < healthTickList.Count; i++)
        {
            var healthTickController = healthTickList[i];
            float tickHealthThreshold = (float) (i + 1) / (float) healthTickList.Count;

            healthTickController.SetColor(healthColor);

            var isTickOvercharged =
                (Mathf.Approximately(percentToOne, tickHealthThreshold)
                    || percentToOne > tickHealthThreshold
                    || percentHealth >= 2)
                && isOvercharged;
            var outlineColor = isTickOvercharged ? overchargeOutlineColor : defaultOutlineColor;
            healthTickController.SetOutlineColor(outlineColor);
        }
    }
}

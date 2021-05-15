using System;
using System.Collections;
using System.Collections.Generic;
using MyAssets.ScriptableObjects.Variables;
using TMPro;
using UnityEngine;

public class SetLevelText : MonoBehaviour
{
     [SerializeField] private TextMeshProUGUI TargetText;
     [SerializeField] private IntReference CurrentLevel;

     private void Update()
     {
          TargetText.text = CurrentLevel.Value.ToString();
     }
}

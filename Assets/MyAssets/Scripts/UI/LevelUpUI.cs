using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelUpUI : MonoBehaviour
{
    [SerializeField] private Button FirstSelection;

    private void OnEnable()
    {
        FirstSelection.Select();
    }
}

using System;
using MyAssets.ScriptableObjects.Variables;
using TMPro;
using UnityEngine;

namespace MyAssets.Scripts.Misc
{
    public class SetIntegerText : MonoBehaviour
    {
        [SerializeField] private IntReference IntegerValue;
        [SerializeField] private TextMeshProUGUI TextMesh;

        private string _initialText;
        
        private void Start()
        {
            _initialText = TextMesh.text;
            IntegerValue.Subscribe(UpdateText);
            UpdateText();
        }

        private void OnDestroy()
        {
            IntegerValue.Unsubscribe(UpdateText);
        }

        private void UpdateText()
        {
            TextMesh.text = _initialText + IntegerValue.Value.ToString();
        }
    }
}
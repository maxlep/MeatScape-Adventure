using System;
using MyAssets.Scripts.Utils;
using TMPro;
using UnityEngine;

namespace MyAssets.Scripts.Misc
{
    public class AssignTextFromFile : MonoBehaviour
    {
        [SerializeField] private TextAsset _textSource;
        [SerializeField] private TextMeshProUGUI _textComponent;
        
        private void Awake()
        {
            if (_textSource == null) return;

            var excerpt = _textSource.TakePassage();
            _textComponent.text = excerpt;
        }
    }
}
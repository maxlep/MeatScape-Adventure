using System;
using MyAssets.ScriptableObjects.Variables;
using Sirenix.Utilities;
using UnityEngine;

namespace MyAssets.Scripts.Misc.VariableWrappers
{
    public class SetEnabled : MonoBehaviour
    {
        [SerializeField] private BoolReference Enabled;
        [SerializeField] private MonoBehaviour[] Targets;

        private void Start()
        {
            Enabled.Subscribe(() =>
            {
                Targets.ForEach(t => t.enabled = Enabled.Value);
            });
        }
    }
}
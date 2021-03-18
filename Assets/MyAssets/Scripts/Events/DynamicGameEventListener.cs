using MyAssets.ScriptableObjects.Events;
using UnityEngine;
using UnityEngine.Events;

namespace MyAssets.Scripts.Events
{
    public class DynamicGameEventListener : MonoBehaviour
    {
        public DynamicGameEvent Event;
        public UnityEvent<System.Object> Response;

        private void OnEnable()
        { Event.RegisterListener(this); }

        private void OnDisable()
        { Event.UnregisterListener(this); }

        public void OnEventRaised(System.Object obj)
        { Response.Invoke(obj); }
    }
}
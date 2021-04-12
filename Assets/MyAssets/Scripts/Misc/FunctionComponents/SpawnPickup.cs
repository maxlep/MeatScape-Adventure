using System;
using System.Collections.Generic;
using MyAssets.ScriptableObjects.Variables;
using Shapes;
using UnityEngine;

namespace MyAssets.Scripts.Misc.FunctionComponents
{
    public class SpawnPickup : MonoBehaviour
    {
        [Tooltip("GameObject must have component of type Pickupable")]
        [SerializeField] private GameObject _prefab;

        [SerializeField] private BoolReference _startSpawned;
        [SerializeField] private TimerReference _spawnDelay;
        [SerializeField] private IntReference _count;

        private List<GameObject> _spawned;

        private void Awake()
        {
            _spawned = new List<GameObject>();
        }

        private void Start()
        {
            if (_startSpawned.Value)
            {
                for (int i = 0; i < _count.Value; i++)
                {
                    Spawn();
                }
            }
            else
            {
                _spawnDelay.RestartTimer();
            }
        }

        private void Update()
        {
            _spawnDelay.UpdateTime();
            if (_spawnDelay.IsFinished)
            {
                var spawned = TrySpawn();
                if (spawned && _spawned.Count + 1 < _count.Value)
                {
                    _spawnDelay.RestartTimer();
                }
            }
        }

        public bool TrySpawn()
        {
            if (_spawned.Count < _count.Value || _count.Value <= 0)
            {
                Spawn();
                return true;
            }
            return false;
        }

        private void Spawn()
        {
            var spawned = Instantiate(_prefab, transform);
            _spawned.Add(spawned);
            spawned.GetComponentInChildren<Pickupable>().AddListenerOnPickup(() =>
            {
                if (_spawnDelay.IsStopped)
                {
                    _spawnDelay.RestartTimer();
                }
                _spawned.Remove(spawned);
            });
        }
    }
}
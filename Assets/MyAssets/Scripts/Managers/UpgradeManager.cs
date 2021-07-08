using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MyAssets.ScriptableObjects.Variables;
using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance;
    
    [SerializeField] private List<IntVariable> Stats;
    [SerializeField] private List<IntVariable> CurrrentTrophies;

    private List<StatSpawnTracker> statSpawnCount = new List<StatSpawnTracker>();

    private class StatSpawnTracker
    {
        public IntVariable stat;
        public int count;

        public StatSpawnTracker(IntVariable stat, int count)
        {
            this.stat = stat;
            this.count = count;
        }
    }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this);
        
        //Init the statSpawnCount list with list of stats
        Stats.ForEach(s => statSpawnCount.Add(new StatSpawnTracker(s, 0)));
    }

    public IntVariable GetRandomStatWithRemoval()
    {
        //Order by ascending spawn count, then get min
        statSpawnCount = statSpawnCount.OrderBy(t => t.count).ToList();

        statSpawnCount[0].count += 1;
        return statSpawnCount[0].stat;
    }
    
    public bool TrySpendTrophy()
    {
        foreach (var trophyCount in CurrrentTrophies)
        {
            if (trophyCount.Value > 0)
            {
                trophyCount.Value -= 1;
                return true;
            }
        }

        return false;
    }
}

using MyAssets.ScriptableObjects.Variables;
using UnityEngine;

public class HealthIcons : MonoBehaviour
{
    [SerializeField] private GameObject HealthIconPrefab;
    [SerializeField] private IntReference PlayerHealth;

    private GameObject[] healthIcons;
    
    private void Awake()
    {
        //Create all necessary health icons
        healthIcons = new GameObject[PlayerHealth.Value];
        for(int i = 0; i < healthIcons.Length; i++) {
            healthIcons[i] = Instantiate(HealthIconPrefab, this.transform);
        }
        PlayerHealth.Subscribe(this.SetIconCount);
    }

    private int GetEnabledHealthIconsCount() {
        int count = 0;
        for(int i = 0; i < healthIcons.Length; i++) {
            if(healthIcons[i].activeSelf) count++;
        }
        return count;
    }

    private void SetIconCount() {
        if(PlayerHealth.Value > -1) {
            int enabledHealthIconsCount = GetEnabledHealthIconsCount();
            if(enabledHealthIconsCount == PlayerHealth.Value) return;
            if(enabledHealthIconsCount > PlayerHealth.Value) {
                for(int i = 0; i < healthIcons.Length; i++) {
                    if(healthIcons[i].activeSelf) {
                        healthIcons[i].SetActive(false);
                        enabledHealthIconsCount--;
                        if(enabledHealthIconsCount == PlayerHealth.Value) return;
                    }
                }
            }
            if(enabledHealthIconsCount < PlayerHealth.Value) {
                for(int i = 0; i < healthIcons.Length; i++) {
                    if(!healthIcons[i].activeSelf) {
                        healthIcons[i].SetActive(true);
                        enabledHealthIconsCount++;
                        if(enabledHealthIconsCount == PlayerHealth.Value) return;
                    }
                }
            }
        }
    }
}

using Sirenix.OdinInspector;
using UnityEngine;

public class DebugManager : MonoBehaviour
{
    [SerializeField] [OnValueChanged(nameof(SetInitialDebugMode))] private bool enableDebugGUI;
    
    public static DebugManager Instance;

    public bool EnableDebugGUI => enableDebugGUI;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this);

        InputManager.Instance.onEnableDebug += ToggleDebugMode;
    }

    private void SetInitialDebugMode()
    {
        SetDebugMode(enableDebugGUI);
    }

    private void ToggleDebugMode()
    {
        SetDebugMode(!enableDebugGUI);
    }

    private void SetDebugMode(bool enableDebugGUI)
    {
        this.enableDebugGUI = enableDebugGUI;

        if (enableDebugGUI)
        {
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}

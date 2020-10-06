using UnityEngine;

public class DebugManager : MonoBehaviour
{
    public static DebugManager Instance;

    public bool EnableDebugGUI {get; private set;}

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this);

        InputManager.Instance.onEnableDebug += ToggleDebugMode;
    }

    private void ToggleDebugMode()
    {
        EnableDebugGUI = !EnableDebugGUI;

        if (EnableDebugGUI)
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

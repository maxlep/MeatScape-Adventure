using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using MyAssets.ScriptableObjects.Variables;
using UnityEngine;

public class CameraShakeManager : MonoBehaviour
{
    public static CameraShakeManager Instance;
    
    [SerializeField] private CinemachineFreeLook cinemachineVirtualCamera;

    private CinemachineBasicMultiChannelPerlin cinemachineBasicMultiChannelPerlinTop;
    private CinemachineBasicMultiChannelPerlin cinemachineBasicMultiChannelPerlinMiddle;
    private CinemachineBasicMultiChannelPerlin cinemachineBasicMultiChannelPerlinBottom;
    private LTDescr shakeTween = new LTDescr();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this);
        
        cinemachineBasicMultiChannelPerlinTop = cinemachineVirtualCamera.GetRig(0)
            .GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        
        cinemachineBasicMultiChannelPerlinMiddle = cinemachineVirtualCamera.GetRig(1)
            .GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        
        cinemachineBasicMultiChannelPerlinBottom = cinemachineVirtualCamera.GetRig(2)
            .GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
    }

    public void ShakeCamera(float amplitude, float frequency, float duraiton)
    {
        //Cancel current shake if in progress
        LeanTween.cancel(shakeTween.id);

        //Lerp amp and freq from inputs to 0 over duration
        shakeTween = LeanTween.value(1f, 0f, duraiton)
            .setOnUpdate(t =>
            {
                cinemachineBasicMultiChannelPerlinTop.m_AmplitudeGain = amplitude * t;
                cinemachineBasicMultiChannelPerlinTop.m_FrequencyGain = frequency * t;
                
                cinemachineBasicMultiChannelPerlinMiddle.m_AmplitudeGain = amplitude * t;
                cinemachineBasicMultiChannelPerlinMiddle.m_FrequencyGain = frequency * t;
                
                cinemachineBasicMultiChannelPerlinBottom.m_AmplitudeGain = amplitude * t;
                cinemachineBasicMultiChannelPerlinBottom.m_FrequencyGain = frequency * t;
            })
            .setOnComplete(_ =>
            {
                cinemachineBasicMultiChannelPerlinTop.m_AmplitudeGain = 0f;
                cinemachineBasicMultiChannelPerlinTop.m_FrequencyGain = 0f;
                
                cinemachineBasicMultiChannelPerlinMiddle.m_AmplitudeGain = 0f;
                cinemachineBasicMultiChannelPerlinMiddle.m_FrequencyGain = 0f;
                
                cinemachineBasicMultiChannelPerlinBottom.m_AmplitudeGain = 0f;
                cinemachineBasicMultiChannelPerlinBottom.m_FrequencyGain = 0f;
            });
    }
}

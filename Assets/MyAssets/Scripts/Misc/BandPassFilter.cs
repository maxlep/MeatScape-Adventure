using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class BandPassFilter : MonoBehaviour
{
    [SerializeField] private AudioHighPassFilter HighPassFilter;
    [SerializeField] private AudioLowPassFilter LowPassFilter;
    
    //Bandwidth as a percent of the whole log range (Ex. .5f means width is half the log range)
    [SerializeField] [Range(0f, 1f)] [OnValueChanged("UpdateFilters")]
    private float Bandwidth = .1f;
    
    [SerializeField] [LogRange(freqMin, freqCenter, freqMax)] [OnValueChanged("UpdateFilters")]
    private float BandCenter = 1000f;
    
    private const float freqMin = 0f;
    private const float freqCenter = 1000f;
    private const float freqMax = 20000f;

    private float bandMin;
    private float bandMax;

    private void Awake()
    {
        UpdateFilters();
    }


    [Button]
    public void Print()
    {
        Debug.Log($"Bandcenter: {BandCenter} | Bandwidth: {Bandwidth} | \nBandMin: {bandMin} | BandMax: {bandMax}");
    }

    public void UpdateFilters()
    {
        LogRangeConverter rangeConverter = new LogRangeConverter(freqMin, freqCenter, freqMax);
        float bandCenterNormalized = rangeConverter.ToNormalized(BandCenter);
        float bandMinNormalized = Mathf.Clamp01(bandCenterNormalized - Bandwidth);
        float bandMaxNormalized = Mathf.Clamp01(bandCenterNormalized + Bandwidth);
        
        bandMin = rangeConverter.ToRange(bandMinNormalized);
        bandMax = rangeConverter.ToRange(bandMaxNormalized);

        HighPassFilter.cutoffFrequency = bandMin;
        LowPassFilter.cutoffFrequency = bandMax;
    }

    //For automating the band center via some parameter
    public void SetBandCenter(float percent)
    {
        LogRangeConverter rangeConverter = new LogRangeConverter(freqMin, freqCenter, freqMax);
        BandCenter = rangeConverter.ToRange(percent);
        UpdateFilters();
    }
}

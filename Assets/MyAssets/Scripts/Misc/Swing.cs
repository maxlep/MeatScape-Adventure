using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Swing : MonoBehaviour
{
    [SerializeField] private Transform SwingPivot;
    [SerializeField] private float SwingTime = 5f;
    [SerializeField] private float SwingAngle = 80f;
    [SerializeField] private LeanTweenType EaseType = LeanTweenType.easeInOutCubic;

    private Quaternion startRot;
    private LTDescr swingTween;
    private bool isReverse;

    private void Start()
    {
        startRot = SwingPivot.localRotation;
        StartSwingTween();
    }

    private void OnDisable()
    {
        if (swingTween != null) LeanTween.cancel(swingTween.id);
    }

    private void StartSwingTween()
    {
        if (isReverse)
            swingTween = LeanTween.value(startRot.y - SwingAngle, startRot.y + SwingAngle, SwingTime);
        else
            swingTween = LeanTween.value(startRot.y + SwingAngle, startRot.y - SwingAngle, SwingTime);

        swingTween.setEase(EaseType);
        swingTween.setOnUpdate(y =>
        {
            Quaternion newRot = Quaternion.Euler(startRot.x, startRot.z, y);
            SwingPivot.localRotation = newRot;
        });
        swingTween.setOnComplete(_ =>
        {
            isReverse = !isReverse;
            StartSwingTween();
        });
    }
}

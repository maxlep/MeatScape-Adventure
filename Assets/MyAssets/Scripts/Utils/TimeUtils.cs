using System;

namespace MyAssets.Scripts.Utils
{
    public static class TimeUtils
    {
        public static void SetTimeout(float time, Action callback)
        {
            LeanTween.value(0f, 1f, time).setOnComplete(callback);
        }
    }
}

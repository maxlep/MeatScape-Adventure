using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MyAssets.Scripts.Utils
{
    public static class TextAssetUtils
    {
        private static int _subsectionSize = 500;
        private static string[] _separators = {$"{System.Environment.NewLine}{System.Environment.NewLine}"}; 
        
        public static string TakePassage(this TextAsset textAsset)
        {
            int randomChar = Random.Range(0, textAsset.text.Length);
            var subsectionStart = Math.Max(randomChar - _subsectionSize, 0);
            var subsectionLength = Math.Min(_subsectionSize * 2, textAsset.text.Length - subsectionStart);
            string subsection = textAsset.text.Substring(subsectionStart, subsectionLength);
            
            var split = subsection.Split(_separators, StringSplitOptions.None);

            if (split.Length < 2)
            {
                return "ERROR EXTRACTING PASSAGE";
            }
            return split[1];
        }
    }
}
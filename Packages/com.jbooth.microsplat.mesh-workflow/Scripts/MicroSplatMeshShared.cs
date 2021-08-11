//////////////////////////////////////////////////////
// MicroSplat
// Copyright (c) Jason Booth
//////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JBooth.MicroSplat
{

   [System.Serializable]
   public class CombinedOverride
   {
      // for combined mode
      public Texture2D standardAlbedoOverride;
      public Texture2D standardNormalOverride;
      public Texture2D standardPackedOverride;
      public Texture2D standardMetalSmoothOverride;
      public Texture2D standardOcclusionOverride;
      public Texture2D standardHeightOverride;
      public Texture2D standardEmissionOverride;
      public Texture2D standardSpecularOverride;
      public Texture2D standardSSS;
      public bool bStandardUVOverride = false;
      public Vector4 standardUVOverride = new Vector4 (1, 1, 0, 0);
      public bool bStandardColorOverride = false;
      public Color standardColorOverride = Color.white;

      public long GetHash ()
      {
         long h = 3;
         unchecked
         {

            h = h * ((standardAlbedoOverride == null) ? 3 : standardAlbedoOverride.GetNativeTexturePtr ().ToInt64 ()) * 3;
            h = h * ((standardNormalOverride == null) ? 5 : standardAlbedoOverride.GetNativeTexturePtr ().ToInt64 ()) * 5;
            h = h * ((standardPackedOverride == null) ? 7 : standardAlbedoOverride.GetNativeTexturePtr ().ToInt64 ()) * 7;
            h = h * ((standardMetalSmoothOverride == null) ? 13 : standardAlbedoOverride.GetNativeTexturePtr ().ToInt64 ()) * 13;
            h = h * ((standardOcclusionOverride == null) ? 21 : standardAlbedoOverride.GetNativeTexturePtr ().ToInt64 ()) * 17;
            h = h * ((standardHeightOverride == null) ? 31 : standardAlbedoOverride.GetNativeTexturePtr ().ToInt64 ()) * 31;
            h = h * ((standardEmissionOverride == null) ? 37 : standardAlbedoOverride.GetNativeTexturePtr ().ToInt64 ()) * 37;
            h = h * ((standardSpecularOverride == null) ? 41 : standardSpecularOverride.GetNativeTexturePtr ().ToInt64 ()) * 41;
            h = h * ((standardSSS == null) ? 43 : standardSSS.GetNativeTexturePtr ().ToInt64 ()) * 43;
            if (bStandardUVOverride)
            {
               h = h * standardUVOverride.GetHashCode ();
            }
            if (bStandardColorOverride)
            {
               h = h * (int)(1 + standardColorOverride.r * 1001 + standardColorOverride.g * 1007 + standardColorOverride.b * 1009 + standardColorOverride.a * 1003);
            }
         }
         if (h == 0)
         {
            Debug.Log ("Combined override hash returned 0, this should not happen");
         }
         return h;
      }
   }



}

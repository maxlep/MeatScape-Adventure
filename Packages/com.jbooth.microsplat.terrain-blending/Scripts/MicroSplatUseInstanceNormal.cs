
using UnityEngine;

namespace JBooth.MicroSplat
{
   // hack around the issue that the terrain normal map is not available until the second frame. You can use this
   // to have terrain blending work without per-pixel normal maps, but you will get a blip in the first frame on the blended objects.
   [ExecuteInEditMode]
   public class MicroSplatUseInstanceNormal : MonoBehaviour
   {
      int frame = 0;
      void LateUpdate()
      {
         frame++;
         if (frame == 2)
         {
            var t = GetComponent<MicroSplatTerrain>();
            if (t != null)
            {
               t.ApplyBlendMap();
            }
#if !UNITY_EDITOR
         DestroyImmediate (this);
#else
            if (Application.isPlaying)
            {
               DestroyImmediate(this);
            }
#endif
         }
      }
   }

}
//////////////////////////////////////////////////////
// MicroSplat
// Copyright (c) Jason Booth
//////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JBooth.MicroSplat
{
   [ExecuteAlways]
   public class MicroSplatDecal : MonoBehaviour
   {

      public enum SplatMode
      {
         SplatMap,
         StreamMap,
      }

      public enum NormalBlend
      {
         Replace,
         Blend
      }

      public enum AlbedoBlend
      {
         Blend,
         Multiply2X
      }

      public MicroSplatDecalReceiver targetObject;
      public int textureIndex = 0;
      public int splatTextureIndex = 0;
      public float albedoOpacity = 1;
      public float smoothnessOpacity = 1;
      public float heightBlend = 0;
      public float normalOpacity = 1;
      public float splatOpacity = 0;
      public Color tint = Color.white;
      public AlbedoBlend albedoBlend = AlbedoBlend.Blend;

      public SplatMode splatMode = SplatMode.SplatMap;
      public NormalBlend normalBlend = NormalBlend.Replace;

#if __MICROSPLAT_TESSELLATION__
      public float tessOpacity = 1;
      public float tessOffset = 0;
#endif

      public int sortOrder = 0;

#if UNITY_EDITOR
      public static Color staticGizmoColor = new Color(1, 1, 0, 0.35f);
      public static Color staticGizmoSelectedColor = new Color(1, 1, 0, 0.5f);
      public static Color dynamicGizmoColor = new Color(1, 0.25f, 0, 0.35f);
      public static Color dynamicGizmoSelectedColor = new Color(1, 0.25f, 0, 0.5f);

      public enum GizmoMode
      {
         Hide,
         ShowAll,
         Selected
      }
      public static GizmoMode gizmoMode = GizmoMode.ShowAll;
#endif

      public Vector4 splatIndexes = new Vector4(0, 1, 2, 3);



      [SerializeField]
      bool _dynamic = false;
      public bool dynamic
      {
         set
         {
            if (value != _dynamic)
            {
               if (enabled)
               {
                  OnDisable();
               }
               _dynamic = value;
               if (enabled)
               {
                  OnEnable();
               }
            }
         }
         get
         {
            return _dynamic;
         }
      }

#if UNITY_EDITOR
      bool registeredAsDynamic;

#endif

      public void GetShaderData(out Vector4 data1, out Vector4 data2)
      {
         float tessellationData = 0;
#if __MICROSPLAT_TESSELLATION__
         // pack together
         tessellationData = Mathf.Floor(tessOffset * 256) + tessOpacity * 0.95f;
#endif
         // encode bool into sign
         float splatOpacityAndMode = (splatOpacity + 1) * ((splatMode == SplatMode.SplatMap) ? 1 : -1);
         // encode normal mode into sign of normal opacity
         float normalOpacityAndBlend = (normalOpacity + 1) * ((normalBlend == NormalBlend.Replace) ? 1 : -1);
         float albedoOpacityAndBlend = (albedoOpacity + 1) * ((albedoBlend == AlbedoBlend.Blend) ? 1 : -1);
         float textureAndSplatIndex = splatTextureIndex * 100 + textureIndex;
         data1 = new Vector4(textureAndSplatIndex, transform.lossyScale.y, splatOpacityAndMode, tessellationData);
         data2 = new Vector4(albedoOpacityAndBlend, normalOpacityAndBlend, smoothnessOpacity, heightBlend);

      }

      private void OnEnable()
      {
         oldMtx = transform.localToWorldMatrix;

         if (targetObject != null)
         {
#if UNITY_EDITOR
            registeredAsDynamic =
#endif
            targetObject.RegisterDecal(this);
         }
      }

      private void OnDisable()
      {
         if (targetObject != null)
         {
            targetObject.UnregisterDecal(this);
         }
      }

      private void OnDestroy()
      {
         OnDisable();
      }

      public void Reset()
      {
         OnDisable();
         OnEnable();
      }

#if UNITY_EDITOR
      void OnDrawGizmos()
      {
         if (gizmoMode == GizmoMode.ShowAll)
         {
            if (registeredAsDynamic)
               Gizmos.color = dynamicGizmoColor;
            else
               Gizmos.color = staticGizmoColor;

            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(Vector3.zero, Vector3.one);
            Gizmos.color = new Color(0, 0, 0, Gizmos.color.a * 1.2f);
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
         }
      }

      void OnDrawGizmosSelected()
      {
         if (gizmoMode != GizmoMode.Hide)
         {
            if (registeredAsDynamic)
               Gizmos.color = dynamicGizmoSelectedColor;
            else
               Gizmos.color = staticGizmoSelectedColor;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(Vector3.zero, Vector3.one);
            Gizmos.color = new Color(0, 0, 0, Gizmos.color.a * 1.5f);
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
         }
      }


#endif



      Matrix4x4 oldMtx;

      void UpdateRendering()
      {
         if (targetObject != null && targetObject.msObj != null)
         {
            MicroSplatTerrain mst = targetObject.msObj as MicroSplatTerrain;
            if (mst != null)
            {
               targetObject.UpdateDecalInCache(mst.terrain.transform.position, mst.terrain.terrainData.size, this, oldMtx);
               oldMtx = transform.localToWorldMatrix;
            }
#if __MICROSPLAT_MESHTERRAIN__
            MicroSplatMeshTerrain meshTerrain = targetObject.msObj as MicroSplatMeshTerrain;
            if (meshTerrain != null)
            {
               targetObject.UpdateDecalInCache(meshTerrain.transform.position, meshTerrain.GetBounds().size, this, oldMtx);
               oldMtx = transform.localToWorldMatrix;
            }
#endif
         }
      }

      private void Update()
      {
         if (transform.hasChanged)
         {
            transform.hasChanged = false;

            UpdateRendering();
         }
      }


   }
}
//////////////////////////////////////////////////////
// MicroSplat
// Copyright (c) Jason Booth
//////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JBooth.MicroSplat
{
   public class ShaderID
   {
      public static int _GMSTraxBuffer = Shader.PropertyToID ("_GMSTraxBuffer");
      public static int _Offset = Shader.PropertyToID ("_Offset");
      public static int _DepthRT = Shader.PropertyToID ("_DepthRT");
      public static int _RepairDelay = Shader.PropertyToID ("_RepairDelay");
      public static int _RepairRate = Shader.PropertyToID ("_RepairRate");
      public static int _UseTime = Shader.PropertyToID ("_UseTime");
      public static int _RepairTotal = Shader.PropertyToID ("_RepairTotal");
      public static int _BufferBlend = Shader.PropertyToID ("_BufferBlend");
      public static int _SinkStrength = Shader.PropertyToID ("_SinkStrength");

      public static int _GMSTraxBufferPosition = Shader.PropertyToID ("_GMSTraxBufferPosition");
      public static int _GMSTraxBufferWorldSize = Shader.PropertyToID ("_GMSTraxBufferWorldSize");
      public static int _GMSTraxFudgeFactor = Shader.PropertyToID ("_GMSTraxFudgeFactor");

      public static int _CamCaptureHeight = Shader.PropertyToID ("_CamCaptureHeight");
      public static int _CamFarClipPlane = Shader.PropertyToID ("_CamFarClipPlane");
   }

   [ExecuteInEditMode]
   public class TraxManager : MonoBehaviour
   {
      public enum Precision
      {
         Half,
         Full
      }
      public Precision precision = Precision.Full;

      public int bufferSize = 1024;
      public float worldSize = 128;
      public LayerMask layerMask;

      public bool useTime;
      public float repairDelay;
      public float repairRate;
      public float repairTotal;


      // advanced
      public float bufferBlend = 0.5f;
      public float collsionDistance = 1.0f;
      public float sinkStrength = 0.5f;
      public int bufferBlits = 1;

      [HideInInspector] public Camera cam;

      RenderTexture depthRT;
      RenderTexture bufferA;
      RenderTexture bufferB;
      bool bufferBActive;
      Material bufferCopyMat;

      Vector3 lastPosition = Vector3.zero;


#if UNITY_EDITOR
      void OnDrawGizmosSelected ()
      {
         UnityEditor.Handles.color = Color.red;
         UnityEditor.Handles.DrawWireDisc (transform.position, Vector3.up, worldSize);
      }
#endif

      public void Setup ()
      {
         TearDown ();
         RenderTextureDescriptor d;
         if (precision == Precision.Full)
         {
            if (useTime && (repairTotal > 0 || repairDelay > 0))
            {
               d = new RenderTextureDescriptor (bufferSize, bufferSize, RenderTextureFormat.RGFloat, 0);
            }
            else
            {
               d = new RenderTextureDescriptor (bufferSize, bufferSize, RenderTextureFormat.RFloat, 0);
            }
         }
         else
         {
            if (useTime)
            {
               d = new RenderTextureDescriptor (bufferSize, bufferSize, RenderTextureFormat.RGFloat, 0);
            }
            else
            {
               d = new RenderTextureDescriptor (bufferSize, bufferSize, RenderTextureFormat.RHalf, 0);
            }
         }

         bufferA = new RenderTexture (d) { name = "rtTraxBufferA" };
         bufferB = new RenderTexture (d) { name = "rtTraxBufferB" };

         depthRT = new RenderTexture (bufferSize, bufferSize, 32, RenderTextureFormat.Depth, RenderTextureReadWrite.Linear) { name = "rtTraxDepth" };
         if (cam == null)
         {
            GameObject go = new GameObject ("Trax Camera");
            go.hideFlags = HideFlags.HideAndDontSave;
            cam = go.AddComponent<Camera> ();
         }

#if USING_HDRP
         
         var camData = cam.gameObject.AddComponent<UnityEngine.Rendering.HighDefinition.HDAdditionalCameraData>();
         camData.antialiasing = UnityEngine.Rendering.HighDefinition.HDAdditionalCameraData.AntialiasingMode.None;
         camData.backgroundColorHDR = new Color(99999, 0, 0, 1);
         camData.clearColorMode = UnityEngine.Rendering.HighDefinition.HDAdditionalCameraData.ClearColorMode.Color;
         camData.clearDepth = true;
         camData.volumeLayerMask = 0;
         camData.renderingPathCustomFrameSettings.litShaderMode = UnityEngine.Rendering.HighDefinition.LitShaderMode.Forward;
#endif


         cam.orthographic = true;
         cam.orthographicSize = worldSize;

         cam.transform.forward = Vector3.up;
         cam.orthographicSize = worldSize;

         cam.nearClipPlane = 0;
         cam.farClipPlane = 2000;
         cam.cullingMask = (int)layerMask;

         cam.clearFlags = CameraClearFlags.Color;
         cam.backgroundColor = new Color (99999, 0, 0, 1);

         bufferCopyMat = new Material (Shader.Find ("Hidden/MicroSplat/TraxBuffer"));

         // I tried initializing this with a Graphics.Blit, but it causes an issue where the
         // texture starts uncleared
         cam.transform.position = new Vector3 (0, -99999, 0); // move it where we won't render something
         cam.targetTexture = depthRT;
         cam.Render ();
         cam.targetTexture = bufferA;
         cam.Render ();
         cam.targetTexture = bufferB;
         cam.Render ();

         // Material copyDepthMat = new Material(Shader.Find("Hidden/MicroSplat/CopyDepth"));
         // Graphics.Blit(depthRT, bufferA, copyDepthMat);
         // Graphics.Blit(depthRT, bufferB, copyDepthMat);
      }

      public void TearDown ()
      {
         RenderTexture.active = null;
         if (cam != null)
         {
            cam.targetTexture = null;
            DestroyImmediate (cam.gameObject);
         }
         DisposeRenderTexture (ref depthRT);
         DisposeRenderTexture (ref bufferA);
         DisposeRenderTexture (ref bufferB);


         if (bufferCopyMat != null) DestroyImmediate (bufferCopyMat);

         bufferCopyMat = null;
         cam = null;
      }

      void DisposeRenderTexture (ref RenderTexture rt)
      {
         if (rt == null) return;
         rt.Release ();
         DestroyImmediate (rt);
         rt = null;
      }

      Texture2D bufferFetch = null;
      /// <summary>
      /// Given a world position, will return how high the trax buffer is.
      /// Note that the trax buffer only stores the world position of the depression, and starts infinitely
      /// far away from the terrian. The trax buffer does not know where the terrain is, or how high off
      /// the terrain tessellation can go. Rather, it only stores what the minimum value of any object
      /// rendered into the trax buffer was, and if repair is used that value is essentially pushed up
      /// over time. So a bird flying overhead will lower the buffer height, even though it will have no
      /// effect on the terrain. The buffer is cleared to 99999 and any value outside the buffer will return
      /// the same.
      /// </summary>
      /// <param name="terrainPosition"></param>
      /// <returns></returns>
      public float GetBufferAtPosition (Vector3 terrainPosition)
      {
         if (bufferA == null)
            return 99999;

         if (bufferFetch == null)
         {
            bufferFetch = new Texture2D (1, 1, TextureFormat.RGBAFloat, false, true);
         }
         RenderTexture t = bufferBActive ? bufferB : bufferA;
         Vector2 uv = new Vector2 (terrainPosition.x, terrainPosition.z);
         uv -= new Vector2 (transform.position.x, transform.position.z);
         uv += new Vector2 (worldSize * 0.5f, worldSize * 0.5f);
         uv /= worldSize;
         uv *= t.width;
         int x = (int)uv.x;
         int y = (int)uv.y;
         if (x > t.width || y > t.height || x < 0 || y < 0)
            return 99999;

         var old = RenderTexture.active;
         RenderTexture.active = t;

         bufferFetch.ReadPixels (new Rect (x, y, 1, 1), 0, 0);
         bufferFetch.Apply ();
         Color c = bufferFetch.GetPixel (0, 0);
         RenderTexture.active = old;
         return c.r;
      }

      /*
      private void Update ()
      {
         Debug.Log (GetBufferAtPosition (this.transform.position + new Vector3 (UnityEngine.Random.value * 600 - 256, 0, Random.value * 600 - 256)));
      }
      */

      private void OnEnable ()
      {
         Setup ();

      }

      private void OnDisable ()
      {
         TearDown ();
      }

      private float SnapToPixel (float v, int textureSize, float orthoSize)
      {
         float worldPixel = (orthoSize * 2.0f) / textureSize;
         v = (int)(v / worldPixel);
         v *= worldPixel;
         return v;
      }


      private void LateUpdate ()
      {
         if (!Application.isPlaying)
         {
            cam.targetTexture = bufferA;
            Shader.SetGlobalTexture (ShaderID._GMSTraxBuffer, bufferA);
            return;
         }


         Vector3 position = transform.position + Vector3.up;
         position.x = SnapToPixel (position.x, bufferSize, worldSize);
         position.z = SnapToPixel (position.z, bufferSize, worldSize);
         position.y -= 1000;
         cam.transform.position = position;

         Vector3 offset = lastPosition - position;
         offset.x = SnapToPixel (offset.x, bufferSize, worldSize);
         offset.z = SnapToPixel (offset.z, bufferSize, worldSize);

         offset.x /= worldSize;
         offset.z /= worldSize;

         offset.x *= 0.5f;
         offset.z *= 0.5f;

         bufferCopyMat.SetVector (ShaderID._Offset, new Vector2 (offset.x, offset.z));

         // render into composite buffer
         cam.targetTexture = depthRT;

         bufferCopyMat.SetTexture (ShaderID._DepthRT, depthRT);
         bufferCopyMat.SetFloat (ShaderID._RepairDelay, repairDelay);
         bufferCopyMat.SetFloat (ShaderID._RepairRate, 1.0f / Mathf.Max (0.001f, repairRate));
         bufferCopyMat.SetFloat (ShaderID._UseTime, useTime ? 1 : 0);
         bufferCopyMat.SetFloat (ShaderID._RepairTotal, repairTotal);
         bufferCopyMat.SetFloat (ShaderID._BufferBlend, bufferBlend);
         bufferCopyMat.SetFloat (ShaderID._SinkStrength, sinkStrength);

         bufferCopyMat.SetFloat (ShaderID._CamCaptureHeight, position.y);
         bufferCopyMat.SetFloat (ShaderID._CamFarClipPlane, cam.farClipPlane);

         RenderTexture A = bufferA;
         RenderTexture B = bufferB;
         if (bufferBActive) Swap (ref A, ref B);

         Graphics.Blit (A, B, bufferCopyMat);
         bufferBActive = !bufferBActive;

         bufferCopyMat.SetVector (ShaderID._Offset, new Vector2 (0, 0));
         bufferCopyMat.SetFloat (ShaderID._UseTime, 0);
         Shader.SetGlobalTexture (ShaderID._GMSTraxBuffer, B);

         for (int i = 0; i < bufferBlits; ++i)
         {
            Graphics.Blit (B, A, bufferCopyMat);
            bufferBActive = !bufferBActive;
            Shader.SetGlobalTexture (ShaderID._GMSTraxBuffer, A);
            Swap (ref A, ref B);
         }

         Shader.SetGlobalVector (ShaderID._GMSTraxBufferPosition, position);
         Shader.SetGlobalFloat (ShaderID._GMSTraxBufferWorldSize, worldSize);
         Shader.SetGlobalFloat (ShaderID._GMSTraxFudgeFactor, collsionDistance);

         lastPosition = position;
      }

      void Swap<T> (ref T a, ref T b)
      {
         T temp = a;
         a = b;
         b = temp;
      }
   }
}

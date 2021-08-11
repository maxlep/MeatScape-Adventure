//////////////////////////////////////////////////////
// MicroSplat
// Copyright (c) Jason Booth
//////////////////////////////////////////////////////


using UnityEngine;
using System.Collections;
using UnityEditor;


namespace JBooth.MicroSplat
{
#if __MICROSPLAT__ && __MICROSPLAT_MESH__

   public partial class MeshPainterWindow : EditorWindow
   {
      double deltaTime = 0;
      double lastTime = 0;
      bool painting = false;
      public TextureArrayConfig config;
      public int textureIndex = 0;

      public Vector3 oldpos = Vector3.zero;
      public float brushSize = 3;
      public float brushFlow = 1;
      public float brushRotation = 0;
      public Color brushDisplayColor = Color.blue;
      public Vector2 angleFilter = new Vector2(-1, 1);
      //public bool projectionFilter = false;

      public System.Action<MeshJob[]> OnBeginStroke;
      public System.Action<MeshJob, bool> OnStokeModified;  // bool is true when doing a fill or other non-bounded opperation
      public System.Action OnEndStroke;

      RenderTexture debugBrush;
      int debugBrushSubmeshIndex;


      public Vector2 lastMousePosition;
      void OnSceneGUI(SceneView sceneView)
      {
         deltaTime = EditorApplication.timeSinceStartup - lastTime;
         lastTime = EditorApplication.timeSinceStartup;

         if (!enabled || Selection.activeGameObject == null)
         {
            return;
         }

         if (VerifyData() == false)
         {
            return;
         }
            

         RaycastHit hit;
         float distance = float.MaxValue;
         Vector3 mousePosition = Event.current.mousePosition;
         Vector2 uv = Vector2.zero;
         Vector2 uv2 = Vector2.zero;

         // So, in 5.4, Unity added this value, which is basically a scale to mouse coordinates for retna monitors.
         // Not all monitors, just some of them.
         // What I don't get is why the fuck they don't just pass me the correct fucking value instead. I spent hours
         // finding this, and even the paid Unity support my company pays many thousands of dollars for had no idea
         // after several weeks of back and forth. If your going to fake the coordinates for some reason, please do
         // it everywhere to not just randomly break things everywhere you don't multiply some new value in. 
         float mult = EditorGUIUtility.pixelsPerPoint;

         mousePosition.y = sceneView.camera.pixelHeight - mousePosition.y * mult;
         mousePosition.x *= mult;
         Vector3 fakeMP = mousePosition;
         fakeMP.z = 20;
         Vector3 point = sceneView.camera.ScreenToWorldPoint(fakeMP);
         Vector3 normal = Vector3.forward;
         Ray ray = sceneView.camera.ScreenPointToRay(mousePosition);

         bool registerUndo = (Event.current.type == EventType.MouseDown && Event.current.button == 0 && Event.current.alt == false);

         if (meshes == null)
         {
            return;
         }
         bool testHit = false;
         for (int i = 0; i < meshes.Length; ++i)
         {
            if (meshes[i] == null)
               continue;
            // Early out if we're not in the area..
            var cld = meshes[i].collider;
            Bounds b = cld.bounds;
            b.Expand(brushSize * 2);
            if (!b.IntersectRay(ray))
            {
               continue;
            }
            testHit = true;
            if (registerUndo)
            {
               painting = true;
               for (int x = 0; x < jobEdits.Length; ++x)
               {
                  jobEdits[x] = false;
               }
               if (i == 0 && OnBeginStroke != null)
               {
                  OnBeginStroke(meshes);
               }
            }

            if (cld.Raycast(ray, out hit, float.MaxValue))
            {
               if (Event.current.shift == false)
               {
                  if (hit.distance < distance)
                  {
                     uv2 = hit.lightmapCoord;
                     uv = hit.textureCoord;
                     distance = hit.distance;
                     point = hit.point;
                     normal = hit.normal;
                  }
               }
               else
               {
                  point = oldpos;
               }
            }
            else
            {
               if (Event.current.shift == true)
               {
                  point = oldpos;
               }
            }
         }

         if (Event.current.type == EventType.MouseMove && Event.current.shift)
         {
            brushSize += Event.current.delta.x * (float)deltaTime * 6.0f;
         }

         if (Event.current.rawType == EventType.MouseUp)
         {
            EndStroke();
         }
         if (Event.current.type == EventType.MouseMove && Event.current.alt)
         {
            brushSize += Event.current.delta.y * (float)deltaTime;
         }


         // eat current event if mouse event and we're painting
         if (Event.current.isMouse && painting)
         {
            Event.current.Use();
         }

         if (Event.current.type == EventType.Layout)
         {
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(GetHashCode(), FocusType.Passive));
         }

         // only paint once per frame
         if (Event.current.type != EventType.Repaint)
         {
            return;
         }

         if (!testHit)
         {
            return;
         }

         if (meshes.Length > 0 && painting)
         {
            for (int i = 0; i < meshes.Length; ++i)
            {
               Bounds b = meshes[i].collider.bounds;
               b.Expand(brushSize * 2);
               if (!b.IntersectRay(ray))
               {
                  continue;
               }
               if (jobEdits[i] == false)
               {
                  jobEdits[i] = true;
                  meshes[i].RegisterUndo((MeshJob.UndoBuffer)(textureIndex / 4));
               }
               PaintMeshGPU(meshes[i], point, normal);
               if (OnStokeModified != null)
               {
                  OnStokeModified(meshes[i], false);
               }
            }
         }
         else
         {
            for (int i = 0; i < meshes.Length; ++i)
            {
               PaintMeshGPU(meshes[i], point, normal, true);
            }
         }

         lastMousePosition = Event.current.mousePosition;
         // update views
         sceneView.Repaint();
         HandleUtility.Repaint();
      }


      void EndStroke()
      {
         painting = false;
         if (OnEndStroke != null)
         {
            OnEndStroke();
         }
      }


      Material brushMat;
      Material brushApplyStrokeMat;


      public Camera brushCam;
      public Projector brushProjector;


      void SetupBrush(MeshJob mj, Vector3 brushWorldPos, Vector3 normal, bool isFill)
      {
         // setup data
         if (brushMat == null)
         {
            brushMat = new Material(Shader.Find("Hidden/MicroSplatMeshBrush"));
         }
         if (brushApplyStrokeMat == null)
         {
            brushApplyStrokeMat = new Material(Shader.Find("Hidden/MicroSplatMeshBrushApply"));
         }
         if (brushCam == null)
         {
            GameObject go = new GameObject();
            go.hideFlags = HideFlags.HideAndDontSave;
            brushCam = go.AddComponent<Camera>();
         }

         if (brushProjector == null)
         {
            GameObject go = new GameObject();
            go.hideFlags = HideFlags.HideAndDontSave;
            brushProjector = go.AddComponent<Projector>();
            brushProjector.orthographic = true;
            brushProjector.material = new Material(Shader.Find("Hidden/MicroSplatMeshPaintProjector"));

         }


         brushCam.transform.position = brushWorldPos; 
         if (SceneView.currentDrawingSceneView != null) // fill mode
         {
            brushCam.transform.rotation = SceneView.currentDrawingSceneView.camera.transform.rotation;
         }
         brushCam.orthographic = true;
         brushCam.nearClipPlane = brushSize;
         brushCam.farClipPlane = brushSize;
         brushCam.orthographicSize = brushSize;
         brushCam.aspect = 1;
         Vector3 euler = brushCam.transform.rotation.eulerAngles;
         euler.z += brushRotation;
         brushCam.transform.rotation = Quaternion.Euler(euler);
         var viewMtx = brushCam.worldToCameraMatrix;
         var projMtx = brushCam.projectionMatrix;

         // Pass this to shader to project it like a spot light with cookie texture
         var brushWorldToProjMtx = GL.GetGPUProjectionMatrix(projMtx, true) * viewMtx;

         // setup
         //var obj2world = mj.msMesh.transform.localToWorldMatrix;
         Matrix4x4 obj2world = Matrix4x4.TRS(mj.msMesh.transform.position, Quaternion.identity, Vector3.one);
         Matrix4x4 obj2Scale = Matrix4x4.TRS (Vector3.zero, mj.msMesh.transform.rotation, mj.msMesh.transform.lossyScale);

         brushMat.SetTexture("_MainTex", curBrush);
         brushMat.SetMatrix("_BrushWorldToProjMtx", brushWorldToProjMtx);
         brushMat.SetMatrix("_Obj2World", obj2world);
         brushMat.SetMatrix("_Obj2Scale", obj2Scale);
         brushMat.SetVector("_AngleFilter", angleFilter);
         //brushMat.SetFloat ("_ProjectionFilter", projectionFilter ? 10 : 1);
         brushMat.SetFloat ("_BrushSize", brushSize);
         brushMat.SetFloat("useUV2", mj.msMesh.keywordSO.IsKeywordEnabled("_MESHUV2") ? 1 : 0);
         brushMat.SetVector ("_MouseWorldPos", brushWorldPos);
         brushMat.SetFloat ("_IsFill", isFill ? 1 : 0);

         // projector setup
         brushProjector.orthographicSize = brushSize;
         brushProjector.farClipPlane = brushSize;
         brushProjector.nearClipPlane = brushSize;
         brushProjector.material.SetTexture("_Tex", curBrush);
         brushProjector.transform.position = brushWorldPos;
         brushProjector.transform.rotation = Quaternion.Euler(euler);
      }

      void PaintMeshGPU(MeshJob mj, Vector3 brushWorldPos, Vector3 normal, bool preview = false, bool isFill = false)
      {
         if (mj == null || mj.msMesh == null)
            return;

         if (SceneView.currentDrawingSceneView == null && !isFill)
         {
            return;
         }

         if (!isFill)
         {
            Handles.color = new Color (0.9f, 0, 0, 0.8f);

            Handles.SphereHandleCap (0, brushWorldPos, Quaternion.identity, brushSize * 0.075f, EventType.Repaint);
            if (mj.msMesh.keywordSO != null && !mj.msMesh.keywordSO.IsKeywordEnabled("_MSRENDERLOOP_SURFACESHADER"))
            {
               Handles.DrawWireDisc (brushWorldPos, normal, brushSize);
            }            
         }

         for (int subMeshIdx = 0; subMeshIdx < mj.msMesh.subMeshEntries.Count; ++subMeshIdx)
         {
            var sub = mj.msMesh.subMeshEntries [subMeshIdx];
            if (sub.subMeshOverride.active == false)
               continue;

            if (sub.subMeshOverride.controlTextures == null || sub.subMeshOverride.controlTextures.Length == 0 || sub.subMeshOverride.controlTextures [0] == null)
               continue;

            SetupBrush (mj, brushWorldPos, normal, isFill);
            if (sub.subMeshOverride.bUVRangeOverride)
            {
               brushMat.SetVector ("_UVMeshRange", sub.subMeshOverride.uvRange);
            }
            else
            {
               brushMat.SetVector ("_UVMeshRange", new Vector4(0,0,1f,1f));
            }
            brushProjector.material.SetColor ("_BrushColor", brushDisplayColor);

            if (preview)
            {
               return;
            }

            if (tab != Tab.Texture)
            {
               PaintFXMeshGPU (mj, brushWorldPos, preview, isFill);
               continue;
            }

            float pressure = Event.current.pressure > 0 ? Event.current.pressure : 1.0f;
            pressure *= Time.deltaTime;

            int index = textureIndex / 4;
            if (index >= sub.subMeshOverride.controlTextures.Length)
            {
               index = sub.subMeshOverride.controlTextures.Length - 1;
            }
            Texture2D splatTex = sub.subMeshOverride.controlTextures [index];

            int channelIdx = textureIndex;
            while (channelIdx > 3)
               channelIdx -= 4;



            RenderTexture rt = RenderTexture.GetTemporary (splatTex.width, splatTex.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear, 1);
            RenderTexture rt2 = RenderTexture.GetTemporary (splatTex.width, splatTex.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear, 1);


            RenderTexture.active = rt;
            GL.PushMatrix ();
            GL.LoadIdentity ();
            GL.Clear (true, true, Color.black);
            brushMat.SetPass (0);
            Graphics.DrawMeshNow (mj.sharedMesh, Vector3.zero, Quaternion.identity, subMeshIdx);
            GL.PopMatrix ();
            RenderTexture.active = null;

            if (debugBrush == null && showDebug)
            {
               debugBrush = new RenderTexture (256, 256, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            }
            if (!preview && showDebug)
            {
               if (subMeshIdx == debugBrushSubmeshIndex)
               {
                  Graphics.Blit (rt, debugBrush);
               }
            }


            // blit stroke to control texture
            brushApplyStrokeMat.SetTexture ("_BrushBuffer", rt);
            brushApplyStrokeMat.SetInt ("_channel", channelIdx);
            brushApplyStrokeMat.SetFloat ("_BrushFlow", isFill ? 1 : brushFlow * pressure);
            brushApplyStrokeMat.SetFloat ("_BrushTarget", brushTargetValue);
            brushApplyStrokeMat.SetVector ("_EdgeBuffer", new Vector2 (1.0f / rt.width, 1.0f / rt.height));

            var ct = sub.subMeshOverride.controlTextures;
            brushApplyStrokeMat.SetTexture ("_Control0", ct [0]);
            brushApplyStrokeMat.SetTexture ("_Control1", ct.Length > 1 && ct [1] != null ? ct [1] : Texture2D.blackTexture);
            brushApplyStrokeMat.SetTexture ("_Control2", ct.Length > 2 && ct [2] != null ? ct [2] : Texture2D.blackTexture);
            brushApplyStrokeMat.SetTexture ("_Control3", ct.Length > 3 && ct [3] != null ? ct [3] : Texture2D.blackTexture);
            brushApplyStrokeMat.SetTexture ("_Control4", ct.Length > 4 && ct [4] != null ? ct [4] : Texture2D.blackTexture);
            brushApplyStrokeMat.SetTexture ("_Control5", ct.Length > 5 && ct [5] != null ? ct [5] : Texture2D.blackTexture);
            brushApplyStrokeMat.SetTexture ("_Control6", ct.Length > 6 && ct [6] != null ? ct [6] : Texture2D.blackTexture);
            brushApplyStrokeMat.SetTexture ("_Control7", ct.Length > 7 && ct [7] != null ? ct [7] : Texture2D.blackTexture);
            brushApplyStrokeMat.SetInt ("_ControlIndex", textureIndex / 4);


            Graphics.Blit (sub.subMeshOverride.controlTextures [textureIndex / 4], rt2, brushApplyStrokeMat);
            RenderTexture.active = rt2;


            splatTex.ReadPixels (new Rect (0, 0, rt2.width, rt2.height), 0, 0);
            splatTex.Apply ();
            NormalizeMesh (mj);


            RenderTexture.active = null;
            RenderTexture.ReleaseTemporary (rt);
            RenderTexture.ReleaseTemporary (rt2);
         }

         mj.msMesh.Sync ();
      }

   }
#endif
}
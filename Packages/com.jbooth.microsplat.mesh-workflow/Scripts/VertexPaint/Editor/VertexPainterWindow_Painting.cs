using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;

#if __MICROSPLAT__

namespace JBooth.MicroSplat.VertexPainter
{
   public partial class VertexPainterWindow : EditorWindow
   {
      // for external tools
      public System.Action<PaintJob[]> OnBeginStroke;
      public System.Action<PaintJob, bool> OnStokeModified;  // bool is true when doing a fill or other non-bounded opperation
      public System.Action OnEndStroke;


      // C# doesn't have *& or **, so it's not easy to pass a reference to a value for changing.
      // instead, we wrap the setter into a templated lambda which allows us to pass a changable
      // reference around via a function which sets it. Pretty tricky sis, but I'd rather just
      // be able to pass the freaking reference already..
      // Note the ref object, which is there just to prevent boxing of Vector/Color structs. Also
      // note the complete lack of type safety, etc.. ugh..

      // whats worse- this could also be condensed down to a macro, which would actually be MORE
      // safe in terms of potential bugs than all this; and it would be like a dozen lines to boot.

      
      public bool            enabled;
      public Vector3         oldpos = Vector3.zero;
      public float           brushSize = 1;
      public float           brushFlow = 8;
      public float           brushFalloff = 1; // linear
      public int             channel = 0;
      public float           targetValue = 1.0f;

      public bool            showVertexPoints = false;
      public float           showVertexSize = 1;

      public enum BrushVisualization
      {
         Sphere,
         Disk
      }
      public BrushVisualization brushVisualization = BrushVisualization.Sphere;
      public PaintJob[]      jobs = new PaintJob[0];
      // bool used to know if we've registered an undo with this object or not
      public bool[] jobEdits = new bool[0];


      void InitMeshes()
      {
         List<PaintJob> pjs = new List<PaintJob>();
         Object[] objs = Selection.GetFiltered(typeof(GameObject), SelectionMode.Editable | SelectionMode.Deep);
         for (int i = 0; i < objs.Length; ++i)
         {
            GameObject go = objs[i] as GameObject;
            if (go != null)
            {
               MeshFilter mf = go.GetComponent<MeshFilter>();
               Renderer r = go.GetComponent<Renderer>();
               MicroSplatVertexMesh vmesh = go.GetComponent<MicroSplatVertexMesh> ();
               if (mf != null && r != null && mf.sharedMesh != null && mf.sharedMesh.isReadable && vmesh != null)
               {
                  pjs.Add(new PaintJob(mf, r, vmesh));
               }
            }
         }

         jobs = pjs.ToArray();
         jobEdits = new bool[jobs.Length];
      }

      void SetWireframeDisplay(Renderer r, bool hidden)
      {
         EditorUtility.SetSelectedRenderState(r, hidden ? 
         EditorSelectedRenderState.Hidden : EditorSelectedRenderState.Highlight);
      }

      

      void OnUndo()
      {
         for (int i = 0; i < jobs.Length; ++i)
         {
            if (jobs[i].stream != null)
            {
               jobs[i].stream.Apply(false);
            }
         }
      }

      public bool IsFXEnabled(PaintJob j)
      {
         return (j.vertexMesh.keywordSO.IsKeywordEnabled ("_STREAMS") ||
            j.vertexMesh.keywordSO.IsKeywordEnabled ("_LAVA") ||
            j.vertexMesh.keywordSO.IsKeywordEnabled ("_WETNESS") ||
            j.vertexMesh.keywordSO.IsKeywordEnabled ("_PUDDLES"));
      }

      int GetChannel()
      {
         int c = channel;
#if __MICROSPLAT_STREAMS__
         if (tab == Tab.Wetness)
         {
            c = 28;
         }
         else if (tab == Tab.Puddles)
         {
            c = 29;
         }
         else if (tab == Tab.Streams)
         {
            c = 30;
         }
         else if (tab == Tab.Lava)
         {
            c = 31;
         }
#endif
         return c;
      }

      public void FillMesh(PaintJob job)
      {
         PrepBrushMode(job);
         int c = GetChannel ();

         if (c >= 28)
         {
            for (int i = 0; i < job.verts.Length; ++i)
            {
               Color clr = job.stream.colors [i];
               var uv1 = job.stream.uv1 [i];
               var uv2 = job.stream.uv2 [i];
               ToTemp (clr, uv1, uv2);
               tempWeights [c] = targetValue;
               FromTemp (ref clr, ref uv1, ref uv2);
               job.stream.colors [i] = clr;
               job.stream.uv1 [i] = uv1;
               job.stream.uv2 [i] = uv2;
            }
         }
         else
         {
            for (int i = 0; i < job.verts.Length; ++i)
            {
               Color clr = job.stream.colors [i];
               var uv1 = job.stream.uv1 [i];
               var uv2 = job.stream.uv2 [i];
               ToTemp (clr, uv1, uv2);

               for (int x = 0; x < 28; ++x)
               {
                  tempWeights [x] = 0;
               }
               tempWeights [c] = 1;
               FromTemp (ref clr, ref uv1, ref uv2);
               job.stream.colors [i] = clr;
               job.stream.uv1 [i] = uv1;
               job.stream.uv2 [i] = uv2;

            }
         }
         job.stream.Apply();
         if (OnStokeModified != null)
         {
            OnStokeModified(job, true);
         }
         
      }



      public void InitColors(PaintJob j)
      {
         Color[] colors = j.stream.colors;
         if (colors == null || colors.Length != j.verts.Length)
         {
            Color[] orig = j.meshFilter.sharedMesh.colors;
            if (j.meshFilter.sharedMesh.colors != null && j.meshFilter.sharedMesh.colors.Length > 0)
            {
               j.stream.colors = orig;
            }
            else
            {
               j.stream.SetColor(Color.white, j.verts.Length);
            }
         }
      }

      public void InitUV1 (PaintJob j)
      {
         var uvs = j.stream.uv1;
         if (uvs == null || uvs.Count != j.verts.Length)
         {
            if (j.meshFilter.sharedMesh.uv2 != null && j.meshFilter.sharedMesh.uv2.Length == j.verts.Length)
            {
               List<Vector4> nuv = new List<Vector4> (j.meshFilter.sharedMesh.vertices.Length);
               j.meshFilter.sharedMesh.GetUVs (1, nuv);
               j.stream.uv1 = nuv;
            }
            else
            {
               j.stream.SetUV1 (Vector2.zero, j.verts.Length);
            }
         }
      }

      public void InitUV2 (PaintJob j)
      {
         var uvs = j.stream.uv2;
         if (uvs == null || uvs.Count != j.verts.Length)
         {
            if (j.meshFilter.sharedMesh.uv3 != null && j.meshFilter.sharedMesh.uv3.Length == j.verts.Length)
            {
               List<Vector4> nuv = new List<Vector4> (j.meshFilter.sharedMesh.vertices.Length);
               j.meshFilter.sharedMesh.GetUVs (2, nuv);
               j.stream.uv2 = nuv;
            }
            else
            {
               j.stream.SetUV2 (Vector2.zero, j.verts.Length);
            }
         }
      }

      public void InitPositions(PaintJob j)
      {
         Vector3[] pos = j.stream.positions;
         if (pos == null || pos.Length != j.verts.Length)
         {
            int vc = j.meshFilter.sharedMesh.vertexCount;
            if (j.stream.positions == null || j.stream.positions.Length != vc)
            {
               j.stream.positions = new Vector3[j.meshFilter.sharedMesh.vertices.Length];
               j.meshFilter.sharedMesh.vertices.CopyTo(j.stream.positions, 0);
            }
         }
         return;
      }

      
      public void PrepBrushMode(PaintJob j)
      {
         InitColors (j);
         InitUV1 (j);
         InitUV2 (j);
      }


      void DrawVertexPoints(PaintJob j, Vector3 point)
      {
         if (j.HasStream() && j.HasData())
         {
            PrepBrushMode(j);
         }
         if (j.renderer == null)
         {
            return;
         }
         // convert point into local space, so we don't have to convert every point
         var mtx = j.renderer.transform.localToWorldMatrix;
         point = j.renderer.transform.worldToLocalMatrix.MultiplyPoint3x4(point);
         // for some reason this doesn't handle scale, seems like it should
         // we handle it poorly until I can find a better solution
         float scale = 1.0f / Mathf.Abs(j.renderer.transform.lossyScale.x);

         float bz = scale * brushSize;
         bz *= bz;

         for (int i = 0; i < j.verts.Length; ++i)
         {
            //float d = Vector3.Distance(point, j.verts[i]);
            var p = j.verts[i];
            float x = point.x - p.x;
            float y = point.y - p.y;
            float z = point.z - p.z;
            float dist = x * x + y * y + z * z;

            if (dist < bz)
            {
               Vector3 wp = mtx.MultiplyPoint(j.verts[i]);
               Handles.SphereHandleCap(0, point, Quaternion.identity, HandleUtility.GetHandleSize(wp) * 0.02f * showVertexSize, EventType.Repaint);

            }
         }
      }

      Color DecodeToColor (float v)
      {
         var vi = (uint)(v * (256.0f * 256.0f * 256.0f * 256.0f));
         var ex = (int)(vi / (256 * 256 * 256) % 256);
         var ey = (int)((vi / (256 * 256)) % 256);
         var ez = (int)((vi / (256)) % 256);
         var ew = (int)(vi % 256);
         var e = new Color (ex / 255.0f, ey / 255.0f, ez / 255.0f, ew / 255.0f);
         return e;
      }

      float EncodeToFloat (Color enc)
      {
         var ex = (uint)(enc.r * 255);
         var ey = (uint)(enc.g * 255);
         var ez = (uint)(enc.b * 255);
         var ew = (uint)(enc.a * 255);
         var v = (ex << 24) + (ey << 16) + (ez << 8) + ew;
         return v / (256.0f * 256.0f * 256.0f * 256.0f);
      }

      float [] tempWeights = new float [32];
      void ToTemp(Color c, Vector4 uv1, Vector4 uv2)
      {
         Color s0 = DecodeToColor (c.r);
         Color s1 = DecodeToColor (c.g);
         Color s2 = DecodeToColor (c.b);
         Color s3 = DecodeToColor (c.a);
         Color s4 = DecodeToColor (uv1.z);
         Color s5 = DecodeToColor (uv1.w);
         Color s6 = DecodeToColor (uv2.z);
         Color s7 = DecodeToColor (uv2.w);

         tempWeights [0] = s0.r;
         tempWeights [1] = s0.g;
         tempWeights [2] = s0.b;
         tempWeights [3] = s0.a;
         tempWeights [4] = s1.r;
         tempWeights [5] = s1.g;
         tempWeights [6] = s1.b;
         tempWeights [7] = s1.a;
         tempWeights [8] = s2.r;
         tempWeights [9] = s2.g;
         tempWeights [10] = s2.b;
         tempWeights [11] = s2.a;
         tempWeights [12] = s3.r;
         tempWeights [13] = s3.g;
         tempWeights [14] = s3.b;
         tempWeights [15] = s3.a;
         tempWeights [16] = s4.r;
         tempWeights [17] = s4.g;
         tempWeights [18] = s4.b;
         tempWeights [19] = s4.a;
         tempWeights [20] = s5.r;
         tempWeights [21] = s5.g;
         tempWeights [22] = s5.b;
         tempWeights [23] = s5.a;
         tempWeights [24] = s6.r;
         tempWeights [25] = s6.g;
         tempWeights [26] = s6.b;
         tempWeights [27] = s6.a;
         tempWeights [28] = s7.r;
         tempWeights [29] = s7.g;
         tempWeights [30] = s7.b;
         tempWeights [31] = s7.a;
      }

      void FromTemp(ref Color c, ref Vector4 uv1, ref Vector4 uv2)
      {
         c.r = EncodeToFloat (new Color (tempWeights [0], tempWeights [1], tempWeights [2], tempWeights [3]));
         c.g = EncodeToFloat (new Color (tempWeights [4], tempWeights [5], tempWeights [6], tempWeights [7]));
         c.b = EncodeToFloat (new Color (tempWeights [8], tempWeights [9], tempWeights [10], tempWeights [11]));
         c.a = EncodeToFloat (new Color (tempWeights [12], tempWeights [13], tempWeights [14], tempWeights [15]));
         uv1.z = EncodeToFloat (new Color (tempWeights [16], tempWeights [17], tempWeights [18], tempWeights [19]));
         uv1.w = EncodeToFloat (new Color (tempWeights [20], tempWeights [21], tempWeights [22], tempWeights [23]));
         uv2.z = EncodeToFloat (new Color (tempWeights [24], tempWeights [25], tempWeights [26], tempWeights [27]));
         uv2.w = EncodeToFloat (new Color (tempWeights [28], tempWeights [29], tempWeights [30], tempWeights [31]));
      }

      void NormalizeTempWeights(bool fx)
      {
         float total = 0;
         for (int i = 0; i < 28; ++i)
         {
            total += tempWeights [i];
         }
			for (int i = 0; i < 28; ++i)
         {
            tempWeights [i] /= total;
         }
      }

      void PaintMesh(PaintJob j, Vector3 point, float value)
      {
         bool affected = false;
         PrepBrushMode(j);
         // convert point into local space, so we don't have to convert every point
         point = j.renderer.transform.worldToLocalMatrix.MultiplyPoint3x4(point);
         // for some reason this doesn't handle scale, seems like it should
         // we handle it poorly until I can find a better solution
         float scale = 1.0f / Mathf.Abs(j.renderer.transform.lossyScale.x);

         float bz = scale * brushSize;
         bz *= bz;

         float pressure = Event.current.pressure > 0 ? Event.current.pressure : 1.0f;

         bool modPos = !(j.stream.positions == null || j.stream.positions.Length == 0);
         
         {
            bool fx = IsFXEnabled (j);
            int cnl = GetChannel ();
            for (int i = 0; i < j.verts.Length; ++i)
            {
               Vector3 p = modPos ? j.stream.positions[i] : j.verts[i];
               float x = point.x - p.x;
               float y = point.y - p.y;
               float z = point.z - p.z;
               float dist = x * x + y * y + z * z;

               if (dist < bz)
               {
                  float str = 1.0f - dist / bz;
                  str = Mathf.Pow(str, brushFalloff);
                  float finalStr = str * (float)deltaTime * brushFlow * pressure;
                  if (finalStr > 0)
                  {
                     affected = true;
                     Color c = j.stream.colors [i];
                     var uv1 = j.stream.uv1 [i];
                     var uv2 = j.stream.uv2 [i];
                     ToTemp (c, uv1, uv2);
                     var twv = tempWeights [cnl];
							var after = Mathf.Clamp01(Mathf.Lerp(twv, value, finalStr));
                     
                     tempWeights [cnl] = after;
                     NormalizeTempWeights (fx);
                     
                     FromTemp (ref c, ref uv1, ref uv2);
                     j.stream.colors [i] = c;
                     j.stream.uv1 [i] = uv1;
                     j.stream.uv2 [i] = uv2;
                  }
               }
            }
         }
         if (affected)
         {
            j.stream.Apply();
            if (OnStokeModified != null)
            {
               OnStokeModified(j, false);
            }
         }
      }

      void EndStroke()
      {
         if (OnEndStroke != null)
         {
            OnEndStroke();
         }
         painting = false;
        
         
         for (int i = 0; i < jobs.Length; ++i)
         {
            PaintJob j = jobs[i];
            if (j.HasStream())
            {
               EditorUtility.SetDirty(j.stream);
               EditorUtility.SetDirty(j.stream.gameObject);
            }
         }
      }

      
      
      double deltaTime = 0;
      double lastTime = 0;
      bool painting = false;

      void DoShortcuts()
      {
         if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape)
         {
            enabled = !enabled;
            if (enabled)
            {
               InitMeshes();
               Event.current.Use();
            }
         }
   
         // brush adjustments
         const float adjustSpeed = 0.3f;
         if (Event.current.isKey && Event.current.type == EventType.KeyDown)
         {
            if (Event.current.keyCode == KeyCode.LeftBracket)
            {
               brushSize -= adjustSpeed;
               Repaint();
            }
            else if (Event.current.keyCode == KeyCode.RightBracket)
            {
               brushSize += adjustSpeed;
               Repaint();
            }
            else if (Event.current.keyCode == KeyCode.Semicolon)
            {
               brushFlow -= adjustSpeed;
               Repaint();
            }
            else if (Event.current.keyCode == KeyCode.Quote)
            {
               brushFlow += adjustSpeed;
               Repaint();
            }
            else if (Event.current.keyCode == KeyCode.Period)
            {
               brushFalloff -= adjustSpeed;
               Repaint();
            }
            else if (Event.current.keyCode == KeyCode.Slash)
            {
               brushFlow += adjustSpeed;
               Repaint();
            }
         }
      }

      void OnSceneGUI(SceneView sceneView)
      {
         DoShortcuts();

         deltaTime = EditorApplication.timeSinceStartup - lastTime;
         lastTime = EditorApplication.timeSinceStartup;

         if (jobs.Length == 0 && Selection.activeGameObject != null)
         {
            InitMeshes();
         }

         if (!enabled || jobs.Length == 0 || Selection.activeGameObject == null)
         {
            return;
         }

         if (tab == Tab.Utility)
         {
            return;
         }
         RaycastHit hit;
         float distance = float.MaxValue;
         Vector3 mousePosition = Event.current.mousePosition;

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
         bool toggleWireframe = (Event.current.type == EventType.KeyUp && Event.current.control);

         for (int i = 0; i < jobs.Length; ++i)
         {
            if (jobs[i] == null || jobs[i].meshFilter == null)
               continue;

            // Early out if we're not in the area..
            Bounds b = jobs[i].renderer.bounds;
            b.Expand(brushSize*2);
            if (!b.IntersectRay(ray))
            {
               continue;
            }

            if (registerUndo)
            {
               painting = true;
               // clear job edits
               for (int x = 0; x < jobEdits.Length; ++x)
               {
                  jobEdits[x] = false;
               }
               if (OnBeginStroke != null)
               {
                  OnBeginStroke(jobs);
               }
            }
            if (toggleWireframe)
            {
               SetWireframeDisplay(jobs[i].renderer, hideMeshWireframe);
            }

            Matrix4x4 mtx = jobs[i].meshFilter.transform.localToWorldMatrix;
            Mesh msh = jobs[i].meshFilter.sharedMesh;

            if (jobs[i].HasStream())
            {
               msh = jobs[i].stream.GetModifierMesh(); 
            }
            if (msh == null)
            {
               msh = jobs[i].meshFilter.sharedMesh;
            }
            if (RXLookingGlass.IntersectRayMesh(ray, msh, mtx, out hit))
            {
               if (Event.current.shift == false) 
               {
                  if (hit.distance < distance) 
                  {
                     distance = hit.distance;
                     point = hit.point;
                     oldpos = hit.point;
                     normal = hit.normal;
                     // if we don't have normal overrides, we have to recast against the shared mesh to get it's normal
                     // This could get a little strange if you modify the mesh, then delete the normal data, but in that
                     // case there's no real correct answer anyway without knowing the index of the vertex we're hitting.
                     if (normal.magnitude < 0.1f)
                     {
                        RXLookingGlass.IntersectRayMesh(ray, jobs[i].meshFilter.sharedMesh, mtx, out hit);
                        normal = hit.normal;
                     }
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
            brushFalloff -= Event.current.delta.y * (float)deltaTime * 48.0f;
         }

         if (Event.current.rawType == EventType.MouseUp)
         {
            EndStroke();
         }
         if (Event.current.type == EventType.MouseMove && Event.current.alt)
         {
            brushSize += Event.current.delta.y * (float)deltaTime;
         }


         Handles.color = new Color(1, 0, 0, 0.4f);
         

         if (brushVisualization == BrushVisualization.Sphere)
         {
            Handles.SphereHandleCap(0, point, Quaternion.identity, brushSize * 2, EventType.Repaint);
         }
         else
         {
            Handles.color = new Color(0.8f, 0, 0, 1.0f);
            float r = Mathf.Pow(0.5f, brushFalloff);
            Handles.DrawWireDisc(point, normal, brushSize * r);
            Handles.color = new Color(0.9f, 0, 0, 0.8f);
            Handles.DrawWireDisc(point, normal, brushSize);
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

         


         if (jobs.Length > 0 && painting)
         {
            
            for (int i = 0; i < jobs.Length; ++i)
            {
               Bounds b = jobs[i].renderer.bounds;
               b.Expand(brushSize*2);
               if (!b.IntersectRay(ray))
               {
                  continue;
               }
               if (jobEdits[i] == false)
               {
                  jobEdits[i] = true;
                  Undo.RegisterCompleteObjectUndo(jobs[i].stream, "Vertex Painter Stroke");
               }

               PaintMesh(jobs[i], point, targetValue);
               Undo.RecordObject(jobs[i].stream, "Vertex Painter Stroke");

            }
         }

         if (jobs.Length > 0 && showVertexPoints)
         {
            for (int i = 0; i < jobs.Length; ++i)
            {
               DrawVertexPoints(jobs[i], point);
            }
         }

         // update views
         sceneView.Repaint();
         HandleUtility.Repaint();
      }
   }
}

#endif
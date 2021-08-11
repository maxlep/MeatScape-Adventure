//////////////////////////////////////////////////////
// MicroSplat
// Copyright (c) Jason Booth
//////////////////////////////////////////////////////


using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace JBooth.MicroSplat
{
   #if __MICROSPLAT__ && __MICROSPLAT_MESH__
   public partial class MeshPainterWindow : EditorWindow 
   {
      [MenuItem("Window/MicroSplat/Mesh Painter")]
      public static void ShowWindow()
      {
         var window = GetWindow<JBooth.MicroSplat.MeshPainterWindow>();
         if (window != null)
         {
            window.Init();
            window.Show();
         }
      }

      bool enabled = true;

      MeshJob[] meshes;
      bool[] jobEdits;



      MeshJob FindJob(MicroSplatMesh mesh)
      {
         if (meshes == null)
            return null;
         for (int i = 0; i < meshes.Length; ++i)
         {
            if (meshes[i] != null && meshes[i].msMesh == mesh)
               return meshes[i];
         }
         return null;
      }

      void CleanupJobs()
      {
         if (brushCam != null)
         {
            DestroyImmediate(brushCam);
         }
         if (brushProjector)
         {
            DestroyImmediate(brushProjector);
         }

         if (meshes == null)
            return;
         for (int i = 0; i < meshes.Length; ++i)
         {
            if (meshes[i] != null)
            {
               var j = meshes[i];
               DestroyImmediate(j);
               j.msMesh.Sync();
            }
         }
      }

      void Init()
      {
         CleanupJobs();
         Object[] objs = Selection.GetFiltered(typeof(MicroSplatMesh), SelectionMode.Editable | SelectionMode.Deep);
         List<MeshJob> ts = new List<MeshJob>();
         for (int i = 0; i < objs.Length; ++i)
         {
            MicroSplatMesh msm = objs[i] as MicroSplatMesh;
            if (msm == null)
               continue;
            if (msm.templateMaterial != null)
            {
               var job = FindJob(msm);
               if (job == null)
               {
                  var c = msm.GetComponent<Collider>();
                  if (c != null)
                  {
                     job = MeshJob.CreateInstance<MeshJob>();
                     job.msMesh = msm;
                     job.collider = c;
                     job.sharedMesh = msm.GetComponent<MeshFilter>().sharedMesh;
                  }

               }
               ts.Add(job);
               
            }
         }

         meshes = ts.ToArray();
         jobEdits = new bool[ts.Count];
      }

      void OnSelectionChange()
      {
         Init();
         this.Repaint();
      }

      void OnFocus() 
      {
   #if UNITY_2019_1_OR_NEWER
         SceneView.duringSceneGui -= this.OnSceneGUI;
         SceneView.duringSceneGui += this.OnSceneGUI;
   #else
         SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
         SceneView.onSceneGUIDelegate += this.OnSceneGUI;
   #endif

         Undo.undoRedoPerformed -= this.OnUndo;
         Undo.undoRedoPerformed += this.OnUndo;

         UnityEditor.SceneManagement.EditorSceneManager.sceneSaving -= OnSceneSaving;
         UnityEditor.SceneManagement.EditorSceneManager.sceneSaving += OnSceneSaving;

         this.titleContent = new GUIContent("MicroSplat Mesh Painter");
         Init();
         Repaint();
      }

      void OnSceneSaving (UnityEngine.SceneManagement.Scene scene, string path)
      {
         SaveControlTextures ();
         SaveFXTextures ();
      }



      void OnUndo()
      {
         if (meshes != null)
         {
            for (int i = 0; i < meshes.Length; ++i)
            {
               var m = meshes [i];
               if (m != null && m.msMesh != null)
               {
                  m.RestoreUndo ();
               }
            }
         }
         Repaint();
      }

      void OnInspectorUpdate()
      {
         // unfortunate...
         Repaint ();
      }

      void OnDestroy() 
      {
         
         if (EditorUtility.DisplayDialog("Save Changes?", "Save your changes?", "yes", "no"))
         {
            SaveControlTextures ();
            SaveFXTextures ();
         }
         CleanupJobs();

   #if UNITY_2019_1_OR_NEWER
         SceneView.duringSceneGui -= this.OnSceneGUI;
   #else
         SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
   #endif
      }


   }
#else //#if __MICROSPLAT__ && __MICROSPLAT_MESH__

   public class MeshPainterWindow : EditorWindow 
   {
      [MenuItem("Window/MicroSplat/Mesh Painter")]
      public static void ShowWindow()
      {
         var window = GetWindow<JBooth.MicroSplat.MeshPainterWindow>();
         if (window != null)
         {
            window.Show();
         }
      }


      void OnGUI()
      {
         EditorGUILayout.HelpBox ("MicroSplat is not installed, and must be installed to use this feature.", MessageType.Error);
         if (GUILayout.Button ("Get MicroSplat"))
         {
            Application.OpenURL("https://assetstore.unity.com/packages/tools/terrain/microsplat-96478?aid=1011l37NJ&utm_source=aff");
         }
      }
   }
#endif
}


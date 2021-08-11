using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

/*  VertexPainterWindow
 *    - Jason Booth
 * 
 *    Uses Unity 5.0+ MeshRenderer.additionalVertexStream so that you can paint per-instance vertex colors on your meshes.
 * A component is added to your mesh to serialize this data and set it at load time. This is more effecient than making
 * duplicate meshes, and certainly less painful than saving them as separate asset files on disk. However, if you only have
 * one copy of the vertex information in your game and want to burn it into the original mesh, you can use the save feature
 * to save a new version of your mesh with the data burned into the verticies, avoiding the need for the runtime component. 
 * 
 * In other words, bake it if you need to instance the paint job - however, if you want tons of the same instances painted
 * uniquely in your scene, keep the component version and skip the baking..
 * 
 * One possible optimization is to have the component free the array after updating the mesh when in play mode..
 * 
 * Also supports burning data into the UV channels, in case you want some additional channels to work with, which also
 * happen to be full 32bit floats. You can set a viewable range; so if your floats go from 0-120, it will remap it to
 * 0-1 for display in the shader. That way you can always see your values, even when they go out of color ranges.
 * 
 * Note that as of this writing Unity has a bug in the additionalVertexStream function. The docs claim the data applied here
 * will supply or overwrite the data in the mesh, however, this is not true. Rather, it will only replace the data that's 
 * there - if your mesh has no color information, it will not upload the color data in additionalVertexStream, which is sad
 * because the original mesh doesn't need this data. As a workaround, if your mesh does not have color channels on the verts,
 * they will be created for you. 
 * 
 * There is another bug in additionalVertexStream, in that the mesh keeps disapearing in edit mode. So the component
 * which holds the data caches the mesh and keeps assigning it in the Update call, but only when running in the editor
 * and not in play mode. 
 * 
 * Really, the additionalVertexStream mesh should be owned by the MeshRenderer and saved as part of the objects instance
 * data. That's essentially what the VertexInstaceStream component does, but it's annoying and wasteful of memory to do
 * it this way since it doesn't need to be on the CPU at all. Enlighten somehow does this with the UVs it generates
 * this way, but appears to be handled specially. Oh, Unity..
*/

#if __MICROSPLAT__

namespace JBooth.MicroSplat.VertexPainter
{
   public partial class VertexPainterWindow : EditorWindow 
   {
      enum Tab
      {
         Paint = 0,
#if __MICROSPLAT_STREAMS__
         Wetness,
         Puddles,
         Streams,
         Lava,
#endif
         Utility,
      }

      string[] tabNames =
      {
         "Paint",
#if __MICROSPLAT_STREAMS__
         "Wetness",
         "Puddles",
         "Streams",
         "Lava",
#endif
         "Utility",
      };


      Tab tab = Tab.Paint;

      bool hideMeshWireframe = false;
      
      bool DrawClearButton(string label)
      {
         if (GUILayout.Button(label, GUILayout.Width(46)))
         {
            return (EditorUtility.DisplayDialog("Confirm", "Clear " + label + " data?", "ok", "cancel"));
         }
         return false;
      }

      static Dictionary<string, bool> rolloutStates = new Dictionary<string, bool>();
      static GUIStyle rolloutStyle;
      public static bool DrawRollup(string text, bool defaultState = true, bool inset = false)
      {
         if (rolloutStyle == null)
         {
            rolloutStyle = GUI.skin.box;
            rolloutStyle.normal.textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;
         }
         GUI.contentColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;
         if (inset == true)
         {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.GetControlRect(GUILayout.Width(40));
         }

         if (!rolloutStates.ContainsKey(text))
         {
            rolloutStates[text] = defaultState;
         }
         if (GUILayout.Button(text, rolloutStyle, new GUILayoutOption[]{GUILayout.ExpandWidth(true), GUILayout.Height(20)}))
         {
            rolloutStates[text] = !rolloutStates[text];
         }
         if (inset == true)
         {
            EditorGUILayout.GetControlRect(GUILayout.Width(40));
            EditorGUILayout.EndHorizontal();
         }
         return rolloutStates[text];
      }

      Vector2 scroll;
      void OnGUI()
      {
         
         if (Selection.activeGameObject == null)
         {
            EditorGUILayout.LabelField("No objects selected. Please select an object with a MicroSplatVertexMesh component on it");
            return;
         }

         DrawChannelGUI();

         tab = (Tab)GUILayout.Toolbar((int)tab, tabNames);
         if (tab != Tab.Utility)
         {
            scroll = EditorGUILayout.BeginScrollView(scroll);
            DrawPaintGUI();
         }
         else
         {
            scroll = EditorGUILayout.BeginScrollView(scroll);
            DrawUtilityGUI();
         }

         EditorGUILayout.EndScrollView();
      }


      void DrawChannelGUI()
      {
         EditorGUILayout.Separator();
         GUI.skin.box.normal.textColor = Color.white;
         if (DrawRollup ("Vertex Painter"))
         {
            bool oldEnabled = enabled;
            enabled = GUILayout.Toggle (enabled, "Active (ESC)");
            if (enabled != oldEnabled)
            {
               InitMeshes ();
            }
            EditorGUILayout.BeginHorizontal ();
            bool emptyStreams = false;
            for (int i = 0; i < jobs.Length; ++i)
            {
               if (!jobs [i].HasStream ())
                  emptyStreams = true;
            }
            EditorGUILayout.EndHorizontal ();
            if (emptyStreams)
            {
               if (GUILayout.Button ("Add Vertex Stream"))
               {
                  for (int i = 0; i < jobs.Length; ++i)
                  {
                     jobs [i].EnforceStream ();
                  }
               }
            }


            brushVisualization = (BrushVisualization)EditorGUILayout.EnumPopup ("Brush Visualization", brushVisualization);
            EditorGUILayout.BeginHorizontal ();
            showVertexPoints = GUILayout.Toggle (showVertexPoints, "Show Brush Influence");
            showVertexSize = EditorGUILayout.Slider (showVertexSize, 0.2f, 10);
            EditorGUILayout.EndHorizontal ();
            bool oldHideMeshWireframe = hideMeshWireframe;
            hideMeshWireframe = !GUILayout.Toggle (!hideMeshWireframe, "Show Wireframe (ctrl-W)");

            if (hideMeshWireframe != oldHideMeshWireframe)
            {
               for (int i = 0; i < jobs.Length; ++i)
               {
                  SetWireframeDisplay (jobs [i].renderer, hideMeshWireframe);
               }
            }
         }

            
         EditorGUILayout.Separator();
         GUILayout.Box("", new GUILayoutOption[]{GUILayout.ExpandWidth(true), GUILayout.Height(1)});
         EditorGUILayout.Separator();
 
      }

      void DrawBrushSettingsGUI()
      {
         if (jobs == null || jobs.Length == 0 || jobs [0] == null || jobs [0].vertexMesh == null )
            return;

         int maxTex = 27;
         Texture2DArray ta = null;
         if (jobs [0].vertexMesh.templateMaterial != null)
         {
            ta = jobs [0].vertexMesh.templateMaterial.GetTexture ("_Diffuse") as Texture2DArray;
            if (ta != null && ta.depth > 0)
            {
               maxTex = ta.depth - 1;
            }
         }
         if (tab == Tab.Paint)
         {
            if (ta != null)
            {
               channel = MicroSplatUtilities.DrawTextureSelector (channel, ta);
            }
            else
            {
               channel = EditorGUILayout.IntSlider ("Texture Index", channel, 0, maxTex);
            }
         }
         targetValue = EditorGUILayout.Slider ("Target Value", targetValue, 0.0f, 1.0f);
         brushSize      = EditorGUILayout.Slider("Brush Size", brushSize, 0.01f, 90.0f);
         brushFlow      = EditorGUILayout.Slider("Brush Flow", brushFlow, 0.1f, 128.0f);
         brushFalloff   = EditorGUILayout.Slider("Brush Falloff", brushFalloff, 0.1f, 3.5f);

         EditorGUILayout.Separator();
         GUILayout.Box("", new GUILayoutOption[]{GUILayout.ExpandWidth(true), GUILayout.Height(1)});
         EditorGUILayout.Separator();

      }


      

      void DrawPaintGUI()
      {

         GUILayout.Box("Brush Settings", new GUILayoutOption[]{GUILayout.ExpandWidth(true), GUILayout.Height(20)});

         DrawBrushSettingsGUI();
 
         EditorGUILayout.BeginHorizontal();
         if (GUILayout.Button("Fill"))
         {
            if (OnBeginStroke != null)
            {
               OnBeginStroke(jobs);
            }
            for (int i = 0; i < jobs.Length; ++i)
            {
               Undo.RecordObject(jobs[i].stream, "Vertex Painter Fill");
               FillMesh(jobs[i]);
            }
            if (OnEndStroke != null)
            {
               OnEndStroke();
            }
            Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
         }
         EditorGUILayout.EndHorizontal();

      }

      List<IVertexPainterUtility> utilities = new List<IVertexPainterUtility>();
      void InitPluginUtilities()
      {
         if (utilities == null || utilities.Count == 0)
         {
            var interfaceType = typeof(IVertexPainterUtility);
            var all = System.AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(x => x.GetTypes())
            .Where(x => interfaceType.IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
            .Select(x => System.Activator.CreateInstance(x));


            foreach (var o in all)
            {
               IVertexPainterUtility u = o as IVertexPainterUtility;
               if (u != null)
               {
                  utilities.Add(u);
               }
            }
            utilities = utilities.OrderBy(o=>o.GetName()).ToList();
         }
      }

      void DrawUtilityGUI()
      {
         InitPluginUtilities();
         for (int i = 0; i < utilities.Count; ++i)
         {
            var u = utilities[i];
            if (DrawRollup(u.GetName(), false))
            {
               u.OnGUI(jobs);
            }
         }
      }


      void OnFocus() 
      {
         if (painting)
         {
            EndStroke();
         }

#if UNITY_2019_1_OR_NEWER
            SceneView.duringSceneGui -= this.OnSceneGUI;
            SceneView.duringSceneGui += this.OnSceneGUI;
#else
         SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
         SceneView.onSceneGUIDelegate += this.OnSceneGUI;
#endif

         Undo.undoRedoPerformed -= this.OnUndo;
         Undo.undoRedoPerformed += this.OnUndo;
         this.titleContent = new GUIContent("Vertex Paint");
         Repaint();

      }
      
      void OnInspectorUpdate()
      {
         // unfortunate...
         Repaint ();
      }
      
      void OnSelectionChange()
      {
         InitMeshes();
         this.Repaint();
      }
      
      void OnDestroy() 
      {
#if UNITY_2019_1_OR_NEWER
            SceneView.duringSceneGui -= this.OnSceneGUI;
#else
         SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
#endif
        }
    }
}

#endif
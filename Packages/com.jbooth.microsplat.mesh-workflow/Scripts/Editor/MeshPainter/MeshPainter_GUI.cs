//////////////////////////////////////////////////////
// MicroSplat
// Copyright (c) Jason Booth
//////////////////////////////////////////////////////

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;


namespace JBooth.MicroSplat
{
#if __MICROSPLAT__ && __MICROSPLAT_MESH__
   public partial class MeshPainterWindow : EditorWindow
   {

      public bool showDebug;


      public Texture2D[] brushes;
      public Texture2D curBrush;

      Texture2D SaveTexture (string path, Texture2D tex, bool overwrite = false, TextureWrapMode wrapMode = TextureWrapMode.Repeat)
      {
         if (overwrite || !System.IO.File.Exists(path))
         {
            if (path.EndsWith(".png"))
            {
               path = path.Replace (".png", ".tga");
            }
            var bytes = tex.EncodeToTGA();


            System.IO.File.WriteAllBytes(path, bytes);
            AssetDatabase.Refresh();
            AssetImporter ai = AssetImporter.GetAtPath(path);
            TextureImporter ti = ai as TextureImporter;
            ti.sRGBTexture = false;
            ti.textureCompression = TextureImporterCompression.Uncompressed;
            var ftm = ti.GetDefaultPlatformTextureSettings();
            ftm.format = TextureImporterFormat.RGBA32;
            ti.SetPlatformTextureSettings(ftm);

            ti.mipmapEnabled = true;
            ti.isReadable = true;
            ti.filterMode = FilterMode.Bilinear;
            ti.wrapMode = wrapMode;
            ti.SaveAndReimport();
         }
         return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
      }

      int brushIndex = 0;
      int brushDisplaySize = 64;
      void DrawBrushGUI()
      {
         if (brushes == null || brushes.Length == 0)
         {
            brushes = Resources.LoadAll<Texture2D>("MicroSplatBrushes");
         }
         if (brushes == null)
         {
            EditorGUILayout.HelpBox ("Brushes cannot be loaded!", MessageType.Error);
            return;
         }
         EditorGUILayout.BeginHorizontal();
         EditorGUILayout.LabelField("brushes");
         brushDisplaySize = EditorGUILayout.IntSlider(brushDisplaySize, 32, 128);
         EditorGUILayout.EndHorizontal();

         brushIndex = MicroSplatUtilities.SelectionGrid(brushIndex, brushes, brushDisplaySize);
         curBrush = brushes[brushIndex];
      }


      void DrawFillGUI()
      {
         EditorGUILayout.BeginHorizontal();
         if (GUILayout.Button("Fill"))
         {
            if (OnBeginStroke != null)
            {
               OnBeginStroke(meshes);
            }
            for (int i = 0; i < meshes.Length; ++i)
            {
               var m = meshes [i];
               if (m != null)
               {
                  FillMesh (m);
                  if (OnStokeModified != null)
                  {
                     OnStokeModified (meshes [i], true);
                  }
               }
            }
            if (OnEndStroke != null)
            {
               OnEndStroke();
            }
         }

         if (GUILayout.Button ("Fill with Filters"))
         {
            if (OnBeginStroke != null)
            {
               OnBeginStroke (meshes);
            }
            for (int i = 0; i < meshes.Length; ++i)
            {
               // not sure why this doesn't saturate, but it's very fast, so fuck it for now..
               for (int j = 0; j < 10; ++j)
               {
                  PaintMeshGPU (meshes [i], Vector3.one, Vector2.one, false, true);
               }
               if (OnStokeModified != null)
               {
                  OnStokeModified(meshes[i], true);
               }
            }
            if (OnEndStroke != null)
            {
               OnEndStroke();
            }

         }

         EditorGUILayout.EndHorizontal();
      }

      bool VerifyTextureBrushes ()
      {
         if (meshes == null || meshes.Length == 0)
         {
            EditorGUILayout.HelpBox ("Please select a MicroSplatMesh to begin", MessageType.Info);
            return false;
         }
         for (int i = 0; i < meshes.Length; ++i)
         {
            var m = meshes [i];
            if (m.msMesh.keywordSO.IsKeywordEnabled("_PROCEDURALTEXTURE"))
            {
               EditorGUILayout.HelpBox ("Procedural Texture Mode does not allow painting of textures", MessageType.Info);
               return false;
            }
         }
         return true;
      }

      bool VerifyData()
      {
         if (meshes == null || meshes.Length == 0)
         {
            EditorGUILayout.HelpBox("Please select a MicroSplatMesh to begin", MessageType.Info);
            return false;
         }
         for (int i = 0; i < meshes.Length; ++i)
         {
            var m = meshes[i];
            if (m == null)
            {
               EditorGUILayout.HelpBox("Please make sure your mesh has a mesh collider on it for painting", MessageType.Info);
               return false;
            }
            if (m.msMesh == null)
            {
               // tbis should never happen, but just in case
               EditorGUILayout.HelpBox("Game Object does not have MicroSplatMesh Component", MessageType.Info);
               return false;
            }
            if (m.collider == null)
            {
               EditorGUILayout.HelpBox("Mesh " + m.msMesh.gameObject.name + " does not have a collider, a collider is needed to paint", MessageType.Info);
               return false;
            }
            if (m.msMesh.templateMaterial == null || m.msMesh.keywordSO == null)
            {
               EditorGUILayout.HelpBox("Mesh " + m.msMesh.gameObject.name + " does not have a template material or keyword list assigned", MessageType.Info);
               return false;
            }
            if (!m.msMesh.keywordSO.IsKeywordEnabled("_MICROSPLAT"))
            {
               EditorGUILayout.HelpBox("Mesh " + m.msMesh.gameObject.name + " is not using a MicroSplat shader for it's material", MessageType.Info);
               return false;
            }
            if (!m.msMesh.keywordSO.IsKeywordEnabled("_MICROMESH"))
            {
               EditorGUILayout.HelpBox("Mesh " + m.msMesh.gameObject.name + " is not using a material with MicroSplatMesh set on it", MessageType.Info);
               return false;
            }
            if (!m.msMesh.keywordSO.IsKeywordEnabled ("_PROCEDURALTEXTURE"))
            {
               for (int sub = 0; sub < m.msMesh.subMeshEntries.Count; ++sub)
               {
                  var subMesh = m.msMesh.subMeshEntries [sub];
                  if (subMesh.subMeshOverride.active)
                  {
                     if (subMesh.subMeshOverride.controlTextures == null || subMesh.subMeshOverride.controlTextures.Length == 0 || subMesh.subMeshOverride.controlTextures [0] == null)
                     {
                        EditorGUILayout.HelpBox ("Mesh " + m.msMesh.gameObject.name + " does not have control textures assigned", MessageType.Info);
                        return false;
                     }
                  }
               }
            }
            if (m.msMesh.keywordSO.IsKeywordEnabled ("_DISABLESPLATMAPS"))
            {
               EditorGUILayout.HelpBox("Mesh " + m.msMesh.gameObject.name + " is set to disable splat mapping", MessageType.Info);
               return false;
            }

         }
         return true;
      }


      string [] tabNames = null;
      int [] tabValues = null;
      int tabIndex = 0;


      void OnGUI()
      {
         if (VerifyData() == false)
         {
            return;
         }

         DrawSettingsGUI();
         if (tabNames == null)
         {
            tabNames = System.Enum.GetNames (typeof (Tab));
            var ar = System.Enum.GetValues (typeof (Tab));
            tabValues = new int [ar.Length];
            for (int i = 0; i < ar.Length; ++i)
            {
               tabValues [i] = System.Convert.ToInt32(ar.GetValue (i));
            }
         }


         EditorGUI.BeginChangeCheck ();
         tabIndex = GUILayout.Toolbar(tabIndex, tabNames);
         if (EditorGUI.EndChangeCheck())
         {
            tab = (Tab)(tabValues[tabIndex]);
         }
         if (tab != Tab.Texture)
         {
            OnFXGUI();
            return;
         }

         if (tab == Tab.Texture && VerifyTextureBrushes() == false)
         {
            return;
         }

         if (MicroSplatUtilities.DrawRollup("Brush Settings"))
         {
            DrawBrushSettingsGUI();
         }
         DrawFillGUI();
         DrawSaveGUI();

      }

      void SaveControlTextures()
      {
         for (int i = 0; i < meshes.Length; ++i)
         {
            for (int sub = 0; sub < meshes [i].msMesh.subMeshEntries.Count; ++sub)
            {
               var subMesh = meshes [i].msMesh.subMeshEntries [sub];
               var textures = subMesh.subMeshOverride.controlTextures;
               for (int j = 0; j < textures.Length; ++j)
               {
                  string path = AssetDatabase.GetAssetPath (textures [j]);
                  var bytes = textures [j].EncodeToPNG ();
                  System.IO.File.WriteAllBytes (path, bytes);
               }
            }
         }
         AssetDatabase.Refresh();
      }

      void DrawSaveGUI()
      {
         EditorGUILayout.Space();
         EditorGUILayout.BeginHorizontal();
         if (GUILayout.Button("Save"))
         {
            SaveControlTextures ();
         }

         EditorGUILayout.EndHorizontal();
         EditorGUILayout.Space();
      }

      void DrawSettingsGUI()
      {
         EditorGUILayout.Separator();
         GUI.skin.box.normal.textColor = Color.white;
         if (MicroSplatUtilities.DrawRollup("MicroSplat Mesh Painter"))
         {
            bool oldEnabled = enabled;
            if (Event.current.isKey && Event.current.keyCode == KeyCode.Escape && Event.current.type == EventType.KeyUp)
            {
               enabled = !enabled;
            }
            enabled = GUILayout.Toggle(enabled, "Active (ESC)");
            if (enabled != oldEnabled)
            {
               Init();
            }

            EditorGUILayout.Separator();
            GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(1) });
            EditorGUILayout.Separator();
         }
      }


      int textureSize = 64;
      void DrawBrushSettingsGUI(bool showTextures = true)
      {
         if (meshes.Length > 0 && meshes[0] != null && meshes[0].msMesh != null && meshes[0].msMesh.templateMaterial != null)
         {
            DrawBrushGUI();
            if (showTextures)
            {
               var m = meshes[0];
               var ta = m.msMesh.templateMaterial.GetTexture("_Diffuse") as Texture2DArray;

               EditorGUILayout.BeginHorizontal();
               EditorGUILayout.LabelField("textures");
               textureSize = EditorGUILayout.IntSlider(textureSize, 32, 128);
               EditorGUILayout.EndHorizontal();
               textureIndex = MicroSplatUtilities.SelectionGrid(textureIndex, ta, textureSize);
            }
         }
            
         brushSize      = EditorGUILayout.Slider("Brush Size", brushSize, 0.01f, 90.0f);
         brushFlow      = EditorGUILayout.Slider("Brush Flow", brushFlow, 0.1f, 10.0f);
         brushRotation  = EditorGUILayout.Slider("Rotation", brushRotation, 0, 360);
         brushTargetValue = EditorGUILayout.Slider("Target Value", brushTargetValue, 0, 1);
         //projectionFilter = EditorGUILayout.Toggle ("Backface Filter", projectionFilter);
         EditorGUILayout.MinMaxSlider("AngleFilter", ref angleFilter.x, ref angleFilter.y, -1, 1);

         brushDisplayColor     = EditorGUILayout.ColorField("Brush Display Color", brushDisplayColor);

         EditorGUILayout.Separator();
         GUILayout.Box("", new GUILayoutOption[]{GUILayout.ExpandWidth(true), GUILayout.Height(1)});
         EditorGUILayout.Separator();

         EditorGUILayout.BeginHorizontal ();
         showDebug = EditorGUILayout.Toggle ("Show Debug", showDebug);
         debugBrushSubmeshIndex = EditorGUILayout.IntField ("SubMeshIdx", debugBrushSubmeshIndex);
         EditorGUILayout.EndHorizontal ();

         if (showDebug && debugBrush != null)
         {
            GUI.DrawTexture (EditorGUILayout.GetControlRect (GUILayout.Height (256)), debugBrush, ScaleMode.ScaleToFit);
         }


      }

      Color[] cachedColors = new Color[8];

      void SampleColors(MeshJob t, int x, int y, int sub)
      {
         for (int i = 0; i < 8; ++i)
         {
            if (t.msMesh.subMeshEntries[sub].subMeshOverride.controlTextures.Length > i && t.msMesh.subMeshEntries[sub].subMeshOverride.controlTextures [i] != null)
            {
               cachedColors[i] = t.msMesh.subMeshEntries[sub].subMeshOverride.controlTextures [i].GetPixel(x, y);
            }
            else
            {
               cachedColors[i] = new Color(0, 0, 0, 0);
            }
         }
      }

      void Normalize()
      {
         float newVal = GetCacheValue(textureIndex);
         float total = 0.0f;
         for (int i = 0; i < 32; ++i)
         {
            if (i != textureIndex)
            {
               int c = i / 4;
               total += cachedColors[c][i-c*4];

            }
         }
         if (total > 1.0f / 255.0f)
         {
            float mod = (1.0f - newVal) / total;
            for (int i = 0; i < 32; ++i)
            {
               if (i != textureIndex)
               {
                  int c = i / 4;
                  int off = i - c * 4;
                  cachedColors[c][off] *= mod;
               }
            }
         }
         else
         {
            for (int i = 0; i < 32; ++i)
            {
               int c = i / 4;
               int off = i - c * 4;
               cachedColors[c][off] = (i == textureIndex) ? 1.0f : 0.0f;
            }
         }
      }

      void WriteColors(MeshJob t, int x, int y, int sub)
      {
         Normalize();
         // set
         for (int i = 0; i < t.msMesh.subMeshEntries[sub].subMeshOverride.controlTextures.Length; ++i)
         {
            if (t.msMesh.subMeshEntries [sub].subMeshOverride.controlTextures[i] != null)
            {
               t.msMesh.subMeshEntries [sub].subMeshOverride.controlTextures [i].SetPixel(x, y, cachedColors[i]);
            }
         }
      }

      void SetCacheValue(int texIdx, float val)
      {
         int cidx = texIdx / 4;
         cachedColors[cidx][texIdx - cidx*4] = val;
      }

      float GetCacheValue(int texIdx)
      {
         int cidx = texIdx / 4;
         Color c = cachedColors[cidx];
         return c[texIdx - cidx*4];
      }

      List<Color[]> cachedTextureData;


      static Material normalizeMat;
      static Texture2D pureBlack;
      public static void NormalizeMesh(MeshJob t)
      {
         if (normalizeMat == null)
         {
            normalizeMat = new Material (Shader.Find ("Hidden/MicroSplatMeshNormalize"));
         }
         if (pureBlack == null)
         {
            pureBlack = new Texture2D (1, 1);
            pureBlack.SetPixel (0, 0, new Color (0, 0, 0, 0));
            pureBlack.Apply ();
         }
         for (int sub = 0; sub < t.msMesh.subMeshEntries.Count; ++sub)
         {
            if (t.msMesh.subMeshEntries [sub].subMeshOverride.active)
            {
               var ct = t.msMesh.subMeshEntries [sub].subMeshOverride.controlTextures;
               normalizeMat.SetTexture ("_Control0", ct [0]);
               normalizeMat.SetTexture ("_Control1", ct.Length > 1 && ct [1] != null ? ct [1] : pureBlack);
               normalizeMat.SetTexture ("_Control2", ct.Length > 2 && ct [2] != null ? ct [2] : pureBlack);
               normalizeMat.SetTexture ("_Control3", ct.Length > 3 && ct [3] != null ? ct [3] : pureBlack);
               normalizeMat.SetTexture ("_Control4", ct.Length > 4 && ct [4] != null ? ct [4] : pureBlack);
               normalizeMat.SetTexture ("_Control5", ct.Length > 5 && ct [5] != null ? ct [5] : pureBlack);
               normalizeMat.SetTexture ("_Control6", ct.Length > 6 && ct [6] != null ? ct [6] : pureBlack);
               normalizeMat.SetTexture ("_Control7", ct.Length > 7 && ct [7] != null ? ct [7] : pureBlack);

               for (int i = 0; i < ct.Length; ++i)
               {
                  normalizeMat.SetInt ("_ControlIndex", i);
                  var tex = ct [i];
                  RenderTexture rt = RenderTexture.GetTemporary (tex.width, tex.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
                  Graphics.Blit (Texture2D.blackTexture, rt, normalizeMat);
                  RenderTexture.active = rt;
                  tex.ReadPixels (new Rect (0, 0, tex.width, tex.height), 0, 0);
                  tex.Apply ();
                  RenderTexture.active = null;
                  RenderTexture.ReleaseTemporary (rt);
               }
            }
         }

      }

      void FillMesh(MeshJob t)
      {
         //Init(); // causes mesh job to go null.. 
         t.RegisterUndo(MeshJob.UndoBuffer.ControlAll);
         for (int sub = 0; sub < t.msMesh.subMeshEntries.Count; ++sub)
         {
            var subMesh = t.msMesh.subMeshEntries [sub];
            for (int i = 0; i < subMesh.subMeshOverride.controlTextures.Length; ++i)
            {
               Texture2D tex = subMesh.subMeshOverride.controlTextures [i];
               int width = tex.width;
               int height = tex.height;

               for (int x = 0; x < width; ++x)
               {
                  for (int y = 0; y < height; ++y)
                  {
                     SampleColors (t, x, y, sub);
                     SetCacheValue (textureIndex, 1);
                     WriteColors (t, x, y, sub);
                  }
               }

               tex.Apply ();
            }
         }

      }

   }
#endif
         }

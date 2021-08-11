//////////////////////////////////////////////////////
// MicroSplat
// Copyright (c) Jason Booth
//////////////////////////////////////////////////////


using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;


#if __MICROSPLAT__ && __MICROSPLAT_MESH__
namespace JBooth.MicroSplat
{
   public partial class MeshPainterWindow : EditorWindow
   {
      static float brushTargetValue = 1; // target value for brush to lerp towards


      int autoDampenSize = 3;
      int autoDampenBlur = 1;
      Color brushPaintColor = Color.white;

      void InitFXData()
      {
         for (int i = 0; i < meshes.Length; ++i)
         {
            var mj = meshes[i];
            var m = mj.msMesh;
            if (m.templateMaterial != null && m.templateMaterial.HasProperty("_StreamControl"))
            {
               for (int sub = 0; sub < m.subMeshEntries.Count; ++sub)
               {
                  if (m.subMeshEntries[sub].subMeshOverride.streamTex == null)
                  {
                     CreateFXTexture (m, m.subMeshEntries[sub], sub);
                  }
               }
            }
            if (m.templateMaterial != null && m.templateMaterial.HasProperty ("_DisplacementDampening"))
            {
               for (int sub = 0; sub < m.subMeshEntries.Count; ++sub)
               {
                  if (m.subMeshEntries[sub].subMeshOverride.displacementDampening == null)
                  {
                     CreateDisplacementDampeningTexture (m, m.subMeshEntries[sub], sub);
                  }
               }
            }
            if (m.templateMaterial != null && m.templateMaterial.HasProperty("_GlobalTintTex"))
            {
               for (int sub = 0; sub < m.subMeshEntries.Count; ++sub)
               {
                  if (m.subMeshEntries [sub].subMeshOverride.displacementDampening == null)
                  {
                     CreateTintTexture (m, m.subMeshEntries [sub], sub);
                  }
               }
            }
         }
      }

      void CreateDisplacementDampeningTexture(MicroSplatMesh mgr, MicroSplatMesh.SubMeshEntry sub, int subIdx)
      {
         if (sub.subMeshOverride.active == false)
            return;
         Texture2D tex = sub.subMeshOverride.displacementDampening;
         
         // if we still don't have a texture, create one
         if (tex == null)
         {
            int width = sub.subMeshOverride.createTextureSizeX;
            int height = sub.subMeshOverride.createTextureSizeY;
            if (sub.subMeshOverride.controlTextures[0] != null)
            {
               width = sub.subMeshOverride.controlTextures [0].width;
               height = sub.subMeshOverride.controlTextures [0].height;
            }
            tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            sub.subMeshOverride.displacementDampening = tex;
            Color c = new Color(0, 0, 0, 1);
            for (int x = 0; x < tex.width; ++x)
            {
               for (int y = 0; y < tex.height; ++y)
               {
                  tex.SetPixel(x, y, c);
               }
            }
            tex.Apply();
            tex.wrapMode = TextureWrapMode.Repeat;
            var path = MicroSplatUtilities.RelativePathFromAsset(mgr.templateMaterial);
            path += "/" + mgr.name + "_dispdampen" + subIdx + ".png";
            sub.subMeshOverride.displacementDampening = SaveTexture(path, tex);
         }

         mgr.Sync();
      }

      void CreateTintTexture (MicroSplatMesh mgr, MicroSplatMesh.SubMeshEntry sub, int subIdx)
      {
         if (sub.subMeshOverride.active == false)
            return;
         Texture2D tex = sub.subMeshOverride.tint;

         // if we still don't have a texture, create one
         if (tex == null)
         {
            int width = sub.subMeshOverride.createTextureSizeX;
            int height = sub.subMeshOverride.createTextureSizeY;
            if (sub.subMeshOverride.controlTextures [0] != null)
            {
               width = sub.subMeshOverride.controlTextures [0].width;
               height = sub.subMeshOverride.controlTextures [0].height;
            }
            tex = new Texture2D (width, height, TextureFormat.RGBA32, false);
            sub.subMeshOverride.tint = tex;
            Color c = new Color (0.5f, 0.5f, 0.5f, 1);
            for (int x = 0; x < tex.width; ++x)
            {
               for (int y = 0; y < tex.height; ++y)
               {
                  tex.SetPixel (x, y, c);
               }
            }
            tex.Apply ();
            tex.wrapMode = TextureWrapMode.Clamp;
            var path = MicroSplatUtilities.RelativePathFromAsset (mgr.templateMaterial);
            path += "/" + mgr.name + "_tint" + subIdx + ".png";
            sub.subMeshOverride.tint = SaveTexture (path, tex);
         }

         mgr.Sync ();
      }

      void CreateFXTexture(MicroSplatMesh mgr, MicroSplatMesh.SubMeshEntry sub, int subIdx)
      {
         if (sub.subMeshOverride.active == false)
            return;


         Texture2D tex = sub.subMeshOverride.streamTex;

         // if we still don't have a texture, create one
         if (tex == null)
         {
            int width = sub.subMeshOverride.createTextureSizeX;
            int height = sub.subMeshOverride.createTextureSizeY;
            if (sub.subMeshOverride.controlTextures [0] != null)
            {
               width = sub.subMeshOverride.controlTextures [0].width;
               height = sub.subMeshOverride.controlTextures [0].height;
            }
            tex = new Texture2D(width, height, TextureFormat.RGBA32, true, true);
            sub.subMeshOverride.streamTex = tex;
            Color c = new Color(0, 0, 0, 0);
            for (int x = 0; x < tex.width; ++x)
            {
               for (int y = 0; y < tex.height; ++y)
               {
                  tex.SetPixel(x, y, c);
               }
            }
            tex.Apply();
            tex.wrapMode = TextureWrapMode.Repeat;
            var path = MicroSplatUtilities.RelativePathFromAsset(mgr.templateMaterial);
            path += "/" + mgr.name + "_stream_data" + subIdx + ".png";
            sub.subMeshOverride.streamTex = SaveTexture(path, tex, false, TextureWrapMode.Repeat);
         }

         mgr.Sync();
      }


      bool VerifyFXData()
      {
         if (meshes == null || meshes.Length == 0)
            return false;

         for (int i = 0; i < meshes.Length; ++i)
         {
            var m = meshes[i];
            if (m.msMesh == null || m.msMesh.templateMaterial == null || m.msMesh.keywordSO == null)
            {
               EditorGUILayout.HelpBox("Mesh(s) are not setup for MicroSplat, please set them up", MessageType.Error);
               return false;
            }
         }

         InitFXData();

         for (int i = 0; i < meshes.Length; ++i)
         {
            var mj = meshes[i];
            var mst = mj.msMesh;
            if (mst != null)
            {
               for (int sub = 0; sub < mst.subMeshEntries.Count; ++sub)
               {
                  var subMesh = mst.subMeshEntries [sub];
                  var tex = subMesh.subMeshOverride.streamTex;
                  if (tex != null)
                  {
                     AssetImporter ai = AssetImporter.GetAtPath (AssetDatabase.GetAssetPath (tex));
                     TextureImporter ti = ai as TextureImporter;
                     if (ti == null || !ti.isReadable)
                     {
                        EditorGUILayout.HelpBox ("Control texture is not read/write", MessageType.Error);
                        if (GUILayout.Button ("Fix it!"))
                        {
                           ti.isReadable = true;
                           ti.SaveAndReimport ();
                        }
                        return false;
                     }

                     bool isLinear = ti.sRGBTexture == false;
                     bool isRGB32 = ti.textureCompression == TextureImporterCompression.Uncompressed && ti.GetDefaultPlatformTextureSettings ().format == TextureImporterFormat.RGBA32;

                     if (isRGB32 == false || isLinear == false || ti.wrapMode != TextureWrapMode.Repeat)
                     {
                        EditorGUILayout.HelpBox ("Control texture is not in the correct format (Uncompressed, linear, repeat, RGBA32)", MessageType.Error);
                        if (GUILayout.Button ("Fix it!"))
                        {

                           ti.sRGBTexture = false;
                           ti.textureCompression = TextureImporterCompression.Uncompressed;
                           var ftm = ti.GetDefaultPlatformTextureSettings ();
                           ftm.format = TextureImporterFormat.RGBA32;
                           ti.SetPlatformTextureSettings (ftm);

                           ti.mipmapEnabled = true;
                           ti.wrapMode = TextureWrapMode.Repeat;
                           ti.SaveAndReimport ();
                        }
                        return false;
                     }
                  }
                  tex = subMesh.subMeshOverride.displacementDampening;
                  if (tex != null)
                  {
                     AssetImporter ai = AssetImporter.GetAtPath (AssetDatabase.GetAssetPath (tex));
                     TextureImporter ti = ai as TextureImporter;
                     if (ti == null || !ti.isReadable)
                     {
                        EditorGUILayout.HelpBox ("Displacement Dampening texture is not read/write", MessageType.Error);
                        if (GUILayout.Button ("Fix it!"))
                        {
                           ti.isReadable = true;
                           ti.SaveAndReimport ();
                        }
                        return false;
                     }
                     bool isLinear = ti.sRGBTexture == false;
                     bool isRGB32 = ti.textureCompression == TextureImporterCompression.Uncompressed && ti.GetDefaultPlatformTextureSettings ().format == TextureImporterFormat.RGBA32;

                     if (isRGB32 == false || isLinear == false || ti.wrapMode != TextureWrapMode.Repeat)
                     {
                        EditorGUILayout.HelpBox ("Displacement Dampening texture is not in the correct format (RGBA, repeat)", MessageType.Error);
                        if (GUILayout.Button ("Fix it!"))
                        {
                           ti.sRGBTexture = false;
                           ti.textureCompression = TextureImporterCompression.Uncompressed;
                           var ftm = ti.GetDefaultPlatformTextureSettings ();
                           ftm.format = TextureImporterFormat.RGBA32;
                           ti.SetPlatformTextureSettings (ftm);

                           ti.mipmapEnabled = false;
                           ti.wrapMode = TextureWrapMode.Repeat;
                           ti.SaveAndReimport ();
                        }
                        return false;
                     }
                  }
               }
            }
         }
         return true;

      }

      void DrawFXFillGUI(int channel)
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
               FillFX(meshes[i], channel, brushTargetValue);
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
         if (GUILayout.Button("Clear"))
         {
            if (OnBeginStroke != null)
            {
               OnBeginStroke(meshes);
            }
            for (int i = 0; i < meshes.Length; ++i)
            {
               FillFX(meshes[i], channel, 0, true);
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


      void FillFX(MeshJob t, int channel, float val, bool isClear = false)
      {
         InitFXData();

         if (channel < 4)
         {
            t.RegisterUndo(MeshJob.UndoBuffer.FX);
            for (int i = 0; i < t.msMesh.subMeshEntries.Count; ++i)
            {
               var sub = t.msMesh.subMeshEntries [i];
               Texture2D tex = sub.subMeshOverride.streamTex;
               if (tex == null)
               {
                  Debug.LogError ("Stream texture not found, assign to MicroSplatMesh component");
                  return;
               }
               int width = tex.width;
               int height = tex.height;
               for (int x = 0; x < width; ++x)
               {
                  for (int y = 0; y < height; ++y)
                  {
                     var c = tex.GetPixel (x, y);
                     c [channel] = val;
                     tex.SetPixel (x, y, c);
                  }
               }
               tex.Apply ();
            }
            
         }
         else if (channel == 5) // displacement
         {
            t.RegisterUndo(MeshJob.UndoBuffer.Dampening);
            for (int i = 0; i < t.msMesh.subMeshEntries.Count; ++i)
            {
               Texture2D tex = t.msMesh.subMeshEntries[i].subMeshOverride.displacementDampening;
               int width = tex.width;
               int height = tex.height;
               Color c = new Color (val, val, val, val);
               for (int x = 0; x < width; ++x)
               {
                  for (int y = 0; y < height; ++y)
                  {
                     tex.SetPixel (x, y, c);
                  }
               }
               tex.Apply ();
            }
         }
         else if (channel == 4) // displacement
         {
            t.RegisterUndo (MeshJob.UndoBuffer.Tint);
            for (int i = 0; i < t.msMesh.subMeshEntries.Count; ++i)
            {
               Texture2D tex = t.msMesh.subMeshEntries [i].subMeshOverride.tint;
               Color c = brushPaintColor;
               if (isClear)
               {
                  c = Color.gray;
               }
               int width = tex.width;
               int height = tex.height;
               for (int x = 0; x < width; ++x)
               {
                  for (int y = 0; y < height; ++y)
                  {
                     tex.SetPixel (x, y, c);
                  }
               }
               tex.Apply ();
            }
         }
      }

      void AutoGenerateDisplacementDampeningFromChart()
      {
         for (int i = 0; i < meshes.Length; ++i)
         {
            for (int sub = 0; sub < meshes[i].msMesh.subMeshEntries.Count; ++sub)
            {
               Texture2D splatTex = meshes [i].msMesh.subMeshEntries[sub].subMeshOverride.displacementDampening;
               if (splatTex == null)
               {
                  continue;
               }
               meshes [i].RegisterUndo (MeshJob.UndoBuffer.Dampening);
               // setup the brush, we'll hijack the green channel since it draws the UV chart.
               SetupBrush (meshes [i], Vector3.one, Vector3.one, true);
               brushMat.SetVector ("_UVMeshRange", meshes [i].msMesh.subMeshEntries [sub].subMeshOverride.uvRange);

               RenderTexture rt = RenderTexture.GetTemporary (splatTex.width, splatTex.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
               RenderTexture rt2 = RenderTexture.GetTemporary (splatTex.width, splatTex.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
               RenderTexture.active = rt;

               GL.PushMatrix ();
               GL.LoadIdentity ();
               GL.Clear (true, true, Color.black);
               brushMat.SetPass (0);
               Graphics.DrawMeshNow (meshes [i].sharedMesh, Vector3.zero, Quaternion.identity);
               GL.PopMatrix ();
               RenderTexture.active = null;


               Material mat = new Material (Shader.Find ("Hidden/MicroSplatMeshAutoDampening"));
               Graphics.Blit (rt, rt2, mat);
               if (autoDampenSize > 1)
               {
                  Material expand = new Material (Shader.Find ("Hidden/MicroSplatMeshAutoDampeningExpand"));
                  for (int x = 1; x < autoDampenSize; ++x)
                  {
                     Graphics.Blit (rt2, rt);
                     Graphics.Blit (rt, rt2, expand);
                  }
                  DestroyImmediate (expand);
               }

               if (autoDampenBlur > 0)
               {
                  Material blur = new Material (Shader.Find ("Hidden/MicroSplatMeshAutoDampeningBlur"));
                  Graphics.Blit (rt, rt2, blur);
                  for (int x = 1; x < autoDampenBlur; ++x)
                  {
                     Graphics.Blit (rt2, rt);
                     Graphics.Blit (rt, rt2, blur);
                  }
                  DestroyImmediate (blur);
               }
               // read back
               RenderTexture.active = rt2;
               splatTex.ReadPixels (new Rect (0, 0, splatTex.width, splatTex.height), 0, 0);
               splatTex.Apply ();
               RenderTexture.active = null;


               RenderTexture.ReleaseTemporary (rt);
               RenderTexture.ReleaseTemporary (rt2);
               DestroyImmediate (mat);
            }
            
         }

         MicroSplatMesh.SyncAll ();
      }


      enum Tab
      {
         Texture = 0
#if __MICROSPLAT_STREAMS__
         , Wetness = 1, Puddles = 2,Streams = 3,Lava = 4
#endif
#if __MICROSPLAT_TESSELLATION__
         , Displacement = 5
#endif
#if __MICROSPLAT_GLOBALTEXTURE__
         , Tint = 6
#endif
      }

      Tab tab = Tab.Texture;

      Vector2 scroll;
      void OnFXGUI()
      {
         if (VerifyFXData() == false)
         {
            EditorGUILayout.HelpBox("Please select a Mesh with MicroSplatMesh component setup to begin", MessageType.Info);
            return;
         }

         //DrawSettingsGUI();


         bool hasWetness = false;
         bool hasPuddles = false;
         bool hasStreams = false;
         bool hasLava = false;
         bool hasDisplacement = false;
         bool hasTint = false;

         for (int i = 0; i < meshes.Length; ++i)
         {
            var t = meshes[i];
            if (t != null && t.msMesh != null && t.msMesh.keywordSO != null)
            {
               if (!hasWetness)
                  hasWetness = t.msMesh.keywordSO.IsKeywordEnabled("_WETNESS");
               if (!hasPuddles)
                  hasPuddles = t.msMesh.keywordSO.IsKeywordEnabled("_PUDDLES");
               if (!hasStreams)
                  hasStreams = t.msMesh.keywordSO.IsKeywordEnabled("_STREAMS");
               if (!hasLava)
                  hasLava = t.msMesh.keywordSO.IsKeywordEnabled("_LAVA");
               if (!hasDisplacement)
                  hasDisplacement = t.msMesh.keywordSO.IsKeywordEnabled ("_DISPLACEMENTDAMPENING");
               if (!hasTint)
                  hasTint = t.msMesh.keywordSO.IsKeywordEnabled ("_GLOBALTINT");
            }
         }
#if __MICROSPLAT_STREAMS__
         if (tab == Tab.Wetness)
         {
            if (hasWetness)
            {
               if (MicroSplatUtilities.DrawRollup ("Brush Settings"))
               {
                  DrawBrushSettingsGUI (false);
               }
               DrawFXFillGUI (0);
            }
            else
            {
               EditorGUILayout.HelpBox ("Wetness is not enabled on your shader, please enable in the shader options if you want to paint wetness", MessageType.Warning);
            }
         }
         else if (tab == Tab.Puddles)
         {
            if (hasPuddles)
            {
               if (MicroSplatUtilities.DrawRollup ("Brush Settings"))
               {
                  DrawBrushSettingsGUI (false);
               }
               DrawFXFillGUI (1);
            }
            else
            {
               EditorGUILayout.HelpBox ("Puddles is not enabled on your shader, please enable in the shader options if you want to paint puddles", MessageType.Warning);
            }
         }
         else if (tab == Tab.Streams)
         {
            if (hasStreams)
            {
               if (MicroSplatUtilities.DrawRollup ("Brush Settings"))
               {
                  DrawBrushSettingsGUI (false);
               }
               DrawFXFillGUI (2);
            }
            else
            {
               EditorGUILayout.HelpBox ("Streams are not enabled on your shader, please enable in the shader options if you want to paint streams", MessageType.Warning);
            }
         }
         else if (tab == Tab.Lava)
         {
            if (hasLava)
            {
               if (MicroSplatUtilities.DrawRollup ("Brush Settings"))
               {
                  DrawBrushSettingsGUI (false);
               }
               DrawFXFillGUI (3);
            }
            else
            {
               EditorGUILayout.HelpBox ("Lava is not enabled on your shader, please enable in the shader options if you want to paint lava", MessageType.Warning);
            }
         }
#endif
#if __MICROSPLAT_TESSELLATION__
         if (tab == Tab.Displacement)
         {
            if (hasDisplacement)
            {
               if (MicroSplatUtilities.DrawRollup ("Brush Settings"))
               {
                  DrawBrushSettingsGUI (false);
               }
               DrawFXFillGUI (4);
               autoDampenSize = EditorGUILayout.IntSlider ("Edge Size", autoDampenSize, 1, 16);
               autoDampenBlur = EditorGUILayout.IntSlider ("Edge Blur", autoDampenBlur, 0, autoDampenSize);
               if (GUILayout.Button ("Auto-Generate from UV chart"))
               {
                  AutoGenerateDisplacementDampeningFromChart ();
               }
            }
            else
            {
               EditorGUILayout.HelpBox ("Displacement Dampening is not enabled on your shader, please enable in the shader options if you want to paint displacement dampening", MessageType.Warning);
            }
         }
#endif
#if __MICROSPLAT_GLOBALTEXTURE__
         if (tab == Tab.Tint)
         {
            if (hasTint)
            {
               if (MicroSplatUtilities.DrawRollup ("Brush Settings"))
               {
                  DrawBrushSettingsGUI (false);
               }
               brushPaintColor = EditorGUILayout.ColorField ("Color", brushPaintColor);
               DrawFXFillGUI (4);
              
            }
            else
            {
               EditorGUILayout.HelpBox ("Global Tint is not enabled on your shader, please enable in the shader options if you want to paint displacement dampening", MessageType.Warning);
            }
         }
#endif

         DrawFXSaveGUI ();

      }

      void SaveFXTextures()
      {
         for (int i = 0; i < meshes.Length; ++i)
         {
            for (int sub = 0; sub < meshes [i].msMesh.subMeshEntries.Count; ++sub)
            {
               var subMesh = meshes [i].msMesh.subMeshEntries [sub];
               if (subMesh.subMeshOverride.streamTex != null)
               {
                  string path = AssetDatabase.GetAssetPath (subMesh.subMeshOverride.streamTex);
                  var bytes = subMesh.subMeshOverride.streamTex.EncodeToPNG ();
                  System.IO.File.WriteAllBytes (path, bytes);
               }
               if (subMesh.subMeshOverride.displacementDampening != null)
               {
                  string path = AssetDatabase.GetAssetPath (subMesh.subMeshOverride.displacementDampening);
                  var bytes = subMesh.subMeshOverride.displacementDampening.EncodeToPNG ();
                  System.IO.File.WriteAllBytes (path, bytes);
               }
               if (subMesh.subMeshOverride.tint != null)
               {
                  string path = AssetDatabase.GetAssetPath (subMesh.subMeshOverride.tint);
                  var bytes = subMesh.subMeshOverride.tint.EncodeToPNG ();
                  System.IO.File.WriteAllBytes (path, bytes);
               }
            }
         }
         AssetDatabase.Refresh();
      }

      void DrawFXSaveGUI()
      {
         EditorGUILayout.Space();
         EditorGUILayout.BeginHorizontal();
         if (GUILayout.Button("Save"))
         {
            SaveFXTextures ();
         }

         EditorGUILayout.EndHorizontal();
         EditorGUILayout.Space();
      }

      static Material brushApplyFXStrokeMat;

      void PaintFXMeshGPU(MeshJob mj, Vector3 brushWorldPos, bool preview = false, bool isFill = false)
      {
         int channelIdx = ((int)tab - 1);
         
         if (brushApplyFXStrokeMat == null)
         {
            brushApplyFXStrokeMat = new Material(Shader.Find("Hidden/MicroSplatMeshFXBrushApply"));
         }

         if (mj == null || mj.msMesh == null)
         {
            return;
         }

         for (int sub = 0; sub < mj.msMesh.subMeshEntries.Count; ++sub)
         {
            var subMesh = mj.msMesh.subMeshEntries [sub];

            var splatTex = subMesh.subMeshOverride.streamTex;
            if (channelIdx == 4)
            {
               splatTex = subMesh.subMeshOverride.displacementDampening;
            }
            else if (channelIdx == 5)
            {
               splatTex = subMesh.subMeshOverride.tint;
            }

            if (splatTex == null)
            {
               return;
            }

            float pressure = Event.current.pressure > 0 ? Event.current.pressure : 1.0f;
            pressure *= Time.deltaTime;

            RenderTexture rt = RenderTexture.GetTemporary (splatTex.width, splatTex.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear, 1);
            RenderTexture rt2 = RenderTexture.GetTemporary (splatTex.width, splatTex.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear, 1);

            RenderTexture.active = rt;

            GL.PushMatrix ();
            GL.LoadIdentity ();

            GL.Clear (true, true, Color.black);
            brushMat.SetPass (0);

            Graphics.DrawMeshNow (mj.sharedMesh, Vector3.zero, Quaternion.identity);
            GL.PopMatrix ();
  

            // blit stroke to control texture
            brushApplyFXStrokeMat.SetTexture ("_BrushBuffer", rt);
            brushApplyFXStrokeMat.SetInt ("_channel", channelIdx);
            brushApplyFXStrokeMat.SetFloat ("_BrushFlow", isFill ? 1 : brushFlow * pressure);
            brushApplyFXStrokeMat.SetVector ("_EdgeBuffer", new Vector2 (1.0f / rt.width, 1.0f / rt.height));
            brushApplyFXStrokeMat.SetFloat ("_TargetValue", brushTargetValue);
            brushApplyFXStrokeMat.SetColor ("_TargetColor", brushPaintColor);
            brushApplyFXStrokeMat.SetTexture ("_Control0", splatTex);
            brushApplyFXStrokeMat.SetFloat ("_IsFill", isFill ? 1 : 0);
            brushProjector.material.SetColor ("_BrushColor", brushDisplayColor);

            Graphics.Blit (splatTex, rt2, brushApplyFXStrokeMat);
            RenderTexture.active = rt2;

            if (!preview)
            {
               splatTex.ReadPixels (new Rect (0, 0, rt2.width, rt2.height), 0, 0);
               splatTex.Apply ();
            }

            RenderTexture.active = null;
            RenderTexture.ReleaseTemporary (rt);
            RenderTexture.ReleaseTemporary (rt2);
         }
         mj.msMesh.Sync();
      }

   }

}
#endif

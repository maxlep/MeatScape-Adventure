//////////////////////////////////////////////////////
// MicroSplat
// Copyright (c) Jason Booth
//////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
#if __MICROSPLAT__ && __MICROSPLAT_MESH__
using JBooth.MicroSplat;

[CustomEditor (typeof (MicroSplatMesh))]
[CanEditMultipleObjects]
public partial class MicroSplatMeshEditor : Editor
{
   static GUIContent CTemplateMaterial = new GUIContent ("Template Material", "Material to use for this object");
   static GUIContent CStandardDiffuse = new GUIContent ("Albedo Override", "Albedo texture");
   static GUIContent CStandardNormal = new GUIContent ("Normal Override", "Normal Map");
   static GUIContent CStandardSmoothMetal = new GUIContent ("Smoothness/Metal Override", "Smoothness Metal texture");
   static GUIContent CStandardEmission = new GUIContent ("Emission Override", "Emissive texture");
   static GUIContent CStandardOcclusion = new GUIContent ("Occlusion Override", "Occlusion texture");
   static GUIContent CStandardHeight = new GUIContent ("Height Override", "Height texture");
   static GUIContent CStandardSpecular = new GUIContent ("Specular Override", "Specular Texture");
   static GUIContent CStandardPackedMap = new GUIContent ("Packed Map Override", "(R) Metallic, (G) Smoothness, (B) Height (A) Occlusion");
   static GUIContent CStandardSSS = new GUIContent ("SSS Override", "(G) Thickness");

#if __MICROSPLAT_GLOBALTEXTURE__
   static GUIContent geoTexOverride = new GUIContent ("Geo Texture Override", "If you want each object to have it's own geo texture instead of the one defined in the material, add it here");
   static GUIContent geoTintOverride = new GUIContent ("Tint Texture Override", "If you want each object to have it's own global tint instead of the one defined in the material, add it here");
   static GUIContent geoNormalOverride = new GUIContent ("Global Normal Override", "If you want each object to have it's own global normal instead of the one defined in the material, add it here");
   static GUIContent geoSAOMOverride = new GUIContent ("Global SOAM Override", "If you want each mesh to have it's own Smoothness(R), AO(G) and Metallic (B) map instead of the one defined in the material, add it here");
   static GUIContent geoEmisOverride = new GUIContent ("Global Emissive Override", "If you want each mesh to have it's own Emissive map instead of the one defined in the material, set it here");
#endif

#if __MICROSPLAT_SNOW__
   static GUIContent snowMaskOverride = new GUIContent ("Snow Mask Override", "Overrides the snow mask for this mesh");
#endif

#if __MICROSPLAT_ALPHAHOLE__
   static GUIContent clipMapOverride = new GUIContent ("Clip Map Override", "Provide a unique clip map for each object");
#endif

#if __MICROSPLAT_PROCTEX__
   static GUIContent biomeOverride = new GUIContent ("Biome Map Override", "Biome map for this mesh");
   static GUIContent biomeOverride2 = new GUIContent ("Biome Map2 Override", "Biome map for this mesh");
   static GUIContent cavityMapOverride = new GUIContent ("Cavity Map Override", "Cavity map for this mesh");
#endif

#if __MICROSPLAT_STREAMS__
   static GUIContent streamOverride = new GUIContent ("Stream Map Override", "Stream map for this mesh");
#endif

#if (VEGETATION_STUDIO || VEGETATION_STUDIO_PRO)
   static GUIContent CVSGrassMap = new GUIContent("Grass Map", "Grass Map from Vegetation Studio");
   static GUIContent CVSShadowMap = new GUIContent("Shadow Map", "Shadow map texture from Vegetation Studio");
#endif

   static GUIContent CUVRange = new GUIContent ("UV Range", "The uv range is computed automatically from the mesh UVs, such that the control maps fully encapsulate the UV coordinates of your mesh. You can recalculate the values if the mesh has changed. Note that you can use this to share control textures between multiple objects. If you aranged the UVs of 4 objects into the quadrants of a 0-1 space, you could set this value to 0,0,1,1 to make it take the full 0-1 space, and each mesh would only paint into it's quadrant");

   int numControlTextures = 1;

   public string lastPath;
   static bool overrideTextureCount = false;


   Texture2D CreateControlTexture (int height, int width, string path, string prefix, int index)
   {
      Texture2D tex = new Texture2D (height, width, TextureFormat.ARGB32, true, true);
      var bytes = tex.EncodeToPNG ();
      string fullpath = MicroSplatUtilities.MakeAbsolutePath (path);
      string name = "/" + prefix + "_control" + index + ".png";
      System.IO.File.WriteAllBytes (fullpath + name, bytes);
      AssetDatabase.Refresh ();
      AssetImporter ai = AssetImporter.GetAtPath (path + name);
      TextureImporter ti = ai as TextureImporter;
      ti.sRGBTexture = false;
      ti.isReadable = true;
      ti.textureCompression = TextureImporterCompression.Uncompressed;
      ti.filterMode = FilterMode.Bilinear;
      ti.wrapMode = TextureWrapMode.Repeat;
      ti.SaveAndReimport ();
      return AssetDatabase.LoadAssetAtPath<Texture2D> (ti.assetPath);
   }

   static GUIContent COverrideTexCount = new GUIContent ("Override Texture Count", "Manually specify the number of control textures to use instead of sizing it for the current array");

   public bool createTextureArray = true;
   public MicroSplatMeshModule.ShaderType meshSetupType = MicroSplatMeshModule.ShaderType.Combined;
   static GUIContent CMeshSetupType = new GUIContent ("Shader Type", "Do you want a regular splat map shader, or one which is applied over the existing texturing?");
   static GUIContent CCreateTextureArray = new GUIContent ("Create Texture Array Config", "Create a Texture Array Config to generate texture arrays from");


   public Vector4 ComputeMeshRange (MicroSplatMesh msm, int subMesh)
   {
      MeshFilter f = msm.GetComponent<MeshFilter> ();
      MeshRenderer r = msm.GetComponent<MeshRenderer> ();
      if (f == null || r == null)
      {
         return new Vector4 (0, 0, 1, 1);
      }

      Mesh m = f.sharedMesh;
      Material mat = msm.templateMaterial;

      if (m == null || mat == null)
      {
         return new Vector4 (0, 0, 1, 1);
      }

      if (subMesh >= m.subMeshCount)
      {
         return new Vector4 (0, 0, 1, 1);
      }
      var indexes = m.GetIndices (subMesh);


      MicroSplatKeywords keywords = MicroSplatUtilities.FindOrCreateKeywords (mat);

      var uv = m.uv;
      if (keywords.IsKeywordEnabled ("_MESHUV2") && m.uv2 != null && m.uv2.Length != 0)
      {
         uv = m.uv2;
      }
      if (uv == null)
      {
         return new Vector4 (0, 0, 1, 1);
      }

      Vector4 range = new Vector4 (uv [0].x, uv [0].y, uv [0].x, uv [0].y);
      for (int i = 0; i < indexes.Length; ++i)
      {
         var v = uv [indexes [i]];
         range.x = Mathf.Min (v.x, range.x);
         range.y = Mathf.Min (v.y, range.y);
         range.z = Mathf.Max (v.x, range.z);
         range.w = Mathf.Max (v.y, range.w);
      }
      return range;
   }

   public bool DoSetupGUI ()
   {
      if (lastPath == null)
      {
         lastPath = Application.dataPath;
      }
      bool needSync = false;
      MicroSplatMesh msm = target as MicroSplatMesh;
      if (msm.templateMaterial == null)
      {
         EditorGUILayout.HelpBox ("Mesh Painting needs a shader and material, assign one or create a new one", MessageType.Info);
         meshSetupType = (MicroSplatMeshModule.ShaderType)EditorGUILayout.EnumPopup (CMeshSetupType, meshSetupType);
         createTextureArray = EditorGUILayout.Toggle (CCreateTextureArray, createTextureArray);
         var mats = msm.rend.sharedMaterials;

         // resize our entry list to match
         if (meshSetupType != MicroSplatMeshModule.ShaderType.Combined || mats == null || mats.Length == 0)
         {
            if (msm.subMeshEntries.Count != 1)
            {
               msm.subMeshEntries = new List<MicroSplatMesh.SubMeshEntry> ();
               MicroSplatMesh.SubMeshEntry e = new MicroSplatMesh.SubMeshEntry ();
               e.subMeshOverride = new MicroSplatMesh.SubMeshOverride ();
               e.subMeshOverride.active = true;
               e.subMeshOverride.materialName = (mats != null && mats.Length > 0 && mats [0] != null ? mats [0].name : "default");
               msm.subMeshEntries.Add (e);
            }
         }
         else // combined
         {
            while (msm.subMeshEntries.Count < mats.Length)
            {
               var mat = mats [msm.subMeshEntries.Count];
               MicroSplatMesh.SubMeshEntry e = new MicroSplatMesh.SubMeshEntry ();
               e.subMeshOverride = new MicroSplatMesh.SubMeshOverride ();
               e.subMeshOverride.active = true;
               e.subMeshOverride.materialName = (mat != null ? mat.name : "default");
               // copy some of the properties over if they exist
               if (mat != null)
               {
                  e.combinedOverride = new CombinedOverride ();
                  if (mat.HasProperty ("_MainTex"))
                  {
                     e.combinedOverride.standardAlbedoOverride = mat.GetTexture ("_MainTex") as Texture2D;
                  }
                  if (mat.HasProperty ("_BumpMap"))
                  {
                     e.combinedOverride.standardNormalOverride = mat.GetTexture ("_BumpMap") as Texture2D;
                  }
                  if (mat.HasProperty ("_MainTex_ST"))
                  {
                     Vector4 uvScale = mat.GetVector ("_MainTex_ST");
                     if (uvScale != e.subMeshOverride.UVOverride)
                     {
                        e.subMeshOverride.UVOverride = uvScale;
                        e.subMeshOverride.bUVOverride = true;
                     }
                  }
               }
               msm.subMeshEntries.Add (e);
            }
            while (msm.subMeshEntries.Count > mats.Length)
            {
               msm.subMeshEntries.RemoveAt (msm.subMeshEntries.Count - 1);
            }
         }
         using (new GUILayout.VerticalScope (GUI.skin.box))
         {
            if (msm.subMeshEntries.Count > 1)
            {
               for (int i = 0; i < msm.subMeshEntries.Count; ++i)
               {
                  EditorGUILayout.BeginHorizontal ();
                  var e = msm.subMeshEntries [i];
                  e.subMeshOverride.active = EditorGUILayout.Toggle (e.subMeshOverride.materialName, e.subMeshOverride.active);
                  EditorGUILayout.EndHorizontal ();
               }

            }


            if (GUILayout.Button ("Create Required Data"))
            {
               var path = EditorUtility.OpenFolderPanel ("Select Location", lastPath, "");
               if (!string.IsNullOrEmpty (path))
               {
                  lastPath = path;
                  path = MicroSplatUtilities.MakeRelativePath (path);
                  if (!path.EndsWith ("MicroSplatData"))
                  {
                     path += "/MicroSplatData";
                  }
                  System.IO.Directory.CreateDirectory (MicroSplatUtilities.MakeAbsolutePath (path));
                  List<string> keywords = new List<string> ();
                  keywords.Add ("_MICROSPLAT");
                  keywords.Add (MicroSplatBaseFeatures.DefineFeature._MICROMESH.ToString ());
                  if (meshSetupType == MicroSplatMeshModule.ShaderType.Overlay)
                  {
                     keywords.Add (MicroSplatMeshModule.DefineFeature._MESHOVERLAYSPLATS.ToString ());
                  }
                  else if (meshSetupType == MicroSplatMeshModule.ShaderType.Combined)
                  {
                     keywords.Add (MicroSplatMeshModule.DefineFeature._MESHCOMBINED.ToString ());
                  }
                  var mat = MicroSplatShaderGUI.NewShaderAndMaterial (path, msm.gameObject.name, keywords.ToArray ());
                  msm.templateMaterial = mat;
                  AssetDatabase.Refresh ();
                  Selection.activeObject = mat;
                  mat.SetVector ("_UVScale", new Vector4 (5, 5, 0, 0));
                  msm.propData = MicroSplatShaderGUI.FindOrCreatePropTex (msm.templateMaterial);


                  if (createTextureArray)
                  {
                     path = MicroSplatUtilities.MakeRelativePath (path);
                     var config = TextureArrayConfigEditor.CreateConfig (path);
                     msm.templateMaterial.SetTexture ("_Diffuse", config.diffuseArray);
                     msm.templateMaterial.SetTexture ("_NormalSAO", config.normalSAOArray);
                  }
                  msm.Sync ();
                  EditorUtility.SetDirty (msm);
               }
            }
         }
         return false; // template may be null..
      }

      if (msm.templateMaterial.GetTexture ("_Diffuse") == null && !msm.keywordSO.IsKeywordEnabled ("_DISABLESPLATMAPS"))
      {
         EditorGUILayout.HelpBox ("You must create and assign a texture array to the material", MessageType.Info);
         if (GUILayout.Button ("Select Material"))
         {
            Selection.activeObject = msm.templateMaterial;
         }
         if (GUILayout.Button ("Create Texture Array Config"))
         {
            var path = EditorUtility.OpenFolderPanel ("Select Location", lastPath, "");
            if (!string.IsNullOrEmpty (path))
            {
               lastPath = path;
               path = MicroSplatUtilities.MakeRelativePath (path);
               var config = TextureArrayConfigEditor.CreateConfig (path);
               msm.templateMaterial.SetTexture ("_Diffuse", config.diffuseArray);
               msm.templateMaterial.SetTexture ("_NormalSAO", config.normalSAOArray);
            }
         }
         else
            return false;
      }

      if (msm.propData == null)
      {
         msm.propData = MicroSplatShaderGUI.FindOrCreatePropTex (msm.templateMaterial);
         EditorUtility.SetDirty (msm);
      }

      if (msm.keywordSO == null)
      {
         msm.keywordSO = MicroSplatUtilities.FindOrCreateKeywords (msm.templateMaterial);
         EditorUtility.SetDirty (msm);
      }

#if __MICROSPLAT_PROCTEX__
      if (msm.keywordSO.IsKeywordEnabled ("_PROCEDURALTEXTURE") || msm.keywordSO.IsKeywordEnabled ("_PCHEIGHTGRADIENT") || msm.keywordSO.IsKeywordEnabled ("_PCHEIGHTHSV"))
      {
         var old = msm.procTexCfg;
         msm.procTexCfg = MicroSplatProceduralTexture.FindOrCreateProceduralConfig (msm.templateMaterial);
         if (old != msm.procTexCfg)
         {
            EditorUtility.SetDirty (msm);
         }
      }
#endif



      if (msm.templateMaterial != null && msm.propData == null)
      {
         msm.propData = MicroSplatShaderGUI.FindOrCreatePropTex (msm.templateMaterial);
      }
      if (!msm.keywordSO.IsKeywordEnabled ("_DISABLESPLATMAPS") || !msm.keywordSO.IsKeywordEnabled("_PROCEDURALTEXTURE"))
      {
         bool fail = msm.subMeshEntries.Count == 0;
         if (!fail)
         {
            for (int x = 0; x < msm.subMeshEntries.Count; ++x)
            {
               if (msm.subMeshEntries [x].subMeshOverride.active == true)
               {
                  if (msm.subMeshEntries [x].subMeshOverride.controlTextures == null || msm.subMeshEntries [x].subMeshOverride.controlTextures.Length == 0)
                  {
                     fail = true;
                  }
               }
            }
         }
         if (fail)
         {
            EditorGUILayout.HelpBox ("Mesh Painting needs control textures", MessageType.Info);
            var diffArray = msm.templateMaterial.GetTexture ("_Diffuse") as Texture2DArray;
            overrideTextureCount = EditorGUILayout.Toggle (COverrideTexCount, overrideTextureCount);
            if (diffArray != null && !overrideTextureCount)
            {
               numControlTextures = Mathf.RoundToInt ((float)diffArray.depth / 4.0f + 0.3f);
               if (msm.keywordSO.IsKeywordEnabled ("_MESHSUBARRAY"))
               {
                  numControlTextures = 1;
               }
               EditorGUILayout.LabelField ("Control Textures needed for array : " + numControlTextures);
            }
            else
            {
               numControlTextures = EditorGUILayout.IntSlider ("Control Texture Count", numControlTextures, 1, 8);
            }

            using (new GUILayout.VerticalScope (GUI.skin.box))
            {
               if (!msm.keywordSO.IsKeywordEnabled ("_DISABLESPLATMAPS") && !msm.keywordSO.IsKeywordEnabled ("_PROCEDURALTEXTURE"))
               { 
                  for (int i = 0; i < msm.subMeshEntries.Count; ++i)
                  {
                     EditorGUI.indentLevel += 1;
                     EditorGUILayout.PropertyField (serializedObject.FindProperty (string.Format ("subMeshEntries.Array.data[{0}]", i)).FindPropertyRelative ("subMeshOverride").FindPropertyRelative ("controlTextures"), true);
                     serializedObject.ApplyModifiedProperties ();
                     var e = msm.subMeshEntries [i];
                     EditorGUILayout.BeginHorizontal ();
                     EditorGUILayout.PrefixLabel ("Texture Size");
                     e.subMeshOverride.createTextureSizeX = EditorGUILayout.IntField (e.subMeshOverride.createTextureSizeX, GUILayout.Width (80));
                     e.subMeshOverride.createTextureSizeY = EditorGUILayout.IntField (e.subMeshOverride.createTextureSizeY, GUILayout.Width (80));
                     e.subMeshOverride.createTextureSizeX = Mathf.Clamp (e.subMeshOverride.createTextureSizeX, 16, 4096);
                     e.subMeshOverride.createTextureSizeY = Mathf.Clamp (e.subMeshOverride.createTextureSizeY, 16, 4096);
                     EditorGUILayout.EndHorizontal ();
                     EditorGUI.indentLevel -= 1;

                  }

                  if (GUILayout.Button ("Create Control Textures"))
                  {
                     var path = MicroSplatUtilities.RelativePathFromAsset (msm.templateMaterial);
                     if (!string.IsNullOrEmpty (path))
                     {
                        lastPath = path;
                        path = MicroSplatUtilities.MakeRelativePath (path);
                        for (int subMesh = 0; subMesh < msm.subMeshEntries.Count; ++subMesh)
                        {
                           msm.subMeshEntries [subMesh].subMeshOverride.controlTextures = new Texture2D [numControlTextures];
                           for (int i = 0; i < numControlTextures; ++i)
                           {
                              Texture2D tex = CreateControlTexture (msm.subMeshEntries [subMesh].subMeshOverride.createTextureSizeX, msm.subMeshEntries [subMesh].subMeshOverride.createTextureSizeY, path, msm.gameObject.name + "_" + msm.subMeshEntries [subMesh].subMeshOverride.materialName, i);
                              msm.subMeshEntries [subMesh].subMeshOverride.controlTextures [i] = tex;
                           }
                           msm.subMeshEntries [subMesh].subMeshOverride.uvRange = ComputeMeshRange (msm, subMesh);
                        }


                     }
                     needSync = true;
                  }
               }
            }

            if (needSync)
            {
               msm.Sync ();
            }
         }

         if (msm.subMeshEntries.Count > 0 && msm.subMeshEntries[0].subMeshOverride != null && msm.subMeshEntries[0].subMeshOverride.controlTextures != null
            && !msm.keywordSO.IsKeywordEnabled ("_DISABLESPLATMAPS") && !msm.keywordSO.IsKeywordEnabled ("_PROCEDURALTEXTURE"))
         {
            var e = msm.subMeshEntries [0];

            if (e.subMeshOverride.controlTextures.Length > 8)
            {
               System.Array.Resize (ref e.subMeshOverride.controlTextures, 8);
            }

            if (e.subMeshOverride.controlTextures != null && e.subMeshOverride.controlTextures.Length > 0 && e.subMeshOverride.controlTextures [0] != null)
            {
               var ta = msm.templateMaterial.GetTexture ("_Diffuse") as Texture2DArray;
               if (ta != null && ta.depth > e.subMeshOverride.controlTextures.Length * 4)
               {
                  if (GUILayout.Button ("Add Control Textures"))
                  {
                     for (int sb = 0; sb != msm.subMeshEntries.Count; ++sb)
                     {
                        var sub = msm.subMeshEntries [sb];
                        var src = sub.subMeshOverride.controlTextures [0];
                        var path = AssetDatabase.GetAssetPath (e.subMeshOverride.controlTextures [0]);
                        path = MicroSplatUtilities.MakeRelativePath (path);
                        path = path.Substring (0, path.LastIndexOf ("/"));
                        Texture2D tex = CreateControlTexture (src.width, src.height, path, msm.gameObject.name, sub.subMeshOverride.controlTextures.Length);
                        System.Array.Resize<Texture2D> (ref sub.subMeshOverride.controlTextures, sub.subMeshOverride.controlTextures.Length + 1);
                        sub.subMeshOverride.controlTextures [sub.subMeshOverride.controlTextures.Length - 1] = tex;
                     }
                  }
               }
            }
         }
         else
         {
            fail = !msm.keywordSO.IsKeywordEnabled ("_DISABLESPLATMAPS") && !msm.keywordSO.IsKeywordEnabled ("_PROCEDURALTEXTURE");
         }

         return !fail;
      }

      
      return true;
   }

   MicroSplatCompressor.Options compressorOptions = new MicroSplatCompressor.Options ();

   public override void OnInspectorGUI ()
   {
      bool needSync = false;
      MicroSplatMesh msm = target as MicroSplatMesh;
      EditorGUI.BeginChangeCheck ();
      msm.templateMaterial = EditorGUILayout.ObjectField (CTemplateMaterial, msm.templateMaterial, typeof (Material), false) as Material;

      if (DoSetupGUI ())
      {
         if (msm.templateMaterial.HasProperty ("_MeshSubArrayIndexes"))
         {
            if (MicroSplatUtilities.DrawRollup ("Mesh Sub Array", true, true))
            {
               var ta = (Texture2DArray)msm.templateMaterial.GetTexture ("_Diffuse");
               EditorGUI.BeginChangeCheck ();
               Vector4 v = msm.splatOverride.subArray;
               if (ta != null)
               {
                  EditorGUILayout.HelpBox ("Select which textures to use from the total array", MessageType.Info);
                  v.x = (float)MicroSplatUtilities.DrawTextureSelector ((int)v.x, ta, true);
                  v.y = (float)MicroSplatUtilities.DrawTextureSelector ((int)v.y, ta, true);
                  v.z = (float)MicroSplatUtilities.DrawTextureSelector ((int)v.z, ta, true);
                  v.w = (float)MicroSplatUtilities.DrawTextureSelector ((int)v.w, ta, true);
               }
               if (EditorGUI.EndChangeCheck ())
               {
                  msm.splatOverride.subArray = v;
                  EditorUtility.SetDirty (msm);
                  needSync = true;
               }
            }
         }

         if (msm.propData == null && msm.templateMaterial != null)
         {
            msm.propData = MicroSplatShaderGUI.FindOrCreatePropTex (msm.templateMaterial);
            if (msm.propData == null)
            {
               // this should really never happen, but users seem to have issues with unassigned propData's a lot. I think
               // this is from external tools like MapMagic not creating it, but the above call should create it.
               EditorGUILayout.HelpBox ("PropData is null, please assign", MessageType.Error);
               msm.propData = EditorGUILayout.ObjectField ("Per Texture Data", msm.propData, typeof (MicroSplatPropData), false) as MicroSplatPropData;
            }
         }


         for (int subMeshIdx = 0; subMeshIdx < msm.subMeshEntries.Count; ++subMeshIdx)
         {
            var subMesh = msm.subMeshEntries [subMeshIdx];

            if (subMesh.subMeshOverride.active == false)
               continue;
            using (new GUILayout.VerticalScope (GUI.skin.box))
            {
               if (MicroSplatUtilities.DrawRollup (subMesh.subMeshOverride.materialName, true, true))
               {
                  EditorGUI.BeginChangeCheck ();
                  using (new GUILayout.VerticalScope (GUI.skin.box))
                  {
                     EditorGUI.indentLevel++;
                     // draw these so we can unassign them
                     EditorGUILayout.PropertyField (serializedObject.FindProperty (string.Format ("subMeshEntries.Array.data[{0}]", subMeshIdx)).FindPropertyRelative ("subMeshOverride").FindPropertyRelative ("controlTextures"), true);

                     serializedObject.ApplyModifiedProperties ();
                     if (msm.keywordSO.IsKeywordEnabled ("_DISPLACEMENTDAMPENING"))
                     {
                        Rect r = EditorGUILayout.GetControlRect (GUILayout.Height (18));
                        r.width -= 18;
                        subMesh.subMeshOverride.displacementDampening = EditorGUI.ObjectField (r, "Displacement Dampening", subMesh.subMeshOverride.displacementDampening, typeof (Texture2D), false) as Texture2D;
                     }

                     if (!msm.keywordSO.IsKeywordEnabled ("_DISABLESPLATMAPS"))
                     {
                        EditorGUILayout.BeginHorizontal ();
                        subMesh.subMeshOverride.bUVRangeOverride = EditorGUILayout.Toggle (subMesh.subMeshOverride.bUVRangeOverride, GUILayout.Width (30));
                        GUI.enabled = subMesh.subMeshOverride.bUVRangeOverride;
                        subMesh.subMeshOverride.uvRange = EditorGUILayout.Vector4Field (CUVRange, subMesh.subMeshOverride.uvRange);
                        GUI.enabled = true;
                        if (GUILayout.Button ("Update", GUILayout.Width (60)))
                        {
                           subMesh.subMeshOverride.uvRange = ComputeMeshRange (msm, subMeshIdx);
                           EditorUtility.SetDirty (msm);
                        }
                        EditorGUILayout.EndHorizontal ();
                     }
                  }

                  if (msm.keywordSO.IsKeywordEnabled ("_MESHCOMBINED"))
                  {
                     using (new GUILayout.VerticalScope ("Combined Overrides", GUI.skin.box))
                     {
                        EditorGUILayout.Space ();
                        EditorGUILayout.Space ();
                        MicroSplatUtilities.DrawTextureField (msm, CStandardDiffuse, ref subMesh.combinedOverride.standardAlbedoOverride, "_MESHCOMBINED");
                        MicroSplatUtilities.DrawTextureField (msm, CStandardNormal, ref subMesh.combinedOverride.standardNormalOverride, "_MESHCOMBINED");
                        MicroSplatUtilities.DrawTextureField (msm, CStandardSmoothMetal, ref subMesh.combinedOverride.standardMetalSmoothOverride, "_MESHCOMBINEDSMOOTHMETAL");
                        MicroSplatUtilities.DrawTextureField (msm, CStandardHeight, ref subMesh.combinedOverride.standardHeightOverride, "_MESHCOMBINEDHEIGHT");
                        MicroSplatUtilities.DrawTextureField (msm, CStandardEmission, ref subMesh.combinedOverride.standardEmissionOverride, "_MESHCOMBINEDEMISSION");
                        MicroSplatUtilities.DrawTextureField (msm, CStandardOcclusion, ref subMesh.combinedOverride.standardOcclusionOverride, "_MESHCOMBINEDOCCLUSION");
                        MicroSplatUtilities.DrawTextureField (msm, CStandardPackedMap, ref subMesh.combinedOverride.standardPackedOverride, "_MESHCOMBINEDPACKEDMAP");
                        MicroSplatUtilities.DrawTextureField (msm, CStandardSpecular, ref subMesh.combinedOverride.standardSpecularOverride, "_USESPECULARWORKFLOW", "_MESHCOMBINEDSPECULARMAP");
                        MicroSplatUtilities.DrawTextureField (msm, CStandardSSS, ref subMesh.combinedOverride.standardSSS, "_MESHCOMBINEDSSSMAP");
#if __MICROSPLAT_STREAMS__
                        MicroSplatUtilities.DrawTextureField (msm, streamOverride, ref subMesh.subMeshOverride.streamTex, "_WETNESS", "_PUDDLES", "_STREAMS", "_LAVA", false);
#endif

                        EditorGUILayout.BeginHorizontal ();
                        subMesh.combinedOverride.bStandardColorOverride = EditorGUILayout.Toggle (subMesh.combinedOverride.bStandardColorOverride, GUILayout.Width (30));
                        GUI.enabled = subMesh.combinedOverride.bStandardColorOverride;
                        subMesh.combinedOverride.standardColorOverride = EditorGUILayout.ColorField ("Albedo Tint", subMesh.combinedOverride.standardColorOverride);
                        GUI.enabled = true;
                        EditorGUILayout.EndHorizontal ();

                        EditorGUILayout.BeginHorizontal ();
                        subMesh.combinedOverride.bStandardUVOverride = EditorGUILayout.Toggle (subMesh.combinedOverride.bStandardUVOverride, GUILayout.Width (30));
                        GUI.enabled = subMesh.combinedOverride.bStandardUVOverride;
                        subMesh.combinedOverride.standardUVOverride = EditorGUILayout.Vector4Field ("UV Scale/Offset", subMesh.combinedOverride.standardUVOverride);
                        GUI.enabled = true;
                        EditorGUILayout.EndHorizontal ();
                     }

                  }
                  using (new GUILayout.VerticalScope ("Splat Overrides", GUI.skin.box))
                  {
                     EditorGUILayout.Space ();
                     EditorGUILayout.Space ();
                     EditorGUILayout.BeginHorizontal ();
                     subMesh.subMeshOverride.bUVOverride = EditorGUILayout.Toggle (subMesh.subMeshOverride.bUVOverride, GUILayout.Width (30));
                     GUI.enabled = subMesh.subMeshOverride.bUVOverride;
                     subMesh.subMeshOverride.UVOverride = EditorGUILayout.Vector4Field ("Splat UV Scale override", subMesh.subMeshOverride.UVOverride);
                     GUI.enabled = true;
                     EditorGUILayout.EndHorizontal ();

                     if (msm.keywordSO.IsKeywordEnabled ("_TESSDISTANCE"))
                     {
                        EditorGUILayout.BeginHorizontal ();
                        msm.splatOverride.bDisplacementOverride = EditorGUILayout.Toggle (msm.splatOverride.bDisplacementOverride, GUILayout.Width (30));
                        GUI.enabled = msm.splatOverride.bDisplacementOverride;
                        msm.splatOverride.displacementOverride = EditorGUILayout.Slider ("Displacement override", msm.splatOverride.displacementOverride, 0, 3);
                        GUI.enabled = true;
                        EditorGUILayout.EndHorizontal ();
                     }
#if __MICROSPLAT_GLOBALTEXTURE__
                     MicroSplatUtilities.DrawTextureField (msm, geoTexOverride, ref msm.geoTextureOverride, "_GEOMAP");
                     MicroSplatUtilities.DrawTextureField (msm, geoTintOverride, ref msm.tintMapOverride, "_GLOBALTINT");
                     MicroSplatUtilities.DrawTextureField (msm, geoNormalOverride, ref msm.globalNormalOverride, "_GLOBALNORMALS");
                     MicroSplatUtilities.DrawTextureField (msm, geoSAOMOverride, ref msm.globalSAOMOverride, "_GLOBALSMOOTHAOMETAL");
                     MicroSplatUtilities.DrawTextureField (msm, geoEmisOverride, ref msm.globalEmisOverride, "_GLOBALEMIS");
#endif

#if __MICROSPLAT_SNOW__
                     MicroSplatUtilities.DrawTextureField (msm, snowMaskOverride, ref msm.snowMaskOverride, "_SNOWMASK");
#endif
                     // alpha hole override
#if __MICROSPLAT_ALPHAHOLE__
                     MicroSplatUtilities.DrawTextureField (msm, clipMapOverride, ref msm.clipMap, "_ALPHAHOLETEXTURE");
#endif

#if (VEGETATION_STUDIO || VEGETATION_STUDIO_PRO)
                  // vsstudio overrides
                  MicroSplatUtilities.DrawTextureField (msm, CVSGrassMap, ref msm.vsGrassMap, "_VSGRASSMAP");
                  MicroSplatUtilities.DrawTextureField (msm, CVSShadowMap, ref msm.vsShadowMap, "_VSSHADOWMAP");
#endif
#if __MICROSPLAT_PROCTEX__
                     MicroSplatUtilities.DrawTextureField (msm, biomeOverride, ref msm.procBiomeMask, "_PCBIOMEMASK");
                     MicroSplatUtilities.DrawTextureField (msm, biomeOverride2, ref msm.procBiomeMask2, "_PCBIOMEMASK2");
                     MicroSplatUtilities.DrawTextureField (msm, cavityMapOverride, ref msm.cavityMap, "_PROCEDURALTEXTURE");
#endif
                  }
                  EditorGUI.indentLevel--;

                  //MicroSplatUtilities.DrawTextureField (msm, perPixelNormal, ref msm.perPixelNormal, "_PERPIXELNORMAL");
                  if (EditorGUI.EndChangeCheck ())
                  {
                     needSync = true;
                  }
               }
            }
         }

         if (MicroSplatUtilities.DrawRollup ("Baking", false, true) && !msm.keywordSO.IsKeywordEnabled ("_DISABLESPLATMAPS"))
         {
            BakingGUI (msm);
         }
         MicroSplatCompressor.DrawGUI (msm, compressorOptions);
         if (MicroSplatUtilities.DrawRollup ("Debug", false, true))
         {
            EditorGUI.indentLevel += 2;
            using (new GUILayout.VerticalScope (GUI.skin.box))
            {
               EditorGUILayout.HelpBox ("These should not need to be edited unless something funky has happened. They are automatically managed by MicroSplat.", MessageType.Info);
               msm.propData = EditorGUILayout.ObjectField ("Per Texture Data", msm.propData, typeof (MicroSplatPropData), false) as MicroSplatPropData;
               msm.keywordSO = EditorGUILayout.ObjectField ("Keywords", msm.keywordSO, typeof (MicroSplatKeywords), false) as MicroSplatKeywords;
#if __MICROSPLAT_PROCTEX__
               msm.procTexCfg = EditorGUILayout.ObjectField ("Procedural Config", msm.procTexCfg, typeof (MicroSplatProceduralTextureConfig), false) as MicroSplatProceduralTextureConfig;
#endif

               //msm.blendMat = EditorGUILayout.ObjectField(CBlendMat, msm.blendMat, typeof(Material), false) as Material;
               //msm.perPixelNormal = EditorGUILayout.ObjectField("Normal Data", msm.perPixelNormal, typeof(Texture2D), false) as Texture2D;

               EditorGUILayout.LabelField ("Unique Materials tracked: " + MicroSplatMesh.GetRegistrySize ());
               if (GUILayout.Button ("Clear Material Cache"))
               {
                  MicroSplatMesh.ClearMaterialCache ();
               }
               EditorGUI.indentLevel -= 2;
            }
         }


         EditorGUILayout.BeginHorizontal ();
         if (GUILayout.Button ("Sync"))
         {
            var mgr = target as MicroSplatMesh;
            mgr.Sync ();
         }
         if (GUILayout.Button ("Sync All"))
         {
            MicroSplatMesh.SyncAll ();
         }
         EditorGUILayout.EndHorizontal ();


         if (EditorGUI.EndChangeCheck ())
         {
            EditorUtility.SetDirty (msm);
         }

      }
      if (needSync)
      {
         msm.Sync ();
      }
   }
}

#else
   [CustomEditor (typeof(MicroSplatMesh))]
public class MicroSplatMeshEditor : Editor
{
   public override void OnInspectorGUI()
   {
      EditorGUILayout.HelpBox ("MicroSplat is not installed, and must be installed to use this feature.", MessageType.Error);
      if (GUILayout.Button ("Get MicroSplat"))
      {
         Application.OpenURL("https://assetstore.unity.com/packages/tools/terrain/microsplat-96478?aid=1011l37NJ&utm_source=aff");
      }
   }
}
#endif

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

[CustomEditor (typeof (MicroSplatVertexMesh))]
[CanEditMultipleObjects]
public class MicroSplatVertexMeshEditor : Editor
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
   static GUIContent geoSAOMOverride = new GUIContent ("Global SOAM Override", "If you want each terrain to have it's own Smoothness(R), AO(G) and Metallic (B) map instead of the one defined in the material, add it here");
   static GUIContent geoEmisOverride = new GUIContent ("Global Emissive Override", "If you want each terrain to have it's own Emissive map instead of the one defined in the material, set it here");
#endif
#if __MICROSPLAT_SNOW__
   static GUIContent snowMaskOverride = new GUIContent ("Snow Mask Override", "Allows you to override the snow mask on this pbject");
#endif

#if __MICROSPLAT_ALPHAHOLE__
   static GUIContent clipMapOverride = new GUIContent ("Clip Map Override", "Provide a unique clip map for each object");
#endif

#if __MICROSPLAT_PROCTEX__
   static GUIContent biomeOverride = new GUIContent ("Biome Map Override", "Biome map for this mesh");
   static GUIContent biomeOverride2 = new GUIContent ("Biome Map Override", "Biome map for this mesh");
   static GUIContent CCavityMap = new GUIContent ("Cavity Map Override", "Cavity map for this mesh");
#endif


#if (VEGETATION_STUDIO || VEGETATION_STUDIO_PRO)
   static GUIContent CVSGrassMap = new GUIContent("Grass Map", "Grass Map from Vegetation Studio");
   static GUIContent CVSShadowMap = new GUIContent("Shadow Map", "Shadow map texture from Vegetation Studio");
#endif

   public string lastPath;


   public bool createTextureArray = true;
   public MicroSplatMeshModule.ShaderType meshSetupType = MicroSplatMeshModule.ShaderType.Combined;
   static GUIContent CMeshSetupType = new GUIContent ("Shader Type", "Do you want a regular splat map shader, or one which is applied over the existing texturing?");
   static GUIContent CCreateTextureArray = new GUIContent ("Create Texture Array Config", "Create a Texture Array Config to generate texture arrays from");


   public bool DoSetupGUI ()
   {
      if (lastPath == null)
      {
         lastPath = Application.dataPath;
      }

      MicroSplatVertexMesh msm = target as MicroSplatVertexMesh;
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
               msm.subMeshEntries = new List<MicroSplatVertexMesh.SubMeshEntry> ();
               MicroSplatVertexMesh.SubMeshEntry e = new MicroSplatVertexMesh.SubMeshEntry ();
               e.subMeshOverride = new MicroSplatVertexMesh.SubMeshOverride ();
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
               MicroSplatVertexMesh.SubMeshEntry e = new MicroSplatVertexMesh.SubMeshEntry ();
               e.subMeshOverride = new MicroSplatVertexMesh.SubMeshOverride ();
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
                  keywords.Add (MicroSplatBaseFeatures.DefineFeature._MICROVERTEXMESH.ToString ());
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

      // setup mesh overlay material
      if (msm.keywordSO != null && msm.keywordSO.IsKeywordEnabled ("_MESHOVERLAYSPLATS"))
      {
         var path = AssetDatabase.GetAssetPath (msm.templateMaterial);
         path = path.Replace (".mat", "_MeshOverlay.shader");
         if (msm.keywordSO.IsKeywordEnabled("_MSRENDERLOOP_BETTERSHADERS"))
         {
            path = path.Replace(".shader", ".surfshader");
         }
         Shader shader = AssetDatabase.LoadAssetAtPath<Shader> (path);
         if (msm.templateMaterial != shader && shader != null)
         {
            msm.templateMaterial.shader = shader;
            MicroSplatVertexMesh.SyncAll ();
         }
      }


      if (msm.templateMaterial != null && msm.propData == null)
      {
         msm.propData = MicroSplatShaderGUI.FindOrCreatePropTex (msm.templateMaterial);
      }

      return true;
   }

   public override void OnInspectorGUI ()
   {
      bool needSync = false;
      MicroSplatVertexMesh msm = target as MicroSplatVertexMesh;
      EditorGUI.BeginChangeCheck ();
      msm.templateMaterial = EditorGUILayout.ObjectField (CTemplateMaterial, msm.templateMaterial, typeof (Material), false) as Material;
      if (EditorGUI.EndChangeCheck())
      {
         msm.Sync ();
         EditorUtility.SetDirty (msm.gameObject);
      }

      EditorGUI.BeginChangeCheck ();

      if (DoSetupGUI ())
      {
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
                     MicroSplatUtilities.DrawTextureField (msm, CCavityMap, ref msm.cavityMap, "_PROCEDURALTEXTURE");
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
               //msm.terrainDesc = EditorGUILayout.ObjectField("Terrain Descriptor", msm.terrainDesc, typeof(Texture2D), false) as Texture2D;
               //msm.perPixelNormal = EditorGUILayout.ObjectField("Normal Data", msm.perPixelNormal, typeof(Texture2D), false) as Texture2D;

               EditorGUILayout.LabelField ("Unique Materials tracked: " + MicroSplatVertexMesh.GetRegistrySize ());
               if (GUILayout.Button ("Clear Material Cache"))
               {
                  MicroSplatVertexMesh.ClearMaterialCache ();
               }
               EditorGUI.indentLevel -= 2;
            }
         }


         EditorGUILayout.BeginHorizontal ();
         if (GUILayout.Button ("Sync"))
         {
            msm.Sync ();
            EditorUtility.SetDirty (msm);
         }
         if (GUILayout.Button ("Sync All"))
         {
            MicroSplatVertexMesh.SyncAll ();
            EditorUtility.SetDirty (msm);
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
   [CustomEditor (typeof(MicroSplatVertexMesh))]
public class MicroSplatVertexMeshEditor : Editor
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

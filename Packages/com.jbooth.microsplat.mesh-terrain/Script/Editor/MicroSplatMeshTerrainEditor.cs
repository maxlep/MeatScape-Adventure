//////////////////////////////////////////////////////
// MicroSplat
// Copyright (c) Jason Booth
//////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using JBooth.MicroSplat;

[CustomEditor(typeof(MicroSplatMeshTerrain))]
[CanEditMultipleObjects]
public partial class MicroSplatMeshTerrainEditor : Editor
{
#if __MICROSPLAT__


#if __MICROSPLAT_ALPHAHOLE__
   static GUIContent clipMapOverride = new GUIContent("Clip Map Override", "Provide a unique clip map for each terrain");
#endif
   static GUIContent CTemplateMaterial = new GUIContent("Template Material", "Material to use for this terrain");
   static GUIContent CPerPixelNormal = new GUIContent ("Per Pixel Normal", "Normal map representing the terrain normals");

#if __MICROSPLAT_GLOBALTEXTURE__
   static GUIContent geoTexOverride = new GUIContent ("Geo Texture Override", "If you want each terrain object to have it's own geo texture instead of the one defined in the material, add it here");
   static GUIContent geoTintOverride = new GUIContent ("Tint Texture Override", "If you want each terrain object to have it's own global tint instead of the one defined in the material, add it here");
   static GUIContent geoNormalOverride = new GUIContent ("Global Normal Override", "If you want each terrain object to have it's own global normal instead of the one defined in the material, add it here");
   static GUIContent geoSAOMOverride = new GUIContent ("Global SOAM Override", "If you want each terrain to have it's own Smoothness(R), AO(G) and Metallic (B) map instead of the one defined in the material, add it here");
   static GUIContent geoEmisOverride = new GUIContent ("Global Emissive Override", "If you want each terrain to have it's own Emissive map instead of the one defined in the material, set it here");
#endif


#if __MICROSPLAT_SCATTER__
   static GUIContent scatterMapOverride = new GUIContent ("Scatter Map Override", "Scatter control map");
#endif

#if __MICROSPLAT_SNOW__
   static GUIContent snowMaskOverride = new GUIContent ("Snow Mask Override", "If you want a unique snow mask on this terrain");
#endif

#if __MICROSPLAT_PROCTEX__
   static GUIContent biomeOverride = new GUIContent("Biome Map Override", "Biome map for this terrain");
   static GUIContent biomeOverride2 = new GUIContent ("Biome Map2 Override", "Biome map for this terrain");
   static GUIContent CCavityMap = new GUIContent ("Cavity Map", "Cavity Map for procedural texturing based on flow and cavity");
#endif

#if (VEGETATION_STUDIO || VEGETATION_STUDIO_PRO)
   static GUIContent CVSGrassMap = new GUIContent("Grass Map", "Grass Map from Vegetation Studio");
   static GUIContent CVSShadowMap = new GUIContent("Shadow Map", "Shadow map texture from Vegetation Studio");
#endif 
   static GUIContent CBlendMat = new GUIContent("Blend Mat", "Blending material for terrain blending");

   public string lastPath;


   public MicroSplatCompressor.Options compressorOptions = new MicroSplatCompressor.Options ();

   public override void OnInspectorGUI()
   {
      MicroSplatMeshTerrain msm = target as MicroSplatMeshTerrain;
      EditorGUI.BeginChangeCheck();
      msm.templateMaterial = EditorGUILayout.ObjectField(CTemplateMaterial, msm.templateMaterial, typeof(Material), false) as Material;

      if (msm.propData == null)
      {
         msm.propData = MicroSplatShaderGUI.FindOrCreatePropTex(msm.templateMaterial);
         EditorUtility.SetDirty(msm);
      }

      if (msm.keywordSO == null)
      {
         msm.keywordSO = MicroSplatUtilities.FindOrCreateKeywords(msm.templateMaterial);
         EditorUtility.SetDirty(msm);
      }
      if (EditorGUI.EndChangeCheck())
      {
         EditorUtility.SetDirty(msm);
      }
      if (lastPath == null)
      {
         lastPath = Application.dataPath;
      }


      if (msm.templateMaterial != null && msm.propData == null)
      {
         msm.propData = MicroSplatShaderGUI.FindOrCreatePropTex(msm.templateMaterial);
      }

      EditorGUILayout.PropertyField(serializedObject.FindProperty("meshTerrains"), true);

      EditorGUILayout.PropertyField(serializedObject.FindProperty("controlTextures"), true);
      serializedObject.ApplyModifiedProperties();


#if __MICROSPLAT_PROCTEX__
      if (msm.keywordSO != null && (msm.keywordSO.IsKeywordEnabled("_PROCEDURALTEXTURE") || msm.keywordSO.IsKeywordEnabled("_PCHEIGHTGRADIENT") || msm.keywordSO.IsKeywordEnabled("_PCHEIGHTHSV")))
      {
         var old = msm.procTexCfg;
         msm.procTexCfg = MicroSplatProceduralTexture.FindOrCreateProceduralConfig(msm.templateMaterial);
         if (old != msm.procTexCfg)
         {
            EditorUtility.SetDirty(msm);
         }
      }
#endif

#if __MICROSPLAT_TERRAINBLEND__ || __MICROSPLAT_STREAMS__
      DoTerrainDescGUI();
#endif
      EditorGUI.BeginChangeCheck ();

      MicroSplatUtilities.DrawTextureField (msm, CPerPixelNormal, ref msm.perPixelNormal, "_PERPIXNORMAL");

#if __MICROSPLAT_GLOBALTEXTURE__
      MicroSplatUtilities.DrawTextureField (msm, geoTexOverride, ref msm.geoTextureOverride, "_GEOMAP");
      MicroSplatUtilities.DrawTextureField (msm, geoTintOverride, ref msm.tintMapOverride, "_GLOBALTINT");
      MicroSplatUtilities.DrawTextureField (msm, geoNormalOverride, ref msm.globalNormalOverride, "_GLOBALNORMALS");
      MicroSplatUtilities.DrawTextureField (msm, geoSAOMOverride, ref msm.globalSAOMOverride, "_GLOBALSMOOTHAOMETAL");
      MicroSplatUtilities.DrawTextureField (msm, geoEmisOverride, ref msm.globalEmisOverride, "_GLOBALEMIS");
#endif

#if __MICROSPLAT_SCATTER__
      MicroSplatUtilities.DrawTextureField (msm, scatterMapOverride, ref msm.scatterMapOverride, "_SCATTER");
#endif

#if __MICROSPLAT_SNOW__
      MicroSplatUtilities.DrawTextureField (msm, snowMaskOverride, ref msm.snowMaskOverride, "_SNOWMASK");
#endif

#if __MICROSPLAT_ALPHAHOLE__
      MicroSplatUtilities.DrawTextureField (msm, clipMapOverride, ref msm.clipMap, "_ALPHAHOLETEXTURE");
#endif
      
#if __MICROSPLAT_PROCTEX__
      MicroSplatUtilities.DrawTextureField(msm, biomeOverride, ref msm.procBiomeMask, "_PCBIOMEMASK");
      MicroSplatUtilities.DrawTextureField (msm, biomeOverride2, ref msm.procBiomeMask2, "_PCBIOMEMASK2");
      MicroSplatUtilities.DrawTextureField(msm, CCavityMap, ref msm.cavityMap, "_PROCEDURALTEXTURE");
#endif


      if (msm.propData == null && msm.templateMaterial != null)
      {
         msm.propData = MicroSplatShaderGUI.FindOrCreatePropTex (msm.templateMaterial);
         if (msm.propData == null)
         {
            // this should really never happen, but users seem to have issues with unassigned propData's a lot. I think
            // this is from external tools like MapMagic not creating it, but the above call should create it.
            EditorGUILayout.HelpBox ("PropData is null, please assign", MessageType.Error);
            msm.propData = EditorGUILayout.ObjectField("Per Texture Data", msm.propData, typeof(MicroSplatPropData), false) as MicroSplatPropData;
         }
      }

#if (VEGETATION_STUDIO || VEGETATION_STUDIO_PRO)
      if (msm.keywordSO.IsKeywordEnabled("_VSGRASSMAP"))
      {
         EditorGUI.BeginChangeCheck();

         msm.vsGrassMap = EditorGUILayout.ObjectField(CVSGrassMap, msm.vsGrassMap, typeof(Texture2D), false) as Texture2D;

         if (EditorGUI.EndChangeCheck())
         {
            EditorUtility.SetDirty(msm);
         }
      }

      if (msm.keywordSO.IsKeywordEnabled("_VSSHADOWMAP"))
      {
         EditorGUI.BeginChangeCheck();


         msm.vsShadowMap = EditorGUILayout.ObjectField(CVSShadowMap, msm.vsShadowMap, typeof(Texture2D), false) as Texture2D;

         if (EditorGUI.EndChangeCheck())
         {
            EditorUtility.SetDirty(msm);
         }
      }
#endif

      MicroSplatCompressor.DrawGUI (msm, compressorOptions);

      if (MicroSplatUtilities.DrawRollup("Debug", false, true))
      {
         EditorGUI.indentLevel += 2;
         EditorGUILayout.HelpBox("These should not need to be edited unless something funky has happened. They are automatically managed by MicroSplat.", MessageType.Info);
         msm.propData = EditorGUILayout.ObjectField("Per Texture Data", msm.propData, typeof(MicroSplatPropData), false) as MicroSplatPropData;
         msm.keywordSO = EditorGUILayout.ObjectField("Keywords", msm.keywordSO, typeof(MicroSplatKeywords), false) as MicroSplatKeywords;
         msm.blendMat = EditorGUILayout.ObjectField(CBlendMat, msm.blendMat, typeof(Material), false) as Material;
         EditorGUI.indentLevel -= 2;
      }

      if (EditorGUI.EndChangeCheck ())
      {
         MicroSplatMeshTerrain.SyncAll();
      }


      EditorGUILayout.BeginHorizontal();
      if (GUILayout.Button("Sync"))
      {
         var mgr = target as MicroSplatMeshTerrain;
         mgr.Sync();
      }
      if (GUILayout.Button("Sync All"))
      {
         MicroSplatMeshTerrain.SyncAll();
      }
      EditorGUILayout.EndHorizontal();

   }
#endif
}

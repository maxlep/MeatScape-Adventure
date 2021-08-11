//////////////////////////////////////////////////////
// MicroSplat
// Copyright (c) Jason Booth
//////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using JBooth.MicroSplat;

#if __MICROSPLAT__
public partial class MicroSplatMeshTerrainEditor : Editor 
{

   public void DoTerrainDescGUI()
   {
      MicroSplatMeshTerrain bt = target as MicroSplatMeshTerrain;
      
      if (bt.blendMat == null && bt.templateMaterial != null && bt.keywordSO != null && bt.keywordSO.IsKeywordEnabled("_TERRAINBLENDING"))
      {
         var path = AssetDatabase.GetAssetPath(bt.templateMaterial);
         path = path.Replace(".mat", "_TerrainObjectBlend.mat");
         bt.blendMat = AssetDatabase.LoadAssetAtPath<Material>(path);
         if (bt.blendMat == null)
         {
            string shaderPath = path.Replace(".mat", ".shader");
            Shader shader = AssetDatabase.LoadAssetAtPath<Shader>(shaderPath);
			   if (shader == null) 
			   {
				   shaderPath = AssetDatabase.GetAssetPath(bt.templateMaterial.shader);
				   shaderPath = shaderPath.Replace(".shader", "_TerrainObjectBlend.shader");
				   shader = AssetDatabase.LoadAssetAtPath<Shader>(shaderPath);
			   }
            if (shader != null)
            {
               Material mat = new Material(shader);
               AssetDatabase.CreateAsset(mat, path);
               AssetDatabase.SaveAssets();
               MicroSplatMeshTerrain.SyncAll();
            }
         }
      }
 
      bt.terrainDescriptor.heightMap = (Texture2D)EditorGUILayout.ObjectField ("Height Map", bt.terrainDescriptor.heightMap, typeof (Texture2D), false);
   }


}
#endif

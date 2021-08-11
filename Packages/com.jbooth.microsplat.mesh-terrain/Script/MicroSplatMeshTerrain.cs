//////////////////////////////////////////////////////
// MicroSplat
// Copyright (c) Jason Booth
//////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using JBooth.MicroSplat;

[ExecuteInEditMode]
[DisallowMultipleComponent]
public class MicroSplatMeshTerrain : MicroSplatObject 
{
   public delegate void MaterialSyncAll();
   public delegate void MaterialSync(Material m);

   public static event MaterialSyncAll OnMaterialSyncAll;
   public event MaterialSync OnMaterialSync;

   static List<MicroSplatMeshTerrain> sInstances = new List<MicroSplatMeshTerrain>();

   public MeshRenderer[] meshTerrains;

   public Texture2D[] controlTextures;
   [HideInInspector]
   public Material meshBlendMat;

   [HideInInspector]
   public Material meshBlendMatInstance;

   public TerrainDescriptor terrainDescriptor;


   void Awake()
   {
      #if UNITY_EDITOR
      Sync();
      #endif
   }

   void OnEnable()
   {
      sInstances.Add(this);
      #if UNITY_EDITOR
      Sync();
      #endif
   }

   public override TerrainDescriptor GetTerrainDescriptor ()
   {
      if (perPixelNormal != null)
      {
         terrainDescriptor.normalMap = perPixelNormal;
      }
      return terrainDescriptor;
   }

#if !UNITY_EDITOR
   void Start()
   {
      Sync();
   }
#endif

   void OnDisable()
   {
      sInstances.Remove(this);
      Cleanup();
   }



   void Cleanup()
   {
      if (matInstance != null && matInstance != templateMaterial)
      {
         DestroyImmediate(matInstance);
      }
   }
      


   void SyncMeshBlendMat()
   {
      if (meshBlendMatInstance != null && matInstance != null)
      {
         meshBlendMatInstance.CopyPropertiesFromMaterial(matInstance);
      }
   }

   Material GetMeshBlendMatInstance()
   {
      if (meshBlendMat != null)
      {
         if (meshBlendMatInstance == null)
         {
            meshBlendMatInstance = new Material(meshBlendMat);
            SyncMeshBlendMat();
         }
         if (meshBlendMatInstance.shader != meshBlendMat.shader)
         {
            meshBlendMatInstance.shader = meshBlendMat.shader;
            SyncMeshBlendMat();
         }
      }
      return meshBlendMatInstance;
   }

   void ApplyMeshBlendMap()
   {
      if (meshBlendMat != null)
      {
         if (meshBlendMatInstance == null)
         {
            meshBlendMatInstance = new Material(meshBlendMat);
         }

         SyncMeshBlendMat();
      }
   }

   public void Sync()
   {
      if (templateMaterial == null)
         return;

      if (keywordSO == null)
      {
         RevisionFromMat();
      }
      if (keywordSO == null)
         return;

      if (meshTerrains == null || meshTerrains.Length == 0)
         return;

      ApplySharedData (templateMaterial);

      if (matInstance == null)
      {
         matInstance = new Material (templateMaterial);
      }
      matInstance.CopyPropertiesFromMaterial (templateMaterial);
      matInstance.hideFlags = HideFlags.HideAndDontSave;
      ApplyMaps(matInstance);

		if (controlTextures != null && controlTextures.Length > 0)
      {
         ApplyControlTextures(controlTextures, matInstance);
      }


      for (int i = 0; i < meshTerrains.Length; ++i)
      {
         var rend = meshTerrains [i];
         if (rend == null)
            continue;
         rend.sharedMaterial = matInstance;
      }
         
      if (OnMaterialSync != null)
      {
         OnMaterialSync(matInstance);
      }

      ApplyBlendMap();
      ApplyMeshBlendMap();

   }

   public override Bounds GetBounds() 
   { 
      Bounds b = new Bounds();
      bool s = false;
      for (int i = 0; i < meshTerrains.Length; ++i)
      {
         var rend = meshTerrains [i];
         if (rend == null)
            continue;
         if (!s)
         {
            b = rend.bounds;
            s = true;
         }
         else
         {
            b.Encapsulate (rend.bounds);
         }
      }
      return b;
   }

   public static new void SyncAll()
   {
      for (int i = 0; i < sInstances.Count; ++i)
      {
         sInstances[i].Sync();
      }
      if (OnMaterialSyncAll != null)
      {
         OnMaterialSyncAll();
      }
   }


}

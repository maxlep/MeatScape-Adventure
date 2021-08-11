//////////////////////////////////////////////////////
// MicroSplat
// Copyright (c) Jason Booth
//////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if __MICROSPLAT__

using JBooth.MicroSplat;

[ExecuteInEditMode]
[DisallowMultipleComponent]
public class MicroSplatMesh : MicroSplatObject 
{
   public delegate void MaterialSyncAll();
   public delegate void MaterialSync(Material m);

   public static event MaterialSyncAll OnMaterialSyncAll;
   public event MaterialSync OnMaterialSync;

   static List<MicroSplatMesh> sInstances = new List<MicroSplatMesh>();

   [HideInInspector]
   public MeshRenderer rend;


   // this is for things per submesh
   [System.Serializable]
   public class SubMeshOverride
   {
      public string materialName;
#if UNITY_EDITOR
      public int createTextureSizeX = 256;
      public int createTextureSizeY = 256;
#endif
      public bool active;
      public bool bUVOverride = false;
      public Vector4 UVOverride = new Vector4 (1, 1, 0, 0);
      public Texture2D [] controlTextures = new Texture2D [0];
      public Texture2D displacementDampening;
      public Texture2D streamTex;
      public Texture2D tint;
      public bool bUVRangeOverride = false;
      public Vector4 uvRange = new Vector4 (1, 1, 0.5f, 0.5f);



      public long GetHash ()
      {
         long h = 3;
         unchecked
         {
            h = h + ((displacementDampening == null) ? 3 : displacementDampening.GetNativeTexturePtr ().ToInt64 ()) * 3;
            h = h + ((streamTex == null) ? 13 : streamTex.GetNativeTexturePtr ().ToInt64 ()) * 13;
            h = h + ((tint == null) ? 7 : tint.GetNativeTexturePtr().ToInt64()) * 7;
            if (bUVOverride)
            {
               h = h + UVOverride.GetHashCode () * 11;
            }
            h = h + (int)(uvRange.x * 1000 + uvRange.y * 1000 + uvRange.z * 1000 + uvRange.w * 1000);
            if (controlTextures != null)
            {
               for (int i = 0; i < controlTextures.Length; ++i)
               {
                  h = h * (controlTextures [i] == null ? 7 * (i + 1) : controlTextures [i].GetNativeTexturePtr ().ToInt64 () + i);
               }
            }
         }
         if (h == 0)
         {
            Debug.Log ("Submesh override hash returned 0, this should not happen");
         }
         return h;
      }
   }

   // this is for things everything has..
   [System.Serializable]
   public class SplatOverride
   {
      public bool bDisplacementOverride = false;
      public float displacementOverride = 1;
      
      public Vector4 subArray = new Vector4 (0, 1, 2, 3);

      public long GetHash ()
      {
         long h = 3;
         unchecked
         {
            h = h + subArray.GetHashCode () * 13;
            h = h + (bDisplacementOverride ? 5 : 17);
            h = h + (int)(displacementOverride * 1000) * 21;
         }
         if (h == 0)
         {
            Debug.Log ("Splat override hash returned 0, this should not happen");
         }
         return h;
      }
   }

   public SplatOverride splatOverride = new SplatOverride ();

   [System.Serializable]
   public class SubMeshEntry
   {
      public CombinedOverride combinedOverride = new CombinedOverride ();
      public SubMeshOverride subMeshOverride = new SubMeshOverride ();
      public Material matInstance = null;
      public long oldKey = 0;

      public long GetHash()
      {
         unchecked
         {
            return combinedOverride.GetHash () * 7 + subMeshOverride.GetHash () * 3;
         }
      }
   }

   public List<SubMeshEntry> subMeshEntries = new List<SubMeshEntry> ();


   public struct MaterialInstranceEntry
   {
      public int refCount;
      public Material matInstance;
   }

   static Dictionary<long, MaterialInstranceEntry> materialRegistry = new Dictionary<long, MaterialInstranceEntry> ();

   public static int GetRegistrySize() { return materialRegistry.Count; }
   public static void ClearMaterialCache()
   {
      foreach (var k in materialRegistry)
      {
         if (k.Value.matInstance != null)
         {
            GameObject.DestroyImmediate (k.Value.matInstance);
         }
      }
      materialRegistry.Clear ();
      MicroSplatMesh.SyncAll ();
   }

   

   long GetMaterialInstanceHash(int subMesh)
   {
      if (templateMaterial == null)
         return 0;
      long h = 3;
      unchecked
      {
         h = h + templateMaterial.GetInstanceID ();
         h = h + templateMaterial.shader.GetInstanceID ();
         if (subMesh < subMeshEntries.Count)
         {
            h = h + subMeshEntries [subMesh].GetHash ();
         }

         h = h + splatOverride.GetHash ();
         h = h + GetOverrideHash ();

         if (h == 0)
         {
            Debug.LogError ("Material instance hash is 0");
         }
      }
      return h;
   }

   void Cleanup (long key, int subMesh)
   {
      if (subMesh >= subMeshEntries.Count)
      {
         return;
      }

      if (key == 0)
         return;

      MaterialInstranceEntry e;
      if (materialRegistry.TryGetValue (key, out e))
      {
         e.refCount--;
         //Debug.Log ("Releasing matInstance " + e.refCount + " " + e.matInstance + " " + key);

         subMeshEntries [subMesh].matInstance = null;
         if (e.refCount < 0)
         {
            Debug.LogError ("Reference count < 0, something is broken");
         }
         if (e.refCount == 0)
         {
            if (e.matInstance != null)
            {
               //Debug.Log ("Destroying mat instance " + e.matInstance.name + " " + key);
               DestroyImmediate (e.matInstance);
               e.matInstance = null;
            }
            materialRegistry.Remove (key);
         }
         else
         {
            materialRegistry [key] = e; // struct, so have to put it back
         }
      }
      subMeshEntries [subMesh].oldKey = 0;
   }

   void Cleanup ()
   {
      for (int i = 0; i < subMeshEntries.Count; ++i)
      {
         Cleanup (subMeshEntries [i].oldKey, i);
      }
   }

   Material GetMaterialInstance(int subMesh)
   {
      long key = GetMaterialInstanceHash (subMesh);
      if (key == 0)
      {
         Debug.LogError ("0 key found, check hashing functions");
      }
      if (subMesh >= subMeshEntries.Count)
      {
         Debug.LogError ("SubMesh out of range");
      }
      var sub = subMeshEntries [subMesh];

      if (sub.oldKey != key)
      {
         Cleanup (sub.oldKey, subMesh);
      }
      
      sub.oldKey = key;
      MaterialInstranceEntry e;
      if (materialRegistry.TryGetValue (key, out e))
      {
         e.refCount++;
         //Debug.Log ("++refCount : " + e.refCount + "  " + e.matInstance.name + " " + key);
         if (e.matInstance == null)
         {
            e.matInstance = new Material (templateMaterial);
         }
         materialRegistry [key] = e; // struct, so have to assign back..
      }
      else
      {
         e = new MaterialInstranceEntry ();
         e.matInstance = new Material (templateMaterial); 
         e.refCount = 1;
         materialRegistry.Add (key, e);
         //Debug.Log ("Creating entry : " + e.matInstance.name + " " + key);
      }
      sub.matInstance = e.matInstance;
      return e.matInstance;
   }

   void Awake()
   {
      rend = GetComponent<MeshRenderer>();
   }

   void OnEnable()
   {
      sInstances.Add(this);
      Sync();
   }

   void OnDisable()
   {
      sInstances.Remove(this);
      Cleanup();
   }

   

   public void Sync()
   {
      if (templateMaterial == null)
         return;

      if (this == null) // object is deleted
         return;

      if (keywordSO == null)
      {
         RevisionFromMat ();
      }
      if (keywordSO == null)
         return;

      if (rend == null)
      {
         rend = GetComponent<MeshRenderer> ();
      }
      if (rend == null)
      {
         Debug.LogError ("No renderer found on MicroSplatMesh component's game object, cannot sync");
         return;
      }

      if (keywordSO.IsKeywordEnabled ("_MESHOVERLAYSPLATS") == false && rend.sharedMaterials.Length != subMeshEntries.Count)
      {
         var shared = rend.sharedMaterials;
         System.Array.Resize (ref shared, subMeshEntries.Count);
         rend.sharedMaterials = shared;
      }

      ApplySharedData (templateMaterial);

      for (int subMeshIdx = 0; subMeshIdx < subMeshEntries.Count; ++subMeshIdx)
      {
         var subMesh = subMeshEntries [subMeshIdx];
         if (subMesh.subMeshOverride.active == false)
            continue;

         Material m = GetMaterialInstance (subMeshIdx);

         if (m == null)
         {
            m = GetMaterialInstance (subMeshIdx);
         }
         matInstance = m;
         // copy latest, as it may have changed as we don't hash the material data
         m.CopyPropertiesFromMaterial (templateMaterial);

         if (keywordSO.IsKeywordEnabled ("_MESHOVERLAYSPLATS"))
         {
            var materials = rend.sharedMaterials;
            bool hasBlendMat = false;
            // since out instance is hide and don't save, we accept the first null
            // as a slot if our shader isn't found in use already.
            int nullIndex = -1;
            for (int i = 0; i < materials.Length; ++i)
            {
               if (materials [i] == null)
               {
                  nullIndex = i;
               }
               else if (materials [i].shader == matInstance.shader)
               {
                  materials [i] = matInstance;
                  hasBlendMat = true;
               }
            }
            if (!hasBlendMat)
            {
               if (nullIndex > -1)
               {
                  materials [nullIndex] = matInstance;
               }
               else
               {
                  System.Array.Resize<Material> (ref materials, materials.Length + 1);
                  materials [materials.Length - 1] = matInstance;
               }
               rend.sharedMaterials = materials;
            }
         }
         else
         {
            var shared = rend.sharedMaterials;
            shared[subMeshIdx] = m;
            rend.sharedMaterials = shared;
         }

         if (keywordSO.IsKeywordEnabled ("_MESHSUBARRAY"))
         {
            m.SetVector ("_MeshSubArrayIndexes", splatOverride.subArray);
         }

         m.hideFlags = HideFlags.HideAndDontSave;

         if (subMesh.subMeshOverride.bUVRangeOverride)
         {
            m.SetVector("_UVMeshRange", subMesh.subMeshOverride.uvRange);
         }

         ApplyMaps (m);

         if (keywordSO.IsKeywordEnabled ("_MESHCOMBINED"))
         {
            SetMap (m, "_StandardDiffuse", subMesh.combinedOverride.standardAlbedoOverride);
            SetMap (m, "_StandardNormal", subMesh.combinedOverride.standardNormalOverride);
            SetMap (m, "_StandardSmoothMetal", subMesh.combinedOverride.standardMetalSmoothOverride);
            SetMap (m, "_StandardHeight", subMesh.combinedOverride.standardHeightOverride);
            SetMap (m, "_StandardEmission", subMesh.combinedOverride.standardEmissionOverride);
            SetMap (m, "_StandardOcclusion", subMesh.combinedOverride.standardOcclusionOverride);
            SetMap (m, "_StandardPackedMap", subMesh.combinedOverride.standardPackedOverride);
            SetMap (m, "_StandardSSS", subMesh.combinedOverride.standardSSS);
            if (subMesh.combinedOverride.bStandardColorOverride && m.HasProperty ("_StandardDiffuseTint"))
            {
               m.SetColor ("_StandardDiffuseTint", subMesh.combinedOverride.standardColorOverride);
            }
            if (subMesh.combinedOverride.bStandardUVOverride && m.HasProperty ("_StandardUVScaleOffset"))
            {
               m.SetVector ("_StandardUVScaleOffset", subMesh.combinedOverride.standardUVOverride);
            }
            
         }

         if (subMesh.subMeshOverride.bUVOverride && m.HasProperty ("_UVScale"))
         {
            m.SetVector ("_UVScale", subMesh.subMeshOverride.UVOverride);
         }
         if (subMesh.subMeshOverride.bUVOverride && m.HasProperty ("_TriplanarUVScale"))
         {
            m.SetVector ("_TriplanarUVScale", subMesh.subMeshOverride.UVOverride);
         }



         if (splatOverride.bDisplacementOverride && m.HasProperty ("_TessData1"))
         {
            var v = m.GetVector ("_TessData1");
            v.y = splatOverride.displacementOverride;
            m.SetVector ("_TessData1", v);
         }

         if (subMesh.subMeshOverride.controlTextures != null && subMesh.subMeshOverride.controlTextures.Length > 0 && !keywordSO.IsKeywordEnabled ("_DISABLESPLATMAPS"))
         {
            ApplyControlTextures (subMesh.subMeshOverride.controlTextures, m);
         }

         SetMap (m, "_DisplacementDampening", subMesh.subMeshOverride.displacementDampening);
         SetMap (m, "_GlobalTintTex", subMesh.subMeshOverride.tint);
         SetMap (m, "_StreamControl", subMesh.subMeshOverride.streamTex);


#if __MICROSPLAT_TERRAINBLEND__
         // if we have a terrain blendable component on us, we need to sync it..
         var tb = GetComponent<MicroSplatBlendableObject> ();
         if (tb != null)
         {
            tb.Sync ();
         }
#endif

         if (OnMaterialSync != null)
         {
            OnMaterialSync (m);
         }
      }
   }

   public override Bounds GetBounds() { return rend.bounds; }
      
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

#else
public class MicroSplatMesh : MonoBehaviour
{
}
#endif

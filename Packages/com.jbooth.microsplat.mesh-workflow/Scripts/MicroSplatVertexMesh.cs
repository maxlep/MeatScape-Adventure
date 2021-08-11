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
public class MicroSplatVertexMesh : MicroSplatObject
{
   public delegate void MaterialSyncAll ();
   public delegate void MaterialSync (Material m);

   public static event MaterialSyncAll OnMaterialSyncAll;
   public event MaterialSync OnMaterialSync;

   static List<MicroSplatVertexMesh> sInstances = new List<MicroSplatVertexMesh> ();

   [HideInInspector]
   public MeshRenderer rend;


   // this is for things per submesh
   [System.Serializable]
   public class SubMeshOverride
   {
      public string materialName;
      public bool active;
      public bool bUVOverride = false;
      public Vector4 UVOverride = new Vector4 (1, 1, 0, 0);

      public long GetHash ()
      {
         long h = 3;
         unchecked
         {
            if (bUVOverride)
            {
               h = h + UVOverride.GetHashCode () * 3;
            }
         }
         if (h == 0)
         {
            Debug.Log ("Submesh override hash returned 0, this should not happen");
         }
         return h;
      }
   }


   [System.Serializable]
   public class SubMeshEntry
   {
      public CombinedOverride combinedOverride = new CombinedOverride ();
      public SubMeshOverride subMeshOverride = new SubMeshOverride ();
      public Material matInstance = null;
      public long oldKey = 0;

      public long GetHash ()
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

   public static int GetRegistrySize () { return materialRegistry.Count; }
   public static void ClearMaterialCache ()
   {
      foreach (var k in materialRegistry)
      {
         if (k.Value.matInstance != null)
         {
            GameObject.DestroyImmediate (k.Value.matInstance);
         }
      }
      materialRegistry.Clear ();
      MicroSplatVertexMesh.SyncAll ();
   }



   long GetMaterialInstanceHash (int subMesh)
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

   Material GetMaterialInstance (int subMesh)
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
         Cleanup(sub.oldKey, subMesh);
      }
      else
      {
#if UNITY_EDITOR
         // bug in editor when compiling shaders. Brute force, but shouldn't affect runtime.
         if (!Application.IsPlaying(this))
         {
            Cleanup(sub.oldKey, subMesh);
         }
#endif
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

   void Awake ()
   {
      rend = GetComponent<MeshRenderer> ();
   }

   void OnEnable ()
   {
      sInstances.Add (this);
      Sync ();
   }

   void OnDisable ()
   {
      sInstances.Remove (this);
      Cleanup ();
   }



   public void Sync ()
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
         Debug.LogError ("No renderer found on MicroSplatVertexMesh component's game object, cannot sync");
         return;
      }

      if (keywordSO.IsKeywordEnabled ("_MESHOVERLAYSPLATS") == false && rend.sharedMaterials.Length != subMeshEntries.Count)
      {
         var shared = rend.sharedMaterials;
         System.Array.Resize (ref shared, subMeshEntries.Count);
         rend.sharedMaterials = shared;
      }

      ApplySharedData(templateMaterial);

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
            shared [subMeshIdx] = m;
            rend.sharedMaterials = shared;
         }


         m.hideFlags = HideFlags.HideAndDontSave;
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

         if (OnMaterialSync != null)
         {
            OnMaterialSync (m);
         }
      }
   }

   public override Bounds GetBounds () { return rend.bounds; }

   public static new void SyncAll ()
   {
      for (int i = 0; i < sInstances.Count; ++i)
      {
         sInstances [i].Sync ();
      }
      if (OnMaterialSyncAll != null)
      {
         OnMaterialSyncAll ();
      }
   }



}

#else
public class MicroSplatVertexMesh : MonoBehaviour
{
}
#endif

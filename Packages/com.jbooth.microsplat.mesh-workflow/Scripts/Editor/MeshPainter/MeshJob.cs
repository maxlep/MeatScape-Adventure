//////////////////////////////////////////////////////
// MegaSplat
// Copyright (c) Jason Booth
//////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System.Collections.Generic;



namespace JBooth.MicroSplat
{
   #if __MICROSPLAT__ && __MICROSPLAT_MESH__
   public class MeshJob : ScriptableObject
   {
      public MicroSplatMesh msMesh;
      public Collider collider = null;
      public Mesh sharedMesh = null;

      public List<byte[]> undoBuffer = null;
      public List<List<byte[]>> controlAll = null;

      public enum UndoBuffer
      {
         Control0 = 0,
         Control1, 
         Control2,
         Control3,
         Control4,
         Control5,
         Control6,
         Control7,
         ControlAll,
         Tint,
         FX,
         Dampening,
         None
      };

      public UndoBuffer bufferType = UndoBuffer.None;

      Texture2D GetTexture(UndoBuffer type, int subMesh)
      {
         if (subMesh >= msMesh.subMeshEntries.Count)
         {
            Debug.LogError ("SubMesh out of range " + subMesh);
            return null;
         }
         int index = (int)type;
         if (index < 8)
         {
            if (msMesh.subMeshEntries[subMesh].subMeshOverride.controlTextures.Length > index)
               return msMesh.subMeshEntries [subMesh].subMeshOverride.controlTextures [index];
         }
         if (type == UndoBuffer.FX)
         {
            return msMesh.subMeshEntries [subMesh].subMeshOverride.streamTex;
         }
         if (type == UndoBuffer.Dampening)
         {
            return msMesh.subMeshEntries [subMesh].subMeshOverride.displacementDampening;
         }
         if (type == UndoBuffer.Tint)
         {
            return msMesh.subMeshEntries [subMesh].subMeshOverride.tint;
         }
         return null;
      }

      public void RegisterUndo(UndoBuffer type)
      {
         bufferType = type;
         if (type == UndoBuffer.ControlAll)
         {
            controlAll = new List<List<byte []>> (msMesh.subMeshEntries.Count);
            for (int i = 0; i < msMesh.subMeshEntries.Count; ++i)
            {
               controlAll.Add (new List<byte []> ());
            }
            for (int subMesh = 0; subMesh < msMesh.subMeshEntries.Count; ++subMesh)
            {
               for (int i = 0; i < msMesh.subMeshEntries[subMesh].subMeshOverride.controlTextures.Length; ++i)
               {
                  controlAll[subMesh].Add (msMesh.subMeshEntries [subMesh].subMeshOverride.controlTextures[i].GetRawTextureData ());
               }
            }
         }
         else
         {
            undoBuffer = new List<byte []> (msMesh.subMeshEntries.Count);
            for (int subMesh = 0; subMesh < msMesh.subMeshEntries.Count; ++subMesh)
            {
               var tex = GetTexture (type, subMesh);
               if (tex != null)
               {
                  undoBuffer.Add(tex.GetRawTextureData ());
               }
            }
         }

         UnityEditor.Undo.RegisterCompleteObjectUndo (this, "Mesh Paint Edit");

      }

      public void RestoreUndo()
      {
         if (bufferType == UndoBuffer.ControlAll && controlAll != null)
         {
            for (int i = 0; i < controlAll.Count; ++i)
            {
               if (i >= msMesh.subMeshEntries.Count)
               {
                  continue;
               }
               var subMesh = msMesh.subMeshEntries [i];
               for (int j = 0; j < controlAll [i].Count; ++j)
               {
                  if (subMesh.subMeshOverride.controlTextures.Length > j)
                  {
                     var tex = subMesh.subMeshOverride.controlTextures [j];
                     if (tex != null)
                     {
                        tex.LoadRawTextureData (controlAll [i] [j]);
                        tex.Apply ();
                     }
                  }
               }
            }
         }
         else if (undoBuffer != null && undoBuffer.Count > 0 && bufferType != UndoBuffer.None)
         {
            for (int i = 0; i < undoBuffer.Count; ++i)
            {
               if (i >= msMesh.subMeshEntries.Count)
                  continue;

               var sub = msMesh.subMeshEntries [i];
               var tex = GetTexture (bufferType, i);
               tex.LoadRawTextureData (undoBuffer[i]);
               tex.Apply ();
            }
            if (bufferType != UndoBuffer.Dampening && bufferType != UndoBuffer.FX && bufferType != UndoBuffer.Tint)
            {
               // renormalize..
               MeshPainterWindow.NormalizeMesh (this);
            }
         }
      }
   }
   #endif
}

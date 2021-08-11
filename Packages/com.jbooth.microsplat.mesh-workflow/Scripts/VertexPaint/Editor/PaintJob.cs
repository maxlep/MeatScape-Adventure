using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace JBooth.MicroSplat.VertexPainter
{
   public class PaintJob
   {
      public MeshFilter meshFilter;
      public Renderer renderer;
      public VertexInstanceStream _stream;
      public MicroSplatVertexMesh vertexMesh;
      // cache of data we often need so we don't have to cross the c#->cpp bridge often 
      public Vector3[] verts;

      // getters which take stream into account
      public Vector3 GetPosition(int i)
      {
         if (stream.positions != null && stream.positions.Length == verts.Length)
            return stream.positions[i];
         return verts[i];
      }

      public bool HasStream() { return _stream != null; }
      public bool HasData()
      {
         if (_stream == null)
            return false;

         int vertexCount = verts.Length;
         bool hasColors = (stream.colors != null && stream.colors.Length == vertexCount);
         bool hasPositions = (stream.positions != null && stream.positions.Length == vertexCount);

         return (hasColors || hasPositions);
      }

      public void EnforceStream()
      {
         if (_stream == null && renderer != null && meshFilter != null)
         {
            _stream = meshFilter.gameObject.AddComponent<VertexInstanceStream>();
         }
      }

      public VertexInstanceStream stream
      {
         get
         {
            if (_stream == null)
            {
               if (meshFilter == null)
               { // object has been deleted
                  return null;
               }
               _stream = meshFilter.gameObject.GetComponent<VertexInstanceStream>();
               if (_stream == null)
               {
                  _stream = meshFilter.gameObject.AddComponent<VertexInstanceStream>();
               }
               else
               {
                  _stream.Apply();
               }
            }
            return _stream;
         }

      }

      public PaintJob(MeshFilter mf, Renderer r, MicroSplatVertexMesh vmesh)
      {
         meshFilter = mf;
         renderer = r;
         vertexMesh = vmesh;
         _stream = r.gameObject.GetComponent<VertexInstanceStream>();
         verts = mf.sharedMesh.vertices;
      }
   }
}
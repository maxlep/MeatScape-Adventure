using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

/* Holds streams of data to override the colors or UVs on a mesh without making the mesh unique. This is more
 * memory efficient than burning the color data into many copies of a mesh, and much easier to manage. 
 * 
*/
namespace JBooth.MicroSplat.VertexPainter
{
   [ExecuteInEditMode]
   public class VertexInstanceStream : MonoBehaviour
   {
      public bool keepRuntimeData = false;


      [HideInInspector]
      [SerializeField]
      private Color[] _colors;

      [HideInInspector]
      [SerializeField]
      private List<Vector4> _uv1;

      [HideInInspector]
      [SerializeField]
      private List<Vector4> _uv2;

      [HideInInspector]
      [SerializeField]
      private Vector3[] _positions;

      public Color[] colors 
      { 
         get 
         { 
            return _colors; 
         }
         set
         {
            enforcedColorChannels = (! (_colors == null || (value != null && _colors.Length != value.Length)));
            _colors = value;
            Apply();
         }
      }

      public List<Vector4> uv1 { get { return _uv1; } set { _uv1 = value; Apply (); } }
      public List<Vector4> uv2 { get { return _uv2; } set { _uv2 = value; Apply (); } }

      public Vector3[] positions { get { return _positions; } set { _positions = value; Apply(); } }

      #if UNITY_EDITOR
      Vector3[] cachedPositions;
      public Vector3 GetSafePosition(int index)
      {
         if (_positions != null && index < _positions.Length)
         {
            return _positions[index];
         }
         if (cachedPositions == null || cachedPositions.Length == 0)
         {
            MeshFilter mf = GetComponent<MeshFilter>();
            if (mf == null || mf.sharedMesh == null)
            {
               Debug.LogError("No Mesh Filter or Mesh available");
               return Vector3.zero;
            }
            cachedPositions = mf.sharedMesh.vertices;
         }
         if (index < cachedPositions.Length)
         {
            return cachedPositions[index];
         }
         return Vector3.zero;
      }

      

      #endif


   	void Start()
      {
         Apply(!keepRuntimeData);
         if (keepRuntimeData)
         {
            var mf = GetComponent<MeshFilter>();
            _positions = mf.sharedMesh.vertices;
         }
      }

      void OnDestroy()
      {
         if (!Application.isPlaying)
         {
            MeshRenderer mr = GetComponent<MeshRenderer> ();
            if ( mr != null )
               mr.additionalVertexStreams = null;
         }
      }

      bool enforcedColorChannels = false;
      void EnforceOriginalMeshHasColors(Mesh stream)
      {
         if (enforcedColorChannels == true)
            return;
         enforcedColorChannels = true;
         MeshFilter mf = GetComponent<MeshFilter>();
         Color[] origColors = mf.sharedMesh.colors;
         if (stream != null && stream.colors.Length > 0 && (origColors == null || origColors.Length == 0))
         {
            // workaround for unity bug; dispite docs claim, color channels must exist on the original mesh
            // for the additionalVertexStream to work. Which is, sad...
            mf.sharedMesh.colors = stream.colors;
         }
      }

      #if UNITY_EDITOR
      public void SetColor(Color c, int count) { _colors = new Color[count]; for (int i = 0; i < count; ++i) { _colors[i] = c; } Apply(); }
      public void SetUV1 (Vector4 uv, int count) { _uv1 = new List<Vector4> (count); for (int i = 0; i < count; ++i) { _uv1.Add (uv); } Apply (); }
      public void SetUV2 (Vector4 uv, int count) { _uv2 = new List<Vector4> (count); for (int i = 0; i < count; ++i) { _uv2.Add (uv); } Apply (); }
      #endif

      public Mesh Apply(bool markNoLongerReadable = true)
      {
         MeshRenderer mr = GetComponent<MeshRenderer>();
         MeshFilter mf = GetComponent<MeshFilter>();

         if (mr != null && mf != null && mf.sharedMesh != null)
         {
            int vertexCount = mf.sharedMesh.vertexCount;
            Mesh stream = meshStream;
            if (stream == null || vertexCount != stream.vertexCount)
            {
               if (stream != null)
               {
                  DestroyImmediate(stream);
               }
#if UNITY_2017_3_OR_NEWER
               var format = mf.sharedMesh.indexFormat;
#endif
               stream = new Mesh();
#if UNITY_2017_3_OR_NEWER
               stream.indexFormat = format;
#endif

               // even though the docs say you don't need to set the positions on your avs, you do.. 
               stream.vertices = new Vector3[mf.sharedMesh.vertexCount];

               // wtf, copy won't work?
               // so, originally I did a copyTo here, but with a unity patch release the behavior changed and
               // the verticies would all become 0. This seems a funny thing to change in a patch release, but
               // since getting the data from the C++ side creates a new array anyway, we don't really need
               // to copy them anyway since they are a unique copy already.
               stream.vertices = mf.sharedMesh.vertices;
               // another Unity bug, when in editor, the paint job will just disapear sometimes. So we have to re-assign
               // it every update (even though this doesn't get called each frame, it appears to loose the data during
               // the editor update call, which only happens occationaly. 
               stream.MarkDynamic();
               stream.triangles = mf.sharedMesh.triangles;
               meshStream = stream;

               stream.hideFlags = HideFlags.HideAndDontSave;
            }
            if (_positions != null && _positions.Length == vertexCount) { stream.vertices = _positions; }
            if (_colors != null && _colors.Length == vertexCount) { stream.colors = _colors; } else { stream.colors = null; }
            if (_uv1 != null && _uv1.Count == vertexCount) { stream.SetUVs (1, _uv1); }  else { stream.uv2 = null; }
            if (_uv2 != null && _uv2.Count == vertexCount) { stream.SetUVs (2, _uv2); } else { stream.uv3 = null; }


            EnforceOriginalMeshHasColors (stream);
 
            if (!Application.isPlaying || Application.isEditor)
            {
               // only mark no longer readable in game..
               markNoLongerReadable = false;
            }

            stream.UploadMeshData(markNoLongerReadable);
            mr.additionalVertexStreams = stream;
            return stream;
         }
         return null;
      }

      // keep this around for updates..
      private Mesh meshStream;
      // In various versions of unity, you have to set .additionalVertexStreams every frame. 
      // This was broken in 5.3, then broken again in 5.5..

   #if UNITY_EDITOR

      public Mesh GetModifierMesh() { return meshStream; }
      private MeshRenderer meshRend = null;
      void Update()
      {
         // turns out this happens in play mode as well.. grrr..
         if (meshRend == null)
         {
            meshRend = GetComponent<MeshRenderer>();
         }
         //if (!Application.isPlaying)
         {
            meshRend.additionalVertexStreams = meshStream;
         }
      }
   #endif
   }
}

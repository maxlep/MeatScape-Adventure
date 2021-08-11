//////////////////////////////////////////////////////
// Terain To Mesh
// Copyright (c) Jason Booth
//////////////////////////////////////////////////////


using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using JBooth.MicroSplat;

namespace JBooth.TerrainToMesh
{
#if __MICROSPLAT__ && __MICROSPLAT_MESHTERRAIN__
   public class TerrainToMesh : EditorWindow
   {
      [MenuItem ("Window/MicroSplat/Terrain To Mesh")]
      public static void ShowWindow ()
      {
         var window = GetWindow<JBooth.TerrainToMesh.TerrainToMesh> ();
         if (window != null)
         {
            window.Show ();
            window.Init ();
         }
      }

      public List<Terrain> selectedTerrains = new List<Terrain> ();

      void Init ()
      {
         Object [] objs = Selection.GetFiltered (typeof (Terrain), SelectionMode.Editable | SelectionMode.Deep);
         selectedTerrains.Clear ();
         for (int i = 0; i < objs.Length; ++i)
         {
            Terrain t = objs [i] as Terrain;
            if (t == null)
               continue;

            selectedTerrains.Add (t);
         }
      }

      void OnSelectionChange ()
      {
         Init ();
      }

      public class Settings
      {
         public int chunkDiv = 1;
         public int subDiv = 64;
         public bool convertMaterial = true;
         public bool deleteTerrains = false;
         public bool generateLowResShader = false;
         public bool addColliders = false;
         public string prefix = "";
         public string postfix = "";
      }

      public Settings settings = new Settings ();

      static GUIContent CChunkDiv = new GUIContent ("Chunks", "Number of objects to break terrains into per side");
      static GUIContent CSubDiv = new GUIContent ("Sub Divisions", "Sub divisions per terrain chunk per side");
      static GUIContent CAddColliders = new GUIContent ("Add Colliders", "Add colliders to terrain chunks. This is required for terrain blending");

      void OnGUI ()
      {
         if (selectedTerrains.Count == 0)
         {
            EditorGUILayout.HelpBox ("Select terrains with MicroSplat on them to continue", MessageType.Info);
            return;
         }
         for (int i = 0; i < selectedTerrains.Count; ++i)
         {
            if (selectedTerrains [0] == null || selectedTerrains [0].GetComponent<MicroSplatTerrain> () == null)
            {
               EditorGUILayout.HelpBox ("Terrain must be setup with MicroSplat to convert", MessageType.Error);
               return;
            }
         }

         settings.chunkDiv = EditorGUILayout.IntSlider (CChunkDiv, settings.chunkDiv, 1, 32);
         settings.subDiv = EditorGUILayout.IntSlider (CSubDiv, settings.subDiv, 16, 170);
         settings.addColliders = EditorGUILayout.Toggle (CAddColliders, settings.addColliders);

         settings.convertMaterial = EditorGUILayout.Toggle ("Convert Material", settings.convertMaterial);
         settings.generateLowResShader = EditorGUILayout.Toggle ("Generate Separate Shader", settings.generateLowResShader);
         settings.deleteTerrains = EditorGUILayout.Toggle ("Delete Terrains", settings.deleteTerrains);

         settings.prefix = EditorGUILayout.TextField ("Naming Prefix", settings.prefix);
         settings.postfix = EditorGUILayout.TextField ("Naming Postfix", settings.postfix);

         if (GUILayout.Button ("Convert"))
         {
            if (selectedTerrains [0] == null)
            {
               Debug.LogError ("Select a terrain with MicroSplat on it.");
               return;
            }
            var mst = selectedTerrains [0].GetComponent<MicroSplatTerrain> ();
            if (mst == null || mst.templateMaterial == null)
            {
               Debug.Log ("Did not find MicroSplatTerrain component and material template on terrain");
            }
            else
            {

               string baseDir = JBooth.MicroSplat.MicroSplatUtilities.RelativePathFromAsset (mst.templateMaterial);
               if (settings.generateLowResShader)
               {
                  baseDir += "/MeshTerrain/MicroSplatData";
                  if (!System.IO.Directory.Exists (baseDir))
                  {
                     System.IO.Directory.CreateDirectory (baseDir);
                  }
               }


               for (int i = 0; i < selectedTerrains.Count; ++i)
               {
                  var meshPath = baseDir + "/" + selectedTerrains [i].name + "_meshes.asset";
                  Mesh root = new Mesh ();
                  root.name = "Meshes";

                  AssetDatabase.CreateAsset (root, meshPath);
                  Convert (selectedTerrains [i], baseDir, root, settings);
               }
               AssetDatabase.SaveAssets ();
            }
         }

      }


      static Mesh CreateSubChunk (Terrain t, Vector3 worldStart, Vector2 uvStart, int dx, int dy, int div, Settings s)
      {
         int chunkDiv = s.chunkDiv;
         int subDiv = s.subDiv;

         float size = t.terrainData.size.x;
         float pSize = size * 0.5f;
         pSize /= chunkDiv;

         string meshName = div > 0 ? t.name + "_" + dx + "_" + dy : t.name;
         Mesh mesh = new Mesh ();
         mesh.name = meshName;
         Vector3 [] vertices = new Vector3 [subDiv * subDiv];
         Vector3 [] normals = new Vector3 [vertices.Length];
         for (int z = 0; z < subDiv; z++)
         {
            float zPos = ((float)z / (subDiv - 1) - .5f) * pSize * 2;
            for (int x = 0; x < subDiv; x++)
            {
               float xPos = ((float)x / (subDiv - 1) - .5f) * pSize * 2;

               float uvX = (float)x / (float)(subDiv - 1);
               float uvY = (float)z / (float)(subDiv - 1);
               uvX /= (float)chunkDiv;
               uvY /= (float)chunkDiv;
               uvX += uvStart.x;
               uvY += uvStart.y;

               Vector3 worldPos = new Vector3 (xPos + pSize + worldStart.x, 0, zPos + pSize + worldStart.y);
               worldPos += t.GetPosition ();
               float yPos = t.SampleHeight (worldPos);
               Vector3 nm = t.terrainData.GetInterpolatedNormal (xPos + pSize, zPos + pSize);
               vertices [x + z * subDiv] = new Vector3 (xPos + pSize + worldStart.x, yPos, zPos + pSize + worldStart.y);
               normals [x + z * subDiv] = new Vector3 (nm.x, nm.y, nm.z);
            }
         }


         Vector2 [] uvs = new Vector2 [vertices.Length];
         for (int v = 0; v < subDiv; v++)
         {
            for (int u = 0; u < subDiv; u++)
            {
               Vector2 uv = new Vector2 ((float)u / (subDiv - 1), (float)v / (subDiv - 1));
               uv.x /= chunkDiv;
               uv.y /= chunkDiv;
               uv.x += uvStart.x;
               uv.y += uvStart.y;
               uvs [u + v * subDiv] = uv;
            }
         }
         int nbFaces = (subDiv - 1) * (subDiv - 1);
         int [] triangles = new int [nbFaces * 6];
         int tidx = 0;
         for (int face = 0; face < nbFaces; face++)
         {
            // Retrieve lower left corner from face ind
            int i = face % (subDiv - 1) + (face / (subDiv - 1) * subDiv);

            triangles [tidx++] = i + subDiv;
            triangles [tidx++] = i + 1;
            triangles [tidx++] = i;

            triangles [tidx++] = i + subDiv;
            triangles [tidx++] = i + subDiv + 1;
            triangles [tidx++] = i + 1;
         }

         mesh.vertices = vertices;
         mesh.normals = normals;
         mesh.uv = uvs;
         mesh.triangles = triangles;

         mesh.RecalculateNormals ();
         mesh.RecalculateTangents ();
         mesh.RecalculateBounds ();


         return mesh;
      }

      static Texture2D SaveHeightTexture (RenderTexture rt, string basePath)
      {
         Texture2D tex = new Texture2D (rt.width, rt.height, TextureFormat.R16, true, true);
         var old = RenderTexture.active;

         RenderTexture.active = rt;
         tex.ReadPixels (new Rect (0, 0, tex.width, tex.height), 0, 0);
         tex.Apply ();
         RenderTexture.active = old;

         JBooth.MicroSplat.HDTextureImporter.Write (tex, basePath + "_height", true, true);
         AssetDatabase.Refresh ();
         return AssetDatabase.LoadAssetAtPath<Texture2D> (basePath + "_height.hdtexture");

      }

      public static void Convert (Terrain t, string baseDir, Mesh root, Settings s)
      {
         int chunkDiv = s.chunkDiv;

         var mst = t.GetComponent<MicroSplatTerrain> ();
         if (mst == null)
         {
            Debug.LogError ("MicroSplat Terrain missing");
            return;
         }

         var oldInstance = mst.terrain.drawInstanced;
         mst.terrain.drawInstanced = false;

         float chunkSkip = t.terrainData.size.x / (float)chunkDiv;

         GameObject meshes = new GameObject (s.prefix + t.name + s.postfix);
         meshes.transform.position = t.transform.position;

#if __MICROSPLAT_STREAMS__
         var origst = mst.GetComponent<StreamManager> ();
         if (origst != null)
         {
            var newst = meshes.AddComponent<StreamManager> ();
            EditorUtility.CopySerialized (origst, newst);
         }
#endif

         MicroSplatMeshTerrain msm = meshes.AddComponent<MicroSplatMeshTerrain> ();

         if (s.convertMaterial)
         {
            if (s.generateLowResShader)
            {
               if (!System.IO.File.Exists (baseDir + "/MicroSplat.mat"))
               {
                  FileUtil.CopyFileOrDirectory (AssetDatabase.GetAssetPath (mst.templateMaterial), baseDir + "/MicroSplat.mat");
               }
               if (!System.IO.File.Exists (baseDir + "/MicroSplat_keywords.asset"))
               {
                  FileUtil.CopyFileOrDirectory (AssetDatabase.GetAssetPath (mst.keywordSO), baseDir + "/MicroSplat_keywords.asset");
               }

               if (!System.IO.File.Exists (baseDir + "/MicroSplat.shader"))
               {
                  FileUtil.CopyFileOrDirectory (AssetDatabase.GetAssetPath (mst.templateMaterial.shader), baseDir + "/MicroSplat.shader");
               }

               AssetDatabase.Refresh ();

               msm.templateMaterial = AssetDatabase.LoadAssetAtPath<Material> (baseDir + "/MicroSplat.mat");
               msm.templateMaterial.shader = AssetDatabase.LoadAssetAtPath<Shader> (baseDir + "/MicroSplat.shader");
               msm.keywordSO = AssetDatabase.LoadAssetAtPath<MicroSplatKeywords> (baseDir + "/MicroSplat_keywords.asset");
               msm.templateMaterial.CopyPropertiesFromMaterial (mst.templateMaterial);
               if (!msm.keywordSO.IsKeywordEnabled ("_MICROMESHTERRAIN"))
               {
                  msm.keywordSO.EnableKeyword ("_MICROMESHTERRAIN");
               }
               if (msm.keywordSO.IsKeywordEnabled ("_OUTPUTDIGGER"))
               {
                  msm.keywordSO.DisableKeyword ("_OUTPUTDIGGER");
               }
               if (msm.keywordSO.IsKeywordEnabled ("_MICRODIGGERMESH"))
               {
                  msm.keywordSO.DisableKeyword ("_MICRODIGGERMESH");
               }
               if (oldInstance)
               {
                  msm.keywordSO.EnableKeyword ("_PERPIXNORMAL");
               }

               MicroSplatShaderGUI.MicroSplatCompiler c = new MicroSplatShaderGUI.MicroSplatCompiler ();
               c.Compile (msm.templateMaterial);
            }
         }

         msm.perPixelNormal = mst.perPixelNormal;
         msm.blendMat = mst.blendMat;
         msm.propData = mst.propData;
         msm.streamTexture = mst.streamTexture;
         
         

         if (mst.keywordSO.IsKeywordEnabled("_DYNAMICFLOWS") || mst.keywordSO.IsKeywordEnabled("_TERRAINBLENDING"))
         {
            msm.terrainDescriptor.heightMap = SaveHeightTexture (mst.terrain.terrainData.heightmapTexture, baseDir + "/" + mst.name);
         }
         if (mst.keywordSO.IsKeywordEnabled("_PERPIXNORMAL") || mst.keywordSO.IsKeywordEnabled("_TERRAINBLENDING") || (mst.terrain != null && mst.terrain.drawInstanced))
         {
            msm.perPixelNormal = MicroSplatTerrainEditor.GenerateTerrainNormalMap (mst);
            msm.terrainDescriptor.normalMap = msm.perPixelNormal;
         }
#if (VEGETATION_STUDIO || VEGETATION_STUDIO_PRO)
        msm.vsGrassMap = mst.vsGrassMap;
        msm.vsShadowMap = mst.vsShadowMap;
#endif

#if __MICROSPLAT_ALPHAHOLE__
         msm.clipMap = mst.clipMap;
#endif

#if __MICROSPLAT_PROCTEX__
         msm.procBiomeMask = mst.procBiomeMask;
         msm.procBiomeMask2 = mst.procBiomeMask2;
         msm.procTexCfg = mst.procTexCfg;
         msm.cavityMap = mst.cavityMap;
#endif

#if __MICROSPLAT_SCATTER__
         msm.scatterMapOverride = mst.scatterMapOverride;
#endif

#if __MICROSPLAT_GLOBALTEXTURE__
         msm.tintMapOverride = mst.tintMapOverride;
         msm.geoTextureOverride = mst.geoTextureOverride;
         msm.globalNormalOverride = mst.globalNormalOverride;
         msm.globalEmisOverride = mst.globalEmisOverride;
         msm.globalSAOMOverride = mst.globalSAOMOverride;
#endif

#if __MICROSPLAT_SNOW__
         msm.snowMaskOverride = mst.snowMaskOverride;
#endif


         // export splat textures
         var textures = t.terrainData.alphamapTextures;
         for (int i = 0; i < textures.Length; ++i)
         {
            var path = baseDir + "/splat_" + t.name + i.ToString ();
            var bytes = textures [i].EncodeToTGA ();
            System.IO.File.WriteAllBytes (path + ".tga", bytes);
            AssetDatabase.Refresh ();
            var ai = AssetImporter.GetAtPath (MicroSplat.MicroSplatUtilities.MakeRelativePath (path + ".tga"));


            var ti = ai as TextureImporter;
            if (ti != null)
            {
               var ps = ti.GetDefaultPlatformTextureSettings ();

               if (ti.isReadable == true ||
                  ti.wrapMode != TextureWrapMode.Repeat || ps.format != TextureImporterFormat.RGBA32 ||
                  ps.textureCompression != TextureImporterCompression.Compressed ||
                  ps.overridden != true ||
                  ti.filterMode != FilterMode.Bilinear ||
                  ti.sRGBTexture != false)
               {
                  ti.sRGBTexture = false;
                  ti.filterMode = FilterMode.Bilinear;
                  ti.mipmapEnabled = true;
                  ti.wrapMode = TextureWrapMode.Repeat;
                  ti.isReadable = true;
                  ps.format = TextureImporterFormat.RGBA32;
                  ps.textureCompression = TextureImporterCompression.Compressed;
                  ps.overridden = true;
                  ti.SetPlatformTextureSettings (ps);
                  ti.SaveAndReimport ();
               }
            }
         }

         msm.controlTextures = new Texture2D [textures.Length];
         for (int i = 0; i < textures.Length; ++i)
         {
            var p = MicroSplat.MicroSplatUtilities.MakeRelativePath (baseDir + "/splat_" + t.gameObject.name + i + ".tga");

            Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D> (p);
            msm.controlTextures [i] = tex;
         }

         MeshRenderer [] rends = new MeshRenderer [chunkDiv * chunkDiv];
         int chunkIdx = 0;
         for (int chunkX = 0; chunkX < chunkDiv; ++chunkX)
         {
            for (int chunkY = 0; chunkY < chunkDiv; ++chunkY)
            {
               Vector2 worldStart = new Vector2 (chunkX * chunkSkip, chunkY * chunkSkip);
               Vector2 uvStart = new Vector2 (worldStart.x / t.terrainData.size.x, worldStart.y / t.terrainData.size.x);
               Mesh mesh = CreateSubChunk (t, worldStart, uvStart, chunkX, chunkY, chunkDiv, s);
               AssetDatabase.AddObjectToAsset (mesh, root);
               GameObject go = new GameObject (t.name + "_" + chunkX + "_" + chunkY);
               MeshRenderer rend = go.AddComponent<MeshRenderer> ();
               var filter = go.AddComponent<MeshFilter> ();
               filter.sharedMesh = mesh;
               if (s.addColliders)
               {
                  var mc = go.AddComponent<MeshCollider> ();
                  mc.sharedMesh = mesh;
               }

               go.transform.position = t.GetPosition ();
               go.transform.SetParent (meshes.transform);

               rends [chunkIdx] = rend;
               chunkIdx++;

            }
         }
         msm.meshTerrains = rends;


         if (chunkDiv == 1)
         {
            // remove parent object
            var child = msm.transform.GetChild (0).gameObject;
            var nm = child.AddComponent<MicroSplatMeshTerrain> ();
            EditorUtility.CopySerialized (msm, nm);
            child.transform.SetParent (null, true);
            DestroyImmediate (meshes);
            nm.Sync ();
         }
         else
         {
            MicroSplatObject.SyncAll();
         }

         mst.terrain.drawInstanced = oldInstance;

         if (s.deleteTerrains)
         {
            DestroyImmediate (t);
         }
      }

   }
#endif
         }


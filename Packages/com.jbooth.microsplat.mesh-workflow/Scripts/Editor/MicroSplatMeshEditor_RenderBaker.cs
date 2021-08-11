//////////////////////////////////////////////////////
// MicroSplat
// Copyright (c) Jason Booth
//////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;

#if __MICROSPLAT__ && __MICROSPLAT_MESH__
using JBooth.MicroSplat;

public partial class MicroSplatMeshEditor : Editor
{
   public enum Resolutions
   {
      k256 = 256,
      k512 = 512,
      k1024 = 1024,
      k2048 = 2048, 
      k4096 = 4096, 
      k8192 = 8192
   };

   public enum Passes
   {
      Albedo = 1,
      Height = 2,
      Normal = 4,
      Metallic = 8,
      Smoothness = 16,
      AO = 32,
      Emissive = 64,
      FinalNormal = 128,
#if __MICROSPLAT_PROCTEX__
      ProceduralSplat0 = 256,
      ProceduralSplat1 = 512,
      ProceduralSplat2 = 1024,
      ProceduralSplat3 = 2048,
      ProceduralSplat4 = 4096,
      ProceduralSplat5 = 8192,
      ProceduralSplat6 = 16384,
      ProceduralSplat7 = 32768,
#endif
   };
    
   public Passes passes = 0;
   public Resolutions res = Resolutions.k1024;

   bool needsBake = false;
   void BakingGUI(MicroSplatMesh job)
   {
      if (needsBake && Event.current.type == EventType.Repaint)
      {
         needsBake = false;
         Bake(job);
      }
      res = (Resolutions)EditorGUILayout.EnumPopup(new GUIContent("Resolution"), res);
#if UNITY_2017_3_OR_NEWER
      passes = (Passes)EditorGUILayout.EnumFlagsField(new GUIContent("Features"), passes);
#else
      passes = (Passes)EditorGUILayout.EnumMaskPopup(new GUIContent("Features"), passes);
#endif
      if (GUILayout.Button("Export Selected"))
      {
         needsBake = true;
      }
   }

   bool IsEnabled(Passes p)
   {
      return ((int)passes & (int)p) == (int)p;
   }


   class MeshDef
   {
      public Vector3[] verts;
      public int[] faces;
      public Color[] color;
      public Vector4[] uv0;
      public Vector4[] uv1;
      public Vector4[] uv2;
      public Vector4[] uv3;

      public static int kMaxVert = 60000;

      public MeshDef(int triCount)
      {
         verts = new Vector3[triCount];
         faces = new int[triCount];
         color = new Color[triCount];
         uv0 = new Vector4[triCount];
         uv1 = new Vector4[triCount];
         uv2 = new Vector4[triCount];
         uv3 = new Vector4[triCount];
      }
   }

   static void RemoveKeyword(List<string> keywords, string keyword)
   {
      if (keywords.Contains(keyword))
      {
         keywords.Remove(keyword);
      }
   }

   static Material SetupMaterial(Material template, Material mat, MicroSplatBaseFeatures.DebugOutput debugOutput)
   {
      MicroSplatShaderGUI.MicroSplatCompiler comp = new MicroSplatShaderGUI.MicroSplatCompiler();
      var kwds = MicroSplatUtilities.FindOrCreateKeywords(template);
      List<string> keywords = new List<string>(kwds.keywords);

      RemoveKeyword(keywords, "_SNOW");
      RemoveKeyword(keywords, "_PARALLAX");
      RemoveKeyword(keywords, "_TESSDISTANCE");
      RemoveKeyword(keywords, "_WINDPARTICULATE");
      RemoveKeyword(keywords, "_SNOWPARTICULATE");
      RemoveKeyword(keywords, "_GLITTER");
      RemoveKeyword(keywords, "_SNOWGLITTER");
      RemoveKeyword(keywords, "_WORLDSPACEUV");
      RemoveKeyword (keywords, "_SPECULARFROMMETALLIC");
      RemoveKeyword (keywords, "_USESPECULARWORKFLOW");
      RemoveKeyword (keywords, "_BDRFLAMBERT");
      RemoveKeyword (keywords, "_BDRF1");
      RemoveKeyword (keywords, "_BDRF2");
      RemoveKeyword (keywords, "_BDRF3");

      keywords.Add(FeatureFromOutput(debugOutput).ToString());
      keywords.Add ("_RENDERBAKE");
      keywords.Add ("_UNLIT");


      // bool blendable = keywords.Contains("_MESHOVERLAYSPLATS") || keywords.Contains("_MESHCOMBINED");
      string shader = comp.Compile(keywords.ToArray(), "RenderBake_" + debugOutput.ToString(), null, null); // blendable?
      System.IO.File.WriteAllText ("Assets/shader" + debugOutput.ToString () + ".shader", shader);
      Shader s = ShaderUtil.CreateShaderAsset(shader);
      Material renderMat = new Material(mat);
      renderMat.shader = s;
      renderMat.CopyPropertiesFromMaterial (mat);
      renderMat.enableInstancing = false;
      return renderMat;
   }

   List<GameObject> BuildRenderObjects(MicroSplatMesh job,MicroSplatBaseFeatures.DebugOutput debugOutput)
   {
      List<GameObject> goes = new List<GameObject>();

      
      Material renderMat = SetupMaterial(job.templateMaterial, job.matInstance, debugOutput);
      Mesh src = job.GetComponent<MeshFilter>().sharedMesh;
      var srcTri = src.triangles;
      List<Vector4> srcUV0 = new List<Vector4>();
      List<Vector4> srcUV1 = new List<Vector4>();
      List<Vector4> srcUV2 = new List<Vector4>();
      List<Vector4> srcUV3 = new List<Vector4>();
      src.GetUVs(0, srcUV0);
      src.GetUVs(1, srcUV1);
      src.GetUVs(2, srcUV2);
      src.GetUVs(3, srcUV3);

      var srcColor = src.colors;

      int srcVCount = src.vertexCount;
         
      List<MeshDef> defs = new List<MeshDef>();

      int triCount = srcTri.Length;
      int left = triCount;
      while (left > 0)
      {
         defs.Add(new MeshDef(Mathf.Clamp(left, 0, MeshDef.kMaxVert)));
         left -= MeshDef.kMaxVert;
      }

      for (int i = 0; i < triCount; i++)
      {
         int defIdx = (int)(i / MeshDef.kMaxVert);
         int idxOffset = i - (defIdx * MeshDef.kMaxVert);
         var d = defs[defIdx];
         d.faces[idxOffset] = idxOffset;
         int vIdx = srcTri[i];
         d.verts[idxOffset] = new Vector3(srcUV0[vIdx].x, srcUV0[vIdx].y, 0);
         d.uv0[idxOffset] = srcUV0[vIdx];
         if (idxOffset < d.uv1.Length && vIdx < srcUV1.Count)
         {
            d.uv1 [idxOffset] = srcUV1 [vIdx];
         }
         if (idxOffset < d.uv2.Length && vIdx < srcUV2.Count)
         {
            d.uv2[idxOffset] = srcUV2[vIdx];
         }
         if (idxOffset < d.uv3.Length && vIdx < srcUV3.Count)
         {
            d.uv3[idxOffset] = srcUV3[vIdx];
         }
         if (idxOffset < d.color.Length && vIdx < srcColor.Length)
         {
            d.color[idxOffset] = srcColor[vIdx];
         }
      }
      for (int i = 0; i < defs.Count; ++i)
      {
         var d = defs[i];
         Mesh renderMesh = new Mesh();
         renderMesh.vertices = d.verts;
         renderMesh.triangles = d.faces;
         renderMesh.colors = d.color;
         renderMesh.SetUVs(0, new List<Vector4>(d.uv0));
         renderMesh.SetUVs(1, new List<Vector4>(d.uv1));
         renderMesh.SetUVs(2, new List<Vector4>(d.uv2));
         renderMesh.SetUVs(3, new List<Vector4>(d.uv3));
         renderMesh.RecalculateBounds();
         renderMesh.UploadMeshData(false);


         GameObject go = new GameObject();
         go.AddComponent<MeshRenderer>().sharedMaterial = renderMat;
         go.AddComponent<MeshFilter>().sharedMesh = renderMesh;
         go.transform.position = new Vector3(0, 10000, 0);
         goes.Add(go);
      }

      return goes;
   }

   MicroSplatBaseFeatures.DebugOutput OutputFromPass(Passes p)
   {
      if (p == Passes.Albedo)
      {
         return MicroSplatBaseFeatures.DebugOutput.Albedo;
      }
      else if (p == Passes.AO)
      {
         return MicroSplatBaseFeatures.DebugOutput.AO;
      }
      else if (p == Passes.Emissive)
      {
         return MicroSplatBaseFeatures.DebugOutput.Emission;
      }
      else if (p == Passes.Height)
      {
         return MicroSplatBaseFeatures.DebugOutput.Height;
      }
      else if (p == Passes.Metallic)
      {
         return MicroSplatBaseFeatures.DebugOutput.Metallic;
      }
      else if (p == Passes.Normal)
      {
         return MicroSplatBaseFeatures.DebugOutput.Normal;
      }
      else if (p == Passes.Smoothness)
      {
         return MicroSplatBaseFeatures.DebugOutput.Smoothness;
      }
      else if (p == Passes.FinalNormal)
      {
         return MicroSplatBaseFeatures.DebugOutput.FinalNormalTangent;
      }
#if __MICROSPLAT_PROCTEX__
      else if (p == Passes.ProceduralSplat0)
      {
         return MicroSplatBaseFeatures.DebugOutput.ProceduralSplatOutput0;
      }
      else if (p == Passes.ProceduralSplat1)
      {
         return MicroSplatBaseFeatures.DebugOutput.ProceduralSplatOutput1;
      }
      else if (p == Passes.ProceduralSplat2)
      {
         return MicroSplatBaseFeatures.DebugOutput.ProceduralSplatOutput2;
      }
      else if (p == Passes.ProceduralSplat3)
      {
         return MicroSplatBaseFeatures.DebugOutput.ProceduralSplatOutput3;
      }
      else if (p == Passes.ProceduralSplat4)
      {
         return MicroSplatBaseFeatures.DebugOutput.ProceduralSplatOutput4;
      }
      else if (p == Passes.ProceduralSplat5)
      {
         return MicroSplatBaseFeatures.DebugOutput.ProceduralSplatOutput5;
      }
      else if (p == Passes.ProceduralSplat6)
      {
         return MicroSplatBaseFeatures.DebugOutput.ProceduralSplatOutput6;
      }
      else if (p == Passes.ProceduralSplat7)
      {
         return MicroSplatBaseFeatures.DebugOutput.ProceduralSplatOutput7;
      }
#endif

      return MicroSplatBaseFeatures.DebugOutput.Albedo;
   }

   static MicroSplatBaseFeatures.DefineFeature FeatureFromOutput(MicroSplatBaseFeatures.DebugOutput p)
   {
      if (p == MicroSplatBaseFeatures.DebugOutput.Albedo)
      {
         return MicroSplatBaseFeatures.DefineFeature._DEBUG_OUTPUT_ALBEDO;
      }
      else if (p == MicroSplatBaseFeatures.DebugOutput.AO)
      {
         return MicroSplatBaseFeatures.DefineFeature._DEBUG_OUTPUT_AO;
      }
      else if (p == MicroSplatBaseFeatures.DebugOutput.Emission)
      {
         return MicroSplatBaseFeatures.DefineFeature._DEBUG_OUTPUT_EMISSION;
      }
      else if (p == MicroSplatBaseFeatures.DebugOutput.Height)
      {
         return MicroSplatBaseFeatures.DefineFeature._DEBUG_OUTPUT_HEIGHT;
      }
      else if (p == MicroSplatBaseFeatures.DebugOutput.Metallic)
      {
         return MicroSplatBaseFeatures.DefineFeature._DEBUG_OUTPUT_METAL;
      }
      else if (p == MicroSplatBaseFeatures.DebugOutput.Normal)
      {
         return MicroSplatBaseFeatures.DefineFeature._DEBUG_OUTPUT_NORMAL;
      }
      else if (p == MicroSplatBaseFeatures.DebugOutput.Smoothness)
      {
         return MicroSplatBaseFeatures.DefineFeature._DEBUG_OUTPUT_SMOOTHNESS;
      }
      else if (p == MicroSplatBaseFeatures.DebugOutput.FinalNormalTangent)
      {
         return MicroSplatBaseFeatures.DefineFeature._DEBUG_FINALNORMALTANGENT;
      }
#if __MICROSPLAT_PROCTEX__
      else if (p == MicroSplatBaseFeatures.DebugOutput.ProceduralSplatOutput0)
      {
         return MicroSplatBaseFeatures.DefineFeature._DEBUG_OUTPUT_SPLAT0;
      }
      else if (p == MicroSplatBaseFeatures.DebugOutput.ProceduralSplatOutput1)
      {
         return MicroSplatBaseFeatures.DefineFeature._DEBUG_OUTPUT_SPLAT1;
      }
      else if (p == MicroSplatBaseFeatures.DebugOutput.ProceduralSplatOutput2)
      {
         return MicroSplatBaseFeatures.DefineFeature._DEBUG_OUTPUT_SPLAT2;
      }
      else if (p == MicroSplatBaseFeatures.DebugOutput.ProceduralSplatOutput3)
      {
         return MicroSplatBaseFeatures.DefineFeature._DEBUG_OUTPUT_SPLAT3;
      }
      else if (p == MicroSplatBaseFeatures.DebugOutput.ProceduralSplatOutput4)
      {
         return MicroSplatBaseFeatures.DefineFeature._DEBUG_OUTPUT_SPLAT4;
      }
      else if (p == MicroSplatBaseFeatures.DebugOutput.ProceduralSplatOutput5)
      {
         return MicroSplatBaseFeatures.DefineFeature._DEBUG_OUTPUT_SPLAT5;
      }
      else if (p == MicroSplatBaseFeatures.DebugOutput.ProceduralSplatOutput6)
      {
         return MicroSplatBaseFeatures.DefineFeature._DEBUG_OUTPUT_SPLAT6;
      }
      else if (p == MicroSplatBaseFeatures.DebugOutput.ProceduralSplatOutput7)
      {
         return MicroSplatBaseFeatures.DefineFeature._DEBUG_OUTPUT_SPLAT7;
      }
#endif

      return MicroSplatBaseFeatures.DefineFeature._DEBUG_OUTPUT_ALBEDO;
   }

   Camera SetupCamera()
   {
      Camera cam = new GameObject("cam").AddComponent<Camera>();
      cam.orthographic = true;
      cam.orthographicSize = 0.5f;
      cam.transform.position = new Vector3(0.5f, 10000.5f, -1);
      cam.nearClipPlane = 0.1f;
      cam.farClipPlane = 2.0f;
      cam.enabled = false;
      cam.depthTextureMode = DepthTextureMode.None;
      cam.clearFlags = CameraClearFlags.Color;
      cam.backgroundColor = Color.grey;
      return cam;
   }

   void Bake(MicroSplatMesh job)
   {
      Camera cam = SetupCamera();
      string baseDir = MicroSplatUtilities.RelativePathFromAsset (job.templateMaterial);
      // for each pass
      int pass = 1;
      Passes lastPass = Passes.Emissive;
#if __MICROSPLAT_PROCTEX__
      lastPass = Passes.ProceduralSplat7;
#endif
      while (pass <= (int)(lastPass))
      {
         Passes p = (Passes)pass;
         pass *= 2;
         if (!IsEnabled(p))
         {
            continue;
         }
         MicroSplatBaseFeatures.DebugOutput debugOutput = OutputFromPass(p);

         var readWrite = (p == Passes.Albedo || p == Passes.Emissive) ?
            RenderTextureReadWrite.sRGB : RenderTextureReadWrite.Linear;
 
         RenderTexture rt = RenderTexture.GetTemporary((int)res, (int)res, 0, RenderTextureFormat.ARGB32, readWrite);
         RenderTexture.active = rt;
         cam.targetTexture = rt;

         var meshes = BuildRenderObjects(job, debugOutput);

         bool fog = RenderSettings.fog;
         if (p == Passes.Normal)
         {
            cam.backgroundColor = new Color (0.5f, 0.5f, 1);
         }
         else if (p == Passes.Smoothness || p == Passes.Emissive || p == Passes.Metallic)
         {
            cam.backgroundColor = Color.black;
         }
         else if (p == Passes.AO)
         {
            cam.backgroundColor = Color.white;
         }
         else
         {
            cam.backgroundColor = Color.gray;
         }
         var ambInt = RenderSettings.ambientIntensity;
         var reflectInt = RenderSettings.reflectionIntensity;
         RenderSettings.ambientIntensity = 0;
         RenderSettings.reflectionIntensity = 0;
         Unsupported.SetRenderSettingsUseFogNoDirty(false);
         cam.Render();
         Unsupported.SetRenderSettingsUseFogNoDirty(fog);

         RenderSettings.ambientIntensity = ambInt;
         RenderSettings.reflectionIntensity = reflectInt;

         Texture2D tex = new Texture2D((int)res, (int)res, TextureFormat.ARGB32, false, (p != Passes.Albedo && p != Passes.Emissive));
         tex.ReadPixels(new Rect(0, 0, (int)res, (int)res), 0, 0);
         RenderTexture.active = null;
         RenderTexture.ReleaseTemporary(rt);

         for (int x = 0; x < tex.width; ++x)
         {
            for (int y = 0; y < tex.height; ++y)
            {
               Color c = tex.GetPixel(x, y);
               c.a = 1;
               tex.SetPixel(x, y, c);
            }
         }

         tex.Apply();

         var bytes = tex.EncodeToTGA();
         string texPath = baseDir + "/" + job.gameObject.name + "_" + debugOutput.ToString();
         System.IO.File.WriteAllBytes(texPath + ".tga", bytes);


         for (int i = 0; i < meshes.Count; ++i)
         {
            if (meshes[i] == null)
               continue;
            MeshRenderer mr = meshes[i].GetComponent<MeshRenderer>();
            MeshFilter mf = meshes[i].GetComponent<MeshFilter>();
            
            if (mr != null && mr.sharedMaterial != null)
            {
               if (mr.sharedMaterial.shader != null)
               {
                  GameObject.DestroyImmediate(meshes[i].GetComponent<MeshRenderer>().sharedMaterial.shader);
               }
               GameObject.DestroyImmediate(meshes[i].GetComponent<MeshRenderer>().sharedMaterial);
            }

            if (mf != null && mf.sharedMesh != null)
            {
               GameObject.DestroyImmediate(meshes[i].GetComponent<MeshFilter>().sharedMesh);
            }

            GameObject.DestroyImmediate(meshes[i]);
            
         }


      }
      GameObject.DestroyImmediate(cam.gameObject);
      AssetDatabase.Refresh();

   }


}

#endif
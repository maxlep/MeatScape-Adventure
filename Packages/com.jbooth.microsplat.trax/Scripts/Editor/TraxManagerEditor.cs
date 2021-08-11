//////////////////////////////////////////////////////
// MicroSplat
// Copyright (c) Jason Booth
//////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using JBooth.MicroSplat;

[CustomEditor(typeof(TraxManager))]
public class TraxManagerEditor : Editor
{

   static GUIContent CBufferSize = new GUIContent ("Buffer Size", "Size of the Buffer Textures");
   static GUIContent CWorldSize = new GUIContent ("World Size", "How big is the area the effect is centered over");
   static GUIContent CLayerMask = new GUIContent ("Layer Mask", "Layer for all objects which impact the surface");
   static GUIContent CRepairDelay = new GUIContent ("Repair Delay", "How long until tracks start to fade, in seconds");
   static GUIContent CRepairRate = new GUIContent ("Total Repair Time", "How long tracks take to decay, in seconds per meter");
   static GUIContent CRepairTotal = new GUIContent ("Stop Repair After", "If set, repairs will stop after this many seconds. This can be useful so tracks remain, but you can run over them again and still get an effect");
   static GUIContent CBufferBlend = new GUIContent ("Buffer Blend", "Controls how hard the cuts are- lower values allow the material to spread more");
   static GUIContent CCollisionDistance = new GUIContent ("Collision Distance", "This controls how far away something begins to collide with the terrain surface. This is because Physics often doesn't let things get close enough, so we use a fudge factor");
   static GUIContent CSinkStrength = new GUIContent ("Sink Strength", "How fast objects sink into the terrain. If this is too high, you can get edging artifacts because the buffer hasn't been filtered yet. Too low, and tracks become faint unless you stay in the same position for a long time");
   static GUIContent CBufferBlits = new GUIContent ("Buffer Blits", "How many blurring passes are applied per frame.");
   static GUIContent CPrecision = new GUIContent ("Precision", "Half precision saves half of the memory, but at the cost of precision of the impressions and time");

   public enum BufferSize
   {
      k128 = 128,
      k256 = 256,
      k512 = 512,
      k1024 = 1024,
      k2048 = 2048,
      k4096 = 4096,
      k8192 = 8192,
   }
   bool advancedFoldout = false;
   public override void OnInspectorGUI ()
   {
      TraxManager tm = (TraxManager)target;

      EditorGUILayout.HelpBox ("How to use:\n1. Create a layer for objects which leave tracks in the terrain.\n2. Assign your objects to the layer, and assign the layer to the property below.\n3. Parent this object to the player, or put it in the middle of the effected areas.\n4. Turn on the features you want in the MicroSplat terrain shader.", MessageType.Info);

      var oldBufferSie = tm.bufferSize;
      var oldUseTime = tm.useTime;
      var oldRepairDelay = tm.repairRate;
      var oldRepairTotal = tm.repairTotal;
      var oldPrecision = tm.precision;

      EditorGUI.BeginChangeCheck ();
      serializedObject.Update ();

      EditorGUILayout.PropertyField (serializedObject.FindProperty ("layerMask"), CLayerMask);

      BufferSize bs = (BufferSize)tm.bufferSize;
      bs = (BufferSize)EditorGUILayout.EnumPopup (CBufferSize, bs);
      tm.bufferSize = (int)bs;
      tm.worldSize = EditorGUILayout.Slider (CWorldSize, tm.worldSize, 16, 4096);
      tm.precision = (TraxManager.Precision)EditorGUILayout.EnumPopup (CPrecision, tm.precision);
      // display pixel size
      float ps = tm.worldSize / tm.bufferSize;
      EditorGUILayout.HelpBox (string.Format ("Pixel Size: {0:0.00} meters\nAnything smaller that twice this size cannot be represented accurately in the buffer", ps), MessageType.None);
      
      
      tm.useTime = EditorGUILayout.Toggle("Time Manager", tm.useTime);
      if (tm.useTime)
      {
         EditorGUI.indentLevel++;
         tm.repairDelay = EditorGUILayout.FloatField (CRepairDelay, tm.repairDelay);
         tm.repairRate = EditorGUILayout.FloatField (CRepairRate, tm.repairRate);
         tm.repairTotal = EditorGUILayout.FloatField (CRepairTotal, tm.repairTotal);
         EditorGUI.indentLevel--;
      }

      advancedFoldout = EditorGUILayout.Foldout (advancedFoldout, "Advanced");
      if (advancedFoldout)
      {
         tm.bufferBlend = EditorGUILayout.Slider (CBufferBlend, tm.bufferBlend, 0.2f, 1.0f);
         tm.collsionDistance = EditorGUILayout.Slider (CCollisionDistance, tm.collsionDistance, 0.25f, 1.5f);
         tm.sinkStrength = EditorGUILayout.Slider (CSinkStrength, tm.sinkStrength, 0.2f, 1.0f);
         tm.bufferBlits = EditorGUILayout.IntSlider (CBufferBlits, tm.bufferBlits, 0, 3);
      }

      // compute debug data
      int mem = 0;
      int bz = tm.bufferSize * tm.bufferSize;
      if (tm.precision == TraxManager.Precision.Full)
      {
         if (tm.useTime && (tm.repairTotal > 0 || tm.repairDelay > 0))
         {
            mem = bz * 5 * 4; // 5 channels, 4 bytes
         }
         else
         {
            mem = bz * 3 * 4; // 3 channels, 4 bytes
         }
      }
      else
      {
         if (tm.useTime)
         {
            mem = bz * 5 * 4;
         }
         else
         {
            mem = bz * 3 * 2;
         }
      }


      EditorGUILayout.HelpBox ("Total Memory for buffers: " + System.Convert.ToDecimal (mem).ToString ("#,##0"), MessageType.Info);

      if (EditorGUI.EndChangeCheck())
      {
         EditorUtility.SetDirty (tm);

         // resetup the buffers if certain things have changed
         if (oldBufferSie != tm.bufferSize ||
            oldUseTime != tm.useTime ||
            (oldRepairDelay <= 0 && tm.repairDelay > 0) ||
            (oldRepairDelay > 0 && tm.repairDelay <= 0) ||
            (oldRepairTotal <= 0 && tm.repairTotal > 0) ||
            (oldPrecision != tm.precision) ||
            (oldRepairTotal > 0 && tm.repairTotal <= 0))
         {
            tm.Setup ();
         }
         
      }

      serializedObject.ApplyModifiedProperties ();
      
   }
}

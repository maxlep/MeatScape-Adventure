using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[InlineEditor(InlineEditorObjectFieldModes.Foldout)]
[CreateAssetMenu(fileName = "ParticleSystemSceneReference", menuName = "SceneReferences/ParticleSystemSceneReference", order = 0)]
public class ParticleSystemSceneReference : SceneReference<ParticleSystem> {}

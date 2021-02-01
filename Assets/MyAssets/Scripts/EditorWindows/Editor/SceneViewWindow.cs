using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MyAssets.ScriptableObjects.Variables;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// SceneViewWindow class.
/// </summary>
public class SceneViewWindow : EditorWindow
{
    /// <summary>
    /// Tracks scroll position.
    /// </summary>
    private Vector2 scrollPos;

    private static int recentSceneCount = 10;
    private static List<(String name, String path)> recentSceneTuples = new List<(string name, string path)>();
    
    /// <summary>
    /// Initialize window state.
    /// </summary>
    [MenuItem("Window/Scene View")]
    internal static void Init()
    {
        // EditorWindow.GetWindow() will return the open instance of the specified window or create a new
        // instance if it can't find one. The second parameter is a flag for creating the window as a
        // Utility window; Utility windows cannot be docked like the Scene and Game view windows.
        var window = (SceneViewWindow)GetWindow(typeof(SceneViewWindow), false, "Scene View");
        window.position = new Rect(window.position.xMin + 100f, window.position.yMin + 100f, 200f, 400f);
    }
    
    private void OnEnable()
    {
        EditorSceneManager.sceneOpened += RegisterRecentScene;
        RegisterRecentScene(EditorSceneManager.GetActiveScene(), OpenSceneMode.Single);
    }

    private void OnDisable()
    {
        EditorSceneManager.sceneOpened -= RegisterRecentScene;
    }

    private void RegisterRecentScene(Scene scene, OpenSceneMode mode)
    {
        //If contains, remove before re-adding to top
        if (recentSceneTuples.Contains((scene.name, scene.path)))
        {
            recentSceneTuples.Remove((scene.name, scene.path));
        }
        
        recentSceneTuples.Add((scene.name, scene.path));

        //Limit number of recent scenes in list
        if (recentSceneTuples.Count > recentSceneCount)
        {
            recentSceneTuples.RemoveAt(0);
        }
    }

    /// <summary>
    /// Called on GUI events.
    /// </summary>
    internal void OnGUI()
    {
        EditorGUILayout.BeginVertical();
        this.scrollPos = EditorGUILayout.BeginScrollView(this.scrollPos, false, false);
        
        //Show recent scenes
        GUILayout.Label("Recent Scenes", EditorStyles.boldLabel);
        for (var i = recentSceneTuples.Count - 1; i > -1; i--)
        {
            var pressed = GUILayout.Button(i + ": " + recentSceneTuples[i].name, new GUIStyle(GUI.skin.GetStyle("Button")) { alignment = TextAnchor.MiddleLeft });
            if (pressed)
            {
                if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                {
                    EditorSceneManager.OpenScene(recentSceneTuples[i].path);
                }
            }
            
        }
        
        //Show scenes in build
        GUILayout.Label("Scenes In Build", EditorStyles.boldLabel);
        for (var i = 0; i < EditorBuildSettings.scenes.Length; i++)
        {
            var scene = EditorBuildSettings.scenes[i];
            if (scene.enabled)
            {
                var sceneName = Path.GetFileNameWithoutExtension(scene.path);
                var pressed = GUILayout.Button(i + ": " + sceneName, new GUIStyle(GUI.skin.GetStyle("Button")) { alignment = TextAnchor.MiddleLeft });
                if (pressed)
                {
                    if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                    {
                        EditorSceneManager.OpenScene(scene.path);
                    }
                }
            }
        }
        
        
        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }
}
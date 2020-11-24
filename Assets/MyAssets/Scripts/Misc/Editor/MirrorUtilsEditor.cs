using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
public class MirrorUtilsEditor : MonoBehaviour
{
    [MenuItem("Mirror Utils/Mirror Selected Transform on X Axis")]
    public static void DuplicateSelectedObject()
    {
        if (Selection.gameObjects.Length == 0)
        {
            Debug.LogError("No selection of Transforms to mirror!");
            return;
        }

        // Make a copy of the original objects hierarchy index, so that we can fix the order later on
        SortedDictionary<int, GameObject> copiedObjectOriginalHierarchyIndex = new SortedDictionary<int, GameObject>();

        // This is where Unity will place the duplicated objects, below the current children of the parent object
        int startHierarchyIndex = Selection.gameObjects[0].transform.parent.childCount;

        for (int i = 0; i < Selection.gameObjects.Length; i++)
        {
            GameObject objectToCopy = Selection.gameObjects[i];
            int hierarchyIndex = objectToCopy.transform.GetSiblingIndex();
            GameObject copy = Editor.Instantiate(objectToCopy, objectToCopy.transform.parent);

            // Add the hierarchy location to the sorted list
            copiedObjectOriginalHierarchyIndex.Add(hierarchyIndex, copy);

            // Translate the objects position in the X axis
            copy.transform.Translate(-copy.transform.localPosition.x * 2, 0, 0, Space.World);
            // Invert the X rotation
            copy.transform.localRotation = new Quaternion(copy.transform.localRotation.x * -1.0f,
                                            copy.transform.localRotation.y,
                                            copy.transform.localRotation.z,
                                            copy.transform.localRotation.w * -1.0f);

            // Run a few string replaces to change Left, _L etc to Right, _R for easier readibility
            string name = objectToCopy.name;
            name = name.Replace("Left", "Right");
            name = name.Replace("_L", "_R");
            name = name.Replace(" L", " R");
            copy.name = name;

            //Recurse for children as well
            MirrorNamesRecursively(copy.transform);
        }

        // Now, sort the objects based on their origal objects index, and force their positions
        int orderOfItems = 0;
        foreach (var item in  copiedObjectOriginalHierarchyIndex)
        {
            item.Value.transform.SetSiblingIndex(startHierarchyIndex + orderOfItems);
            orderOfItems++;
        }
    }

    private static void MirrorNamesRecursively(Transform trans)
    {
        //Recurse for children as well
        foreach (Transform child in trans)
        {
            MirrorNamesRecursively(child);
        }
        
        string newName = trans.name;
        newName = newName.Replace("Left", "Right");
        newName = newName.Replace("_L", "_R");
        newName = newName.Replace(" L", " R");
        trans.name = newName;
    }
}
#endif
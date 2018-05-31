using System.Collections.Generic;
using DatabaseHelper;
using UnityEditor;
using UnityEngine;

namespace DialogueSystem {
    public static class SaveManager {
        static Dictionary<int, ScriptableObject> refDict;
        static List<ScriptableObject> savedSO;

        #region Saving Methods

        public static EditorCache SaveCanvas (string filePath, bool createCopy, EditorCache cache) {
            if (!ValidPath (ref filePath)) {
                Debug.LogError ("the file path: " + filePath + ", does not exist");
                return cache;
            }
            Reset ();

            if (createCopy) {
                CopyRefs (cache);
                cache = ReplaceSO (cache) as EditorCache;
                ReplaceRefs (cache);
            }
            SaveAsset (cache, filePath);

            foreach (ScriptableObject objRef in cache.GetAllReferences (true)) {
                if (!objRef)
                    throw new UnityException ("' " + objRef.GetType () + "' Database is missing reference. Cannot save the dialogue canvas.");
                SaveRefs (objRef, cache, false);
            }
            DialogueEditorGUI.Save ();
            return cache;
        }

        public static void SaveObjects (ScriptableObject subObj, ScriptableObject mainObj) {
            if (!subObj || !mainObj) 
                throw new UnityException ("The " + (!subObj ? "sub" : "main") + " object is missing reference. Cannot save the object's references.");

            if (!AssetDatabase.Contains (mainObj) || AssetDatabase.Contains (subObj))
                return;
            Reset ();
            SaveRefs (subObj, mainObj, false);
            DialogueEditorGUI.Save ();
        }

        #region Copy Utilities

        static void CopyRefs (ScriptableObject item) {
            Stack<ScriptableObject> workStack = new Stack<ScriptableObject> ();
            workStack.Push (item);

            while (workStack.Count > 0) {
                item = workStack.Pop ();

                if (!item)
                    continue;

                if (CloneAndAddToList (item))
                    if (item is ISOHandling)
                        foreach (ScriptableObject obj in (item as ISOHandling).GetAllReferences (false))
                            workStack.Push (obj);
            }
        }

        static bool CloneAndAddToList (ScriptableObject item) {
            if (refDict.ContainsKey (item.GetInstanceID ()))
                return false;
            refDict.Add (item.GetInstanceID (), Clone (item));
            return true;
        }

        static ScriptableObject Clone (ScriptableObject item) {
            ScriptableObject newSO = Object.Instantiate (item);
            newSO.name = item.name;
            return newSO;
        }

        #endregion

        #region Replace Utilities

        static void ReplaceRefs (ScriptableObject item) {
            Stack<ScriptableObject> workStack = new Stack<ScriptableObject> ();
            workStack.Push (item);

            while (workStack.Count > 0) {
                item = workStack.Pop ();

                if (!item)
                    continue;

                if (item is ISOHandling) {
                    ISOHandling handlingObj = item as ISOHandling;

                    if (handlingObj.GetAllReferences (true).Length == 0)
                        continue;
                    int number = 0;

                    while (!handlingObj.GetAllReferences (true)[number])
                        number++;

                    if (!refDict.ContainsValue (handlingObj.GetAllReferences (true)[number])) {
                        handlingObj.ReplaceAllReferences (so => ReplaceSO (so));

                        foreach (ScriptableObject obj in handlingObj.GetAllReferences (false))
                            workStack.Push (obj);
                    }
                }
            }
        }

        static ScriptableObject ReplaceSO (ScriptableObject item) {
            if (!item)
                return null;

            if (refDict.ContainsValue (item)) {
                Debug.LogWarning ("The object '" + item.name + ", " + item.GetInstanceID () + "'  is already replaced.");
                return item;
            }
            ScriptableObject clonedObj;

            if (refDict.TryGetValue (item.GetInstanceID (), out clonedObj)) {
                //Debug.Log ("Replaced: " + item.name + ", " + item.GetInstanceID () + " to " + clonedObj.GetInstanceID ());
                return clonedObj;
            } else if (CloneAndAddToList (item)) {
                Debug.LogWarning ("The object '" + item.name + ", " + item.GetInstanceID () + "' was not cloned. Returning a properly cloned object and added the object to list.");
                item = ReplaceSO (item);
                ReplaceRefs (item);
                return item;
            } else
                throw new UnityException ();
        }

        #endregion

        #region Save Utilities

        static void SaveRefs (ScriptableObject subSO, ScriptableObject mainSO, bool saveAllRefs) {
            Stack<ScriptableObject> workStack = new Stack<ScriptableObject> ();
            workStack.Push (subSO);

            while (workStack.Count > 0) {
                subSO = workStack.Pop ();

                if (!subSO || !mainSO)
                    continue;

                if (savedSO.Contains (subSO))
                    continue;
                SaveSubAsset (subSO, mainSO);

                if (subSO is ISOHandling)
                    foreach (ScriptableObject obj in (subSO as ISOHandling).GetAllReferences (false))
                        workStack.Push (obj);
            }
        }

        static void SaveAsset (ScriptableObject obj, string savePath) {
            if (!obj)
                throw new UnityException ();

            if (string.IsNullOrEmpty (savePath))
                throw new UnityException ();
            AssetDatabase.CreateAsset (obj, savePath);
            savedSO.Add (obj);
        }

        static void SaveSubAsset (ScriptableObject obj, string savePath) {
            if (!obj)
                throw new UnityException ();

            if (string.IsNullOrEmpty (savePath))
                throw new UnityException ();
            AssetDatabase.AddObjectToAsset (obj, savePath);
            obj.hideFlags = HideFlags.HideInHierarchy;
            savedSO.Add (obj);
        }

        static void SaveSubAsset (ScriptableObject subObj, ScriptableObject mainObj) {
            if (!mainObj || !subObj)
                throw new UnityException ();
            AssetDatabase.AddObjectToAsset (subObj, mainObj);
            subObj.hideFlags = HideFlags.HideInHierarchy;
            savedSO.Add (subObj);
        }

        #endregion

        #endregion

        #region Loading Methods

        public static EditorCache LoadCanvas (string filePath) {
            EditorCache cache;
            if (TryLoadResource (filePath, out cache))
                cache.Init ();
            return cache;
                    
        }

        #region Loading Utilities

        public static bool TryLoadResource<T> (string filePath, out T asset) where T : ScriptableObject {
            if (!ValidPath (ref filePath))
                throw new UnityException ();
                asset = AssetDatabase.LoadAssetAtPath (filePath, typeof (T)) as T;

            if (!asset)
                return false;
            return true;
        }

        static Object[] LoadResources (string filePath) {
            if (string.IsNullOrEmpty (filePath))
                throw new UnityException ();
            return AssetDatabase.LoadAllAssetsAtPath (filePath);
        }

        #endregion

        #endregion

        #region Utility Methods

        static void Reset () {
            refDict = new Dictionary<int, ScriptableObject> ();
            savedSO = new List<ScriptableObject> ();
        }

        static bool ValidPath (ref string path) {
            if (string.IsNullOrEmpty (path)) {
                Debug.LogError ("");
                return false;
            }

            if (path.Contains (Application.dataPath))
                path = path.Replace (Application.dataPath, "Assets");

            if (AssetDatabase.IsValidFolder (path.Contains (".") ? path.Remove (path.LastIndexOf ("/")) : path))
                return true;
            return false;
        }

        #endregion
    }
}

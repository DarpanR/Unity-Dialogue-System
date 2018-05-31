using System;
using UnityEditor;
using UnityEngine;

namespace DialogueSystem {
    public class EditorCache : ScriptableObject, DatabaseHelper.ISOHandling {
        [SerializeField]
        NodeDatabase nodes;
        [SerializeField]
        ActorDatabase actors;
        [SerializeField]
        ConditionDatabase conditions;
        [SerializeField]
        EditorStates states;

        public NodeDatabase Nodes { get { return nodes; } private set { nodes = value; } }
        public ActorDatabase Actors { get { return actors; } private set { actors = value; } }
        public ConditionDatabase Conditions { get { return conditions; } private set { conditions = value; } }
        public EditorStates States { get { return states; } private set { states = value; } }

        [SerializeField]
        string canvasName;
        [SerializeField]
        string savePath;

        public string CanvasName { get { return canvasName; } private set { canvasName = value; } }
        public string SavePath { get { return savePath; } private set { savePath = value; } }
        public bool IsFileSaved { get { return !string.IsNullOrEmpty (SavePath) || AutoLoaded; } }
        public bool AutoLoaded { get { return AssetDatabase.GetAssetPath (this).Equals (ResourceManager.TEMPFILEPATH); } }

        #region Instantiation Methods

        public static EditorCache NewCache () {
            EditorCache cache = CreateInstance<EditorCache> ();
            cache.CanvasName = "New Canvas";
            cache.Actors = ActorDatabase.CreateNew<ActorDatabase> (cache.CanvasName);
            cache.Conditions = ConditionDatabase.CreateNew<ConditionDatabase> (cache.CanvasName);
            cache.Nodes = NodeDatabase.CreateNew<NodeDatabase> (cache.CanvasName);
            cache.States = new EditorStates (cache);

            cache.Init ();
            SaveManager.SaveCanvas (ResourceManager.TEMPFILEPATH, false, cache);
            return cache;
        }

        public static EditorCache LoadCache () {
            EditorCache cache = SaveManager.LoadCanvas (ResourceManager.TEMPFILEPATH) ?? NewCache ();
            return cache;
        }

        public static EditorCache LoadCache (string filePath) {
            EditorCache cache = SaveManager.LoadCanvas (filePath);

            if (!cache.AutoLoaded)
                SaveManager.SaveCanvas (ResourceManager.TEMPFILEPATH, cache.IsFileSaved, cache);
            return cache;
        }

        public static EditorCache LoadCache (EditorCache loadedCache) {
            EditorCache cache = CreateInstance<EditorCache> ();

            cache.Nodes = loadedCache.Nodes;
            cache.Actors = loadedCache.Actors;
            cache.Conditions = loadedCache.Conditions;
            cache.States = loadedCache.States;

            SaveManager.SaveCanvas (ResourceManager.TEMPFILEPATH, true, cache);
            cache.Init ();
            return cache;
        }

        public static EditorCache SaveCache (string filePath, EditorCache cache) {
            if (filePath == ResourceManager.TEMPFILEPATH) {
                Debug.LogWarning ("Dont be saving stuff here dawg. plox file somewhere else to saving.");
                return cache;
            }
            cache = SaveManager.SaveCanvas (filePath, cache.IsFileSaved, cache);
            cache.SavePath = filePath;
            cache.Init ();
            return cache;
        }

        #region Instantiation Utilities

        public void Init () {
            States = new EditorStates (this);
            DialogueEditorGUI.UpdateEnvironment (this);
            Actors.Init ();
            Conditions.Init ();
            Nodes.Init ();
        }

        public static bool ValidateCanvas (EditorCache cache) {
            if (!cache) {
                Debug.LogError ("Validation failed. No fking cache mate");
                return false;
            }

            if (string.IsNullOrEmpty (cache.CanvasName)) {
                Debug.LogError ("Validation failed. This Cache does not have a name.");
                return false;
            }

            if (cache.States == null) {
                cache.states = new EditorStates (cache);
                //Debug.LogError ("Validation failed. This Cache does not have a EditorStates reference");
                //return false;
            }

            foreach (ScriptableObject obj in cache.GetAllReferences (true)) {
                if (!obj) {
                    Debug.LogError ("Validation failed. The reference object '" + obj.GetType ().Name + "' is missing reference.");
                    return false;
                }

                if (!obj.name.Contains (cache.CanvasName)) {
                    Debug.LogError ("Validation failed. The reference object '" + obj.GetType ().Name + "' does not match the cache.");
                    return false;
                }
                //if (!cache.ValidateObject (obj))
                //    return false;
            }
            return true;
        }

        //bool ValidateObject (ScriptableObject obj) {
        //    if (!obj) {
        //        Debug.LogError ("Validation failed. The reference object '" + obj.GetType ().Name + "' is missing Reference.");
        //        return false;
        //    }

        //    if (obj is DatabaseHelper.ISOHandling)
        //        foreach (ScriptableObject refObj in (obj as DatabaseHelper.ISOHandling).GetAllReferences (false))
        //            if (!ValidateObject (refObj))
        //                return false;
        //    return true;
        //}

        public void OnDestroy () {
            foreach (ScriptableObject obj in GetAllReferences (true))
                EditorUtility.SetDirty (obj);
        }

        #endregion

        #endregion

        #region Serialization Methods

        public ScriptableObject[] GetAllReferences (bool fetchAllRefs) {
            return new ScriptableObject[] { Nodes, Actors, Conditions };
        }

        public void ReplaceAllReferences (Func<ScriptableObject, ScriptableObject> ReplacedSO) {
            nodes = ReplacedSO (nodes) as NodeDatabase;
            actors = ReplacedSO (actors) as ActorDatabase;
            conditions = ReplacedSO (conditions) as ConditionDatabase;
        }

        public void SaveNewObject (CanvasObject saveObject) {
            SaveNewObject (saveObject, false);
        }

        public void SaveNewObject (CanvasObject saveObject, bool updateSelection) {
            SaveManager.SaveObjects (saveObject, this);

            if (updateSelection)
                DialogueEditorGUI.UpdateSelection (new EditorStates (saveObject.Position.center.ToString (), States));
        }
        
        public void SaveNewObject (CanvasObject subObject, CanvasObject mainObject, bool updateSelection = false) {
            SaveManager.SaveObjects (subObject, mainObject);

            if (updateSelection)
                DialogueEditorGUI.UpdateSelection (new EditorStates (subObject.Position.center.ToString(), States));
        }
    }

        #endregion
}

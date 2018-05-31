using System;
using System.Reflection;
using UnityEngine;
using UnityEditor;

namespace DialogueSystem {
    public class DialogueEditorWindow : EditorWindow {
        static DialogueEditorWindow curWindow;

        DateTime lastCheck;
        EditorCache cache;
        bool openDebug;

        #region Editow Window Methods

        [MenuItem ("Window/Dialogue Editor %#m")]
        public static void OpenWindow () {
            curWindow = GetWindow<DialogueEditorWindow> ("Dialogue System");
            curWindow.titleContent = new GUIContent ("Dialogue Editor");
            curWindow.minSize = new Vector2 (800, 800);
            curWindow.Show ();
        }

        [UnityEditor.Callbacks.OnOpenAsset (1)]
        private static bool AutoOpenCanvas (int instanceID, int line) { 
            if (EditorUtility.InstanceIDToObject (instanceID) is EditorCache) {
                OpenWindow ();
                curWindow.cache = EditorCache.LoadCache (AssetDatabase.GetAssetPath (instanceID));
                curWindow.lastCheck = DateTime.Now;
                return true;
            }
            return false;
        }

        public static void InitCanvas () {
            ResourceManager.SetupPaths ();
            CanvasGUI.SetupGUI ();
            NodeTypes.FetchAllNodes ();
            NoduleTypes.FetchAllNodules ();
            OverlayMenuTypes.FetchAllOverlayMenus ();
            InputSystem.SetupInputHandlers ();
        }

        public void OnEnable () {
            OnDestroy ();
            DialogueEditorGUI.OnRepaint += Repaint;
            DialogueEditorGUI.OnSave += AssetDatabase.SaveAssets;
            DialogueEditorGUI.OnSave += AssetDatabase.Refresh;
            DialogueEditorGUI.OnSave += Repaint;
            lastCheck = DateTime.Now.Add (-ResourceManager.CheckRate);
            InitCanvas ();
        }

        public void OnDestroy () {
            DialogueEditorGUI.OnRepaint -= Repaint;
            DialogueEditorGUI.OnSave -= Repaint;
            DialogueEditorGUI.OnSave -= AssetDatabase.SaveAssets;
            DialogueEditorGUI.OnSave -= AssetDatabase.Refresh;
        }

        #endregion

        #region OnGUI

        public void OnGUI () {
            CanvasGUI.Position = position;

            if (DateTime.Now - lastCheck >= ResourceManager.CheckRate) {
                if (!EditorCache.ValidateCanvas (cache)) {
                    Debug.LogWarning ("Loading last opened session.");
                    InitCanvas ();
                    cache = EditorCache.LoadCache ();
                } else
                    cache.Init ();
                lastCheck = DateTime.Now;
            }

            InputSystem.EarlyInputEvents (cache.States);
            DialogueEditorGUI.OnGUI (cache.States);
            ToolBar ();
            InputSystem.LateInputEvents (cache.States);
            cache.States.UpdateEvents ();

            if (openDebug = CanvasGUI.Toggle (new Rect (5, 5, 130, 20), new GUIContent ("Debug Window"), openDebug))
                DebuggerWindow ();
        }

        void DebuggerWindow () {
            EditorState state = cache.States.curState;
            float curheight = 5;

            //TODO: cache debug
            //TODO: states debug
            //TODO: state debug into states debug

            foreach (FieldInfo field in state.GetType ().GetFields ())
                if (field.FieldType.IsArray) {
                    Array list = field.GetValue (state) as Array;

                    if (list.GetValue (0) == null)
                        continue;
                    GUI.Label (new Rect (5, curheight += 20, 1000, 20), new GUIContent (field.Name + ": "));

                    for (int j = 0; j < list.Length; j++)
                        GUI.Label (new Rect (10, curheight += 20, 1000, 20), new GUIContent (list.GetValue (j).ToString ()));
                } else
                    GUI.Label (new Rect (5, curheight += 20, 1000, 20), new GUIContent (field.Name + ": " + (field.GetValue (state) == null ? "null" : field.GetValue (state).ToString ())));
        }

        void ToolBar () {
            CanvasGUI.BeginGroup (CanvasGUI.ToolBarRect, GUI.skin.box, cache.States.curSpace == EventSpace.Toolbar);

            GUI.Label (new Rect (5, 5, CanvasGUI.ToolBarRect.width - 10, 20), new GUIContent (cache.CanvasName + (cache.AutoLoaded ? " (Last Session)" : "")));

            if (CanvasGUI.Button (new Rect (5, 30, CanvasGUI.ToolBarRect.width - 10, 20), "Save"))
                cache = (cache.IsFileSaved && !cache.AutoLoaded) ? EditorCache.SaveCache (cache.SavePath ?? AssetDatabase.GetAssetPath (cache), cache) : SaveCanvas ();

            if (CanvasGUI.Button (new Rect (5, 55, CanvasGUI.ToolBarRect.width - 10, 20), "Save As"))
                SaveCanvas ();

            if (CanvasGUI.Button (new Rect (5, 80, CanvasGUI.ToolBarRect.width - 10, 20), "Load"))
                LoadCanvas ();

            if (CanvasGUI.Button (new Rect (5, 105, CanvasGUI.ToolBarRect.width - 10, 20), "New Canvas"))
                cache = EditorCache.NewCache ();

            if (CanvasGUI.Button (new Rect (5, 130, CanvasGUI.ToolBarRect.width - 10, 20), "Settings"))
                Setting (); 

            CanvasGUI.EndGroup ();
        }

        public EditorCache LoadCanvas () {
            return LoadnSaveAs (true);
        }

        public EditorCache SaveCanvas () {
            return LoadnSaveAs (false);
        }

        EditorCache LoadnSaveAs (bool load) {
            string filePath = (load) ? EditorUtility.OpenFilePanel ("Load Dialogue Canvas", ResourceManager.SAVEPATH, "asset") :
                EditorUtility.SaveFilePanelInProject ("Save Dialogue Canvas", cache.CanvasName, "asset", "Select path to save Dialogue Canvas.", ResourceManager.SAVEPATH);

            if (string.IsNullOrEmpty (filePath)) {
                ShowNotification (new GUIContent ("No " + (load ? "load" : "save") + " path chosen."));
                return cache;
            }
            return (load) ? EditorCache.LoadCache (filePath) : EditorCache.SaveCache (filePath, cache);
        }

        #endregion

        void Setting () { }
    }
}

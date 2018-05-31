using System;
using UnityEngine;	

namespace DialogueSystem {
    public static class DialogueEditorGUI {
        public static EditorCache Cache { get; private set; }
        public static EditorStates States { get; private set; }

        public static Action OnRepaint;
        public static void Repaint () { OnRepaint.Invoke (); }
        public static Action OnSave;
        public static void Save () { OnSave.Invoke (); }

        enum OptionTab { actor, condition };
        static OptionTab tab = OptionTab.actor;

        public static void UpdateEnvironment (EditorCache cache) {
            Cache = cache;
            States = cache.States;
        }

        public static void OnGUI (EditorStates states) {
            States = states;
            EditorState curState = states.curState;

            CanvasGUI.BeginGroup (new Rect (curState.panDelta, curState.canvasSize), (states.curSpace | EventSpace.CanvasSpace) == EventSpace.CanvasSpace);

            for (int i = 0; i < Cache.Nodes.Count; i++)
                Cache.Nodes.Get (i).DrawConnection ();

            if (curState.connectionIndex > -1) {
                BaseNodule startNodule = curState.selectedObject as BaseNodule;

                if (curState.connectionIndex > startNodule.Nodules.Count - 1)
                    InputHandlers.OnSelectConnection (states);
                else
                    CanvasGUI.DrawConnection (startNodule, startNodule.Nodules.Get (curState.connectionIndex), Color.red);
            }
            Cache.Nodes.OnGUI ();

            CanvasGUI.EndGroup ();
            CanvasGUI.BeginGroup (CanvasGUI.OptionRect, GUI.skin.box, states.curSpace == EventSpace.Actor);

            switch (tab) {
                case OptionTab.actor:
                Cache.Actors.OnGUI ();
                break;

                case OptionTab.condition:
                Cache.Conditions.OnGUI ();
                break;
            }

            if (CanvasGUI.Button (new Rect (5, 5, 70, 20), "Conditions"))
                tab = OptionTab.condition;
            else if (CanvasGUI.Button (new Rect (80, 5, 50, 20), "Actors"))
                tab = OptionTab.actor;

            CanvasGUI.EndGroup ();
        }

        public static void UpdateSelection () {
            UpdateSelection (new EditorStates ("none", States));
        }

        public static void UpdateSelection (EditorStates states) {
            if (string.IsNullOrEmpty (states.info))
                throw new UnityException ();

            if (states.info == "none")
                States.curState.focusedObjects = new ScriptableObject[] { null };
            else {
                states.info = states.info.Substring (1, states.info.Length - 2);
                string[] coords = states.info.Split (',');

                if (coords.Length == 2)
                    states.curState.focusedObjects = CanvasGUI.ObjectAtPosition (new Vector2 (float.Parse (coords[0]), float.Parse (coords[1])));
            }
            InputSystem.HandleEventSpace (states);
            InputSystem.HandleSelecting (states);
        }
    }	
}

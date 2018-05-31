using System;
using System.Linq;
using UnityEngine;	

namespace DialogueSystem {
    public static class InputHandlers {

        #region Event Handler

        #region Node Drag

        [EventHandler (EventType.MouseDown, 120, EventSpace.Node)]
        public static void StartNodeDrag (EditorStates states) {
            if (states.curEvent.type == EventType.Used)
                return;
            EditorState state = states.curState;

            if (states.curEvent.button == 0 && !state.dragNode && GUIUtility.hotControl == 0) {
                state.dragStartPos = state.startPos = states.mousePos;
                state.dragNode = true;
            } else if (state.dragNode)
                EndNodeDrag (states);
        }

        [EventHandler (EventType.MouseDrag)]
        public static void OnNodeDrag (EditorStates states) {
            EditorState state = states.curState;

            if (state.dragNode) {
                if (states.mousePos.x < CanvasGUI.CanvasRect.xMin || states.mousePos.x > CanvasGUI.CanvasRect.xMax)
                    states.mousePos.x = state.dragStartPos.x;

                if (states.mousePos.y < CanvasGUI.CanvasRect.yMin || states.mousePos.y > CanvasGUI.CanvasRect.yMax)
                    states.mousePos.y = state.dragStartPos.y;
                BaseNode node = state.selectedObject as BaseNode;

                if (node is OptionNode && (node as OptionNode).MainNode)
                    EndNodeDrag (states);
                state.dragDelta = states.mousePos - state.dragStartPos;
                node.UpdateAllPosition (state.dragDelta);
                state.dragStartPos = states.mousePos;
                DialogueEditorGUI.Repaint ();
            }
        }

        [EventHandler (EventType.MouseUp)]
        public static void EndNodeDrag (EditorStates states) {
            EditorState state = states.curState;

            if (state.dragNode) {
                if (state.selectedObject is OptionNode && (states.mousePos - state.startPos).magnitude > 10) {
                    states.mousePos = CanvasGUI.CanvasToScreenPosition (state, states.mousePos);

                    foreach (MainNode node in DialogueEditorGUI.Cache.Nodes.OfType<MainNode> ())
                        if (node.Options.OptionAddRect.Contains (states.mousePos)) {
                            DialogueEditorGUI.Cache.Nodes.Remove (state.selectedObject as OptionNode);
                            node.Options.Add (state.selectedObject as OptionNode);
                            break;
                        }
                }
                state.dragNode = false;
                DialogueEditorGUI.Repaint ();
            }
        }

        #endregion

        #region Window Pan

        [EventHandler (EventType.MouseDown, 110, EventSpace.Canvas)]
        public static void StartWindowPan (EditorStates states) {
            if (states.curEvent.type == EventType.Used)
                return;
            EditorState state = states.curState;

            if (states.curEvent.button == 0 && !state.panWindow) {
                state.dragStartPos = states.mousePos;
                state.panWindow = true;
            } else if (state.panWindow)
                EndWindowPan (states);
        }

        [EventHandler (EventType.MouseDrag)]
        public static void OnWindowPan (EditorStates states) {
            EditorState state = states.curState;

            if (state.panWindow) {
                if (states.mousePos.x < CanvasGUI.CanvasRect.xMin || states.mousePos.x > CanvasGUI.CanvasRect.xMax)
                    states.mousePos.x = state.dragStartPos.x;

                if (states.mousePos.y < CanvasGUI.CanvasRect.yMin || states.mousePos.y > CanvasGUI.CanvasRect.yMax)
                    states.mousePos.y = state.dragStartPos.y;
                state.panDelta += states.mousePos - state.dragStartPos;

                if (state.panDelta.x > 0)
                    state.panDelta.x = 0;
                else if ((state.panDelta - CanvasGUI.CanvasRect.size).x < -state.canvasSize.x)
                    state.panDelta.x = (CanvasGUI.CanvasRect.size - state.canvasSize).x;

                if (state.panDelta.y > 0)
                    state.panDelta.y = 0;
                else if ((state.panDelta - CanvasGUI.CanvasRect.size).y < -state.canvasSize.y)
                    state.panDelta.y = (CanvasGUI.CanvasRect.size - state.canvasSize).y;
                state.dragStartPos = states.mousePos;
                DialogueEditorGUI.Repaint ();
            }
        }

        [EventHandler (EventType.MouseUp)]
        public static void EndWindowPan (EditorStates states) {
            EditorState state = states.curState;

            if (state.panWindow)
                state.panWindow = false;
        }

        #endregion

        #region Nodule Connection

        [EventHandler (EventType.MouseDown, 10)]
        public static void OnMakingConnection (EditorStates states) {
            EditorState state = states.curState;

            if (state.makeConnection) {
                switch (states.curSpace) {
                    case EventSpace.Node:
                    BaseNode node = state.selectedObject as BaseNode;

                    state.endNodule = node.Nodules.Find (i => i.GetType () == (state.startNodule is OutputNodule ?
                    typeof (InputNodule) : typeof (OutputNodule))) ??
                    BaseNodule.Create (state.startNodule is OutputNodule ? "InputNodule" : "OutputNodule", node);

                    if (!state.endNodule.MainNode)
                        goto default;

                    if (state.startNodule && state.endNodule) {
                        node.Nodules.Add (state.endNodule);
                        state.startNodule.Nodules.Add (state.endNodule);
                    }
                    goto default;

                    case EventSpace.Nodule:
                    state.endNodule = state.selectedObject as BaseNodule;

                    if (state.startNodule && state.endNodule)
                        state.startNodule.Nodules.Add (state.endNodule);
                    goto default;

                    default:
                    state.startNodule = null;
                    state.endNodule = null;
                    state.makeConnection = false;
                    break;
                }
            }
        }

        [EventHandler (EventType.MouseDown, 85)]
        public static void OnSelectConnection (EditorStates states) {
            EditorState state = states.curState;

            switch (states.curSpace) {
                case EventSpace.Nodule:
                int count = (state.selectedObject as BaseNodule).Nodules.Count;
                state.connectionIndex = (count > 0 && state.connectionIndex < count - 1) ? state.connectionIndex + 1 : (count == 0) ? -1 : 0;
                break;

                default:
                state.connectionIndex = -1;
                break;
            }
            DialogueEditorGUI.Repaint ();
        }

        #endregion

        #region Actor Editing

        #endregion

        #endregion

        #region Hotkey Handlers

        #endregion
    }
}

using UnityEditor;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using System.Linq;

namespace DialogueSystem {
    public static class InputSystem {

        #region Setup

        public static List<EventHandlerData> eventHandlers;
        static List<HotKeyHandlerData> hotKeyHandlers;
        static List<ContextEntryData> contextEntries;
        static List<ContextFillerData> contextFillers;

        public static void SetupInputHandlers () {
            eventHandlers = new List<EventHandlerData> ();
            hotKeyHandlers = new List<HotKeyHandlerData> ();
            contextEntries = new List<ContextEntryData> ();
            contextFillers = new List<ContextFillerData> ();

            foreach (Assembly assem in AppDomain.CurrentDomain.GetAssemblies ().Where (a => a.FullName.Contains ("Assembly")))
                foreach (Type type in assem.GetTypes ())
                    foreach (MethodInfo method in type.GetMethods (BindingFlags.FlattenHierarchy | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)) {
                        foreach (object attri in method.GetCustomAttributes (true))
                            if (attri is EventHandlerAttribute && EventHandlerAttribute.AssureValidity (method, attri as EventHandlerAttribute)) {
                                eventHandlers.Add (new EventHandlerData (attri as EventHandlerAttribute, Delegate.CreateDelegate (typeof (Action<EditorStates>), method)));
                            } else if (attri is HotkeyAttribute && HotkeyAttribute.AssureValidity (method, attri as HotkeyAttribute)) {
                                hotKeyHandlers.Add (new HotKeyHandlerData (attri as HotkeyAttribute, Delegate.CreateDelegate (typeof (Action<EditorStates>), method)));
                            } else if (attri is ContextEntryAttribute && ContextEntryAttribute.AssureValidity (method, attri as ContextEntryAttribute)) {
                                contextEntries.Add (new ContextEntryData (attri as ContextEntryAttribute, (object callBackObj) => {
                                    if (!(callBackObj is EditorStates))
                                        throw new UnityException ();
                                    Delegate.CreateDelegate (typeof (Action<EditorStates>), method).DynamicInvoke (callBackObj as EditorStates);
                                }));
                            } else if (attri is ContextFillerAttribute && ContextFillerAttribute.AssureValidity (method, attri as ContextFillerAttribute)) {
                                contextFillers.Add (new ContextFillerData (attri as ContextFillerAttribute,
                                    Delegate.CreateDelegate (typeof (Action<EditorStates, GenericMenu>), method)));
                            }
                    }
            eventHandlers.Sort ((handlerA, handlerB) => handlerA.priority.CompareTo (handlerB.priority));
            hotKeyHandlers.Sort ((handlerA, handlerB) => handlerA.priority.CompareTo (handlerB.priority));
        }

        #endregion

        #region EventMethods

        public static void EarlyInputEvents (EditorStates states) {
            OverlayMenu.BeginOverlay ();

            if (!OverlayMenu.HasPopup) {
                CallEventHandler (states, false);
                CallHotkeyHandler (states);
            } else if (states.curEvent.type == EventType.Layout)
                HandleGUILayout (states);
        }

        public static void LateInputEvents (EditorStates states) {
            if (!OverlayMenu.HasPopup) 
                CallEventHandler (states, true);
            OverlayMenu.EndOverlay ();
        }

        static void CallEventHandler (EditorStates states, bool late) {
            foreach (var handler in eventHandlers) {
                if ((handler.eType == null || handler.eType == states.curEvent.type) &&
                    (handler.eSpace == EventSpace.Everything || handler.eSpace == states.curSpace) &&
                    (late ? handler.priority >= 100 : handler.priority < 100)) {
                    handler.actionDel.DynamicInvoke (new object[] { states });
                }
            } 
        }

        static void CallHotkeyHandler (EditorStates states) {
            foreach (var handler in hotKeyHandlers) 
                if (handler.hotKey == states.curEvent.keyCode &&
                    (handler.eModifiers == null || handler.eModifiers == states.curEvent.modifiers) &&
                    (handler.eSpace == EventSpace.Everything || handler.eSpace == states.curSpace)) {
                    handler.actionDel.DynamicInvoke (new object[] { states });
                }
        }
       
        #endregion

        #region Essential Controls

        static EditorStates unfocusControls;
        
        [EventHandler (-4)]
        public static void HandleFocusing (EditorStates states) {
            if (unfocusControls == states && states.curEvent.type == EventType.Repaint) {
                GUIUtility.hotControl = 0;
                GUIUtility.keyboardControl = 0;
                unfocusControls = null;
            }
            states.curState.focusedObjects = CanvasGUI.ObjectAtPosition (CanvasGUI.CanvasToScreenPosition (states.curState, states.mousePos));
        }

        [EventHandler (-3)]
        public static void HandleEventSpace (EditorStates states) {
            EditorState state = states.curState;

            if (CanvasGUI.OptionRect.Contains (states.mousePos)) {
                states.curSpace = EventSpace.Actor;

                if (!(state.focusedObjects[0] is Actor))
                    state.focusedObjects = new ScriptableObject[] { null };
            } else if (CanvasGUI.ToolBarRect.Contains (states.mousePos)) {
                states.curSpace = EventSpace.Toolbar;
                
                if (state.focusedObjects.Length > 0)
                    state.focusedObjects = new ScriptableObject[] { null };
            } else if (state.focusedObjects[0] is BaseNodule)
                states.curSpace = EventSpace.Nodule;
            else if (state.focusedObjects[0] is BaseNode)
                states.curSpace = EventSpace.Node;
            else if (CanvasGUI.CanvasRect.Contains (states.mousePos))
                states.curSpace = EventSpace.Canvas;
            else
                states.curSpace = EventSpace.Everything;
        }

        [EventHandler (EventType.MouseDown, -2)]
        public static void HandleSelecting (EditorStates states) {
            EditorState state = states.curState;

            if (state.focusedObjects[0] != state.selectedObject) {
                state.selectedObject = state.focusedObjects[0];
                unfocusControls = states;

                if (state.selectedObject != null)
                    Selection.activeObject = state.selectedObject;
                DialogueEditorGUI.Repaint ();
            }
        } 

        [EventHandler (EventType.Layout, -1)]
        public static void HandleGUILayout (EditorStates states) {
            ScriptableObject selectedObject = states.curState.selectedObject;

            BaseNode selectedNode = (selectedObject is MainNode) ? selectedObject as BaseNode:
             (selectedObject is BaseNodule) ? (selectedObject as BaseNodule).MainNode :
             (selectedObject is OptionNode) ? (selectedObject as OptionNode).MainNode : 
             selectedObject as BaseNode;
            
            if (selectedNode || states.Nodes.Contains (selectedNode))
                states.Nodes.Move (selectedNode, states.Nodes.Count - 1);
        }

        [EventHandler (EventType.MouseUp, 0)]
        public static void HandleContextMenu (EditorStates states) {
            if (states.curEvent.button == 1) {
                GenericMenu menu = new GenericMenu ();

                foreach (var filler in contextFillers)
                    if (filler.eSpace == EventSpace.Everything || filler.eSpace == states.curSpace)
                        filler.actionDel.DynamicInvoke (new object[] { states, menu });

                foreach (var entry in contextEntries) 
                    if (entry.eSpace == EventSpace.Everything || entry.eSpace == states.curSpace)
                        menu.AddMenuItem (new GUIContent (entry.contextPath), false, entry.menufunc2, states);
                menu.Show (states.mousePos);
            }
        }
    }

    #endregion

    #region Utilities

    #region EventData 

    public struct EventHandlerData {
        public EventType? eType;
        public int priority;
        public EventSpace eSpace;
        public Delegate actionDel;

        public EventHandlerData (Delegate delegateInfo) {
            eType = null;
            priority = 50;
            eSpace = EventSpace.Everything;
            actionDel = delegateInfo;
        }

        public EventHandlerData (EventHandlerAttribute attri, Delegate delegateInfo) {
            eType = attri.eType;
            priority = attri.priority;
            eSpace = attri.eSpace;
            actionDel = delegateInfo;
        }
    }

    public struct HotKeyHandlerData {
        public KeyCode hotKey;
        public EventModifiers? eModifiers;
        public EventType? eType;
        public EventSpace eSpace;
        public int priority;
        public Delegate actionDel;

        public HotKeyHandlerData (HotkeyAttribute attri, Delegate delegateInfo) {
            hotKey = attri.hotKey;
            eModifiers = attri.eModifiers;
            eType = attri.eType;
            priority = attri.priority;
            eSpace = attri.eSpace;
            actionDel = delegateInfo;
        }
    }

    public struct ContextEntryData {
        public EventSpace eSpace;
        public bool on;
        public string contextPath;
        public string GetMethodName { get { return (contextPath.Contains ("/")) ? contextPath.Substring (contextPath.LastIndexOf ("/") + 1) : contextPath; } }
        public GenericMenu.MenuFunction2 menufunc2;

        public ContextEntryData (ContextEntryAttribute attri, GenericMenu.MenuFunction2 menufuncInfo) {
            eSpace = attri.eSpace;
            on = attri.on;
            contextPath = attri.contextPath;
            menufunc2 = menufuncInfo;
        }

        public bool EntrySwitch (EditorState state) {
            return false;
        }
    }

    public struct ContextFillerData {
        public EventSpace eSpace;
        public Delegate actionDel;

        public ContextFillerData (ContextFillerAttribute attri, Delegate delegateInfo) {
            eSpace = attri.eSpace;
            actionDel = delegateInfo;
        }

        public bool EntrySwitch (EditorStates states) { return false; }
    }


    #endregion

    #region EventAttributes

    [AttributeUsage (AttributeTargets.Method, AllowMultiple = true)]
    public class EventHandlerAttribute : Attribute {
        public EventType? eType;
        public int priority;
        public EventSpace eSpace;

        public EventHandlerAttribute () {
            eType = null;
            priority = 50;
            eSpace = EventSpace.Everything;
        }

        public EventHandlerAttribute (int newPriority) {
            eType = null;
            priority = newPriority;
            eSpace = EventSpace.Everything;
        }

        public EventHandlerAttribute (EventType newType) {
            eType = newType;
            priority = 50;
            eSpace = EventSpace.Everything;
        }

        public EventHandlerAttribute (EventType newType, EventSpace newSpace) {
            eType = newType;
            priority = 50;
            eSpace = newSpace;
        }

        public EventHandlerAttribute (EventType newType, int newPriority) {
            eType = newType;
            priority = newPriority;
            eSpace = EventSpace.Everything;
        }

        public EventHandlerAttribute (EventType newType, int newPriority, EventSpace newSpace) {
            eType = newType;
            eSpace = newSpace;
            priority = newPriority;
        }

        internal static bool AssureValidity (MethodInfo method, EventHandlerAttribute attr) {
            if (!method.IsGenericMethod && !method.IsGenericMethodDefinition && (method.ReturnType == null || method.ReturnType == typeof (void))) { // Check if the method has the correct signature
                ParameterInfo[] methodParams = method.GetParameters ();

                if (methodParams.Length == 1 && methodParams[0].ParameterType == typeof (EditorStates))
                    return true;
                else
                    Debug.LogWarning ("Method " + method.Name + " has incorrect signature for EventAttribute!");
            }
            return false;
        }
    }

    [AttributeUsage (AttributeTargets.Method, AllowMultiple = true)]
    public class HotkeyAttribute : Attribute {
        public KeyCode hotKey;
        public EventModifiers? eModifiers;
        public EventType? eType;
        public EventSpace eSpace;
        public int priority;

        public HotkeyAttribute (KeyCode handledKey) {
            hotKey = handledKey;
            eModifiers = null;
            eType = null;
            priority = 50;
        }

        public HotkeyAttribute (KeyCode handledKey, EventModifiers newModifiers) {
            hotKey = handledKey;
            eModifiers = newModifiers;
            eType = null;
            priority = 50;
        }

        public HotkeyAttribute (KeyCode handledKey, EventType LimitEventType) {
            hotKey = handledKey;
            eModifiers = null;
            eType = LimitEventType;
            priority = 50;
        }

        public HotkeyAttribute (KeyCode handledKey, EventType LimitEventType, int priorityValue) {
            hotKey = handledKey;
            eModifiers = null;
            eType = LimitEventType;
            priority = priorityValue;
        }

        public HotkeyAttribute (KeyCode handledKey, EventType LimitEventType, EventModifiers newModifiers) {
            hotKey = handledKey;
            eModifiers = newModifiers;
            eType = LimitEventType;
            priority = 50;
        }

        public HotkeyAttribute (KeyCode handledKey, EventType LimitEventType, int priorityValue, EventModifiers newModifiers) {
            hotKey = handledKey;
            eModifiers = newModifiers;
            eType = LimitEventType;
            priority = priorityValue;
        }

        internal static bool AssureValidity (MethodInfo method, HotkeyAttribute attr) {
            if (!method.IsGenericMethod && !method.IsGenericMethodDefinition && (method.ReturnType == null || method.ReturnType == typeof (void))) { // Check if the method has the correct signature
                ParameterInfo[] methodParams = method.GetParameters ();

                if (methodParams.Length == 1 && methodParams[0].ParameterType.IsAssignableFrom (typeof (EditorStates)))
                    return true;
                else
                    Debug.LogWarning ("Method " + method.Name + " has incorrect signature for HotkeyAttribute!");
            }
            return false;
        }
    }

    [AttributeUsage (AttributeTargets.Method, AllowMultiple = true)]
    public class ContextEntryAttribute : Attribute {
        public EventSpace eSpace;
        public bool on;
        public string contextPath;

        public ContextEntryAttribute () {
            eSpace = EventSpace.Everything;
            on = false;
            contextPath = "";
        }

        public ContextEntryAttribute (EventSpace newSpace) {
            eSpace = newSpace;
            on = false;
            contextPath = "";
        }

        public ContextEntryAttribute (bool switchable) {
            eSpace = EventSpace.Everything;
            on = switchable;
            contextPath = "";
        }

        public ContextEntryAttribute (string newPath) {
            eSpace = EventSpace.Everything;
            on = false;
            contextPath = newPath;
        }

        public ContextEntryAttribute (EventSpace newSpace, bool switchable) {
            eSpace = newSpace;
            on = switchable;
            contextPath = "";
        }

        public ContextEntryAttribute (string newPath, bool switchable) {
            eSpace = EventSpace.Everything;
            on = switchable;
            contextPath = newPath;
        }

        public ContextEntryAttribute (EventSpace newSpace, string newPath) {
            eSpace = newSpace;
            on = false;
            contextPath = newPath;
        }

        public ContextEntryAttribute (EventSpace newSpace, bool switchable, string newPath) {
            eSpace = newSpace;
            on = switchable;
            contextPath = newPath;
        }

        internal static bool AssureValidity (MethodInfo method, ContextEntryAttribute attr) {
            if (!method.IsGenericMethod && !method.IsGenericMethodDefinition && (method.ReturnType == null || method.ReturnType == typeof (void))) { // Check if the method has the correct signature
                ParameterInfo[] methodParams = method.GetParameters ();

                if (methodParams.Length == 1 && methodParams[0].ParameterType == typeof (EditorStates))
                    return true;
                else
                    Debug.LogWarning ("Method " + method.Name + " has incorrect signature for ContextAttribute!");
            }
            return false;
        }
    }

    [AttributeUsage (AttributeTargets.Method, AllowMultiple = true)]
    public class ContextFillerAttribute : Attribute {
        public EventSpace eSpace;

        public ContextFillerAttribute () {
            eSpace = EventSpace.Everything;
        }

        public ContextFillerAttribute (EventSpace newSpace) {
            eSpace = newSpace;
        }

        internal static bool AssureValidity (MethodInfo method, ContextFillerAttribute attr) {
            if (!method.IsGenericMethod && !method.IsGenericMethodDefinition && (method.ReturnType == null || method.ReturnType == typeof (void))) { // Check if the method has the correct signature
                ParameterInfo[] methodParams = method.GetParameters ();

                if (methodParams.Length == 2 && methodParams[0].ParameterType == typeof (EditorStates) && methodParams[1].ParameterType == typeof (GenericMenu))
                    return true;
                else
                    Debug.LogWarning ("Method " + method.Name + " has incorrect signature for ContextFillerAttribute!");
            }
            return false;
        }
    }

    #endregion

    #endregion

}
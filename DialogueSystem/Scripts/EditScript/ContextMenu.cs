using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

namespace DialogueSystem {
    public class GenericMenu : OverlayMenu {
        public delegate void MenuFunction ();
        public delegate void MenuFunction2 (object userData);

        ItemGroup items;
        float GroupIconSize { get { return GUI.skin.label.CalcSize (new GUIContent (">")).x; } }
        float minWidth, minHeight;
        List<ItemGroup> groupsToDraw;

        #region Construction Methods

        public GenericMenu () {
            items = new ItemGroup ();
            controlID = GUIUtility.GetControlID (items.GetHashCode (), FocusType.Keyboard, new Rect (CanvasGUI.GUItoWindowPosition (items.position.position), items.position.size));
        }

        public GenericMenu (Rect newBoundingRect) {
            items = new ItemGroup ();
            controlID = GUIUtility.GetControlID (items.GetHashCode (), FocusType.Keyboard, new Rect (CanvasGUI.GUItoWindowPosition (items.position.position), items.position.size));

            if (newBoundingRect != Rect.zero)
                boundingRect = newBoundingRect;
        }

        public void AddMenuItem (GUIContent content, bool on, MenuFunction menuFunc) {
            MenuItem parent = Addheiracrhy (content.text);

            if (parent != null)
                parent.subItems.Add (new MenuItem (content, on, menuFunc));
            else
                items.Add (new MenuItem (content, on, menuFunc));
        }

        public void AddMenuItem (GUIContent content, bool on, MenuFunction2 menuFunc2, object userData) {
            MenuItem parent = Addheiracrhy (content.text);

            if (parent != null)
                parent.subItems.Add (new MenuItem (content, on, menuFunc2, userData));
            else
                items.Add (new MenuItem (content, on, menuFunc2, userData));
        }

        public void AddDisabledItem (GUIContent content) {
            items.Add (new MenuItem (content, false, null));
        }

        public void AddSaparator (GUIContent content) {
            MenuItem parent = Addheiracrhy (content.text);

            if (parent != null)
                parent.subItems.Add (new MenuItem ());
            else
                items.Add (new MenuItem ());
        }

        MenuItem Addheiracrhy (string path) {
            if (path.Contains ("/")) {
                string[] subFolders = path.Split ('/');
                string folderName = subFolders[0];
                MenuItem parent = items.Find (item => item.content != null && item.content.text == folderName && item.Group);

                if (parent == null)
                    items.Add (parent = new MenuItem (new GUIContent (folderName), false, true));

                for (int i = 1; i < subFolders.Length - 1; i++) {
                    if (parent == null)
                        throw new UnityException ();
                    else if (parent.subItems == null)
                        throw new UnityException ();
                    folderName = subFolders[i];

                    if (parent.subItems.Find (item => item.content != null && item.content.text == folderName && item.Group) == null)
                        parent.subItems.Add (parent = new MenuItem (new GUIContent (folderName), false, true));
                }
                return parent;
            }
            return null;
        }

        void CalculateRects (ItemGroup items, Vector2 position) {
            CalculateRects (items, position, Vector2.zero);
        }

        void CalculateRects (ItemGroup items, Vector2 position, Vector2 offset) {
            float maxWidth = minWidth, curHeight = 0;
            minHeight = GUI.skin.label.CalcSize (items.Get (0).content).y + 4;

            foreach (MenuItem item in items)
                if (item.content.text == "Saparator")
                    curHeight += 3;
                else {
                    maxWidth = Mathf.Max (maxWidth, GUI.skin.label.CalcSize (item.content).x + (item.Group ? GroupIconSize : 0) + 20);
                    curHeight += minHeight;
                }
            bool right = (position.x + offset.x + maxWidth < boundingRect.xMax) ? true : false;
            bool down = (position.y + offset.y + curHeight < boundingRect.yMax) ? true : false;

            items.position = new Rect (position + new Vector2 ((right) ? offset.x : -(offset.x + maxWidth), (down) ? -offset.y : (offset.y - curHeight)), new Vector2 (maxWidth, curHeight));
            curHeight = 0;

            foreach (MenuItem item in items)
                if (item.content.text == "Saparator") {
                    item.position = new Rect (0, curHeight, maxWidth, 3);
                    curHeight += 3;
                } else {
                    item.position = new Rect (0, curHeight, maxWidth, minHeight);

                    if (item.Group)
                        CalculateRects (item.subItems, items.position.position + item.position.center, item.position.size / 2);
                    curHeight += minHeight;
                }

            if (curHeight != items.position.height)
                throw new UnityException ();
        }

        #endregion

        #region GUI Methods

        public void Show (Vector2 menuPosition, float width = 40) {
            if (items.Count <= 0)
                return;
            minWidth = width;
            CalculateRects (items, CanvasGUI.GUItoWindowPosition (menuPosition));
            groupsToDraw = new List<ItemGroup> () { items };

            OnShow ();
        }

        protected override void OnGUI () {
            for (int i = 0; i < groupsToDraw.Count; i++) {
                ItemGroup itemGroup = groupsToDraw[i];
                Rect tolerance = new Rect (itemGroup.position);
                tolerance.xMin -= 20;
                tolerance.yMin -= 20;
                tolerance.xMax += 20;
                tolerance.yMax += 20;
                bool hasFocus = tolerance.Contains (Event.current.mousePosition);

                CanvasGUI.BeginGroup (itemGroup.position, GUI.skin.box, hasFocus);

                foreach (MenuItem item in itemGroup) {
                    if (item.content.text.Equals ("Saparator"))
                        GUI.Label (item.position, "----------");
                    else if (item.position.Contains (Event.current.mousePosition)) {
                        if (Event.current.type == EventType.MouseUp && Event.current.button == 0) {
                            if (item.Group)
                                groupsToDraw.Add (item.subItems);
                            else {
                                item.Execute ();
                                close = true;
                            }
                            Event.current.Use ();
                        }

                        if (item.Group && groupsToDraw.Exists (j => j == item.subItems))
                            item.subItems.forceFocus = true;
                        CanvasGUI.Box (item.position, item.content);
                    } else {
                        if (item.Group && groupsToDraw.Exists (j => j == item.subItems))
                            item.subItems.forceFocus = false;
                        GUI.Label (item.position, item.content);
                    }

                    if (item.Group)
                        GUI.Label (new Rect (item.position.position + new Vector2 (item.position.size.x - GroupIconSize, 0),
                            new Vector2 (GroupIconSize, minHeight)), ">");
                }

                CanvasGUI.EndGroup ();

                if (!hasFocus && !itemGroup.forceFocus)
                    if (i != 0)
                        groupsToDraw.Remove (itemGroup);
                    else if (Event.current.type == EventType.MouseUp)
                        close = true;
            }

            if (groupsToDraw.Count == 0 || close)
                OnClose ();
            DialogueEditorGUI.OnRepaint ();
        }

        #endregion

        #region ContextMenu Classes

        class ItemGroup : IEnumerable {
            List<MenuItem> list;
            public Rect position;
            public bool forceFocus;

            public ItemGroup () {
                list = new List<MenuItem> ();
                position = new Rect ();
                forceFocus = false;
            }

            public int Count { get { return list.Count; } }

            public void Add (MenuItem item) {
                list.Add (item);
            }

            public MenuItem Get (int index) {
                return list[index];
            }

            public MenuItem Find (Predicate<MenuItem> match) {
                return list.Find (match);
            }

            public IEnumerator GetEnumerator () {
                return ((IEnumerable) list).GetEnumerator ();
            }
        }

        class MenuItem {
            public GUIContent content;
            public Rect position;
            public bool on;

            public MenuFunction menuFunc;
            public MenuFunction2 menuFunc2;
            object userData;

            public bool Group { get { return subItems != null; } }
            public ItemGroup subItems;

            public MenuItem () {
                content = new GUIContent ("Saparator");
            }

            public MenuItem (GUIContent _content, bool _on, bool _group) {
                if (_content.text.Contains ("/"))
                    _content.text = _content.text.Substring (_content.text.LastIndexOf ("/") + 1);
                content = _content;
                on = _on;

                if (_group)
                    subItems = new ItemGroup ();
            }

            public MenuItem (GUIContent _content, bool _on, MenuFunction _menuFunc) {
                if (_content.text.Contains ("/"))
                    _content.text = _content.text.Substring (_content.text.LastIndexOf ("/") + 1);
                content = _content;
                on = _on;
                menuFunc = _menuFunc;
            }

            public MenuItem (GUIContent _content, bool _on, MenuFunction2 _menuFunc2, object _userData) {
                if (_content.text.Contains ("/"))
                    _content.text = _content.text.Substring (_content.text.LastIndexOf ("/") + 1);
                content = _content;
                on = _on;
                menuFunc2 = _menuFunc2;
                userData = _userData;
            }

            public void Execute () {
                if (menuFunc != null)
                    menuFunc.Invoke ();
                else if (menuFunc2 != null)
                    menuFunc2.Invoke (userData);
            }
        }

        #endregion
    }

    public static class ContextFill {

        #region Context Filler

        [ContextFiller (EventSpace.Canvas)]
        [ContextFiller (EventSpace.Node)]
        [ContextFiller (EventSpace.Nodule)]
        public static void AddNodes (EditorStates states, GenericMenu menu) {
            foreach (var nodeTypes in NodeTypes.nodeTypes)
                menu.AddMenuItem (new GUIContent ("Add Node/" + nodeTypes.Key.ContextPath), false,
                    AddCallBack, new EditorStates (nodeTypes.Key.GetClassName, states));
        }

        [ContextFiller (EventSpace.Node)]
        [ContextFiller (EventSpace.Nodule)]
        public static void AddNodules (EditorStates states, GenericMenu menu) {
            foreach (var noduleTypes in NoduleTypes.noduleTypes)
                menu.AddMenuItem (new GUIContent ("Add Nodule/" + noduleTypes.Key.ContextPath), false,
                    AddCallBack, new EditorStates (noduleTypes.Key.GetClassName, states));
        }

        static void AddCallBack (object callBackObj) {
            EditorStates states = callBackObj as EditorStates;

            if (states == null)
                throw new UnityException ();
            EditorState state = states.curState;

            if (states.info.Contains ("Node")) {
                switch (states.curSpace) {
                    case EventSpace.Node:
                    if (states.info == "OptionNode") {
                        if (state.selectedObject is MainNode) {
                            MainNode main = state.selectedObject as MainNode;
                            main.Options.Add (OptionNode.Create (main.Options.NextItemName ("Option"), main));
                        } else
                            goto default;
                    } else if (states.info == "MainNode")
                        goto default;
                    else {
                        Debug.LogError ("Cannot recognise name of 'Node'. Add More Error here");
                        return;
                    }
                    break;

                    default:
                    EditorCache cache = DialogueEditorGUI.Cache;
                    cache.Nodes.Add (DialogueNode.Create (states.info,
                        cache.Nodes.NextItemName (states.info),
                        CanvasGUI.CanvasToScreenPosition (state, states.mousePos),
                        cache.Actors.DefaultActor));
                    break;
                }
            } else if (states.info.Contains ("Nodule")) {
                BaseNode node = (states.curSpace == EventSpace.Node) ? state.selectedObject as BaseNode :
                    (states.curSpace == EventSpace.Nodule) ? (state.selectedObject as BaseNodule).MainNode :
                    null;

                if (node)
                    node.Nodules.Add (BaseNodule.Create (states.info, node.Nodules.NextItemName (states.info), node));
            } else
                throw new UnityException ();
        }

        [ContextFiller (EventSpace.Node)]
        [ContextFiller (EventSpace.Nodule)]
        public static void Makeconnections (EditorStates states, GenericMenu menu) {
            switch (states.curSpace) {
                case EventSpace.Node:
                foreach (var noduleTypes in NoduleTypes.noduleTypes)
                    menu.AddMenuItem (new GUIContent ("Make Connections/" + noduleTypes.Key.ContextPath), states.curState.makeConnection,
                        ConnectionCallBack, new EditorStates (noduleTypes.Key.GetClassName, states));
                break;

                case EventSpace.Nodule:
                menu.AddMenuItem (new GUIContent ("Make Connections"), states.curState.makeConnection, ConnectionCallBack, new EditorStates ("nodule", states));
                break;
            }
        }

        public static void ConnectionCallBack (object callBackObj) {
            EditorStates states = callBackObj as EditorStates;

            if (states == null)
                throw new UnityException ();
            EditorState state = states.curState;

            switch (states.curSpace) {
                case EventSpace.Nodule:
                if (!state.makeConnection) {
                    state.startNodule = state.selectedObject as BaseNodule;
                    state.makeConnection = true;
                } else
                    goto default;
                break;

                case EventSpace.Node:
                if (!state.makeConnection) {
                    BaseNode node = (state.selectedObject as BaseNode);
                    state.startNodule = node.Nodules.Find (i => i.GetType ().ToString ().Contains (states.info))
                        ?? BaseNodule.Create (states.info, node.Nodules.NextItemName (states.info), node);

                    if (state.startNodule.MainNode)
                        node.Nodules.Add (state.startNodule);
                    else
                        state.startNodule.Delete ();


                    if (state.startNodule)
                        state.makeConnection = true;
                    else
                        goto default;
                } else
                    goto default;
                break;

                default:
                state.startNodule = null;
                state.endNodule = null;
                state.makeConnection = false;
                break;
            }
        }

        #endregion

        #region Context Entries

        [ContextEntry (EventSpace.Node, "Delete Node")]
        public static void DeleteNode (EditorStates states) {
            EditorState state = states.curState;
            DialogueEditorGUI.Cache.Nodes.Remove (state.selectedObject as DialogueNode);
            (state.selectedObject as DialogueNode).Delete ();
        }

        [ContextEntry (EventSpace.Node, "Duplicate Node")]
        public static void DuplicateNode (EditorStates states) {
            if (states.curState.selectedObject is DialogueNode)
                DialogueEditorGUI.Cache.Nodes.DuplicateNode (states.curState.selectedObject as DialogueNode);
        }

        [ContextEntry (EventSpace.Nodule, "Delete Nodule")]
        public static void DeleteNodule (EditorStates states) {
            (states.curState.selectedObject as BaseNodule).Delete ();
        }

        [ContextEntry (EventSpace.Nodule, "Delete Connection")]
        public static void DeleteConnection (EditorStates states) {
            EditorState state = states.curState;
            BaseNodule nodule = state.selectedObject as BaseNodule;

            if (nodule.Nodules.Count > 0 && state.connectionIndex == -1) {
                state.connectionIndex++;
            }

            if (state.connectionIndex > -1) {
                nodule.Nodules.RemoveAt (state.connectionIndex);
                state.connectionIndex--;
            }
        }

        [ContextEntry (EventSpace.CanvasSpace, "Saparator")]
        public static void Saparator (EditorStates states) {

        }

        [ContextEntry (EventSpace.CanvasSpace, "Save")]
        public static void Save (EditorStates states) {

        }

        [ContextEntry (EventSpace.CanvasSpace, "New")]
        public static void New (EditorStates states) {

        }

        [ContextEntry (EventSpace.CanvasSpace, "Refresh")]
        public static void Refresh (EditorStates states) {

        }

        [ContextEntry (EventSpace.CanvasSpace, "Delete")]
        public static void Delete (EditorStates states) {

        }

        #endregion
    }	
}
using System;
using System.Collections.Generic;
using UnityEngine;

namespace DialogueSystem {
    public abstract class OverlayMenu {

        #region Static Methods

        public static bool HasPopup { get { return (popup != null || popups.Count > 0); } }
        static OverlayMenu popup;
        static List<IOverlayMenu> popups = new List<IOverlayMenu> ();
        
        public static void UpdateOverlays (CanvasObject obj) {
            if (!OverlayMenuTypes.Exists (obj))
                return;
            
            for (int i = 0; i < popups.Count; i++) {
                if (popups[i].Compare (obj)) {
                    popups.RemoveAt (i);
                    return;
                }

                if (popups[i].Compare (obj.GetType ()) && !OverlayMenuTypes.AllowMultiple(obj)) 
                    popups.RemoveAt (i);
            }
            IOverlayMenu menu = OverlayMenuTypes.GetMenu (obj);

            if (menu == null)
                return;
            popups.Add (menu);
            OverlayMenuTypes.Sort (popups);
        }

        public static void BeginOverlay () {
            if (Event.current.type != EventType.layout && Event.current.type != EventType.Repaint) {
                for (int i = 0; i < popups.Count; i++)
                    popups[i].OnGUI ();

                if (popup != null)
                    popup.OnGUI ();
            }
        }

        public static void EndOverlay () {
            if (Event.current.type == EventType.layout || Event.current.type == EventType.Repaint) {
                for (int i = 0; i < popups.Count; i++)
                    popups[i].OnGUI ();

                if (popup != null)
                    popup.OnGUI ();
            }
        }

        #endregion

        #region OverlayMenu Class

        public int controlID;

        protected bool close;
        protected Rect boundingRect = CanvasGUI.WindowRect;

        protected virtual void OnShow () {
            popup = this; 
            GUIUtility.hotControl = GUIUtility.keyboardControl = controlID;
            Event.current.Use ();
        }

        protected virtual void OnClose () {
            popup = null;
            GUIUtility.hotControl = GUIUtility.keyboardControl = 0;
        }

        protected abstract void OnGUI ();

        #endregion

        #region IOverlayMenu Class

        [OverlayMenu (typeof(Actor), false)]
        class ActorMenu : OverlayMenu<Actor> {
            public ActorMenu (Actor actorToEdit) : base (actorToEdit) {
                obj = actorToEdit;
                position = new Rect (CanvasGUI.OptionRect.position + new Vector2 (-150, obj.Position.y), new Vector2 (150, 50));
            }

            public override void OnGUI () {
                Event current = Event.current;
                Rect tolerance = new Rect (position);
                tolerance.xMin -= 20;
                tolerance.xMax += 20;
                tolerance.yMin -= 20;
                tolerance.yMax += 20;

                bool hasFocus = tolerance.Contains (current.mousePosition);

                CanvasGUI.BeginGroup (position, GUI.skin.box, hasFocus);

                string nodeName = obj.name;

                if (CanvasGUI.TextField (new Rect (5, 5, 240, 20), ref nodeName))
                    obj.name = DialogueEditorGUI.Cache.Actors.ItemNames[DialogueEditorGUI.Cache.Actors.ItemNames.IndexOf (obj.name)] = nodeName;
                obj.Tint = UnityEditor.EditorGUI.ColorField (new Rect (5, 30, 140, 20), obj.Tint);

                CanvasGUI.EndGroup ();

                if (current.type == EventType.MouseUp && !hasFocus)
                    close = true;

                if (!obj || close)
                    UpdateOverlays (obj);
            }
        }

        [OverlayMenu (typeof(OutputNodule), false)]
        class NoduleMenu : OverlayMenu<OutputNodule> {
            public NoduleMenu (OutputNodule noduleToEdit) : base (noduleToEdit) {
                Rect rect = new Rect (CanvasGUI.CanvasRect.center - new Vector2 (75, 125), new Vector2 (250, 250));
                position = new Rect (new Vector2 (Mathf.Round (rect.x), Mathf.Round (rect.y)), rect.size);
            }

            public override void OnGUI () {
                Event current = Event.current;
                Rect tolerance = new Rect (position);
                tolerance.xMin -= 20;
                tolerance.xMax += 20;
                tolerance.yMin -= 20;
                tolerance.yMax += 20;

                bool hasFocus = tolerance.Contains (current.mousePosition);

                CanvasGUI.BeginGroup (position, GUI.skin.box, hasFocus);

                ConditionDatabase conditions = DialogueEditorGUI.Cache.Conditions;
                obj.Condition = conditions.Get (CanvasGUI.DropDownMenu (new Rect (5, 5, position.width - 10, 20), position,
                    conditions.GetIndex (obj.Condition), conditions.ItemNames.ToArray ()));

                for (int i = 0; i < obj.ConditionValues.Count; i++) {
                    var val = obj.ConditionValues[i];

                    CanvasGUI.TextLabel (new Rect (30, 30 + 25 * 1, 20, 20), val.userParam.ToString ());

                    string[] names = Enum.GetNames (typeof (EqualityState));
                    val.equality = (EqualityState) Enum.Parse (typeof (EqualityState),
                        names[CanvasGUI.DropDownMenu (new Rect (5, 30 + 25, 100, 20), (int) val.equality, names)]);
                }

                CanvasGUI.EndGroup ();

                if (current.type == EventType.MouseUp && !hasFocus)
                    close = true;

                if (!obj || close)
                    UpdateOverlays (obj);
            }

            ValueType DrawParam (Rect rect, ValueType param) {
                ConditionalState conditional = obj.Condition.Conditional;

                switch (conditional) {
                    case (ConditionalState.Bool):
                    return CanvasGUI.DropDownMenu (rect, (int) param, new string[] { "False", "True" });
                    case (ConditionalState.Float):
                    break;
                    case (ConditionalState.Int):
                    break;
                    case (ConditionalState.None):
                    break;
                }
              
                return null;
            }

            ValueType Parse (string text, Type targetType) {
                ValueType result = null;
                bool isEmpty = string.IsNullOrEmpty (text);

                if (targetType == typeof (bool))
                    result = (isEmpty) ? false : bool.Parse (text);
                else if (targetType == typeof (Int32) || targetType == typeof (int))
                    result = (isEmpty) ? 0 : int.Parse (text);
                else if (targetType == typeof (float))
                    result = (isEmpty) ? 0 : float.Parse (text);
                return result;
                }
            }
        }

        #endregion
    }

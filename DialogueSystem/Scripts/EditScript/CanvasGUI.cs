using UnityEngine;
using UnityEditor;
using System;
using System.Globalization;
using System.Collections.Generic;


namespace DialogueSystem {
    public static class CanvasGUI {
        public static Rect Position { get; set; }
        public static Rect WindowRect { get { return new Rect (Vector2.zero, Position.size); } }
        public static Rect CanvasRect { get { return new Rect (Vector2.zero, new Vector2 (WindowRect.size.x - ToolBarRect.size.x, WindowRect.size.y)); } }
        public static Rect ToolBarRect { get { return new Rect (new Vector2 (WindowRect.size.x - 200, 0), new Vector2 (200, WindowRect.size.y)); } }
        public static Rect OptionRect { get { return new Rect (ToolBarRect.xMin - 200, 0, 200, 200); } }

        public static Color DefaultColor { get; private set; }

        static List<bool> controlHandler;
        static bool allowControl;

        static bool HasFocus (Rect rect) {
            return rect.Contains (Event.current.mousePosition);
        }

        static bool HasSelection (int controlID) {
            return GUIUtility.hotControl == controlID;
        }

        static bool HasKBoardSelection (int controlID) {
            return GUIUtility.keyboardControl == controlID;
        }

        #region GUI Utility

        public static void SetupGUI () {
            DefaultColor = GUI.backgroundColor;
            allowControl = true;
            controlHandler = new List<bool> () { allowControl };
        }

        #endregion

        #region GUIGroup

        public static void BeginGroup (Rect rect, bool hasControl) {
            BeginGroup (rect, GUIStyle.none, DefaultColor, hasControl);
        }

        public static void BeginGroup (Rect rect, GUIStyle style, bool hasControl) {
            BeginGroup (rect, style, DefaultColor, hasControl);
        }

        public static void BeginGroup (Rect rect, Color color, bool hasControl) {
            BeginGroup (rect, GUIStyle.none, color, hasControl);
        }

        public static void BeginGroup (Rect rect, GUIStyle style, Color color, bool hasControl) {
            GUI.backgroundColor = color;
            GUI.BeginGroup (rect, style);
            GUI.backgroundColor = DefaultColor;
            controlHandler.Add (allowControl = hasControl);
        }

        public static void EndGroup () {
            GUI.EndGroup ();

            if (controlHandler.Count < 1 || allowControl != controlHandler[controlHandler.Count - 1])
                throw new UnityException ();
            controlHandler.Remove (allowControl);
            allowControl = controlHandler[controlHandler.Count - 1];
        }

        #endregion

        #region IntField

        public static int IntField (Rect rect, int value, GUIStyle style, Color color) {
            int controlID = GUIUtility.GetControlID (IntFieldHash, FocusType.Keyboard, rect);
            return DoNumberField (recycledEditor, rect, 
        }

        #endregion

        #region FloatField

        public static float FloatField (Rect rect, float value, GUIStyle style, Color color) {
            return -1f;
        }

        #endregion

        internal static RecycledTextEditor activeEditor;
        internal static RecycledTextEditor recycledEditor = new RecycledTextEditor ();
        internal static string originalText = "";
        internal static string recycledCurrentEditingString;
        internal static double recycledCurrentEditingFloat;
        internal static long recycledCurrentEditingInt;
        internal static bool dragToPosition = true;
        internal static bool selectAllOnMouseUp = true;
        internal static bool dragged = false;
        internal static bool postponeMove = false;
        internal static int IntFieldHash = "EditorTextField".GetHashCode ();

        internal class RecycledTextEditor : TextEditor {
            internal static bool actuallyEditing = false;

            internal static bool allowContextCutOrPaste = true;

            internal bool IsEditingControl (int id) {
                return GUIUtility.keyboardControl == id && controlID == id && actuallyEditing && HasFocus (position);
            }

            public virtual void BeginEditing (int id, string newText, Rect position, GUIStyle style, bool multiline, bool passwordField) {
                if (!IsEditingControl (id)) {
                    if (activeEditor != null)
                        activeEditor.EndEditing ();
                    activeEditor = this;
                    controlID = id;
                    originalText = newText;
                    text = newText;
                    this.multiline = multiline;
                    this.style = style;
                    base.position = position;
                    isPasswordField = passwordField;
                    actuallyEditing = true;
                    scrollOffset = Vector2.zero;
                    UnityEditor.Undo.IncrementCurrentGroup ();
                }
            }

            public virtual void EndEditing () {
                if (activeEditor == this)
                    activeEditor = null;
                controlID = 0;
                actuallyEditing = false;
                allowContextCutOrPaste = true;
                UnityEditor.Undo.IncrementCurrentGroup ();
            }
        }

        static void DoNumberField (RecycledTextEditor editor, Rect position, Rect dragHotZone, int id, bool isDouble, ref double doubleVal, ref long longVal, string formatString, GUIStyle style, bool draggable, double dragSensitivity) {
            string allowedletters = (!isDouble) ? "0123456789-*/+%^()" : "inftynaeINFTYNAE0123456789.,-*/+%^()";

            if (draggable) {
                DragNumberValue (editor, position, dragHotZone, id, isDouble, ref doubleVal, ref longVal, formatString, style, dragSensitivity);
            }
            Event current = Event.current;
            string text;
            bool hasFocus = position.Contains (current.mousePosition);
            bool mouseSelected = GUIUtility.hotControl == editor.controlID;
            bool kBoardSelected = GUIUtility.keyboardControl == editor.controlID;

            if (kBoardSelected || (current.type == EventType.MouseDown && current.button == 0 && position.Contains (current.mousePosition))) 
                if (!editor.IsEditingControl (id)) 
                    text = (recycledCurrentEditingString = ((!isDouble) ? longVal.ToString (formatString) : doubleVal.ToString (formatString)));
                 else {
                    text = recycledCurrentEditingString;

                    if (current.type == EventType.ValidateCommand && current.commandName == "UndoRedoPerformed") 
                        text = ((!isDouble) ? longVal.ToString (formatString) : doubleVal.ToString (formatString));
                }
            else 
                text = ((!isDouble) ? longVal.ToString (formatString) : doubleVal.ToString (formatString));

            if (GUIUtility.keyboardControl == id) {
                bool flag;
                text = DoTextField (editor, id, position, text, style, allowedletters, out flag, false, false, false);

                if (flag) {
                    GUI.changed = true;
                    recycledCurrentEditingString = text;
                    if (isDouble) {
                        string a = text.ToLower ();
                        if (a == "inf" || a == "infinity") {
                            doubleVal = double.PositiveInfinity;
                        } else if (a == "-inf" || a == "-infinity") {
                            doubleVal = double.NegativeInfinity;
                        } else {
                            text = text.Replace (',', '.');
                            if (!double.TryParse (text, NumberStyles.Float, CultureInfo.InvariantCulture.NumberFormat, out doubleVal)) {
                                doubleVal = (recycledCurrentEditingFloat = ExpressionEvaluator.Evaluate<double> (text));
                            } else {
                                if (double.IsNaN (doubleVal)) {
                                    doubleVal = 0.0;
                                }
                                recycledCurrentEditingFloat = doubleVal;
                            }
                        }
                    } else if (!long.TryParse (text, out longVal)) {
                        longVal = (recycledCurrentEditingInt = ExpressionEvaluator.Evaluate<long> (text));
                    } else {
                        recycledCurrentEditingInt = longVal;
                    }
                }
            } else {
                bool flag;
                text = DoTextField (editor, id, position, text, style, allowedletters, out flag, false, false, false);
            }
        }

        static int dCandidateState = 1;
        static double dStartValue = 0.0;
        static long dStartIntValue = 0L;
        static Vector2 dStartPos;
        static double dSensitivity = 0.0;

        private static void DragNumberValue (RecycledTextEditor editor, Rect position, Rect dragHotZone, int id, bool isDouble, ref double doubleVal, ref long longVal, string formatString, GUIStyle style, double dragSensitivity) {
            Event current = Event.current;
            switch (current.GetTypeForControl (id)) {
                case EventType.MouseDown:
                if (dragHotZone.Contains (current.mousePosition) && current.button == 0) {
                    EditorGUIUtility.editingTextField = false;
                    GUIUtility.hotControl = id;

                    if (hotTextEditor != null) {
                        activeEditor.EndEditing ();
                    }

                    current.Use ();
                    GUIUtility.keyboardControl = id;
                    dCandidateState = 1;
                    dStartValue = doubleVal;
                    dStartIntValue = longVal;
                    dStartPos = current.mousePosition;
                    dSensitivity = dragSensitivity;
                    current.Use ();
                    EditorGUIUtility.SetWantsMouseJumping (1);
                }
                break;
                case EventType.MouseUp:
                if (GUIUtility.hotControl == id && dCandidateState != 0) {
                    GUIUtility.hotControl = 0;
                    dCandidateState = 0;
                    current.Use ();
                    EditorGUIUtility.SetWantsMouseJumping (0);
                }
                break;
                case EventType.MouseDrag:
                if (GUIUtility.hotControl == id) {
                    int num = dCandidateState;

                    if (num != 1) {
                        if (num == 2) {
                            if (isDouble) {
                                doubleVal += (double) HandleUtility.niceMouseDelta * dSensitivity;

                                if (dSensitivity == 0.0) {
                                    int digits = Math.Max (0, (int) (5.0 - Math.Log10 (Math.Abs (doubleVal))));
                                    double result;

                                    try {
                                        result = Math.Round (doubleVal, digits);
                                    } catch (ArgumentOutOfRangeException) {
                                        result = 0.0;
                                    }
                                    doubleVal = result;
                                } else
                                    doubleVal = Math.Round (doubleVal, (int) Math.Max (0.0, -Math.Floor (Math.Log10 (Math.Abs (dSensitivity)))), MidpointRounding.AwayFromZero);
                            } else {
                                longVal += (long) Math.Round ((double) HandleUtility.niceMouseDelta * dSensitivity);
                            }
                            GUI.changed = true;
                            current.Use ();
                        }
                    } else {
                        if ((Event.current.mousePosition - dStartPos).sqrMagnitude > 16f) {
                            dCandidateState = 2;
                            GUIUtility.keyboardControl = id;
                        }
                        current.Use ();
                    }
                }
                break;
                case EventType.KeyDown:
                if (GUIUtility.hotControl == id && current.keyCode == KeyCode.Escape && dCandidateState != 0) {
                    doubleVal = dStartValue;
                    longVal = dStartIntValue;
                    GUI.changed = true;
                    GUIUtility.hotControl = 0;
                    current.Use ();
                }
                break;
                case EventType.Repaint:
                EditorGUIUtility.AddCursorRect (dragHotZone, MouseCursor.SlideArrow);
                break;
            }
        }

        #region DropDownMenu

        static int curIndex;
        static int passTo;
        static bool pass;

        public static int DropDownMenu (Rect rect, int selectedIndex, string[] displayOptions) {
            return DropDownMenu (rect, Rect.zero, selectedIndex, displayOptions);
        }

        public static int DropDownMenu (Rect rect, Rect boundingRect, int selectedIndex, string[] displayOptions) {
            int controlID = GUIUtility.GetControlID (rect.GetHashCode (), FocusType.Passive, new Rect (GUItoWindowPosition (rect.position), rect.size));
            Event current = Event.current;

            switch (current.GetTypeForControl (controlID)) {
                case EventType.MouseDown:
                if (allowControl && HasFocus (rect) && current.button == 0)
                    GUIUtility.hotControl = controlID;
                break;

                case EventType.MouseUp:
                if (HasSelection (controlID) && !HasFocus (rect)) {
                    GUIUtility.hotControl = 0;
                    break;
                }

                if (allowControl && HasSelection (controlID)) {
                    GenericMenu menu = new GenericMenu (boundingRect) {
                        controlID = controlID
                    };

                    for (int i = 0; i < displayOptions.Length; i++)
                        menu.AddMenuItem (new GUIContent (displayOptions[i]), false, GetIndex, i);
                    menu.Show (new Vector2 (rect.xMin, rect.yMax), rect.width);
                    passTo = controlID;
                    current.Use ();
                }
                break;

                case EventType.Repaint:
                GUI.Box (rect, new GUIContent (displayOptions[selectedIndex]));
                break;
            }

            //if (GUIUtility.hotControl != 0 && !HasSelection)
            //    hasFocus = false;

            if (pass && passTo == controlID) {
                selectedIndex = curIndex;
                GUIUtility.hotControl = passTo = 0;
                pass = false;
            }
            return selectedIndex;
        }

        static void GetIndex (object userData) {
            curIndex = (int) userData;
            pass = true;
        }

        #endregion

        #region Button

        public static bool Button (Rect rect, string text) {
            return Button (rect, new GUIContent (text), GUI.skin.button);
        }

        public static bool Button (Rect rect, GUIContent content) {
            return Button (rect, content, GUI.skin.button);
        }

        public static bool Button (Rect rect, string text, GUIStyle style) {
            return Button (rect, new GUIContent (text), style);
        }

        public static bool Button (Rect rect, string text, Color color) {
            return Button (rect, new GUIContent (text), GUI.skin.button, color);
        }

        public static bool Button (Rect rect, GUIContent content, Color color) {
            return Button (rect, content, GUI.skin.button, color);
        }

        public static bool Button (Rect rect, string text, GUIStyle style, Color color) {
            return Button (rect, new GUIContent (text), style, color);
        }

        public static bool Button (Rect rect, GUIContent content, GUIStyle style, Color color) {
            GUI.backgroundColor = color;
            bool check = Button (rect, content, style);
            GUI.backgroundColor = DefaultColor;

            return check;
        }

        public static bool Button (Rect rect, GUIContent content, GUIStyle style) {
            int controlID = GUIUtility.GetControlID (rect.GetHashCode (), FocusType.Passive, new Rect (GUItoWindowPosition (rect.position), rect.size));
            Event current = Event.current;

            switch (current.GetTypeForControl (controlID)) {
                case EventType.MouseDown:
                if (allowControl && HasFocus (rect) && current.button == 0)
                    GUIUtility.hotControl = controlID;
                break;

                case EventType.MouseUp:
                if (allowControl && HasSelection (controlID)) {
                    GUIUtility.hotControl = 0;

                    if (HasFocus (rect))
                        return true;
                }
                break;

                case EventType.Repaint:
                style.Draw (rect, content, controlID, HasSelection (controlID));
                break;
            }
            return false;
        }

        #endregion

        #region DoubleClick

        static TimeSpan clickSpan = new TimeSpan (0, 0, 0, 0, 500);
        static int clickCount;
        static DateTime start;

        public static bool DoubleClick (Rect rect, string text) {
            return DoubleClick (rect, new GUIContent (text), GUI.skin.button);
        }

        public static bool DoubleClick (Rect rect, GUIContent content) {
            return DoubleClick (rect, content, GUI.skin.button);
        }

        public static bool DoubleClick (Rect rect, string text, Color color) {
            return DoubleClick (rect, new GUIContent (text), GUI.skin.button, color);
        }

        public static bool DoubleClick (Rect rect, string text, GUIStyle style) {
            return DoubleClick (rect, new GUIContent (text), style);
        }

        public static bool DoubleClick (Rect rect, GUIContent content, Color color) {
            return DoubleClick (rect, content, GUI.skin.button, color);
        }

        public static bool DoubleClick (Rect rect, string text, GUIStyle style, Color color) {
            return DoubleClick (rect, new GUIContent (text), style, color);
        }

        public static bool DoubleClick (Rect rect, GUIContent text, GUIStyle style, Color color) {
            GUI.backgroundColor = color;
            bool check = DoubleClick (rect, new GUIContent (text), style);
            GUI.backgroundColor = DefaultColor;

            return check;
        }

        public static bool DoubleClick (Rect rect, GUIContent content, GUIStyle style) {
            int controlID = GUIUtility.GetControlID (rect.GetHashCode (), FocusType.Passive, new Rect (GUItoWindowPosition (rect.position), rect.size));
            Event current = Event.current;
            bool pass = false;

            switch (current.GetTypeForControl (controlID)) {
                case EventType.mouseDown:
                if (allowControl && HasFocus (rect) && current.button == 0) {
                    if (!HasSelection (controlID))
                        clickCount = 0;
                    GUIUtility.hotControl = controlID;
                }
                break;

                case EventType.mouseUp:
                if (!HasFocus (rect) && HasSelection (controlID)) {
                    clickCount = 0;
                    GUIUtility.hotControl = 0;
                    break;
                }

                if (clickCount > 2 || (clickCount > 0 && DateTime.Now - start > clickSpan))
                    clickCount = 0;

                if (allowControl && HasSelection (controlID)) {
                    clickCount++;

                    if (clickCount == 1)
                        start = DateTime.Now;
                    else if (clickCount == 2) {
                        clickCount = 0;
                        GUIUtility.hotControl = 0;

                        if (DateTime.Now - start <= clickSpan)
                            pass = true;
                    }
                    current.Use ();
                }
                break;

                case EventType.Repaint:
                style.Draw (rect, content, controlID, HasSelection (controlID));
                break;
            }
            return pass;
        }

        #endregion

        #region Toggle

        public static bool Toggle (Rect rect, string text, bool value) {
            return Toggle (rect, new GUIContent (text), value, GUI.skin.toggle);
        }

        public static bool Toggle (Rect rect, GUIContent content, bool value) {
            return Toggle (rect, content, value, GUI.skin.toggle);
        }

        public static bool Toggle (Rect rect, GUIContent content, bool value, GUIStyle style) {
            int controlID = GUIUtility.GetControlID (rect.GetHashCode (), FocusType.Passive, new Rect (GUItoWindowPosition (rect.position), rect.size));
            Event current = Event.current;

            switch (current.GetTypeForControl (controlID)) {
                case EventType.MouseDown:
                if (allowControl && HasFocus (rect) && current.button == 0) {
                    GUIUtility.hotControl = controlID;
                    DialogueEditorGUI.Repaint ();
                }
                break;

                case EventType.MouseUp:
                if (allowControl && HasSelection (controlID)) {
                    GUIUtility.hotControl = 0;
                    current.Use ();

                    if (HasFocus (rect))
                        value = !value;
                }
                break;

                case EventType.Repaint:
                style.Draw (rect, content, HasFocus (rect), HasSelection (controlID), value, false);
                break;
            }
            return value;
        }

        #endregion

        #region Box

        public static void Box (Rect rect) {
            GUI.Box (rect, GUIContent.none);
        }

        public static void Box (Rect rect, Color color) {
            GUI.backgroundColor = color;
            GUI.Box (rect, GUIContent.none);
            GUI.backgroundColor = DefaultColor;
        }

        public static void Box (Rect rect, string text) {
            GUI.Box (rect, text);
        }

        public static void Box (Rect rect, GUIContent content) {
            GUI.Box (rect, content);
        }

        public static void Box (Rect rect, string text, Color color) {
            GUI.backgroundColor = color;
            GUI.Box (rect, text);
            GUI.backgroundColor = DefaultColor;
        }

        public static void Box (Rect rect, GUIContent content, Color color) {
            GUI.backgroundColor = color;
            GUI.Box (rect, content);
            GUI.backgroundColor = DefaultColor;
        }

        #endregion

        #region TextLabel

        public static void TextLabel (Rect rect, string text) {
            TextLabel (rect, new GUIContent (text), GUIStyle.none);
        }

        public static void TextLabel (Rect rect, GUIContent content) {
            TextLabel (rect, content, GUIStyle.none);
        }

        public static void TextLabel (Rect rect, string text, Color color) {
            TextLabel (rect, new GUIContent (text), GUIStyle.none, color);
        }

        public static void TextLabel (Rect rect, GUIContent content, Color color) {
            TextLabel (rect, content, GUIStyle.none, color);
        }

        public static void TextLabel (Rect rect, string text, GUIStyle style) {
            TextLabel (rect, new GUIContent (text), style);
        }

        public static void TextLabel (Rect rect, GUIContent content, GUIStyle style) {
            GUI.Label (rect, content);
        }

        public static void TextLabel (Rect rect, string text, GUIStyle style, Color color) {
            TextLabel (rect, new GUIContent (text), style, color);
        }

        public static void TextLabel (Rect rect, GUIContent content, GUIStyle style, Color color) {
            GUI.backgroundColor = color;
            GUI.Label (rect, content, style);
            GUI.backgroundColor = DefaultColor;
        }

        #endregion

        #region TextField

        static TextEditor hotTextEditor;

        public static bool TextField (Rect rect, ref string text) {
            return TextField (rect, ref text, GUI.skin.textField);
        }

        public static bool TextField (Rect rect, ref string text, Color color) {
            return TextField (rect, ref text, GUI.skin.textField, color);
        }

        public static bool TextField (Rect rect, ref string text, GUIStyle style, Color color) {
            GUI.backgroundColor = color;
            bool check = TextField (rect, ref text, style);
            GUI.backgroundColor = DefaultColor;

            return check;
        }

        public static bool TextField (Rect rect, ref string text, GUIStyle style) {
            int controlID = (hotTextEditor != null && hotTextEditor.text == text &&
                GUItoWindowPosition (hotTextEditor.position.position) == GUItoWindowPosition (rect.position)) ?
                hotTextEditor.controlID : GUIUtility.GetControlID (FocusType.Keyboard, rect);

            TextEditor tEditor = (TextEditor) GUIUtility.GetStateObject (typeof (TextEditor), controlID);
            tEditor.controlID = controlID;
            tEditor.text = text;
            tEditor.style = style;
            tEditor.position = rect;
            tEditor.multiline = false;

            tEditor.SaveBackup ();

            bool flag;
            text = DoTextGUI (tEditor, out flag);

            return flag;
        }

        public static string TextField (Rect rect, string text) {
            return TextField (rect, text, GUI.skin.textField);
        }

        public static string TextField (Rect rect, string text, Color color) {
            return TextField (rect, text, GUI.skin.textField, color);
        }

        public static string TextField (Rect rect, string text, GUIStyle style, Color color) {
            GUI.backgroundColor = color;
            text = TextField (rect, text, style);
            GUI.backgroundColor = DefaultColor;

            return text;
        }

        public static string TextField (Rect rect, string text, GUIStyle style) {
            int controlID = (hotTextEditor != null && hotTextEditor.text == text &&
               GUItoWindowPosition (hotTextEditor.position.position) == GUItoWindowPosition (rect.position)) ?
               hotTextEditor.controlID : GUIUtility.GetControlID (FocusType.Keyboard, rect);

            TextEditor tEditor = (TextEditor) GUIUtility.GetStateObject (typeof (TextEditor), controlID);
            tEditor.controlID = controlID;
            tEditor.text = text;
            tEditor.style = style;
            tEditor.position = rect;
            tEditor.multiline = false;

            tEditor.SaveBackup ();
            return DoTextGUI (tEditor);
        }

        #endregion

        #region TextArea

        public static string TextArea (Rect rect, string text) {
            return TextArea (rect, text, GUI.skin.textArea);
        }

        public static string TextArea (Rect rect, string text, Color color) {
            return TextArea (rect, text, GUI.skin.textArea, color);
        }

        public static string TextArea (Rect rect, string text, GUIStyle style, Color color) {
            GUI.backgroundColor = color;
            text = TextArea (rect, text, style);
            GUI.backgroundColor = DefaultColor;

            return text;
        }

        public static string TextArea (Rect rect, string text, GUIStyle style) {
            int controlID = (hotTextEditor != null && hotTextEditor.text == text &&
                GUItoWindowPosition (hotTextEditor.position.position) == GUItoWindowPosition (rect.position)) ?
                hotTextEditor.controlID : GUIUtility.GetControlID (FocusType.Keyboard, rect);

            TextEditor tEditor = (TextEditor) GUIUtility.GetStateObject (typeof (TextEditor), controlID);
            tEditor.controlID = controlID;
            tEditor.text = text;
            tEditor.style = GUI.skin.textField;
            tEditor.position = rect;
            tEditor.multiline = true;

            tEditor.SaveBackup ();
            DoTextGUI (tEditor);

            return tEditor.text;
        }

        #endregion

        //internal static string DoTextGUI (TextEditor teditor) {
        //    bool flag;
        //    return DoTextGUI (teditor, out flag);
        //}

        //internal static string DoTextGUI (RecycledTextEditor tEditor, string text, string allowedLetters, out bool changed) {
        //    Event current = Event.current;
        //    changed = false;
        //    string text2 = text;

        //    if (text == null)
        //        text = string.Empty;

        //    if (HasKBoardSelection (tEditor.controlID) && current.type != EventType.Layout) {
        //        if (tEditor.IsEditingControl (tEditor.controlID);
        //    }

        //    switch (current.GetTypeForControl (tEditor.controlID)) {
        //        case EventType.MouseDown:
        //        if (allowControl)
        //            if (HasFocus (tEditor.position) && current.button == 0) {
        //                if (HasSelection (tEditor.controlID)) {
        //                    if (current.clickCount == 2 && GUI.skin.settings.doubleClickSelectsWord) {
        //                        tEditor.MoveCursorToPosition (current.mousePosition);
        //                        tEditor.SelectCurrentWord ();
        //                        tEditor.DblClickSnap (TextEditor.DblClickSnapping.WORDS);
        //                        tEditor.MouseDragSelectsWholeWords (true);
        //                        dragToPosition = false;
        //                    } else if (current.clickCount == 3 && GUI.skin.settings.tripleClickSelectsLine) {
        //                        tEditor.MoveCursorToPosition (current.mousePosition);
        //                        tEditor.SelectCurrentParagraph ();
        //                        tEditor.DblClickSnap (TextEditor.DblClickSnapping.PARAGRAPHS);
        //                        tEditor.MouseDragSelectsWholeWords (true);
        //                        dragToPosition = false;
        //                    } else {
        //                        tEditor.MoveCursorToPosition (current.mousePosition);
        //                        selectAllOnMouseUp = false;
        //                    }
        //                    current.Use ();
        //                } else {
        //                    GUIUtility.hotControl = GUIUtility.keyboardControl = tEditor.controlID;
        //                    hotTextEditor = tEditor;
        //                    tEditor.MoveCursorToPosition (current.mousePosition);
        //                    tEditor.DetectFocusChange ();
        //                }
        //            } else if (!HasFocus (tEditor.position) && HasSelection (tEditor.controlID)) {
        //                GUIUtility.hotControl = GUIUtility.keyboardControl = 0;
        //                hotTextEditor = null;
        //                tEditor.DetectFocusChange ();
        //            }
        //        break;

        //        case EventType.MouseUp:
        //        if (HasSelection (tEditor.controlID)) {
        //            tEditor.MouseDragSelectsWholeWords (false);
        //            GUIUtility.hotControl = 0;
        //            current.Use ();
        //        }
        //        break;

        //        case EventType.MouseDrag:
        //        if (HasSelection (tEditor.controlID)) {
        //            if (current.shift)
        //                tEditor.MoveCursorToPosition (current.mousePosition);
        //            else
        //                tEditor.SelectToPosition (current.mousePosition);
        //            current.Use ();
        //        }
        //        break;

        //        case EventType.KeyDown:
        //        if (!HasSelection (tEditor.controlID))
        //            break;

        //        if (tEditor.HandleKeyEvent (current)) {
        //            changed = true;
        //        } else {
        //            char character = current.character;
        //            Font font = tEditor.style.font ?? GUI.skin.font;

        //            if (current.keyCode == KeyCode.Tab || character == '\t')
        //                break;

        //            if (character == '\n' && !tEditor.multiline && !current.alt)
        //                break;

        //            if (font.HasCharacter (character) || character == '\n') {
        //                tEditor.Insert (character);
        //                changed = true;
        //            } else if (character == '\0') {
        //                if (Input.compositionString.Length > 0) {
        //                    tEditor.ReplaceSelection ("");
        //                    changed = true;
        //                }
        //                current.Use ();
        //            }
        //        }
        //        break;

        //        case EventType.Repaint:
        //        if (HasKBoardSelection (tEditor.controlID))
        //            tEditor.DrawCursor (tEditor.text);
        //        else
        //            tEditor.style.Draw (tEditor.position, new GUIContent (tEditor.text), tEditor.controlID, false);
        //        break;
        //    }

        //    if (changed) {
        //        GUI.changed = true;
        //        current.Use ();
        //    }
        //    tEditor.UpdateScrollOffsetIfNeeded (current);
        //    return tEditor.text;
        //}

        internal static string DoTextField (RecycledTextEditor editor, int id, Rect position, string text, GUIStyle style, string allowedletters, out bool changed, bool reset, bool multiline, bool passwordField) {
            Event current = Event.current;
            string text2 = text;

            if (text == null)
                text = string.Empty;

            if (EditorGUI.showMixedValue) 
                text = string.Empty;

            if (HasKBoardSelection (id) && current.type != EventType.Layout) {
                if (editor.IsEditingControl (id)) {
                    editor.position = position;
                    editor.style = style;
                    editor.controlID = id;
                    editor.multiline = multiline;
                    editor.isPasswordField = passwordField;
                    editor.DetectFocusChange ();
                } else if (EditorGUIUtility.editingTextField) {
                    editor.BeginEditing (id, text, position, style, multiline, passwordField);

                    if (GUI.skin.settings.cursorColor.a > 0f) 
                        editor.SelectAll ();
                }
            }

            if (editor.controlID == id && GUIUtility.keyboardControl != id)
                editor.controlID = 0;

            bool flag = false;
            string text3 = editor.text;
            EventType typeForControl = current.GetTypeForControl (id);

            switch (typeForControl) {
                case EventType.MouseDown:
                if (HasFocus (position) && current.button == 0) {
                    if (editor.IsEditingControl (id)) {
                        if (Event.current.clickCount == 2 && GUI.skin.settings.doubleClickSelectsWord) {
                            editor.MoveCursorToPosition (Event.current.mousePosition);
                            editor.SelectCurrentWord ();
                            editor.MouseDragSelectsWholeWords (true);
                            editor.DblClickSnap (TextEditor.DblClickSnapping.WORDS);
                            dragToPosition = false;
                        } else if (Event.current.clickCount == 3 && GUI.skin.settings.tripleClickSelectsLine) {
                            editor.MoveCursorToPosition (Event.current.mousePosition);
                            editor.SelectCurrentParagraph ();
                            editor.MouseDragSelectsWholeWords (true);
                            editor.DblClickSnap (TextEditor.DblClickSnapping.PARAGRAPHS);
                            dragToPosition = false;
                        } else {
                            editor.MoveCursorToPosition (Event.current.mousePosition);
                            selectAllOnMouseUp = false;
                        }
                    } else {
                        GUIUtility.keyboardControl = id;
                        editor.BeginEditing (id, text, position, style, multiline, passwordField);
                        editor.MoveCursorToPosition (Event.current.mousePosition);

                        if (GUI.skin.settings.cursorColor.a > 0f) 
                            selectAllOnMouseUp = true;
                    }
                    GUIUtility.hotControl = id;
                    current.Use ();
                }
                goto IL_9B5;

                case EventType.MouseUp:
                if (HasSelection (id)) {
                    if (dragged && dragToPosition) {
                        editor.MoveSelectionToAltCursor ();
                        flag = true;
                    } else if (postponeMove) {
                        editor.MoveCursorToPosition (Event.current.mousePosition);
                    } else if (selectAllOnMouseUp) {
                        if (GUI.skin.settings.cursorColor.a > 0f)
                            editor.SelectAll ();
                        selectAllOnMouseUp = false;
                    }
                    editor.MouseDragSelectsWholeWords (false);
                    dragToPosition = true;
                    dragged = false;
                    postponeMove = false;
                    if (current.button == 0) {
                        GUIUtility.hotControl = 0;
                        current.Use ();
                    }
                }
                goto IL_9B5;

                case EventType.MouseMove:
                case EventType.KeyUp:
                case EventType.ScrollWheel:
                IL_125:
                switch (typeForControl) {
                    case EventType.ValidateCommand:
                    if (HasSelection (id)) {
                        string commandName = current.commandName;

                        if (commandName != null) {
                            if (commandName == "Cut" || commandName == "Copy") {
                                if (editor.hasSelection)
                                    current.Use ();
                            } else if (commandName == "Paste") {
                                if (editor.CanPaste ())
                                    current.Use ();
                            } else if (commandName == "SelectAll" || commandName == "Delete") {
                                current.Use ();
                            } else if (commandName == "UndoRedoPerformed") {
                                editor.text = text;
                                current.Use ();
                            }
                        }
                    }
                    goto IL_9B5;

                    case EventType.ExecuteCommand:
                    if (GUIUtility.keyboardControl == id) {
                        string commandName2 = current.commandName;

                        if (commandName2 != null) {
                            if (commandName2 == "OnLostFocus") {
                                if (activeEditor != null)
                                    activeEditor.EndEditing ();
                                current.Use ();
                            } else if (commandName2 == "Cut") {
                                editor.BeginEditing (id, text, position, style, multiline, passwordField);
                                editor.Cut ();
                                current.Use ();
                            } else if (commandName2 == "Copy") {
                                editor.Copy ();
                                current.Use ();
                            } else if (commandName2 == "Paste") {
                                editor.BeginEditing (id, text, position, style, multiline, passwordField);
                                editor.Paste ();
                                flag = true;
                            } else if (commandName2 == "SelectAll") {
                                editor.SelectAll ();
                                current.Use ();
                            } else if (commandName2 == "Delete") {
                                editor.BeginEditing (id, text, position, style, multiline, passwordField);

                                if (SystemInfo.operatingSystemFamily == OperatingSystemFamily.MacOSX)
                                    editor.Delete ();
                                else
                                    editor.Cut ();
                                flag = true;
                                current.Use ();
                            }
                        }
                    }
                    goto IL_9B5;

                    case EventType.DragExited:
                    goto IL_9B5;

                    //case EventType.ContextClick:
                    //if (HasFocus (position)) {
                    //    if (!editor.IsEditingControl (id)) {
                    //        GUIUtility.keyboardControl = id;
                    //        editor.BeginEditing (id, text, position, style, multiline, passwordField);
                    //        editor.MoveCursorToPosition (Event.current.mousePosition);
                    //    }
                    //    EditorGUI.ShowTextEditorPopupMenu ();
                    //    Event.current.Use ();
                    //}
                    //goto IL_9B5;

                    default:
                    goto IL_9B5;
                }

                case EventType.MouseDrag:
                if (HasSelection (id)) {
                    if (!current.shift && editor.hasSelection && dragToPosition) {
                        editor.MoveAltCursorToPosition (Event.current.mousePosition);
                    } else {
                        if (current.shift)
                            editor.MoveCursorToPosition (Event.current.mousePosition);
                        else
                            editor.SelectToPosition (Event.current.mousePosition);
                        dragToPosition = false;
                        selectAllOnMouseUp = !editor.hasSelection;
                    }
                    dragged = true;
                    current.Use ();
                }
                goto IL_9B5;

                case EventType.KeyDown:
                if (GUIUtility.keyboardControl == id) {
                    char character = current.character;
                    if (editor.IsEditingControl (id) && editor.HandleKeyEvent (current)) {
                        current.Use ();
                        flag = true;
                    //} else if (current.keyCode == KeyCode.Escape) {
                    //    if (editor.IsEditingControl (id)) {
                    //        if (style == EditorStyles.toolbarSearchField || style == EditorStyles.searchField) {
                    //            originalText = "";
                    //        }
                    //        editor.text = originalText;
                    //        editor.EndEditing ();
                    //        flag = true;
                    //    }
                    } else if (character == '\n' || character == '\u0003') {
                        if (!editor.IsEditingControl (id)) {
                            editor.BeginEditing (id, text, position, style, multiline, passwordField);
                            editor.SelectAll ();
                        } else {
                            if (multiline && !current.alt && !current.shift && !current.control) {
                                editor.Insert (character);
                                flag = true;
                                goto IL_9B5;
                            }
                            editor.EndEditing ();
                        }
                        current.Use ();
                    } else if (character == '\t' || current.keyCode == KeyCode.Tab) {
                        if (multiline && editor.IsEditingControl (id)) {
                            bool flag2 = allowedletters == null || allowedletters.IndexOf (character) != -1;
                            bool flag3 = !current.alt && !current.shift && !current.control && character == '\t';
                            if (flag3 && flag2) {
                                editor.Insert (character);
                                flag = true;
                            }
                        }
                    } else if (character != '\u0019' && character != '\u001b') {
                        if (editor.IsEditingControl (id)) {
                            bool flag4 = (allowedletters == null || allowedletters.IndexOf (character) != -1) && character != '\0';
                            if (flag4) {
                                editor.Insert (character);
                                flag = true;
                            } else {
                                if (Input.compositionString != "") {
                                    editor.ReplaceSelection ("");
                                    flag = true;
                                }
                                current.Use ();
                            }
                        }
                    }
                }
                goto IL_9B5;

                case EventType.Repaint: {
                    string text4;
                    if (editor.IsEditingControl (id)) {
                        text4 = ((!passwordField) ? editor.text : "".PadRight (editor.text.Length, '*'));
                    //} else if (EditorGUI.showMixedValue) {
                    //    text4 = EditorGUI.s_MixedValueContent.text;
                    } else
                        text4 = ((!passwordField) ? text : "".PadRight (text.Length, '*'));

                    if (GUIUtility.hotControl == 0)
                        EditorGUIUtility.AddCursorRect (position, MouseCursor.Text);

                    if (!editor.IsEditingControl (id)) {
                        //EditorGUI.BeginHandleMixedValueContentColor ();
                        style.Draw (position, new GUIContent (text4), id, false);
                        //EditorGUI.EndHandleMixedValueContentColor ();
                    } else {
                        editor.DrawCursor (text4);
                    }
                    goto IL_9B5;
                }
                goto IL_125;
            }

            IL_9B5:
            editor.UpdateScrollOffsetIfNeeded (current);
            changed = false;
            if (flag) {
                changed = (text3 != editor.text);
                current.Use ();
            }
            string result;
            if (changed) {
                GUI.changed = true;
                result = editor.text;
            } else {
                RecycledTextEditor.allowContextCutOrPaste = true;
                result = text2;
            }
            return result;
        }

        #region Canvas GUI Methods

        public static ScriptableObject[] ObjectAtPosition (Vector2 position) {
            for (int i = DialogueEditorGUI.Cache.Nodes.Count - 1; i >= 0; i--) {
                BaseNode node = DialogueEditorGUI.Cache.Nodes.Get (i);

                if (node.Position.Contains (position))
                    return new ScriptableObject[] { node };

                for (int j = node.Nodules.Count - 1; j >= 0; j--)
                    if (node.Nodules.Get (j).Position.Contains (position))
                        return new ScriptableObject[] { node.Nodules.Get (j), node };

                if (node is MainNode) {
                    MainNode main = node as MainNode;

                    for (int j = main.Options.Count - 1; j >= 0; j--) {
                        OptionNode option = main.Options.Get (j);

                        if (option.Position.Contains (position))
                            return new ScriptableObject[] { option, main };

                        for (int k = option.Nodules.Count - 1; k >= 0; k--)
                            if (option.Nodules.Get (k).Position.Contains (position))
                                return new ScriptableObject[] { option.Nodules.Get (k), option, main };
                    }
                }
            }
            return new ScriptableObject[] { null };
        }

        public static Vector2 GUItoWindowPosition (Vector2 GUIPos) {
            return GUIUtility.GUIToScreenPoint (GUIPos) - Position.position;
        }
                
        public static Vector2 ScreenToCanvasPosition (Vector2 screenPos) {
            return screenPos + DialogueEditorGUI.States.curState.panDelta;
        }

        public static Vector2 ScreenToCanvasPosition (EditorState state, Vector2 screenPos) {
            return screenPos + state.panDelta;
        }

        public static Vector2 CanvasToScreenPosition (Vector2 objectPos) {
            return objectPos - DialogueEditorGUI.States.curState.panDelta;
        }
        public static Vector2 CanvasToScreenPosition (EditorState state, Vector2 objectPos) {
            return objectPos - state.panDelta;
        }

        public static void DrawConnection (BaseNodule startNodule, BaseNodule endNodule, Color lineColor) {
            Vector2 startPos = new Vector2 (startNodule.side == NoduleSide.Left ? startNodule.Position.xMin : startNodule.Position.xMax, startNodule.Position.center.y);
            Vector2 endPos = new Vector2 (endNodule.side == NoduleSide.Left ? endNodule.Position.xMin : endNodule.Position.xMax, endNodule.Position.center.y);

            Handles.DrawBezier (startPos, endPos, startPos + startNodule.Dir * 80, endPos + endNodule.Dir * 80, lineColor, null, 3);
        }

        #endregion
    }	
}

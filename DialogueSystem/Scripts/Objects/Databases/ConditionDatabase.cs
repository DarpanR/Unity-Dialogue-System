using System;
using UnityEngine;

namespace DialogueSystem {
    public class ConditionDatabase : DatabaseHelper.SODatabase<Condition> {
        public override void Init () {
            base.Init ();

            if (Count == 0 || Get (0).name != "None")
                Insert (0, Condition.Create ("None", ConditionalState.None, new Rect ()));

            for (int i = 0; i < Count; i++) {
                Condition conditional = Get (i);

                if (!conditional) {
                    Debug.LogError ("The Condition at index '" + i + "' is missing its reference. Removing from ConditionDatabase.");
                    RemoveAt (i);
                    i--;
                    continue;
                }

                if (string.IsNullOrEmpty (conditional.name))
                    conditional.name = NextItemName ("New Condition");

                if (!ItemNames.Contains (conditional.name))
                    ItemNames.Add (conditional.name);
                conditional.Init ();
            }
        }

        public override void OnGUI () {
            for (int i = 1; i < Count; i++) {
                Condition condition = Get (i);

                if (CanvasGUI.Button (new Rect (condition.Position.xMax + 2, condition.Position.yMin, 20, 20), "X")) {
                    condition.Delete ();
                    i--;
                    continue;
                }
                condition.OnGUI ();
            }

            if (CanvasGUI.Button (new Rect (CanvasGUI.OptionRect.width - 25, 5, 20, 20), "+")) {
                GenericMenu menu = new GenericMenu (CanvasGUI.OptionRect);
                string[] values = Enum.GetNames (typeof (ConditionalState));

                for (int i = 1; i < values.Length; i++)
                    menu.AddMenuItem (new GUIContent (values[i]), false, AddConditionCallback, values[i]);
                menu.Show (new Vector2 (CanvasGUI.OptionRect.width - 5, 25));
            }
        }

        public override void RemoveAt (int index) {
            for (int i = index; i < Count; i++)
                Get (i).Position = new Rect (5, 5 + 22 * (i - 1), CanvasGUI.OptionRect.width - 34, 20);
            base.RemoveAt (index);
        }

        public override void Remove (Condition item) {
            for (int i = GetIndex (item); i < Count; i++)
                Get (i).Position = new Rect (5, 5 + 22 * (i - 1), CanvasGUI.OptionRect.width - 34, 20);
            base.Remove (item);
        }

        void AddConditionCallback (object userObject) {
            if (userObject == null || !(userObject is string))
                return;

            Add (Condition.Create (NextItemName ("New Condition"),
                (ConditionalState) Enum.Parse (typeof (ConditionalState), (string) userObject),
                new Rect (5, 5 + 22 * (Count), CanvasGUI.OptionRect.width - 34, 20)));
        }
    }
}


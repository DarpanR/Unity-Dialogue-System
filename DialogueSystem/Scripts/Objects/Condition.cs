using System.Linq;
using UnityEngine;

namespace DialogueSystem {
    public enum ConditionalState { None, Float, Int, Bool }

    public class Condition : CanvasObject {
        [SerializeField]
        ConditionalState conditional;

        public ConditionalState Conditional { get { return conditional; } }

        public static Condition Create (string conditionName, ConditionalState state, Rect conditionPos) {
            Condition condition = CreateInstance<Condition> ();
            DialogueEditorGUI.Cache.SaveNewObject (condition);
            condition.Contruct (conditionName, state, conditionPos);
            return condition;
        }

        public void Contruct (string conditionName, ConditionalState state, Rect conditionPos) {
            name = conditionName;
            Position = conditionPos;
            conditional = state;
        }

        public override void Init () { }

        public override void Delete () {
            EditorCache cache = DialogueEditorGUI.Cache;

            foreach (BaseNode node in cache.Nodes)
                foreach (OutputNodule nodule in node.Nodules.OfType<OutputNodule> ())
                    if (nodule.Condition == this)
                        nodule.Condition = null;
            cache.Conditions.Remove (this);
            cache.Conditions.ItemNames.Remove (name);
            DestroyImmediate (this, true);
        }

        public override void OnGUI () {
            CanvasGUI.Box (new Rect (position.position, new Vector2 (20, position.height)),
                new GUIContent (conditional.ToString ()[0].ToString ()));
            string nodeName = name;

            if (CanvasGUI.TextField (new Rect (new Vector2 (position.x + 25, position.y), new Vector2 (140, 20)), ref nodeName))
                name = DialogueEditorGUI.Cache.Conditions.ItemNames[DialogueEditorGUI.Cache.Conditions.ItemNames.IndexOf (name)] = nodeName;
        }
    }
}
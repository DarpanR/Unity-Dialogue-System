using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace DialogueSystem {
    [Node("MainNode")]
    public class MainNode : DialogueNode {
        [SerializeField]
        protected OptionDatabase options;

        public OptionDatabase Options { get { return options; } }

        protected override void Construct () {
            position = new Rect (Vector2.zero, ResourceManager.MAIN_NODE_SIZE);
            textArea = "Speech goes here.";
        }

        protected override void Construct (string nodeTitle, Vector2 nodePos, string nodeTextArea, Actor nodeActor) {
            name = nodeTitle;
            position.position = nodePos;
            textArea = nodeTextArea;
            actor = nodeActor;
            nodules = NoduleDatabase.CreateNew (this);
            options = OptionDatabase.CreateNew (this);
            DialogueEditorGUI.Cache.SaveNewObject (this);
        }

        public override void Delete () {
            while (options.Count > 0)
                options.Get (0).Delete ();
            base.Delete ();
            DialogueEditorGUI.UpdateSelection ();
        }

        public override void Init () {
            base.Init ();

            if (!options) {
                Debug.LogError ("");
                options = OptionDatabase.CreateNew (this);
            }
            options.Init ();
        }
        
        public override void OnGUI () {
            base.OnGUI ();

            CanvasGUI.BeginGroup (Position, GUI.skin.box, actor.Tint, HasControl);

            if (Locked) {
                GUI.Label (new Rect (5, 5, 240, 20), name);
                GUI.Label (new Rect (5, 30, 240, 20), actor.name);
            } else {
                EditorCache cache = DialogueEditorGUI.Cache;
                string nodeName = name;

                if (CanvasGUI.TextField (new Rect (5, 5, 240, 20), ref nodeName))
                    name = cache.Nodes.ItemNames[cache.Nodes.ItemNames.IndexOf (name)] = nodeName;

                ActorDatabase actors = cache.Actors;
                actor = actors.Get (CanvasGUI.DropDownMenu (new Rect (5, 30, 240, 20),
                    position, actors.GetIndex (actor), actors.ItemNames.ToArray ()));
            }

            if (CanvasGUI.Button (new Rect (Position.size.x - 50, 5, 20, 20), new GUIContent ("L"), GUI.skin.button))
                Locked = !Locked;

            if (CanvasGUI.Button (new Rect (Position.size.x - 25, 5, 20, 20), new GUIContent ("X"), GUI.skin.button))
                Delete ();
            textArea = CanvasGUI.TextArea (new Rect (5, 55, 290, 115), textArea);

            if (CanvasGUI.Button (new Rect (5, 175, 290, 20), new GUIContent ("Add Dialogue Option"), GUI.skin.button))
                options.Add (OptionNode.Create (options.NextItemName("Option"), this));

            CanvasGUI.EndGroup ();

            options.OnGUI ();
        }

        public override void DrawConnection () {
            options.DrawConnection ();
            base.DrawConnection ();
        }

        public override void UpdateAllPosition (Vector2 delta) {
            if (options.Count == 0) 
                base.UpdateAllPosition (delta);
            else {
                Vector2 canvasSize = DialogueEditorGUI.States.curState.canvasSize;
                Rect optionRect = options.Get(options.Count - 1).Position;
                Vector2 pos = position.position + delta;

                if (pos.x < 0 || (pos + position.size).x > canvasSize.x)
                    delta.x = 0;

                if (pos.y < 0 || (optionRect.position + optionRect.size).y > canvasSize.y)
                    delta.y = 0;
                position.position += delta;
                NoduleDatabase.ReCalcAllNodulePos (this);
            }
            options.UpdateAllPosition (delta);
        }

        public override ScriptableObject[] GetAllReferences (bool getAllRefs) {
            return new List<ScriptableObject> (base.GetAllReferences (getAllRefs)) { options }.ToArray ();
        }

        public override void ReplaceAllReferences (System.Func<ScriptableObject, ScriptableObject> ReplacedSO) {
            base.ReplaceAllReferences (ReplacedSO);
            options = (OptionDatabase) ReplacedSO (options);
        }
    }
}

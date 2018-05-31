using System;
using System.Collections.Generic;
using UnityEngine;

namespace DialogueSystem {
    [Node ("OptionNode")]
    public class OptionNode : DialogueNode {
        [SerializeField]
        MainNode mainNode;

        public MainNode MainNode { get { return mainNode; } set { mainNode = value; } }
        public Rect MainRect { get { return mainNode.Position; } }

        protected override void Construct () {
            position = new Rect (Vector2.zero, ResourceManager.OPTION_NODE_SIZE);
            textArea = "Player speech goes here.";
        }

        public static OptionNode Create (string _name, MainNode _mainNode) {
            OptionNode option = Instantiate (NodeTypes.GetDefaultNode ("OptionNode")) as OptionNode;
            option.Construct (_name, option.textArea, _mainNode);
            return option;
        }

        public static OptionNode Create (string nodeTitle, string nodeTextArea, MainNode main) {
            OptionNode option = Instantiate (NodeTypes.GetDefaultNode ("OptionNode")) as OptionNode;
            option.Construct (nodeTitle, nodeTextArea, main);
            return option;
        }

        protected override void Construct (string nodeTitle, Vector2 nodePos, string nodeTextArea, Actor nodeActor) {
            mainNode = null;
            base.Construct (nodeTitle, nodePos, nodeTextArea, nodeActor);
        }

        protected void Construct (string nodeTitle, string nodeTextArea, MainNode main) {
            name = nodeTitle;
            textArea = nodeTextArea;
            mainNode = main;
            actor = DialogueEditorGUI.Cache.Actors.Get (0);
            nodules = NoduleDatabase.CreateNew (this);
            DialogueEditorGUI.Cache.SaveNewObject (this, mainNode);
        }

        public override void Init () {
            base.Init ();
        }

        public override void Delete () {
            if (mainNode)
                mainNode.Options.Delete (this);
            base.Delete ();
        }

        public override void OnGUI () {
            base.OnGUI ();

            CanvasGUI.BeginGroup (Position, GUI.skin.box, actor.Tint, HasControl);

            if (Locked)
                GUI.Label (new Rect (5, 5, 240, 20), name);
            else {
                string nodeName = name;

                if (CanvasGUI.TextField (new Rect (5, 5, 240, 20), ref nodeName))
                    name = DialogueEditorGUI.Cache.Nodes.ItemNames[DialogueEditorGUI.Cache.Nodes.ItemNames.IndexOf (name)] = nodeName;
            }

            if (CanvasGUI.Button (new Rect (Position.size.x - 50, 5, 20, 20), new GUIContent ("L"), GUI.skin.button))
                Locked = !Locked;

            if (CanvasGUI.Button (new Rect (Position.size.x - 25, 5, 20, 20), new GUIContent ("X"), GUI.skin.button))
                Delete ();
            textArea = CanvasGUI.TextArea (new Rect (5, 30, 290, 65), textArea);

            CanvasGUI.EndGroup ();
        }

        public override void UpdateAllPosition (Vector2 delta) {
            if (!mainNode)
                base.UpdateAllPosition (delta);
            else {
                position.position += delta;
                NoduleDatabase.ReCalcAllNodulePos (this);
            }
        }

        public override ScriptableObject[] GetAllReferences (bool getAllRefs) {
            List<ScriptableObject> refList = new List<ScriptableObject> (base.GetAllReferences (getAllRefs));

            if (getAllRefs)
                refList.Add (mainNode);
            return refList.ToArray ();
        }

        public override void ReplaceAllReferences (Func<ScriptableObject, ScriptableObject> ReplacedSO) {
            base.ReplaceAllReferences (ReplacedSO);
            mainNode = (MainNode) ReplacedSO (mainNode);
        }
    }
}

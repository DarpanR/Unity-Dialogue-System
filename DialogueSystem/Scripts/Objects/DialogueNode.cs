using System;
using System.Collections.Generic;
using UnityEngine;

namespace DialogueSystem {
    public abstract class DialogueNode : BaseNode {
        [SerializeField]
        protected string textArea;
        [SerializeField]
        protected Actor actor;
        [SerializeField]
        bool locked;

        public String TextArea { get { return TextArea; } }
        public Actor Actor { get { return actor; } set { actor = value; } }
        public bool Locked { get { return locked; } protected set { locked = value; } }

        public static DialogueNode Create (string id, Vector2 nodePos, Actor nodeActor) {
            DialogueNode node = Instantiate (NodeTypes.GetDefaultNode (id)) as DialogueNode;
            node.Construct (DialogueEditorGUI.Cache.Nodes.NextItemName (node.GetType ().Name), nodePos, "Speech goes here.", nodeActor);
            return node;
        }

        public static DialogueNode Create (string id, string nodeTitle, Vector2 nodePos, Actor nodeActor) {
            DialogueNode node = Instantiate (NodeTypes.GetDefaultNode (id)) as DialogueNode;
            node.Construct (nodeTitle, nodePos, "Speech goes here.", nodeActor);
            return node;
        }

        public static DialogueNode Create (string id, string nodeTitle, Vector2 nodePos, string nodeTextArea, Actor nodeActor) {
            DialogueNode node = Instantiate (NodeTypes.GetDefaultNode (id)) as DialogueNode;
            node.Construct (nodeTitle, nodePos, nodeTextArea, nodeActor);
            return node;
        }

        public static T Create<T> (Vector2 nodePos, Actor nodeActor) where T : DialogueNode {
            T node = Instantiate (NodeTypes.GetDefaultNode<T> ()) as T;
            node.Construct (DialogueEditorGUI.Cache.Nodes.NextItemName (node.GetType ().Name), nodePos, "Speech goes here.", nodeActor);
            return node;
        }

        public static T Create<T> (string nodeTitle, Vector2 nodePos, Actor nodeActor) where T : DialogueNode {
            T node = Instantiate (NodeTypes.GetDefaultNode<T> ()) as T;
            node.Construct (nodeTitle, nodePos, "Speech goes here.", nodeActor);
            return node;
        }

        public static T Create<T> (string nodeTitle, Vector2 nodePos, string nodeTextArea, Actor nodeActor) where T : DialogueNode {
            T node = Instantiate (NodeTypes.GetDefaultNode<T> ()) as T;
            node.Construct (nodeTitle, nodePos, nodeTextArea, nodeActor);
            return node;
        }

        protected virtual void Construct (string nodeTitle, Vector2 nodePos, string nodeTextArea, Actor nodeActor) {
            name = nodeTitle;
            position.position = nodePos;
            textArea = nodeTextArea;
            actor = nodeActor;
            nodules = NoduleDatabase.CreateNew (this);
            DialogueEditorGUI.Cache.SaveNewObject (this);
        }

        public override void Init () {
            base.Init ();

            if (!actor) {
                Debug.LogError ("");
                actor = DialogueEditorGUI.Cache.Actors.Get (0);
            } else if (!DialogueEditorGUI.Cache.Actors.Contains (actor))
                throw new UnityException ();
        }

        #region Serialization Methods

        public override ScriptableObject[] GetAllReferences (bool getAllRefs) {
            List<ScriptableObject> refList = new List<ScriptableObject> (base.GetAllReferences (getAllRefs));

            if (getAllRefs)
                refList.Add (actor);
            return refList.ToArray ();
        }

        public override void ReplaceAllReferences (Func<ScriptableObject, ScriptableObject> ReplacedSO) {
            base.ReplaceAllReferences (ReplacedSO);
            actor = (Actor) ReplacedSO (actor);
        }

        #endregion
    }
}
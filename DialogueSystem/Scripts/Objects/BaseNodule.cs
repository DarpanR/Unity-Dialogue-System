using System;
using System.Collections.Generic;
using UnityEngine;

namespace DialogueSystem {
    public enum NoduleSide { Right, Left }

    public abstract class BaseNodule : NodeObject {
        [SerializeField]
        BaseNode mainNode;
        [SerializeField]
        protected NoduleDatabase nodules;

        public BaseNode MainNode { get { return mainNode; } set { mainNode = value; } }
        public NoduleDatabase Nodules { get { return nodules; } }
        public Rect MainRect { get { return mainNode.Position; } }

        public override Rect Position { get { return position = new Rect (MainRect.center + offSet, position.size); } set { } }
        public Vector2 offSet;
        public Vector2 Dir { get; private set; }
        public NoduleSide side;
        public abstract NoduleSide DefaultSide { get; }


        protected override void Construct () {
            position = new Rect (Vector2.zero, ResourceManager.NODULE_SIZE);
            side = DefaultSide;
        }

        public static BaseNodule Create (string className, BaseNode _mainNode) {
            BaseNodule nodule = Instantiate (NoduleTypes.GetDefaultNodule (className));
            nodule.Construct (_mainNode.Nodules.NextItemName (nodule.GetType ().Name), _mainNode);
            return nodule;
        }

        public static BaseNodule Create (string className, string _name, BaseNode _mainNode) {
            BaseNodule nodule = Instantiate (NoduleTypes.GetDefaultNodule (className));
            nodule.Construct (_name, _mainNode);
            return nodule;
        }

        public static T Create<T> (BaseNode _mainNode) where T : BaseNodule {
            T nodule = Instantiate (NoduleTypes.GetDefaultNodule<T> ()) as T;
            nodule.Construct (_mainNode.Nodules.NextItemName (nodule.GetType().Name), _mainNode);
            return nodule;
        }


        public static T Create<T> (string _name, BaseNode _mainNode) where T : BaseNodule {
            T nodule = Instantiate (NoduleTypes.GetDefaultNodule<T> ()) as T;
            nodule.Construct (_name, _mainNode);
            return nodule;
        }

        protected virtual void Construct (string _name, BaseNode _mainNode) {
            name = _name;
            mainNode = (NoduleTypes.CheckCompatibility (_mainNode, this)) ? _mainNode : null;
            nodules = NoduleDatabase.CreateNew (this);

            if (mainNode)
                DialogueEditorGUI.Cache.SaveNewObject (this, mainNode);
        }

        public override void Delete () {
            while (nodules.Count > 0)
                nodules.RemoveAt (0);
            mainNode.Nodules.Remove (this);
            DestroyImmediate (this, true);
        }

        public override void Init () {
            if (!mainNode) {
                Debug.LogError ("");
                Delete ();
            }

            if (nodules == null) {
                Debug.LogError ("");
                nodules = NoduleDatabase.CreateNew (this);
            }

            foreach (BaseNodule connectedNodule in nodules)
                if (!connectedNodule.nodules.Contains (this)) {
                    Debug.LogError ("");
                    connectedNodule.nodules.Add (connectedNodule);
                }
        }

        public override void OnGUI () {
            GUI.Box (Position, name[0].ToString ());
        }

        public void ReCalcXPos () {
            switch (side) {
                case NoduleSide.Left:
                offSet.x = -MainRect.width * 0.5f - position.width;
                Dir = Vector2.left;
                break;

                case NoduleSide.Right:
                offSet.x = MainRect.width * 0.5f;
                Dir = Vector2.right;
                break;
            }
        }

        public override ScriptableObject[] GetAllReferences (bool getAllRefs) {
            List<ScriptableObject> refs = new List<ScriptableObject> { nodules };

            if (getAllRefs)
                refs.Add (mainNode);
            return refs.ToArray ();
        }

        public override void ReplaceAllReferences (Func<ScriptableObject, ScriptableObject> ReplacedSO) {
            nodules = (NoduleDatabase) ReplacedSO (nodules);
            mainNode = (BaseNode) ReplacedSO (mainNode);
        }
    }
}

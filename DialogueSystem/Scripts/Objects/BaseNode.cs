using System;
using System.Linq;
using UnityEngine;

namespace DialogueSystem {
    public abstract class BaseNode : NodeObject {
        [SerializeField]
        protected NoduleDatabase nodules;

        public NoduleDatabase Nodules { get { return nodules; } }

        public bool HasControl { get { return DialogueEditorGUI.States.curState.selectedObject == this; } }
        public override Rect Position { get { return position; } set { position = value; } }

        public override void Delete () {
            while (nodules.Count > 0)
                nodules.Get (0).Delete ();
            DialogueEditorGUI.Cache.Nodes.Remove (this);
            DestroyImmediate (this, true);
        }

        public override void Init () {
            if (!nodules) {
                Debug.LogError ("");
                nodules = NoduleDatabase.CreateNew (this);
            }
            nodules.Init ();
        }

        public override void OnGUI () {
            nodules.OnGUI ();
        }

        public virtual void DrawConnection () {
            foreach (BaseNodule nodule in nodules.OfType<OutputNodule> ())
                foreach (BaseNodule connectedNodule in nodule.Nodules)
                    CanvasGUI.DrawConnection (nodule, connectedNodule, Color.cyan);
        }

        public void UpdateAllPosition () {
            UpdateAllPosition (Vector2.zero);
        }

        public virtual void UpdateAllPosition (Vector2 delta) {
            if ((position.position + delta).x < 0 || (position.position + delta + position.size).x > DialogueEditorGUI.States.curState.canvasSize.x)
                delta.x = 0;

            if ((position.position + delta).y < 0 || (position.position + delta + position.size).y > DialogueEditorGUI.States.curState.canvasSize.y)
                delta.y = 0;
            position.position += delta;
            NoduleDatabase.ReCalcAllNodulePos (this);
        }

        #region Serialization Methods

        public override ScriptableObject[] GetAllReferences (bool getAllRefs) {
            return new ScriptableObject[] { nodules };
        }

        public override void ReplaceAllReferences (Func<ScriptableObject, ScriptableObject> ReplacedSO) {
            nodules = (NoduleDatabase) ReplacedSO (nodules);
        }

        #endregion
    }
}
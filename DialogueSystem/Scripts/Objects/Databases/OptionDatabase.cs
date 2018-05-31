using System;
using System.Collections.Generic;
using UnityEngine;

namespace DialogueSystem {
    public class OptionDatabase : DatabaseHelper.SODatabase<OptionNode> {
        [SerializeField]
        MainNode mainNode;
        [SerializeField]
        float optionSpacing;
        Vector2 nextOptPos;

        public Rect OptionAddRect { get { return new Rect (nextOptPos, ResourceManager.OPTION_NODE_SIZE); } }

        public static OptionDatabase CreateNew (MainNode mainNode) {
            var DB = CreateInstance<OptionDatabase> ();
            DB.name = mainNode.name + " - " + DB.GetType ().Name;
            DB.mainNode = mainNode;

            if (!DB)
                throw new UnityException ();
            DB.Construct ();
            return DB;
        }

        protected override void Construct () {
            base.Construct ();
            optionSpacing = 8;
            ReCalcAllOptionPos ();
        }

        public override void Init () {
            base.Init ();

            for (int i = 0; i < Count; i++) {
                OptionNode option = Get (i);

                if (!option) {
                    Debug.LogError ("");
                    RemoveAt (i);
                    i--;
                    continue;
                }

                if (option.MainNode != this) {
                    option.MainNode = mainNode;
                }
                option.Init ();
            }
            ReCalcAllOptionPos ();
        }

        public override void OnGUI () {
            for (int i = 0; i < Count; i++) {
                OptionNode option = Get (i);
                option.OnGUI ();

                if (!mainNode.Locked && CanvasGUI.Button (new Rect (
                    new Vector2 (option.Position.center.x, option.Position.yMin - optionSpacing * 0.5f) - new Vector2 (45, 14) * 0.5f,
                    new Vector2 (45, 14)), "Unlink")) {
                    Remove (option);
                    continue;
                }
            }
        }

        public override void Add (OptionNode item) {
            base.Add (item);
            GetOptionPos (item);
            item.MainNode = mainNode;
        }

        public override void Insert (int index, OptionNode item) {
            base.Insert (index, item);
            GetOptionPos (item);
            item.MainNode = mainNode;
        }

        public override void Remove (OptionNode item) {
            DialogueEditorGUI.Cache.Nodes.Add (item);
            item.UpdateAllPosition (new Vector2 (50, 50));
            item.MainNode = null;
            base.Remove (item);
            ReCalcAllOptionPos ();
        }

        public void Delete (OptionNode item) {
            DialogueEditorGUI.UpdateSelection (new EditorStates ((GetIndex (item) < Count - 1) ?
                Get (GetIndex (item) + 1).Position.position.ToString () :
                mainNode.Position.center.ToString (), DialogueEditorGUI.States));
            base.Remove (item);
            ReCalcAllOptionPos ();
        }

        public void DrawConnection () {
            for (int i = 0; i < Count; i++)
                Get (i).DrawConnection ();
        }

        public void UpdateAllPosition (Vector2 delta) {
            for (int i = 0; i < Count; i++)
                Get (i).UpdateAllPosition (delta);
            nextOptPos += delta;
        }

        void GetOptionPos (OptionNode option) {
            option.UpdateAllPosition (nextOptPos - option.Position.position);
            nextOptPos.y += option.Position.size.y + optionSpacing;
        }

        void ReCalcAllOptionPos () {
            nextOptPos = new Vector2 (mainNode.Position.xMin, mainNode.Position.yMax + optionSpacing);

            for (int i = 0; i < Count; i++)
                GetOptionPos (Get (i));
        }

        #region Serialization Methods

        public override ScriptableObject[] GetAllReferences (bool getAllRefs) {
            List<ScriptableObject> refList = new List<ScriptableObject> (base.GetAllReferences (getAllRefs)) { };

            if (getAllRefs)
                refList.Add (mainNode);
            return refList.ToArray ();
        }

        public override void ReplaceAllReferences (Func<ScriptableObject, ScriptableObject> ReplacedSO) {
            base.ReplaceAllReferences (ReplacedSO);
            mainNode = ReplacedSO (mainNode) as MainNode;
        }

        #endregion
    }
}

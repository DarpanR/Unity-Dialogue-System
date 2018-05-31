using System;
using System.Collections.Generic;
using UnityEngine;

namespace DialogueSystem {
    public enum EqualityState { Equal, GreaterThan, LessThan }

    [Serializable]
    public class ConditionValue {
        public BaseNodule nodule;
        public EqualityState equality;
        public ValueType userParam;
        public ValueType dataParam;

        public ConditionValue (BaseNodule _nodule, ValueType _dataParam) {
            nodule = _nodule;
            equality = EqualityState.Equal;
            dataParam = _dataParam;
            userParam = false;
        }

        public ConditionValue (BaseNodule _nodule, EqualityState _equality, ValueType _dataParam, ValueType _userParam) {
            nodule = _nodule;
            equality = _equality;
            dataParam = _dataParam;
            userParam = _userParam;
        }
    }

    [Nodule ("OutputNodule", false, "InputNodule")]
    public class OutputNodule : BaseNodule{
        [SerializeField]
        Condition condition;
        [SerializeField]
        List<ConditionValue> conditionValues;

        public Condition Condition { get { return condition; } set { condition = value; } }
        public List<ConditionValue> ConditionValues { get { return conditionValues; } }
        public override NoduleSide DefaultSide { get { return NoduleSide.Right; } }

        protected override void Construct () {
            base.Construct ();
        }

        protected override void Construct (string _name, BaseNode _mainNode) {
            base.Construct (_name, _mainNode);
            conditionValues = new List<ConditionValue> ();
            condition = DialogueEditorGUI.Cache.Conditions.Get (0);
        }

        public override void Init () {
            base.Init ();

            if (!condition || !DialogueEditorGUI.Cache.Conditions.Contains (condition))
                condition = DialogueEditorGUI.Cache.Conditions.Get (0);

            if (conditionValues == null) {
                Debug.LogError ("Missing ConditionPath for " + Nodules.name + ", making a new one k?");
                conditionValues = new List<ConditionValue> ();

                UpdateConditionalValues ();
            }

            if (conditionValues.Count != Nodules.Count)
                throw new UnityException ("The number of objects in ConditionPath: " + conditionValues.Count + " does not match the nodules count: " + Nodules.Count);
        }

        public override void OnGUI () {
            if (CanvasGUI.DoubleClick (position, name[0].ToString (), GUI.skin.box))
                OverlayMenu.UpdateOverlays (this);
        }

        public void UpdateConditionalValues () {
            if (nodules.Count > conditionValues.Count) {
                for (int i = 0; i < nodules.Count; i++)
                    if (!conditionValues.Exists (j => j.nodule == nodules.Get (i)))
                        conditionValues.Add (new ConditionValue (nodules.Get (i), (conditionValues.Count > 0) ?
                            conditionValues[i - 1].userParam : null));
            } else if (conditionValues.Count > nodules.Count)
                for (int i = 0; i < conditionValues.Count; i++)
                    if (!nodules.Exist (j => j == conditionValues[i].nodule))
                        conditionValues.Remove (conditionValues[i]);

            ///Fucked up somewhere.
            if (nodules.Count != conditionValues.Count)
                throw new UnityException (name + "nodule count: " + nodules.Count + ", doesn't match the condition values count: " + conditionValues.Count + ".");
        }
        
        //public void AddNodule (BaseNodule nodule) {
        //    if (!(Nodules.Contains (nodule)) || conditionValues.Exists (i => i.nodule == nodule)) {
        //        Debug.Log ("returning");
        //        return;
        //    }
        //    conditionValues.Add (new ConditionValue (nodule, 
        //        (conditionValues.Count > 0) ? conditionValues[conditionValues.Count - 1].userParam : conditionValues.Count));
        //}

        //public void RemoveNodule (BaseNodule nodule) {
        //    foreach (ConditionValue val in conditionValues)
        //        if (val.nodule == nodule && !Nodules.Contains (nodule)) {
        //            conditionValues.Remove (val);
        //            break;
        //        }
        //}

        public BaseNodule Calculate (object userData) {
            foreach (ConditionValue val in conditionValues) {
                bool passOn = false;
                switch (condition.Conditional) {
                    case ConditionalState.Bool:
                    passOn = (bool) val.userParam == (bool) userData;
                    break;

                    case ConditionalState.Float:
                    float floatData1 = (float) val.userParam;
                    float floatUser = (float) userData;

                    switch (val.equality) {
                        case EqualityState.Equal:
                        passOn = floatUser == floatData1;
                        break;

                        case EqualityState.GreaterThan:
                        passOn = floatUser > floatData1;
                        break;

                        case EqualityState.LessThan:
                        passOn = floatUser < floatData1;
                        break;
                    }
                    break;

                    case ConditionalState.Int:
                    int intData1 = (int) val.userParam;
                    int intUser = (int) userData;

                    switch (val.equality) {
                        case EqualityState.Equal:
                        passOn = intUser == intData1;
                        break;

                        case EqualityState.GreaterThan:
                        passOn = intUser > intData1;
                        break;

                        case EqualityState.LessThan:
                        passOn = intUser < intData1;
                        break;
                    }
                    break;
                }
                if (passOn)
                    return val.nodule;
            }
            return null;
        }

        #region Serialization Methods

        public override ScriptableObject[] GetAllReferences (bool getAllRefs) {
            List<ScriptableObject> refList = new List<ScriptableObject> (base.GetAllReferences (getAllRefs));

            if (getAllRefs)
                refList.Add (condition);
            return refList.ToArray ();
        }

        public override void ReplaceAllReferences (Func<ScriptableObject, ScriptableObject> ReplacedSO) {
            base.ReplaceAllReferences (ReplacedSO);
            condition = (Condition) ReplacedSO (condition);

            for (int i = 0; i < conditionValues.Count; i++) {
                ConditionValue CV = conditionValues[i];
                conditionValues[i] = new ConditionValue (Nodules.Get (i), CV.equality, CV.dataParam, CV.userParam);
            }
        }

        #endregion

        #region Condition Based Classes

//class BoolCondition : ConditionValue<bool> {
        //    public BoolCondition (BaseNodule _nodule, bool _dataParam) : base (_nodule, _dataParam) {
        //        nodule = _nodule;
        //        equality = EqualityState.Equal;
        //        dataParam = _dataParam;
        //    }

        //    public override void OnGUI () {

        //    }
        //}

        //class IntCondition : ConditionValue<int> {
        //    public IntCondition (BaseNodule _nodule, int _dataParam) : base (_nodule, _dataParam) {
        //        nodule = _nodule;
        //        equality = EqualityState.Equal;
        //        dataParam = _dataParam;
        //    }

        //    public override void OnGUI () {
        //        throw new NotImplementedException ();
        //    }
        //}

        //class FloatCondition : ConditionValue<float> {
        //    public FloatCondition (BaseNodule _nodule, float _dataParam) : base (_nodule, _dataParam) {
        //        nodule = _nodule;
        //        equality = EqualityState.Equal;
        //        dataParam = _dataParam;
        //    }

        //    public override void OnGUI () {
        //        throw new NotImplementedException ();
        //    }
        //}

       #endregion
    }
}
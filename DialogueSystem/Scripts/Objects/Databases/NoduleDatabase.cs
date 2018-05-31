using System;
using System.Collections.Generic;
using UnityEngine;

namespace DialogueSystem {
    public class NoduleDatabase : DatabaseHelper.SODatabase<BaseNodule> {
        [SerializeField]
        BaseNode mainNode;
        [SerializeField]
        BaseNodule mainNodule;
        [SerializeField]
        float noduleSpacing;
 
        List<BaseNodule> sideList;

        public static NoduleDatabase CreateNew (NodeObject mainObject) {
            NoduleDatabase DB = CreateInstance<NoduleDatabase> ();
            DB.name = mainObject.name + " - " + DB.GetType ().Name;

            if (mainObject is BaseNode) {
                DB.mainNode = mainObject as BaseNode;
                DB.mainNodule = null;
            } else if (mainObject is BaseNodule) {
                DB.mainNode = null;
                DB.mainNodule = mainObject as BaseNodule;
            } else
                throw new UnityException ();

            if (!DB)
                throw new UnityException ();
            DB.Construct ();
            return DB;
        }

        protected override void Construct () {
            base.Construct ();
            sideList = new List<BaseNodule> ();
            noduleSpacing = 5;
        }

        public override void Init () {
            base.Init ();

            if (!mainNode && !mainNodule)
                throw new UnityException ();
            else if (mainNode && mainNodule)
                throw new UnityException ();

            for (int i = 0; i < Count; i++) {
                BaseNodule nodule = Get (i);

                if (!nodule) {
                    Debug.LogError ("Nodule is null in base node '" + name + "', Removing References.");
                    RemoveAt (i);
                    i--;
                    continue;
                }

                if (!ItemNames.Contains (nodule.name))
                    ItemNames.Add (nodule.name);
                nodule.Init ();
            }

            if (Count > 0)
                ReCalcAllNodulePos (mainNode, true);
        }

        public override void OnGUI () {
            for (int i = 0; i < Count; i++)
                Get (i).OnGUI ();
        }

        #region List Method

        public override void Add (BaseNodule item) {
            if (mainNode && mainNode == item.MainNode) {
                base.Add (item);
                ReCalcAllNodulePos (mainNode, true);
            } else if (mainNodule) {
                if (item.Nodules.Contains (mainNodule) && !Contains (item))
                    base.Add (item);
                else if (NoduleTypes.CheckCompatibility (mainNodule, item)) {
                    base.Add (item);
                    item.Nodules.Add (mainNodule);
                }

                if (mainNodule is OutputNodule)
                    (mainNodule as OutputNodule).UpdateConditionalValues ();
                ReCalcAllNodulePos (mainNodule.MainNode, true);
            }
        }

        public override void Insert (int index, BaseNodule item) {
            if (mainNode == item.MainNode) {
                base.Insert (index, item);
                ReCalcAllNodulePos (mainNode, true);
            } else if (mainNodule && NoduleTypes.CheckCompatibility (mainNodule, item)) {
                base.Insert (index, item);
                item.Nodules.Add (mainNodule);

                if (mainNodule is OutputNodule)
                    (mainNodule as OutputNodule).UpdateConditionalValues ();
                ReCalcAllNodulePos (mainNodule.MainNode, true);
            }
        }

        public override void Remove (BaseNodule item) {
            if (!Contains (item)) {
                Debug.LogError ("Nodule '" + item + "' does not exist in this nodule '" + this + "'.");
                return;
            }

            if (mainNode) {
                if (mainNode is StartNode && !(mainNode as StartNode).DoIt)
                    throw new UnityException ();
                base.Remove (item);
                ReCalcAllNodulePos (mainNode, true);
            } else if (mainNodule) {
                if (!item.Nodules.Contains (mainNodule) && Contains (item))
                    base.Remove (item);
                else {
                    base.Remove (item);
                    item.Nodules.Remove (mainNodule);
                }

                if (Count == 0)
                    item.side = item.DefaultSide;

                if (mainNodule is OutputNodule)
                    (mainNodule as OutputNodule).UpdateConditionalValues ();
                ReCalcAllNodulePos (mainNodule.MainNode);
            }
        }

        public override void RemoveAt (int index) {
            Remove (Get (index));
        }

        public override BaseNodule Replace (int index, BaseNodule item) {
            if (mainNode is StartNode)
                throw new UnityException ();

            if (mainNode != item.MainNode)
                throw new UnityException ();
            return base.Replace (index, item);
        }

        public override void RemoveRange (int startIndex, int endIndex) {
            if (mainNode is StartNode)
                throw new UnityException ();
            base.RemoveRange (startIndex, endIndex);
        }

        #endregion

        #region Static Methods

        public static void ReCalcAllNodulePos (BaseNode node, bool forceReCalc = false) {
            foreach (BaseNodule nodule in node.Nodules) {
                for (int i = 0; i < nodule.Nodules.Count; i++) {
                    BaseNodule connectedNodule = nodule.Nodules.Get (i);
                    BaseNode connectedNode = connectedNodule.MainNode;

                    if (node.Position.xMin > connectedNode.Position.xMin && node.Position.xMin < connectedNode.Position.xMax) {
                        if (i == 0 && nodule.side != NoduleSide.Left) {
                            nodule.side = NoduleSide.Left;
                            forceReCalc = true;
                        }

                        if (connectedNodule.side != NoduleSide.Left) {
                            connectedNodule.side = NoduleSide.Left;
                            forceReCalc = true;
                        }
                    } else if (node.Position.xMax > connectedNode.Position.xMin && node.Position.xMin < connectedNode.Position.xMax) {
                        if (i == 0 && nodule.side != NoduleSide.Right) {
                            nodule.side = NoduleSide.Right;
                            forceReCalc = true;
                        }

                        if (connectedNodule.side != NoduleSide.Right) {
                            connectedNodule.side = NoduleSide.Right;
                            forceReCalc = true;
                        }
                    } else if (node.Position.xMin > connectedNode.Position.xMax) {
                        if (i == 0 && nodule.side != NoduleSide.Left) {
                            nodule.side = NoduleSide.Left;
                            forceReCalc = true;
                        }

                        if (connectedNodule.side != NoduleSide.Right) {
                            connectedNodule.side = NoduleSide.Right;
                            forceReCalc = true;
                        }
                    } else if (node.Position.xMax < connectedNode.Position.xMin) {
                        if (i == 0 && nodule.side != NoduleSide.Right) {
                            nodule.side = NoduleSide.Right;
                            forceReCalc = true;
                        }

                        if (connectedNodule.side != NoduleSide.Left) {
                            connectedNodule.side = NoduleSide.Left;
                            forceReCalc = true;
                        }
                    }

                    if (forceReCalc) {
                        connectedNodule.ReCalcXPos ();
                        ReCalcYPos (connectedNode.Nodules);
                    }
                }

                if (forceReCalc) {
                    nodule.ReCalcXPos ();
                    ReCalcYPos (node.Nodules);
                }
                nodule.Position = new Rect (node.Position.center + nodule.offSet, nodule.Position.size);
            }
        }

        static void ReCalcYPos (NoduleDatabase nodules) {
            nodules.sideList = nodules.FindAll (n => n.side == NoduleSide.Left);
            YCalc (nodules);
            nodules.sideList = nodules.FindAll (n => n.side == NoduleSide.Right);
            YCalc (nodules);
        }

        static void YCalc (NoduleDatabase nodules) {
            if (nodules.sideList.Count == 0)
                return;
            float noduleYSize = 0;

            foreach (BaseNodule nodule in nodules.sideList)
                noduleYSize += nodule.Position.size.y + nodules.noduleSpacing;
            //Debug.Log (nodules.mainNode.name + ", " + nodules.mainNode.Position + ", " + noduleYSize);

            while (noduleYSize > nodules.mainNode.Position.height) {
                nodules.noduleSpacing--;

                foreach (BaseNodule nodule in nodules.sideList)
                    noduleYSize += nodule.Position.size.y + nodules.noduleSpacing;
            }
            float curY = -noduleYSize / 2;

            foreach (BaseNodule nodule in nodules.sideList) {
                nodule.offSet.y = curY;
                curY += nodule.Position.size.y + nodules.noduleSpacing;
            }
            nodules.noduleSpacing = 5;
        }

        #endregion

        #region Serialization Methods

        public override ScriptableObject[] GetAllReferences (bool getAllRefs) {
            List<ScriptableObject> refList = new List<ScriptableObject> (base.GetAllReferences (getAllRefs)) { };

            if (getAllRefs) {
                refList.Add (mainNode);
                refList.Add (mainNodule);
            }
            return refList.ToArray ();
        }

        public override void ReplaceAllReferences (Func<ScriptableObject, ScriptableObject> ReplacedSO) {
            base.ReplaceAllReferences (ReplacedSO);
            mainNode = ReplacedSO (mainNode) as DialogueNode;
            mainNodule = ReplacedSO (mainNodule) as BaseNodule;
        }

        #endregion
    }
}
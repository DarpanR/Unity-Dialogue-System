using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEngine;

namespace DialogueSystem {
    public static class NoduleTypes {
        public static Dictionary<NoduleData, BaseNodule> noduleTypes;

        public static void FetchAllNodules () {
            noduleTypes = new Dictionary<NoduleData, BaseNodule> ();

            foreach (Assembly assem in AppDomain.CurrentDomain.GetAssemblies ().Where (a => a.FullName.Contains ("Assembly")))
                foreach (Type type in assem.GetTypes ().Where (a => !a.IsAbstract && a.IsClass && a.IsSubclassOf (typeof (BaseNodule)))) {
                    if (type.GetCustomAttributes (false).Count () == 0)
                        continue;
                    NoduleAttribute attri = type.GetCustomAttributes (typeof (NoduleAttribute), false)[0] as NoduleAttribute;

                    if (attri != null && !attri.Hide) {
                        NoduleData data = new NoduleData (attri);
                        noduleTypes.Add (data, NodeObject.CreateNew<BaseNodule> (data.GetClassName));
                    }
                }
        }

        public static NoduleData GetNoduleAttritube (string className) {
            return noduleTypes.Keys.Single (data => data.GetClassName.Equals (className));
        }

        public static NoduleData GetNoduleAttritube<T> () {
            return noduleTypes.Keys.Single (data => data.GetClassName.Equals (typeof (T).Name));
        }

        public static BaseNodule GetDefaultNodule (string className) {
            return noduleTypes[GetNoduleAttritube (className)];
        }

        public static BaseNodule GetDefaultNodule<T> () {
            return noduleTypes[GetNoduleAttritube<T> ()];
        }

        public static bool CheckCompatibility (BaseNode node, BaseNodule nodule) {
            if (!node || !nodule) {
                Debug.LogError ("");
                return false;
            }

            if (node is StartNode && node.Nodules.Count > 0) {
                Debug.LogWarning ("");
                return false;
            }

            if (node is OptionNode)
                if ((node as OptionNode).MainNode && nodule is InputNodule) {
                    Debug.LogWarning ("Can not make a Input Nodule for a node connected another.");
                    return false;
                }

            if (node.Nodules.Contains (nodule)) {
                Debug.LogError ("Nodule '" + nodule + "' already exist in this node ' " + node + ".");
                return false;
            }
            return true;
        }

        public static bool CheckCompatibility (BaseNodule startNodule, BaseNodule endNodule) {
            if (!startNodule || !endNodule) {
                Debug.LogWarning ((!startNodule ? "Start " : "End ") + " nodule is null.");
                return false;
            }

            if (!startNodule.MainNode || !endNodule.MainNode) {
                Debug.LogWarning ("");
                return false;
            }

            if (startNodule == endNodule) {
                Debug.Log (startNodule.GetInstanceID () + ", " + endNodule.GetInstanceID ());
                Debug.LogWarning ("Start and end nodules are both the same object.");
                return false;
            }

            if (startNodule.MainNode == endNodule.MainNode) {
                Debug.LogWarning ("Start and end nodules have the same node.");
                return false;
            }

            if (!GetNoduleAttritube (startNodule.GetID).CheckCompatibility (endNodule.GetID)) {
                Debug.LogWarning ("Start and end nodules are not compatible.");
                return false;
            }

            if (startNodule.Nodules.Contains (endNodule) || endNodule.Nodules.Contains(startNodule)) {
                Debug.LogWarning ("Start and end nodules are already connected.");
                return false;
            }

            if (startNodule.MainNode is OptionNode && !(startNodule is OutputNodule) || endNodule.MainNode is OptionNode && !(endNodule is OutputNodule)) {
                OptionNode option = ((startNodule.MainNode is OptionNode) ? startNodule.MainNode : endNodule.MainNode) as OptionNode;

                if (option.MainNode == endNodule.MainNode) {
                    Debug.LogWarning ("Recursive connection is a world of hurt, just dont do it.");
                    return false;
                }
            }

            if (startNodule.MainNode is StartNode && startNodule is InputNodule) {
                Debug.LogError ("");
                return false;
            } else if (endNodule.MainNode is StartNode && endNodule is InputNodule) {
                Debug.LogError ("");
                return false;
            }
            return true;
        }
    }

    public struct NoduleData {
        public string ContextPath { get; private set; }
        public string GetClassName { get { return (ContextPath.Contains ("/")) ? ContextPath.Substring (ContextPath.LastIndexOf ("/") + 1) : ContextPath; } }
        public string[] allCompatibleNodules;

        public NoduleData (NoduleAttribute noduleAttri) {
            ContextPath = noduleAttri.ContextPath;
            allCompatibleNodules = noduleAttri.allCompatibleNodules;
        }

        public bool CheckCompatibility (Type nodule) {
            if (allCompatibleNodules.Contains (nodule.ToString ()))
                return true;
            return false;
        }

        public bool CheckCompatibility (string className) {
            if (allCompatibleNodules.Contains (className))
                return true;
            return false;
        }
    }

    public class NoduleAttribute : Attribute {
        public string ContextPath { get; private set; }
        public bool Hide { get; private set; }
        public string[] allCompatibleNodules;

        public NoduleAttribute (string newContextPath) {
            ContextPath = newContextPath;
            Hide = true;
            allCompatibleNodules = new string[] { "None" };
        }

        public NoduleAttribute (string newContextPath, bool hideNodule, params string[] compatibleNodules) {
            ContextPath = newContextPath;
            Hide = hideNodule;
            allCompatibleNodules = compatibleNodules;
        }
    }
}
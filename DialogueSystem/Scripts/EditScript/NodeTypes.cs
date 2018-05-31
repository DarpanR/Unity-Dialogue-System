using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace DialogueSystem {
    public static class NodeTypes {
        public static Dictionary<NodeData, BaseNode> nodeTypes;

        public static void FetchAllNodes () {
            nodeTypes = new Dictionary<NodeData, BaseNode> ();
            IEnumerable<Assembly> assems = AppDomain.CurrentDomain.GetAssemblies ().Where (a => a.FullName.Contains ("Assembly"));

            foreach (Assembly assem in assems) 
                foreach (Type type in assem.GetTypes ().Where (a => !a.IsAbstract && a.IsClass && a.IsSubclassOf (typeof (BaseNode)))) {
                    NodeAttribute attri = type.GetCustomAttributes (typeof (NodeAttribute), false)[0] as NodeAttribute;

                    if (attri != null && !attri.Hide) {
                        NodeData data = new NodeData (attri);
                        nodeTypes.Add (data, NodeObject.CreateNew<BaseNode> (data.GetClassName));
                    }
                }
        }

        public static NodeData GetNodeAttritube (string className) {
            return nodeTypes.Keys.Single (data => data.GetClassName == className);
        }

        public static NodeData GetNodeAttritube<T> () {
            return nodeTypes.Keys.Single (data => data.GetClassName == typeof (T).Name);
        }

        public static BaseNode GetDefaultNode (string className) {
            return nodeTypes [GetNodeAttritube (className)];
        }

        public static BaseNode GetDefaultNode<T> () where T : DialogueNode {
            return nodeTypes[GetNodeAttritube<T> ()];
        }
    }


    public struct NodeData {
        public string ContextPath { get; private set; }
        public string GetClassName { get { return (ContextPath.Contains ("/")) ? ContextPath.Substring (ContextPath.LastIndexOf ("/") + 1) : ContextPath; } }

        public NodeData (NodeAttribute nodeAttri) {
            ContextPath = nodeAttri.ContextPath;
        }
    }

    public class NodeAttribute : Attribute {
        public string ContextPath { get; private set; }
        public bool Hide { get; private set; }

        public NodeAttribute (string newContextPath, bool hideNode = false) {
            ContextPath = newContextPath;
            Hide = hideNode;
        }
    }
}

using UnityEngine;

namespace DialogueSystem {
    public class NodeDatabase : DatabaseHelper.SODatabase<BaseNode> {
        public override void Init () {
            base.Init ();
            bool startNode = false;

            for (int i = 0; i < Count; i++) {
                BaseNode node = Get (i);

                if (!node) {
                    Debug.LogError ("The Node at index '" + i + "' is missing its reference. Removing from NodeDatabase.");
                    base.RemoveAt (i);
                    i--;
                    continue;
                }

                if (node is StartNode) {
                    if (!startNode)
                        startNode = true;
                    else {
                        Debug.LogError ("");
                        base.Remove (node);
                        (node as StartNode).Delete (true);
                        i--;
                        continue;
                    }
                }

                if (!ItemNames.Contains (node.name))
                    ItemNames.Add (node.name);
                node.Init ();
            }

            if (!startNode)
                base.Add (NodeObject.CreateNew<StartNode> ("StartNode"));
        }

        public override void OnGUI () {
            for (int i = 0; i < Count; i++)
                Get (i).OnGUI ();
        }

        public override void Add (BaseNode item) {
            if (item is StartNode)
                throw new UnityException ();
            base.Add (item);
        }

        public override void Insert (int index, BaseNode item) {
            if (item is StartNode)
                throw new UnityException ();
            base.Insert (index, item);
        }

        public override void Remove (BaseNode item) {
            if (item is StartNode)
                throw new UnityException ();
            base.Remove (item);
        }

        public override void RemoveAt (int index) {
            if (Get (index) is StartNode)
                throw new UnityException ();
            base.RemoveAt (index);
        }

        public override BaseNode Replace (int index, BaseNode item) {
            if (Get (index) is StartNode)
                throw new UnityException ();
            return base.Replace (index, item);
        }

        public void DuplicateNode (DialogueNode item) {
            Add (DialogueNode.Create (item.GetID, item.name + ".copy", item.Position.position + new Vector2 (50, 50), item.Actor));
        }
    }
}

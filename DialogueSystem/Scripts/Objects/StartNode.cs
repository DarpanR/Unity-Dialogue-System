using UnityEngine;

namespace DialogueSystem {
    [Node("StartNode", true)]
    public class StartNode : BaseNode {
        public bool DoIt { get; private set; }

        protected override void Construct () {
            name = "Start Node";
            position = new Rect (CanvasGUI.CanvasToScreenPosition(CanvasGUI.CanvasRect.center), ResourceManager.START_NODE_SIZE);
            nodules = NoduleDatabase.CreateNew (this);
            nodules.Add (BaseNodule.Create<OutputNodule> (this));
            DialogueEditorGUI.Cache.SaveNewObject (this, false);
        }

        public override void Init () {
            base.Init ();

            if (nodules.Count == 0)
                nodules.Add (BaseNodule.Create<OutputNodule> (nodules.NextItemName ("OutputNodule"), this));
            else {
                OutputNodule output = nodules.First (i => i is OutputNodule) as OutputNodule;

                if (!output)
                    nodules.Insert (0, BaseNodule.Create<OutputNodule> (nodules.NextItemName ("OutputNodule"), this));
            }
        }

        public override void OnGUI () {
            base.OnGUI ();
            CanvasGUI.Box (position, new GUIContent ("Start Node"), Color.green);
        }

        public void Delete (bool ReallyTho) {
            DoIt = true;
            base.Delete ();
        }

        public override void Delete () {
            Debug.LogWarning ("Cant Delete This Yo!!");
        }
    }
}

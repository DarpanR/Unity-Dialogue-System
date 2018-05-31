using UnityEngine;
using System.Linq;

namespace DialogueSystem {
    public class Actor : CanvasObject {
        [SerializeField]
        Color tint;
        [SerializeField]
        int count;

        public Color Tint { get { return tint; } set { tint = value; } }
        public int Count { get { return count; } }

        public static Actor Create (string actorName, Color color, Rect actorPos) {
            Actor actor = CreateInstance<Actor> ();
            DialogueEditorGUI.Cache.SaveNewObject (actor);
            actor.Contruct (actorName, color, actorPos);
            return actor;
        }

        public void Contruct (string actorName, Color color, Rect actorPos) {
            name = actorName;
            Position = actorPos;
            tint = color;
        }

        public override void Delete () {
            foreach (DialogueNode node in DialogueEditorGUI.Cache.Nodes.OfType<DialogueNode> ())
                if (node.Actor == this)
                    node.Actor = DialogueEditorGUI.Cache.Actors.DefaultActor;
            DialogueEditorGUI.Cache.Actors.Remove (this);
            DialogueEditorGUI.Cache.Actors.ItemNames.Remove (name);
            DestroyImmediate (this, true);
        }

        public override void Init () {
            if (string.IsNullOrEmpty (name))
                name = "Actor " + (DialogueEditorGUI.Cache.Actors.GetIndex (this) + 1);
            count = 0;

            foreach (DialogueNode node in DialogueEditorGUI.Cache.Nodes.OfType<DialogueNode> ())
                if (node.Actor == this)
                    count++;
        }

        public override void OnGUI () {
            if (CanvasGUI.DoubleClick (position, new GUIContent (name + ": " + Count), GUI.skin.box, tint))
                OverlayMenu.UpdateOverlays (this);
        }
    }
}
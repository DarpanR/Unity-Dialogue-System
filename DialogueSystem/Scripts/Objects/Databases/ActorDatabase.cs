using UnityEngine;
using Random = UnityEngine.Random;

namespace DialogueSystem {
    public class ActorDatabase : DatabaseHelper.SODatabase<Actor> {
        public Actor DefaultActor { get { return Get (0); } }

        public override void Init () {
            base.Init ();

            if (Count == 0)
                base.Add (Actor.Create ("Default Actor", Color.gray, new Rect (5, 27, CanvasGUI.OptionRect.width - 10, 20)));

            if (Get (0).name != "Default Actor")
                base.Insert (0, Actor.Create ("Default Actor", Color.gray, new Rect (5, 27, CanvasGUI.OptionRect.width - 10, 20)));

            for (int i = 0; i < Count; i++) {
                Actor actor = Get (i);

                if (!actor) {
                    Debug.LogError ("The Actor at index '" + i + "' is missing its reference. Removing from ActorDatabase.");
                    RemoveAt (i);
                    i--;
                    continue;
                }

                if (string.IsNullOrEmpty (actor.name))
                    actor.name = NextItemName ("New Actor");

                if (!ItemNames.Contains (actor.name))
                    ItemNames.Add (actor.name);
                actor.Init ();
            }
        }

        public override void OnGUI () {
            for (int i = 0; i < Count; i++) {
                Actor actor = Get (i);

                if (i > 0 && CanvasGUI.Button (new Rect (actor.Position.xMax + 2, actor.Position.yMin, 20, 20), "X")) {
                    actor.Delete ();
                    continue;
                }
                actor.OnGUI ();
            }

            if (CanvasGUI.Button (new Rect (CanvasGUI.OptionRect.width - 25, 5, 20, 20), "+")) {
                Add (Actor.Create (NextItemName("New Actor"),
                    new Color (Random.Range (0.000f, 1.000f), Random.Range (0.000f, 1.000f), Random.Range (0.000f, 1.000f)),
                    new Rect (5, 27 + 22 * (Count), CanvasGUI.OptionRect.width - 34, 20)));
            }
        }

        public override void Add (Actor item) {
            if (item == DefaultActor)
                throw new UnityException ();
            base.Add (item);
        }

        public override void Insert (int index, Actor item) {
            if (index == 0)
                throw new UnityException ();
            base.Insert (index, item);
        }

        public override void Remove (Actor item) {
            if (item == DefaultActor)
                throw new UnityException ();
            for (int i = GetIndex (item); i < Count; i++)
                Get (i).Position = new Rect (5, 27 + 22 * (i - 1), CanvasGUI.OptionRect.width - 34, 20);
            base.Remove (item);
        }

        public override void RemoveAt (int index) {
            if (index >= Count)
                throw new UnityException ();

            if (index == 0)
                throw new UnityException ();
            for (int i = index; i < Count; i++)
                Get (i).Position = new Rect (5, 27 + 22 * (i - 1), CanvasGUI.OptionRect.width - 34, 20);
            base.RemoveAt (index);
        }

        public override void RemoveRange (int startIndex, int endIndex) {
            if (startIndex == 0 || endIndex == 0)
                throw new UnityException ();
            base.RemoveRange (startIndex, endIndex);
        }

        public override Actor Replace (int index, Actor item) {
            if (index == 0 || item == DefaultActor)
                throw new UnityException ();
            return base.Replace (index, item);
        }

        public override void Move (int takeIndex, int placeIndex) {
            if (takeIndex == 0 || placeIndex == 0)
                throw new UnityException ();
            base.Move (takeIndex, placeIndex);
        }

        public override void Move (Actor takeItem, int placeIndex) {
            if (takeItem == DefaultActor || placeIndex == 0)
                throw new UnityException ();
            base.Move (takeItem, placeIndex);
        }
    }
}

using UnityEngine;

namespace DialogueSystem {
    public interface ICanvasObject {
        Rect Position { get; set; }
        string GetID { get; }

        void Delete ();
        void Init ();
        void OnGUI ();
    }
}

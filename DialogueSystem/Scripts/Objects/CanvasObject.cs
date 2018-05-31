using UnityEngine;

namespace DialogueSystem {
    public abstract class CanvasObject : ScriptableObject, ICanvasObject {
        [SerializeField]
        protected Rect position;
        public virtual Rect Position { get { return position; } set { position = value; } }
        public string GetID { get { return GetType ().Name; } }
        
        public abstract void Delete ();
        public abstract void Init ();
        public abstract void OnGUI ();
    }
}

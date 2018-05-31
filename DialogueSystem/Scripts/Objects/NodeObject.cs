using System;
using UnityEngine;

namespace DialogueSystem {
    public abstract class NodeObject : CanvasObject, DatabaseHelper.ISOHandling {
        
        public static T CreateNew<T> () where T : NodeObject {
            T obj = CreateInstance<T> ();
            obj.Construct ();
            return obj;
        }

        public static T CreateNew<T> (string className) where T : NodeObject {
            T obj = CreateInstance (className) as T;
            obj.Construct ();
            return obj;
        }

        protected abstract void Construct ();

        public abstract ScriptableObject[] GetAllReferences (bool getAllRefs);
        public abstract void ReplaceAllReferences (Func<ScriptableObject, ScriptableObject> ReplacedSO);
    }
}

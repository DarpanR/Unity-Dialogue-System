using UnityEngine;

namespace DatabaseHelper {
    public interface ISOHandling {
        ScriptableObject[] GetAllReferences (bool getAllRefs);
        void ReplaceAllReferences (System.Func<ScriptableObject, ScriptableObject> ReplacedSO);
    }
}

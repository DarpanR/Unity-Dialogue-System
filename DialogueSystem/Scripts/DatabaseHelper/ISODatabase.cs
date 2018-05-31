using System.Collections.Generic;
using UnityEngine;

namespace DatabaseHelper {
    public interface ISODatabase<T> where T : ScriptableObject {
        List<string> ItemNames { get; set; }

        void Init ();
        void OnGUI ();
    }
}

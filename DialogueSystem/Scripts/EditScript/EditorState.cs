using UnityEngine;
using System;

namespace DialogueSystem {
    [Flags]
    public enum EventSpace {
        None = 0,
        Nodule = 1,
        Node = 2, 
        Canvas = 4, 
        Actor = 8,
        Toolbar = 16,
        NodeSpace = Nodule | Node,
        CanvasSpace = NodeSpace | Canvas,
        Everything = CanvasSpace | Actor | Toolbar
    }
    
    public class EditorState {
        public EditorStates States { get; private set; }

        public ScriptableObject[] focusedObjects;
        public ScriptableObject selectedObject;

        public bool dragNode, panWindow;
        public Vector2 dragStartPos, startPos, dragDelta, panDelta, canvasSize;

        public bool makeConnection;
        public int connectionIndex;
        public BaseNodule startNodule, endNodule;

        public EditorState (EditorStates editorStates) {
            States = editorStates;
            Reset ();
        }

        public void Reset () {
            focusedObjects = null;
            selectedObject = null;

            dragNode = false;
            panWindow = false;
            dragStartPos = Vector2.zero;
            startPos = Vector2.zero;
            dragDelta = Vector2.zero;
            canvasSize = new Vector2 (10000, 10000);
            panDelta = -canvasSize / 2;

            makeConnection = false;
            connectionIndex = -1;
            startNodule = null;
            endNodule = null;
        }

        public void UpdateParams (EditorState oldES) {
            focusedObjects = oldES.focusedObjects;
            selectedObject = oldES.selectedObject;

            dragNode = oldES.dragNode;
            panWindow = oldES.panWindow;
            dragStartPos = oldES.dragStartPos;
            startPos = oldES.startPos;
            dragDelta = oldES.dragDelta;
            panDelta = oldES.panDelta;

            makeConnection = oldES.makeConnection;
            startNodule = oldES.startNodule;
            endNodule = oldES.endNodule;
        }
	}	
}

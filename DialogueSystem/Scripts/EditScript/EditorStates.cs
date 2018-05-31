using UnityEditor;
using System.Collections.Generic;
using UnityEngine;	

namespace DialogueSystem {
	public class EditorStates {
        public string EditorName { get; private set; }
        public NodeDatabase Nodes { get; private set; }
        public ActorDatabase Actors { get; private set; }
        public ConditionDatabase Conditions { get; private set; }

        public string info;
        public Vector2 mousePos;
        public Event curEvent;
        public EventSpace curSpace;

        List<EditorState> states;
        public EditorState curState;
        public int capacity;
        int curIndex;

        public EditorStates (EditorCache cache) {
            EditorName = cache.CanvasName;
            Nodes = cache.Nodes;
            Actors = cache.Actors;
            Conditions = cache.Conditions;
            Init ();
        }

        public EditorStates (EditorStates editorStates) {
            info = editorStates.info;
            mousePos = editorStates.mousePos;
            curEvent = editorStates.curEvent;
            curSpace = editorStates.curSpace;

            states = editorStates.states;
            curState = editorStates.curState;
            capacity = editorStates.capacity;
            curIndex = editorStates.curIndex;
        }

        public EditorStates (string message, EditorStates editorStates) {
            info = message;
            mousePos = editorStates.mousePos;
            curEvent = editorStates.curEvent;
            curSpace = editorStates.curSpace;

            states = editorStates.states;
            curState = editorStates.curState;
            capacity = editorStates.capacity;
            curIndex = editorStates.curIndex;
        }
        
        void Init () {
            if (states == null)
                states = new List<EditorState> (capacity);
            else if (states.Count > capacity)
                states.RemoveRange (capacity, states.Count - capacity);

            for (int i = 0; i < states.Count; i++) {
                EditorState state = states[i];

                if (state == null) {
                    Debug.LogError ("");
                    states.RemoveAt (i--);
                    continue;
                }
                state.Reset ();
            }
            UpdateEvents ();
            curState = new EditorState (this);
        }

        public void UpdateEvents () {
            curEvent = Event.current;
            mousePos = curEvent.mousePosition;
            curSpace = EventSpace.None;
        }

        public void Update () {
            EditorState oldES = curState;

            if (curIndex != 0)
                states.RemoveRange (curIndex - 1, curIndex);

            if (states.Count >= capacity) {
                curState = states[states.Count - 1];
                states.Remove (curState);
            } else
                curState = new EditorState (this);
            curState.UpdateParams (oldES);
            states.Insert (0, oldES);
            curIndex = 0;
        }

        public void Undo () {
            if (curIndex < capacity) {
                EditorState oldES = curState;
                curState = states[curIndex + 1];
                states.RemoveAt (curIndex);
                states.Insert (curIndex, oldES);
            }
        }

        public void Redo () {
            if (curIndex > 0) {
                EditorState oldES = curState;
                curState = states[curIndex - 1];
                states.RemoveAt (curIndex);
                states.Insert (curIndex, oldES);
            } 
        }
	}	
}

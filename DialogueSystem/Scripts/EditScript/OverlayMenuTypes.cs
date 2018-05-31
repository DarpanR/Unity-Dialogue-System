using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;

namespace DialogueSystem {
    public static class OverlayMenuTypes {
        static List<KeyValuePair<OverlayMenuData, Type>> menuTypes;

        public static void FetchAllOverlayMenus () {
            menuTypes = new List<KeyValuePair<OverlayMenuData, Type>> ();
            IEnumerable<Assembly> assems = AppDomain.CurrentDomain.GetAssemblies ().Where (a => a.FullName.Contains ("Assembly"));

            foreach (Assembly assem in assems)
                foreach (Type type in assem.GetTypes ().Where (a => !a.IsInterface && a.IsClass && a.FullName.Contains ("OverlayMenu"))) {
                    object[] attris = type.GetCustomAttributes (typeof(OverlayMenuAttribute), false);

                    if (!(attris.Length > 0))
                        continue;
                    menuTypes.Add (new KeyValuePair<OverlayMenuData, Type> (new OverlayMenuData (attris[0] as OverlayMenuAttribute), type));
                }
        }

        public static bool Exists (CanvasObject obj) {
            return menuTypes.Exists (i => i.Key.objectType == obj.GetType ());
        }

        public static IOverlayMenu GetMenu (CanvasObject obj) {
            return menuTypes.Single (i => i.Key.objectType == obj.GetType ()).Value.GetConstructors ()[0].Invoke (new object[] { obj }) as IOverlayMenu;
        }

        public static bool AllowMultiple (CanvasObject obj) {
            return menuTypes.Find (i => i.Key.objectType == obj.GetType ()).Key.allowMultiple;
        }

        public static void Sort (List<IOverlayMenu> popUps) {
            popUps.Sort ((first, second) => menuTypes.Find (i => i.Value == first).Key.drawDepth.CompareTo (menuTypes.Find (i => i.Value == second).Key.drawDepth));
        }
    }

    public class OverlayMenuAttribute : Attribute {
        public Type objectType;
        public bool allowMultiple;
        public int drawDepth;

        public OverlayMenuAttribute (Type _objectType) {
            objectType = _objectType;
            allowMultiple = false;
            drawDepth = 1;
        }

        public OverlayMenuAttribute (Type _objectType, bool _allowMultiple) {
            objectType = _objectType;
            allowMultiple = _allowMultiple;
            drawDepth = 1;
        }

        public OverlayMenuAttribute (Type _objectType, int _drawDepth) {
            objectType = _objectType;
            allowMultiple = false;
            drawDepth = _drawDepth;
        }

        public OverlayMenuAttribute (Type _objectType, bool _allowMeltiple, int _drawDepth) {
            objectType = _objectType;
            allowMultiple = _allowMeltiple;
            drawDepth = _drawDepth;
        }
    }

    public struct OverlayMenuData {
        public Type objectType;
        public bool allowMultiple;
        public int drawDepth;

        public OverlayMenuData (OverlayMenuAttribute attribute) {
            objectType = attribute.objectType;
            allowMultiple = attribute.allowMultiple;
            drawDepth = attribute.drawDepth;
        }
    }

    public interface IOverlayMenu {
        void OnGUI ();
        bool Compare<T> (T objToCompare) where T : CanvasObject;
        bool Compare (Type objType);
    }

    public abstract class OverlayMenu<T> : IOverlayMenu where T : CanvasObject {
        protected T obj;
        protected Rect position;
        protected bool close;

        public OverlayMenu () {
            obj = null;
            position = new Rect ();
            close = false;
        }

        public OverlayMenu (T objToEdit) {
            obj = objToEdit;
        }

        public OverlayMenu (T objToEdit, Rect pos) {
            obj = objToEdit;
            position = pos;
        }

        public abstract void OnGUI ();

        public bool Compare<D> (D objToCompare) where D : CanvasObject {
            return obj == objToCompare;
        }

        public bool Compare (Type objType) {
            return obj.GetType () == objType;
        }
    }
}

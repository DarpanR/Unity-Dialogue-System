using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DatabaseHelper {
    public abstract class SODatabase<T> : ScriptableObject, ISODatabase<T>, IEnumerable, ISOHandling where T : ScriptableObject {
        [SerializeField]
        List<T> list;
        [SerializeField]
        List<string> itemNames;

        public List<string> ItemNames { get { return itemNames; } set { itemNames = value; } }

        public static D CreateNew<D> (string newName) where D : SODatabase<T> {
            D DB = CreateInstance<D> ();
            DB.name = newName + " - " + DB.GetType ().Name;
            DB.Construct ();

            if (!DB) 
                throw new UnityException ();
            return DB;
        }

        public static D CreateNew<D> (string className, string newName) where D : SODatabase<T> {
            D DB = CreateInstance (className) as D;
            DB.name = newName + " - " + DB.GetType ().Name;
            DB.Construct ();

            if (!DB)
                throw new UnityException ();
            return DB;
        }

        protected virtual void Construct () {
            list = new List<T> ();
            itemNames = new List<string> ();
        }

        public virtual void Init () {
            if (list == null)
                list = new List<T> ();

            if (itemNames == null)
                itemNames = new List<string> ();
        }

        public abstract void OnGUI ();

        public virtual string NextItemName (string nameToUse) {
            int number = 1;

            while (itemNames.Contains (nameToUse + " " + number))
                number++;
            return nameToUse + " " + number;
        }

        public virtual void Add (T item) {
            if (itemNames.Contains (item.name))
                item.name = NextItemName (item.name);
            list.Add (item);
            itemNames.Add (item.name);
        }

        public virtual void Insert (int index, T item) {
            if (itemNames.Contains (item.name))
                item.name = NextItemName (item.name);
            list.Insert (index, item);
            itemNames.Add (item.name);
        }

        public virtual void Remove (T item) {
            list.Remove (item);
            itemNames.Remove (item.name);
        }

        public virtual void RemoveAt (int index) {
            Remove (list[index]);
            itemNames.Remove (list[index].name);
        }

        public virtual void RemoveRange (int startIndex, int endIndex) {
            RemoveRange (startIndex, endIndex);

            for (int i = 0; i < endIndex - startIndex; i++)
                itemNames.Remove (list[i].name);
        }

        public virtual T Replace (int index, T item) {
            if (itemNames.Contains (item.name))
                item.name = NextItemName (item.name);
            RemoveAt (index);
            Insert (index, item);
            return list[index];
        }

        public virtual void Move (int takeIndex, int placeIndex) {
            if (placeIndex > takeIndex)
                placeIndex--;
            list.RemoveAt (takeIndex);
            list.Insert (placeIndex, list[takeIndex]);
        }

        public virtual void Move (T takeItem, int placeIndex) {
            if (placeIndex > GetIndex (takeItem))
                placeIndex--;
            list.Remove (takeItem);
            list.Insert (placeIndex, takeItem);
        }

        public int Count { get { return list.Count; } }

        public T Get (int index) {
            return list[index];
        }

        public bool Exist (Predicate<T> match) {
            return list.Exists (match);
        }

        public T First (Func<T, bool> match) {
            return list.First (match);
        }

        public T Find (Predicate<T> match) {
            return list.Find (match);
        }

        public List<T> FindAll (Predicate<T> match) {
            return list.FindAll (match);
        }

        public int GetIndex (T item) {
            return list.IndexOf (item);
        }

        public bool Contains (T item) {
            return list.Contains (item);
        }

        public IEnumerator GetEnumerator () {
            return list.GetEnumerator ();
        }

        public virtual ScriptableObject[] GetAllReferences (bool getAllRefs) {
            return list.ToArray ();
        }

        public virtual void ReplaceAllReferences (Func<ScriptableObject, ScriptableObject> ReplacedSO) {
            for (int i = 0; i < list.Count; i++)
                Replace (i, ReplacedSO (list[i]) as T);
        }
    }
}

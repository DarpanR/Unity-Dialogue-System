using System;
using System.IO;
using UnityEditor;
using UnityEngine;	

namespace DialogueSystem {
	public static class ResourceManager {
        public static string RESOURCEPATH { get; private set; }
        public static string SAVEPATH { get { return RESOURCEPATH + "/Saves"; } }
        public static string TEMPPATH { get { return SAVEPATH + "/Temp"; } }
        public static string TEMPFILEPATH {  get { return TEMPPATH + "/Last Session.asset"; } }

        public static Vector2 NODULE_SIZE { get { return new Vector2 (20, 20); } }
        public static Vector2 START_NODE_SIZE { get { return new Vector2 (75, 50); } }
        public static Vector2 OPTION_NODE_SIZE { get { return new Vector2 (300, 100); } }
        public static Vector2 MAIN_NODE_SIZE { get { return new Vector2 (300, 200); } }

        public static TimeSpan CheckRate { get { return new TimeSpan (0,2,0); } }

        public static void SetupPaths () {
            ///Currently Broken In Unity 2017.X
            ///HardCoding for 'ResourcePath' and implementing a IO search on failure.
            ///reimplement this later.
            //        foreach (string folder in AssetDatabase.GetSubFolders ("Assets")) {
            //if (folder.Contains ("DialogueSystem")) {
            //                RESOURCEPATH = folder + "/Resources";
            //                break;
            //            }
            //        }

            //        if (string.IsNullOrEmpty (RESOURCEPATH))
            //            throw new UnityException ();
            RESOURCEPATH = "Assets/DialogueSystem/Resources";

            if (!AssetDatabase.IsValidFolder (RESOURCEPATH)) {
                string dirPath = new System.Diagnostics.StackTrace (true).GetFrame (0).GetFileName ().Replace ("\\","/");
                dirPath = dirPath.Replace (Application.dataPath, "Assets");
                dirPath = dirPath.Remove (dirPath.IndexOf ("/DialogueSystem") + 15);

                ValidatePath (dirPath);
                RESOURCEPATH = dirPath + "/Resources";
            }
            ValidatePath (RESOURCEPATH);
            ValidatePath (SAVEPATH);
            ValidatePath (TEMPPATH);
        }

        public static void ValidatePath (string path) {
            if (path.Contains ("/") && !AssetDatabase.IsValidFolder (path)) {
                int index = path.LastIndexOf ("/");
                string folder = path.Substring (index + 1);
                string parentPath = path.Remove (index);

                Debug.LogError ("Missing folder. Making new folder '" + folder + "' at path '" + parentPath + "'.");
                
                ValidatePath (parentPath);
                AssetDatabase.CreateFolder (parentPath, folder);
            }

            if (!AssetDatabase.IsValidFolder (path))
                throw new UnityException ();
        }
    }
}

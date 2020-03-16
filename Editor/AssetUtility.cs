using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace BSS.TextEditor {
    public static class AssetUtility
    {
        public static string GetAssetRootPath() {
            return Application.dataPath;
        }


        public static string GetSelectAssetPath() {
            string path = Application.dataPath;
            if(Selection.activeObject != null) {
                path = AssetDatabase.GetAssetPath(Selection.activeObject);
            }
            return path;
        }
        public static string GetAssetPath(Object _object) {
            return AssetDatabase.GetAssetPath(_object);
        }

    }
}

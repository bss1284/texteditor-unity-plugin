using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace BSS.TextEditor {
    public static class TextAssetGenerator 
    {
        
        [MenuItem(itemName: "Assets/Create/Text Assets/Empty",priority =101)]
        public static void CreateEmptyFile() {
            CreateTextAsset("New Text.txt", "");
        }
        [MenuItem(itemName: "Assets/Create/Text Assets/Json File", priority = 102)]
        public static void CreateJsonFile() {
            CreateTextAsset("New Json.json", "{}");
        }
        [MenuItem(itemName: "Assets/Create/Text Assets/Md File", priority = 103)]
        public static void CreateMdFile() {
            CreateTextAsset("New File.md", "");
        }

        private static void CreateTextAsset(string name,string content){
            string path =AssetUtility.GetSelectAssetPath()+ "/" + name;
            var file = File.CreateText(path);
            if (content!=null) {
                file.Write(content);
            }
            file.Close();
            AssetDatabase.Refresh();
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object>(path);
        }
    }
}

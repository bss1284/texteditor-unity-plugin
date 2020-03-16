using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
//using Newtonsoft.Json;
using System;

namespace BSS.TextEditor
{
	public class TextAssetEditor : EditorWindow
	{
		private string changeText = "NOTEXT";
        private string originText = "";
		private Vector2 scrollPos = Vector2.zero;
		private bool onlyTextAsset = true;
		private string saveText;
        private int lastKeyboardFocus = -1;

        [MenuItem("Tools/BSS/Editor/Text Editor")]
		public static void ShowWindow()
		{
			GetWindow<TextAssetEditor>(false, "Text Editor", true);
        }
		private void OnSelectionChange()
		{
			saveText = null;
			changeText = "NOTEXT";
			Repaint();
		}
        

        void OnGUI()
		{
            HandleTabKey();
            HandleKeyboard();
            string path = AssetUtility.GetSelectAssetPath();
            string fileName = Path.GetFileName(path);
            string fileExtension = Path.GetExtension(path);
            bool isMdFile = fileExtension == ".md";
            bool isJsonFile = fileExtension == ".json";
            bool isChanged = originText != changeText && IsEnable();

            //Top Layout File Name
            GUILayout.Space(5f);
            GUILayout.BeginHorizontal();
            if (isChanged) {
                GUILayout.Label("*" + fileName);
            } else {
                GUILayout.Label(fileName);
            }
            if (IsEnable()) {
                if(GUILayout.Button("Revert", GUILayout.MaxWidth(100f))) {
                    RevertButton();
                }
            }
            GUILayout.FlexibleSpace();
            onlyTextAsset = GUILayout.Toggle(onlyTextAsset, "OnlyTextAsset", GUILayout.Width(100f), GUILayout.Height(20f));
			GUILayout.EndHorizontal();


            //Invalid Text Type.
            if (!IsEnable())
			{
				DrawUILine(Color.gray);
				if (onlyTextAsset)
				{
					GUILayout.Label("Please Select Text Asset");
				}
				else
				{
					GUILayout.Label("Please Select File");
				}
				return;
			}

            originText = File.ReadAllText(path);



            //Text Area
            GUILayout.BeginHorizontal();
			if (changeText == "NOTEXT")
			{
				changeText = originText;
			}
			scrollPos = GUILayout.BeginScrollView(scrollPos, GUILayout.MaxWidth(position.width));
            GUI.SetNextControlName("BSS_TextEditor");
            changeText = GUILayout.TextArea(changeText, GUILayout.MaxHeight(position.height));
            
            GUILayout.EndScrollView();
			GUILayout.EndHorizontal();



            //Bottom Layout Buttons (Left)
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Save", GUILayout.ExpandWidth(false), GUILayout.MaxHeight(50f), GUILayout.MaxWidth(100f)))
			{
                SaveButton();
            }

            //Bottom Layout Buttons (Right)
            GUILayout.FlexibleSpace();
            if (isMdFile) {
                if(GUILayout.Button("Go To Viewer Online", GUILayout.ExpandWidth(false), GUILayout.MaxHeight(50f), GUILayout.MaxWidth(150f))) {
                    Application.OpenURL("https://dillinger.io/");
                }
            } 
            if (saveText != null)
			{
                if (GUILayout.Button("UndoSave", GUILayout.MaxHeight(50f), GUILayout.MaxWidth(100f)))
				{
					changeText = saveText;
					File.WriteAllText(path, saveText);
					saveText = null;
					AssetDatabase.Refresh();
				}
			}

			GUILayout.EndHorizontal();
		}
		private bool IsEnable()
		{
			if (Selection.activeObject == null) return false;
			if (onlyTextAsset) return Selection.activeObject is TextAsset;
			var attr = File.GetAttributes(AssetUtility.GetSelectAssetPath());
			return !attr.HasFlag(FileAttributes.Directory);

		}

		private void DrawUILine(Color color, int thickness = 2, int padding = 10)
		{
			Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(padding + thickness));
			r.height = thickness;
			r.y += padding / 2;
			r.x -= 2;
			r.width += 6;
			EditorGUI.DrawRect(r, color);
		}



        private bool IsValidJson(string strInput) {
            return true;
            //strInput = strInput.Trim();
            //if((strInput.StartsWith("{") && strInput.EndsWith("}")) || //For object
            //    (strInput.StartsWith("[") && strInput.EndsWith("]"))) //For array
            //{
            //    try {
            //        var obj = Newtonsoft.Json.Linq.JToken.Parse(strInput);
            //        return true;
            //    } catch(JsonReaderException jex) {
            //        //Debug.Log(jex.Message);
            //        return false;
            //    } catch(Exception ex) {
            //        //Debug.Log(ex.ToString());
            //        return false;
            //    }
            //} else {
            //    return false;
            //}
        }
        private void SaveButton() {
            string path = AssetUtility.GetSelectAssetPath();
            string fileExtension = Path.GetExtension(path);
            bool isJsonFile = fileExtension == ".json";
            if(isJsonFile) {
                if(!IsValidJson(changeText)) {
                    EditorUtility.DisplayDialog("Json Validation", "Save Fail.\nJson format is invalid.", "OK");
                    return;
                }
            }
            saveText = File.ReadAllText(path);
            File.WriteAllText(path, changeText);
            AssetDatabase.Refresh();
        }
        private void RevertButton() {
            changeText = originText;
        }

        private void HandleKeyboard() {
            if(GUI.GetNameOfFocusedControl() != "BSS_TextEditor") return;
            if (!IsEnable()) return;
            
            Event current = Event.current;
            if(current.type != EventType.KeyDown) return;
            var textEditor = (UnityEngine.TextEditor)GUIUtility.GetStateObject(typeof(UnityEngine.TextEditor), GUIUtility.keyboardControl);
            if (textEditor==null) return;

            int saveCursor = textEditor.cursorIndex;
            int saveSelect = textEditor.selectIndex;

            switch(current.keyCode) {
                case KeyCode.S:
                    if (current.control) {
                        SaveButton();
                    }
                    break;
                case KeyCode.R:
                    if(current.control) {
                        RevertButton();
                    }
                    break;
            }
        }

        private void HandleTabKey () {
            if(Event.current.keyCode == KeyCode.Tab || Event.current.character == '\t') {
                if(Event.current.type == EventType.KeyUp) {
                    var textEditor = (UnityEngine.TextEditor)GUIUtility.GetStateObject(typeof(UnityEngine.TextEditor), GUIUtility.keyboardControl);
                    if (!Event.current.shift) {
                        for(var i = 0; i < 4; i++) {
                            textEditor.Insert(' ');
                        }
                        changeText = textEditor.text;
                    } else {
                        var min = Math.Min(textEditor.cursorIndex, textEditor.selectIndex);
                        var index = min;
                        var temp = textEditor.text;
                        for(var i = 1; i < 5; i++) {
                            if((min - i) < 0 || temp[min - i] != ' ') {
                                break;
                            }

                            index = min - i;
                        }

                        if(index < min) {
                            textEditor.selectIndex = index;
                            textEditor.cursorIndex = min;
                            textEditor.ReplaceSelection(string.Empty);
                            changeText = textEditor.text;
                        }
                    }
                    
                }
                if (Event.current.type!=EventType.Layout) {
                    Event.current.Use();
                }
            }
        }
    }
}


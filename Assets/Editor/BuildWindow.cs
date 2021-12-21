using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;


namespace ABFrame
{

    public class BuildWindow : EditorWindow
    {
        private GUIStyle StTitle;
        private GUIStyle StContent;
        private static string ConfigPath = Application.dataPath + "/ABConfig.json";

        private BuildTarget CurrentPlatform = BuildTarget.NoTarget;
        private BuildAssetBundleOptions BuildABOptions = BuildAssetBundleOptions.None;
        private Dictionary<string, string> LabelDic = new Dictionary<string, string>();
        private string mNewConfigPath;
        private string mNewABPath;

        private void OnEnable()
        {
            StTitle = new GUIStyle()
            {
                fontSize = 30,
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.BoldAndItalic,
                normal = new GUIStyleState(),
            };
            StTitle.normal.textColor = Color.cyan;
            
            StContent = new GUIStyle()
            {
                fontSize = 15,
                fontStyle = FontStyle.BoldAndItalic,
                normal = new GUIStyleState(),
            };
            StContent.normal.textColor = Color.green;

            if(File.Exists(ConfigPath))
            {
                StreamReader rStreamReader = new StreamReader(ConfigPath);
                string rJson = rStreamReader.ReadToEnd();
                this.LabelDic = JsonConvert.DeserializeObject<Dictionary<string,string>>(rJson);
            }
        }

        [MenuItem("Tools/BuildTool")]
        public static void ShowWindow()
        {
            EditorWindow rThisWindow = EditorWindow.GetWindow(typeof(BuildWindow), false, "BuildWindow", false);
            rThisWindow.minSize = new Vector2(700, 300);
            rThisWindow.maxSize = new Vector2(1000, 1500);
            rThisWindow.Show();
        }

        void OnGUI()
        {
            EditorGUILayout.Space();
            GUILayout.Label("BuildTool", StTitle);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("TargetPlatform:");
            CurrentPlatform = (BuildTarget)EditorGUILayout.EnumPopup((System.Enum)CurrentPlatform, new GUILayoutOption[] { GUILayout.MaxWidth(300) });
            EditorGUILayout.Space(10);
            GUILayout.Label("BuildABOptions:");
            BuildABOptions = (BuildAssetBundleOptions)EditorGUILayout.EnumFlagsField((System.Enum)BuildABOptions, new GUILayoutOption[] { GUILayout.MaxWidth(300) });
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(10);
            if (GUILayout.Button("BuildAllABAsset", GUILayout.Width(120)))
            {
                BuildAssetBundle.BuildAllAB(CurrentPlatform, BuildABOptions);
            }
            EditorGUILayout.Space(10);
            GUILayout.Label("Set AssetBundle Config",this.StContent);
            GUILayout.BeginHorizontal();
            this.mNewConfigPath = EditorGUILayout.TextField("新配置文件", this.mNewConfigPath, new GUILayoutOption[] { GUILayout.MinWidth(600), GUILayout.MaxWidth(800) });
            if (GUILayout.Button("+", GUILayout.Width(28)))
            {
                this.mNewConfigPath = EditorUtility.OpenFolderPanel("Asset File", Application.dataPath, "*.*");
                Debug.Log("所选文件夹为:" + this.mNewConfigPath);
            }
            GUILayout.EndHorizontal();
            
            //配置文件
            this.mNewABPath = EditorGUILayout.TextField("ABPath", this.mNewABPath, new GUILayoutOption[] { GUILayout.MinWidth(600), GUILayout.MaxWidth(800) });
            if (GUILayout.Button("Add New Config"))
            {
                if (!string.IsNullOrEmpty(this.mNewConfigPath)&&!this.LabelDic.ContainsKey(this.mNewConfigPath))
                {
                    this.LabelDic.Add(this.mNewConfigPath,this.mNewABPath);
                }
            }
            EditorGUILayout.Space(10);
            //Label 标记字典
            var rMarkList = new List<string>(this.LabelDic.Count);
            foreach (var rPair in this.LabelDic)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.TextField("FilePath", rPair.Key, new GUILayoutOption[] { GUILayout.MinWidth(400), GUILayout.MaxWidth(600) });
                EditorGUILayout.Space(10);
                EditorGUILayout.TextField("ABPath", rPair.Value, new GUILayoutOption[] { GUILayout.MinWidth(400), GUILayout.MaxWidth(600) });
                if (GUILayout.Button("-", GUILayout.Width(28)))
                {
                    rMarkList.Add(rPair.Key);
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space(1);
            }
            foreach (var rKey in rMarkList)
            {
                this.LabelDic.Remove(rKey);
            }
            EditorGUILayout.Space(1);
            
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("BuildByConfig", GUILayout.Width(120)))
            {
                
            }
            if (GUILayout.Button("UpdateConfig", GUILayout.Width(120)))
            {
                this.UpdateBuildConfig();
            }
            if (GUILayout.Button("SetLabel", GUILayout.Width(120)))
            {
                BuildAssetBundle.SetABNameANDABVarient(this.LabelDic);
            }
            EditorGUILayout.EndHorizontal();
        }
 
        void UpdateBuildConfig() 
        {
            string BuildConfigJson = JsonConvert.SerializeObject(LabelDic);
            if(string.IsNullOrEmpty(BuildConfigJson))
            {
                Debug.LogError("配置文件更新失败");
                return;
            }
            File.WriteAllText(ConfigPath, BuildConfigJson);
            Debug.Log(BuildConfigJson);
        }

    }
}

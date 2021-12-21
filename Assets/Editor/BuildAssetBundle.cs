using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Newtonsoft.Json;
namespace ABFrame
{
    public class BuildAssetBundle : Editor
    {
        public static void BuildAllAB(BuildTarget rTargetPlatform, BuildAssetBundleOptions rOptions)
        {
            if (!Directory.Exists(Application.streamingAssetsPath))
            {
                Directory.CreateDirectory(Application.streamingAssetsPath);
            }
            if (rTargetPlatform == BuildTarget.NoTarget || rOptions == BuildAssetBundleOptions.None)
            {
                Debug.LogError("BuildTarget is NULL or BuildOptions is NONE");
                return;
            }
            BuildPipeline.BuildAssetBundles(Application.streamingAssetsPath, BuildAssetBundleOptions.None & rOptions, rTargetPlatform);
        }

        public static void SetLables(Dictionary<string,string> LabelDic)
        {
            
        }
        
        public static void ClearAssetBundlesName()
        {
            string[] abNames = AssetDatabase.GetAllAssetBundleNames();
            for (int i = 0; i < abNames.Length; i++)
            {
                AssetDatabase.RemoveAssetBundleName(abNames[i], true);
            }
        }
        
        public static void SetABNameANDABVarient(Dictionary<string,string> rDic)
        {
            foreach (var rPair in rDic)
            {
                var tmpPath = rPair.Key.Substring(Application.dataPath.Length - 6);
                AssetImporter importer = AssetImporter.GetAtPath(tmpPath);
                importer.assetBundleName = rPair.Value;
                importer.assetBundleVariant = "ab";
            }
        }
    }
}

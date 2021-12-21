using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

public class AssetBundleLoad
{
    public static AssetBundleLoad Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new AssetBundleLoad();
                _instance.LoadManifest("Assets/StreamingAssets/StreamingAssets");
            }

            return _instance;
        }
    }

    private static AssetBundleLoad _instance;

    public delegate void AssetBundleLoadCallBack(AssetBundle ab);

    private Dictionary<string, string[]> GlobalABdps = new Dictionary<string, string[]>();
    private Dictionary<string, AssetBundleObject> LoadedAssetBundle = new Dictionary<string, AssetBundleObject>();
    private Dictionary<string, string> AssetBundlePathDic = new Dictionary<string, string>();

    /// <summary>
    /// 将信息抽象到一个类里面
    /// </summary>
    private class AssetBundleObject : CustomYieldInstruction
    {
        public string HashName; //hash标识符
        public int RefCount; //引用计数
        public List<AssetBundleLoadCallBack> CallFunList = new List<AssetBundleLoadCallBack>(); //回调函数表
        public AssetBundleCreateRequest Request; //异步加载请求
        public AssetBundle AB; //加载到的ab
        public int DependLoadingCount; //正在加载的依赖数量   为0代表其依赖已经加载完毕
        public List<AssetBundleObject> Depends = new List<AssetBundleObject>(); //依赖项
        public override bool keepWaiting {
            get
            {
                if (this.Request.isDone && DependLoadingCount == 0) 
                {
                    return false;
                }
                return true;
            }
        }
        public IEnumerator CoroutineLoad(AssetBundleObject rABObj, UnityAction rCallBack)
        {
            rABObj.Request = AssetBundle.LoadFromFileAsync(GetABPath(rABObj.HashName));
            yield return rABObj;
            rABObj.AB = rABObj.Request.assetBundle;
            AssetBundleLoad.Instance.LoadedAssetBundle.Add(rABObj.HashName, rABObj);
            rCallBack?.Invoke();
        }
    }

    /// <summary>
    /// Load主Manifest  
    /// </summary>
    /// <param name="rPath">Manifest路径</param>
    public void LoadManifest(string rPath)
    {
        GlobalABdps.Clear();
        //Manifest也是一种AB资源
        AssetBundle rAB = AssetBundle.LoadFromFile(rPath);
        AssetBundleManifest rMainfest = rAB.LoadAsset("AssetBundleManifest") as AssetBundleManifest;
        //遍历Manifest中的ABs
        foreach (string rAssetName in rMainfest.GetAllAssetBundles()
        ) //GetAllAssetBundles =>  Get all the AssetBundles in the manifest.
        {
            string rHashName = rAssetName.Replace(".ab", "");
            string[] dps = rMainfest.GetAllDependencies(rAssetName); //返回该AB的依赖AB
            for (int i = 0; i < dps.Length; i++)
            {
                dps[i] = dps[i].Replace(".ab", "");
            }

            GlobalABdps.Add(rHashName, dps);
        }
        rAB.Unload(true);
        rAB = null;
    }

    /// <summary>
    /// 深度递归遍历依赖项 刷新计数
    /// </summary>
    private void DFSDependRef(AssetBundleObject rABObj)
    {
        rABObj.RefCount++;
        foreach (var rParentAB in rABObj.Depends)
        {
            DFSDependRef(rParentAB);
        }
    }

    /// <summary>
    /// 深度递归遍历依赖项 刷新计数
    /// </summary>
    private void DFSDependRefReduce(AssetBundleObject rABObj)
    {
        rABObj.RefCount--;
        foreach (var rParentAB in rABObj.Depends)
        {
            DFSDependRefReduce(rParentAB);
        }

        if (rABObj.RefCount == 0)
        {
            rABObj.AB.Unload(false);
            this.LoadedAssetBundle[rABObj.HashName] = null;
            this.LoadedAssetBundle.Remove(rABObj.HashName);
            rABObj = null;
        }
    }

    /// <summary>
    /// 异步加载资源
    /// </summary>
    /// <param name="rABName">AB包的名字</param>
    /// <returns></returns>
    private AssetBundleObject LoadAssetBundleAsync(string rABName, UnityAction rCallBack = null)
    {
        if (!GlobalABdps.ContainsKey(rABName))
        {
            Debug.Log($"没有 {rABName} 包");
            return null;
        }

        AssetBundleObject rABObj = null;
        //如果存在
        if (LoadedAssetBundle.ContainsKey(rABName))
        {
            rABObj = LoadedAssetBundle[rABName];
            DFSDependRef(rABObj);
            return rABObj;
        }

        //创建并初始化新ABObj
        rABObj = new AssetBundleObject();
        rABObj.HashName = rABName;
        rABObj.RefCount = 1;

        //加载依赖
        var rDps = this.GlobalABdps[rABObj.HashName];
        rABObj.DependLoadingCount = rDps.Length;
        foreach (var rDpName in rDps)
        {
            var dpObj = LoadAssetBundleAsync(rDpName, delegate { rABObj.DependLoadingCount--; });
            rABObj.Depends.Add(dpObj);
        }

        //异步加载AB包
        LoadAssetAsyn(rABObj, rCallBack);
        return rABObj;
    }

    /// <summary>
    /// 异步加载AB包
    /// </summary>
    /// <param name="rABObj">ABObj</param>
    /// <param name="rCallBack">回调函数表</param>
    private void LoadAssetAsyn(AssetBundleObject rABObj, UnityAction rCallBack) //List<UnityAction<object[]>> rCallBack)
    {
        Game.Instance.StartCoroutine(rABObj.CoroutineLoad(rABObj,rCallBack));
    }

    public void LoadAssetAsync(string rABPath, string rSceneName, UnityAction rCallback)
    {
        LoadAssetBundleAsync(rABPath, rCallback);
    }

    /// <summary>
    /// 卸载资源
    /// </summary>
    /// <param name="rABpath">AB包路径</param>
    /// <param name="rCallback">回调</param>
    public void UnLoadAssetAsync(string rABpath, UnityAction rCallback)
    {
        if (this.LoadedAssetBundle.ContainsKey(rABpath))
        {
            DFSDependRefReduce(this.LoadedAssetBundle[rABpath]);
            rCallback?.Invoke();
        }
    }

    //拼接Path
    public static string GetABPath(string rTARGET)
    {
        if (Application.isEditor)
            return "Assets/StreamingAssets/" + rTARGET + ".ab";
        else
            return Application.streamingAssetsPath + "/" + rTARGET + ".ab";
    }
}
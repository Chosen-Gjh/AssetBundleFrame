using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Game : MonoBehaviour
{
    public static Game Instance;

    public Text txt;
    public void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
    }

    public void Start()
    {
    }

    

    public void ChangeScene1()
    {
        AssetBundleLoad.Instance.UnLoadAssetAsync("scene2",null);
        AssetBundleLoad.Instance.LoadAssetAsync("scene1","Scene1", delegate
        {
            SceneManager.LoadScene("Scene1");
        });
    }
    
    public void ChangeScene2()
    {
        txt.text = AssetBundleLoad.GetABPath("scene1");
        AssetBundleLoad.Instance.UnLoadAssetAsync("scene1",null);
        AssetBundleLoad.Instance.LoadAssetAsync("scene2","Scene2", delegate
        {
            SceneManager.LoadScene("Scene2");
        });
        
    }

    public void Back2GameScene()
    {
        AssetBundleLoad.Instance.UnLoadAssetAsync("scene1",null);
        AssetBundleLoad.Instance.UnLoadAssetAsync("scene2",null);
        SceneManager.LoadScene("Game");
    }
    
    
    
    private void CoroutineHandler(IEnumerator rHandler)
    {
        StartCoroutine(rHandler);
    }
    
}

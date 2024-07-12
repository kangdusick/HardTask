using Cysharp.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if UNITY_ANDROID
#endif
#if UNITY_IOS
using UnityEngine.iOS;
using UnityEngine.SocialPlatforms.GameCenter;
using System.Text;
using System.Security.Cryptography;

#endif
public class LoadManager : MonoBehaviour
{
    public static bool isLoadScene;

    private async void Awake()
    {
        isLoadScene = true;

        Application.targetFrameRate = 60;

        if (!TableManager.isLoadDone)
        {
            TableManager.LoadTables();
        }

        await TimeManager.Init();
    }
    public void OnStartBtnClick()
    {
        GameUtil.Instance.LoadScene("Game");
    }

    public void ChangeLanguage(string language)
    {
        GameUtil.language = language;
        GameUtil.Instance.LoadScene("Load");
    }

    private void OnDestroy()
    {
        isLoadScene = false;
    }
}

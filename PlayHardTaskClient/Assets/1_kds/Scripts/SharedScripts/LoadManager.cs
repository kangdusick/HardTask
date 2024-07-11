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
    [SerializeField] Slider _loadingSlider;
    [SerializeField] TMP_Text _loadingText;
    [SerializeField] TMP_Text _versionText;
    [SerializeField] TMP_Text _goToStoreText;
    [SerializeField] TMP_Text _guestLoginText;
    [SerializeField] TMP_Text _socialLoginText;
    [SerializeField] Button _loginBtn;
    [SerializeField] Button _guestLoginBtn;
    [SerializeField] Image loginIconImage;
    [SerializeField] Sprite gpgsIconSprite;
    [SerializeField] Sprite gamecenterIconSprite;


    [SerializeField] GameObject plzUpdateGo;
    [SerializeField] TMP_Text _plzUpdateText;
    ELanguageTable _eloadingText;
    ELanguageTable EloadingText
    {
        get { return _eloadingText; }
        set
        {
            _eloadingText = value;
            Localization();
        }
    }
    public static bool isLoadScene;

   

    private async void Awake()
    {
        Debug.LogWarning($"기기 ID: {SystemInfo.deviceUniqueIdentifier}");

        
        if (!SRDebug.IsInitialized)
        {
            Debug.unityLogger.logEnabled = false;
        }
#if UNITY_ANDROID
        loginIconImage.sprite = gpgsIconSprite;

#else
        loginIconImage.sprite = gamecenterIconSprite;
#endif
        _versionText.text = $"Version {Application.version}";
        _loginBtn.gameObject.SetActive(false);
        _guestLoginBtn.gameObject.SetActive(false);
        isLoadScene = true;

        Application.targetFrameRate = 60;

        _loadingSlider.value = 0f;
        float progressAdder = 0.15f;

        if (!TableManager.isLoadDone)
        {
            _loadingText.text = "InitTable...";
            TableManager.LoadTables();
        }
        _loadingSlider.value += progressAdder;
        Localization();
        _loadingText.text = "InitTable...";

        if (plzUpdateGo.activeSelf)
        {
            return;
        }
        Debug.LogWarning("상용화 배포 시 확인사항");
        Debug.LogWarning("1. obfuscator 활성화하기");
        Debug.LogWarning("2. 어드레서블 빌드하기");
        EloadingText = ELanguageTable.LoadingTime;
        PreLoading();
        await TimeManager.Init();
        _loadingSlider.value += progressAdder;
    }
    private void PreLoading()
    {

    }
    public void OnStartBtnClick()
    {
        GameUtil.Instance.LoadScene("Lobby");
    }


    private void Localization()
    {
        _loadingText.text = EloadingText.LocalIzeText();
        _goToStoreText.text = (ELanguageTable.storeToClick).LocalIzeText();
        _guestLoginText.text = (ELanguageTable.guestLogin).LocalIzeText();
#if UNITY_ANDROID
        _socialLoginText.text = (ELanguageTable.gpgsLogin).LocalIzeText();

#else
         _socialLoginText.text = (ELanguageTable.gameCenter).LocalIzeText();
#endif
    }
    public void ChangeLanguage()
    {
        Localization();
    }

    private void OnDestroy()
    {
        isLoadScene = false;
    }
}

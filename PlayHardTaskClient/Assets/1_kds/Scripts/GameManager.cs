using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static bool isInGame => SceneManager.GetActiveScene().name == "Game";
    public static GameManager Instance;
    [SerializeField] TMP_Text frameText;
    [SerializeField] DOTweenAnimation gameClearTextAnim;
    [SerializeField] DOTweenAnimation toyPartyTextAnim;
    [SerializeField] DOTweenAnimation dimImageAnim;
    private void Awake()
    {
        Application.targetFrameRate = 60;
        Instance = this;
        EnableGameClearText(false);
        EnableToyPartyText(false);
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;
        float screenRate = screenWidth / screenHeight;
        float targetWidth = 1080f;
        float targetHeight = 1920f;
        float targetRate = targetWidth / targetHeight;
        Camera.main.orthographicSize = 820f * targetRate / screenRate;
    }
    private void Update()
    {
        frameText.text = (1f / Time.deltaTime).ToString("F1");
    }
    public void EnableGameClearText(bool isEnable)
    {
        dimImageAnim.gameObject.SetActive(isEnable);
        dimImageAnim.DORestart();
        gameClearTextAnim.gameObject.SetActive(isEnable);
        gameClearTextAnim.DORestart();

    }
    public void EnableToyPartyText(bool isEnable)
    {
        dimImageAnim.gameObject.SetActive(isEnable);
        dimImageAnim.DORestart();
        toyPartyTextAnim.gameObject.SetActive(isEnable);
        toyPartyTextAnim.DORestart();
    }
}

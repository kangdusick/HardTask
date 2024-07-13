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
    public bool IsCanMouseClick => !BallShooter.Instance.isWhileBallShooterRoutine && !BlockSpawnLine.IsWhileBallSpawning;

    private void Awake()
    {
        Application.targetFrameRate = 60;
        Instance = this;
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;
        float screenRate = screenWidth / screenHeight;
        float targetWidth = 1080f;
        float targetHeight = 1920f;
        float targetRate = targetWidth / targetHeight;
        Camera.main.orthographicSize = 820f * targetRate / screenRate;
        PoolableManager.Instance.Instantiate<PopCommon>(EPrefab.PopCommon).OpenPopup(ELanguageTable.changePoint_Title.LocalIzeText(),ELanguageTable.changePoint_Desc.LocalIzeText());
    }
}

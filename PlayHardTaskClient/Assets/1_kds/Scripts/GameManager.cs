using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static bool isInGame => SceneManager.GetActiveScene().name.Equals("Game");
    public static GameManager Instance;
    public bool IsCanMouseClick => !BallShooter.Instance.isWhileBallShooterRoutine && !BlockSpawnLine.IsWhileBallSpawning && !Boss.Instance.isBossTurn && !isWhileMapMoving;
    public bool isWhileMapMoving;
    public Canvas worldCanvas;
    private bool _isGameEnd;
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
        if(targetRate>=screenRate)
        {
            Camera.main.orthographicSize = 820f * targetRate / screenRate;
        }
        else
        {
            Camera.main.orthographicSize = 820f;
        }
        worldCanvas = GameObject.FindGameObjectWithTag(ETag.WorldCanvas.ToString()).GetComponent<Canvas>();
    }
    public void SetView()
    {
        isWhileMapMoving = true;
        BlockEditor.Instance.transform.DOKill();
        int maxIndexY = 0;
        foreach (var item in HexBlockContainer.hexBlockContainerList)
        {
            if (!ReferenceEquals(item.hexBlock, null) && item.y >= maxIndexY)
            {
                maxIndexY = item.y;
            }
        }
        float mapMove = Mathf.Max(0, maxIndexY - 9) * 75f;
        Debug.Log($"{maxIndexY} {mapMove}");
        BlockEditor.Instance.transform.DOMove(Vector3.up * mapMove, 0.3f).OnComplete(() =>
        {
            isWhileMapMoving = false;
        });
    }
    public void OnClickDetailedInformation()
    {
        var descStringBuilderText = new StringBuilder();
        descStringBuilderText.Append(ELanguageTable.anubis.LocalIzeText()+"\n");
        descStringBuilderText.Append($"hp: {Player.Instance.hpDict.FinalValueDescription}\n");
        descStringBuilderText.Append($"attack: {Player.Instance.fairySpawnChanceDict.FinalValueDescription}\n");
        descStringBuilderText.Append($"fairySpawnChance: {Player.Instance.fairyDamageDict.FinalValueDescription}\n");
        descStringBuilderText.Append($"directAttackDamage: {Player.Instance.directAttackDamageDict.FinalValueDescription}\n");
        descStringBuilderText.Append($"neroDirectAttackDamage: {Player.Instance.neroDirectAttackDamageDict.FinalValueDescription}\n");
        descStringBuilderText.Append($"stunDuration: {Player.Instance.stunDurationDict.FinalValueDescription}\n");
        descStringBuilderText.Append($"requireBallForNeroOrb: {Player.Instance.requireBallForNeroOrbDict.FinalValueDescription}\n");
        descStringBuilderText.Append($"smallBombSpawnChance: {Player.Instance.smallBombSpawnChanceDict.FinalValueDescription}\n");

        descStringBuilderText.Append("\n" + ELanguageTable.reaper.LocalIzeText() + "\n");
        descStringBuilderText.Append($"attack: {Boss.Instance.attackDict.FinalValueDescription}\n");
        descStringBuilderText.Append($"hp: {Boss.Instance.hpDict.FinalValueDescription}\n");
        descStringBuilderText.Append($"requireBallCntForStun: {Boss.Instance.requireBallCntForStunDict.FinalValueDescription}\n");
        descStringBuilderText.Append($"attackCooldown: {Boss.Instance.attackCooldownDict.FinalValueDescription}\n");
        descStringBuilderText.Append($"hpRegenWhenHide: {Boss.Instance.hpRegenWhenHideDict.FinalValueDescription}\n");
        PoolableManager.Instance.Instantiate<PopCommon>(EPrefab.PopCommon).OpenPopup(ELanguageTable.DetailInformation.LocalIzeText(), descStringBuilderText.ToString());
    }
    public void GameEnd(ELanguageTable title)
    {
        if(_isGameEnd)
        {
            return;
        }
        _isGameEnd = true;
        PoolableManager.Instance.Instantiate<PopCommon>(EPrefab.PopCommon).OpenPopup(title.LocalIzeText(), ELanguageTable.gameEndDesc.LocalIzeText(), () =>
        {
            GameUtil.Instance.LoadScene("Load");
        });
    }
}

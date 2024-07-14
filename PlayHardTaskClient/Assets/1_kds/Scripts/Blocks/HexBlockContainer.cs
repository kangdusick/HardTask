using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
public class HexBlockContainer : MonoBehaviour
{
    public static HexBlockContainer[,] hexBlockContainerMatrix;
    public static List<HexBlockContainer> hexBlockContainerList = new();
    public static List<HexBlock> attatchmentHexBlockList = new();
    public static readonly List<EColor> EColorList = new List<EColor>() { EColor.blue, EColor.red, EColor.yellow };
    public readonly static (int x, int y)[] dirList = new (int x, int y)[6] { (2, 0), (-2, 0), (1, 1), (-1, -1), (1, -1), (-1, 1) };//기준점과 바로 인접한 블럭
    public readonly static (int x, int y)[] dirList_Range2 = new (int x, int y)[12] { (-4,0),(-3,-1), (-2,-2),(0,-2),(2,-2),(3,-1),(4,0),(3,1),(2,2),(0,2),(-2,2),(-3,1)};//기준점에서 한칸 건너띄어 인접한 블럭
    public HexBlock hexBlock;
    [SerializeField] Image _hintEffectImage;
    [SerializeField] DOTweenAnimation _hintEffectAnim;
    public int x;
    public int y;
    public const float hexWidth = 88.28125f;
    public const float hexHeight = 100f;
    public void EnableHintEffect(bool isEnableHintEffect)
    {
        if(isEnableHintEffect && _hintEffectImage.enabled == false)
        {
            _hintEffectImage.enabled = true;
            _hintEffectAnim.DOPlay();
        }
        if(!isEnableHintEffect && _hintEffectImage.enabled == true)
        {
            _hintEffectImage.enabled = false;
            _hintEffectAnim.DOPause();
        }
    }
    public static void InitHexBlockContainerMatrix(int width, int height)
    {
        hexBlockContainerMatrix = new HexBlockContainer[width, height];
        hexBlockContainerList.Clear();
        attatchmentHexBlockList.Clear();
        var hexBlockContainerLGoist = GameObject.FindGameObjectsWithTag(ETag.HexBlockContainer.ToString());
        foreach (var hexBlockContainerGo in hexBlockContainerLGoist)
        {
            if (hexBlockContainerGo.activeInHierarchy)
            {
                var hexBlockContainer = hexBlockContainerGo.GetComponent<HexBlockContainer>();
                hexBlockContainerMatrix[hexBlockContainer.x, hexBlockContainer.y] = hexBlockContainer;
                hexBlockContainerList.Add(hexBlockContainer);
            }
        }
    }

    private void Start()
    {
        Destroy(transform.Find(KeyPlayerPrefs.IndexForDebugText).gameObject);
        EnableHintEffect(false);
        ReplacePresetToPoolingObject();
    }
    private void ReplacePresetToPoolingObject()
    {
        if(!ReferenceEquals(hexBlock,null))
        {
            var hexBlockPresetColor = hexBlock.eColor;
            var hexBlockPresetType = hexBlock.eBlockType;
            Debug.Log($"{x},{y}");
            Destroy(hexBlock.gameObject); //프리셋으로 저장했던거 파괴 후 오브젝트 풀링으로 재생성
            PoolableManager.Instance.InstantiateAsync<HexBlock>(EPrefab.HexBlock, transform.position).ContinueWithNullCheck(x =>
            {
                x.Init(hexBlockPresetColor, hexBlockPresetType);
                x.SetHexBlockContainerWithMove(this, 1000f);
            });
        }
    }
    public List<HexBlockContainer> GetNeighborContainerBlockList(int neighborRange = 1)
    {
        var neighborList = new List<HexBlockContainer>();
        for (int i = 1; i <= neighborRange; i++)
        {
            var targetDir = i == 1? dirList : dirList_Range2;
            foreach (var dir in targetDir)
            {
                int neighborIndexX = x + dir.x ;
                int neighborIndexY = y + dir.y ;
                int matrixWidth = hexBlockContainerMatrix.GetLength(0);
                int matrixHeight = hexBlockContainerMatrix.GetLength(1);

                HexBlockContainer neighborHexBlockContainer = null;

                // 인덱스가 유효한지 검사
                if (neighborIndexX >= 0 && neighborIndexX < matrixWidth && neighborIndexY >= 0 && neighborIndexY < matrixHeight)
                {
                    neighborHexBlockContainer = hexBlockContainerMatrix[neighborIndexX, neighborIndexY];
                }

                if (ReferenceEquals(neighborHexBlockContainer, null))
                {
                    continue;
                }
                neighborList.Add(neighborHexBlockContainer);
            }
        }
        return neighborList;
    }
#if UNITY_EDITOR
    private void OnValidate()
    {
        EditorInit(x, y);
    }
    public void EditorInit(int x, int y)
    {
        this.x = x;
        this.y = y;
        transform.position = new Vector3(x * hexWidth * 0.5f, -y * hexHeight * 0.75f, 0f);
        transform.Find(KeyPlayerPrefs.IndexForDebugText).gameObject.GetComponent<TMP_Text>().text = $"{x},{y}";
    }

    [Button]
    private void MakeHexBlockInEditor(EColor eColor, EBlockType eBlockType)
    {
        if (hexBlock != null)
        {
            Undo.DestroyObjectImmediate(hexBlock.gameObject);
        }

        // Addressables를 사용하여 프리팹 로드
        GameObject blockPrefab = Addressables.LoadAssetAsync<GameObject>(EPrefab.HexBlock.OriginName()).WaitForCompletion();

        // 프리팹 인스턴스화
        GameObject blockObj = (GameObject)PrefabUtility.InstantiatePrefab(blockPrefab, GameObject.FindGameObjectWithTag(ETag.BlockEditor.ToString()).transform);

        // HexBlock 컴포넌트 가져오기
        hexBlock = blockObj.GetComponent<HexBlock>();
        if (hexBlock != null)
        {
            hexBlock.transform.position = transform.position;
            hexBlock.Init(eColor, eBlockType);
            hexBlock.SetHexBlockContainerWithMove(this, 1000f);
            Undo.RegisterCreatedObjectUndo(hexBlock.gameObject, "Create HexBlock");
        }
    }
#endif

}

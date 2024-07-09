using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sirenix.OdinInspector;
using SRF;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Mathematics;
using Unity.VisualScripting;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

public class CombinationGenerator<T>
{
    List<T> list;
    int length;
    public CombinationGenerator(List<T> list, int length)
    {
        this.list = list;
        this.length = length;
    }
    public IEnumerable<List<T>> GetCombinations()
    {
        return GetCombinations(0, new List<T>());
    }
    private IEnumerable<List<T>> GetCombinations(int startIndex, List<T> combination)
    {
        if (combination.Count == length)
        {
            yield return new List<T>(combination);
            yield break;
        }
        for (int i = startIndex; i < list.Count; i++)
        {
            combination.Add(list[i]);
            foreach (var item in GetCombinations(i + 1, combination))
            {
                yield return combination;
            }
            combination.RemoveAt(combination.Count - 1);
        }

    }
}
class UnionFind
{
    int[] parent;
    int[] rank;
    int[] size;

    public UnionFind(int n)
    {
        parent = new int[n];
        rank = new int[n];
        size = new int[n];
        for (int i = 0; i < n; i++)
        {
            parent[i] = i;
            size[i] = 1;
        }
    }

    public int Find(int x)
    {
        if (parent[x] != x)
        {
            parent[x] = Find(parent[x]);
        }
        return parent[x];
    }

    public void Union(int x, int y)
    {
        int rootX = Find(x);
        int rootY = Find(y);

        if (rootX == rootY) return;

        if (rank[rootX] < rank[rootY])
        {
            parent[rootX] = rootY;
            size[rootY] += size[rootX];
        }
        else if (rank[rootX] > rank[rootY])
        {
            parent[rootY] = rootX;
            size[rootX] += size[rootY];
        }
        else
        {
            parent[rootY] = rootX;
            rank[rootX]++;
            size[rootX] += size[rootY];
        }
    }

    public List<int> GetUnionList(int x)
    {
        var list = new List<int>();
        var rootx = Find(x);
        for (int i = 0; i < parent.Length; i++)
        {
            if (Find(parent[i]) == rootx)
            {
                list.Add(i);
            }
        }
        return list;
    }
    public int GetSize(int x)
    {
        int rootx = Find(x);
        return size[rootx];
    }
}
public class HexBlockLine
{
    public int a; //y = ax + b
    public int b;
    public List<HexBlock> hexBlockList = new();
    public HexBlockLine(int a, int b)
    {
        this.a = a;
        this.b = b;
    }
}
public enum EDirIndex
{
    Down = 0,
    Up = 1,
    RightDown = 2,
    LeftUp = 3,
    RightUp = 4,
    LeftDown = 5,
}
public class HexBlockContainer : CustomColliderMonobehaviour
{
    public static HexBlockContainer[,] hexBlockContainerMatrix;
    private static List<HexBlockContainer> _highestHexBlockContainerEachColumnList;
    public static readonly List<EColor> EColorList = new List<EColor>() { EColor.blue, EColor.red, EColor.yellow };
    public readonly static (int x, int y)[] dirList = new (int x, int y)[6] { (2, 0), (-2, 0), (1, 1), (-1, -1), (1, -1), (-1, 1) };
    public HexBlock hexBlock;
    [SerializeField] Image _hintEffectImage;
    [SerializeField] DOTweenAnimation _hintEffectAnim;
    public int x;
    public int y;
    public const float hexWidth = 88.28125f;
    public const float hexHeight = 100f;
    const float newBlockMoveSpeed = 600f;
    public static bool IsAllBlockGenerated
    {
        get 
        {
            foreach (var item in CollisionDetectManager.Instance.hexBlockContainerList)
            {
                if(ReferenceEquals(item.hexBlock,null))
                {
                    return false;
                }
            }
            return true;
        }
    }
    public void EnableHintEffect(bool isEnableHintEffect)
    {
        if(isEnableHintEffect)
        {
            _hintEffectImage.enabled = true;
            _hintEffectAnim.DORestart();
        }
        else
        {
            _hintEffectImage.enabled = false;
            _hintEffectAnim.DOPause();
        }
    }
    
    public static HexBlockContainer GetTopEmptyHexBlockContainerInSameColum(int xIndex)
    {
        HexBlockContainer topContainer = null;
        bool isEmptyContainerExist = false;
        for (int j = hexBlockContainerMatrix.GetLength(1) - 1; j >= 0; j--)
        {
            if (!ReferenceEquals(hexBlockContainerMatrix[xIndex, j], null))
            {
                topContainer = hexBlockContainerMatrix[xIndex, j];
                break;
            }
        }
        for (int j = hexBlockContainerMatrix.GetLength(1) - 1; j >= 0; j--)
        {
            if (!ReferenceEquals(hexBlockContainerMatrix[xIndex, j], null) && topContainer.y >= hexBlockContainerMatrix[xIndex, j].y && ReferenceEquals(hexBlockContainerMatrix[xIndex, j].hexBlock, null))
            {
                topContainer = hexBlockContainerMatrix[xIndex, j];
                isEmptyContainerExist = true;
                break;
            }
        }
        if (!isEmptyContainerExist)
        {
            topContainer = null;
        }
        return topContainer;
    }
    public static HexBlockContainer GetEmptyUpperHexBlockContainerInSameColum(HexBlockContainer emptyBlockContainer) //빈 블럭 위에 떠있는 가장 가까운 블럭 찾기
    {
        HexBlockContainer upperEmptyBlock = null;
        var xIndex = emptyBlockContainer.x;
        for (int j = emptyBlockContainer.y - 1; j >= 0; j--)
        {
            if (!ReferenceEquals(hexBlockContainerMatrix[xIndex, j], null) && !ReferenceEquals(hexBlockContainerMatrix[xIndex, j].hexBlock, null))
            {
                upperEmptyBlock = hexBlockContainerMatrix[xIndex, j];
                break;
            }
        }
        return upperEmptyBlock;
    }
    public static void InitHexBlockContainerMatrix(int width, int height)
    {
        hexBlockContainerMatrix = new HexBlockContainer[width, height];
        _highestHexBlockContainerEachColumnList = new();
        var hexBlockContainerList = GameObject.FindGameObjectsWithTag(ETag.HexBlockContainer.ToString());
        foreach (var hexBlockContainerGo in hexBlockContainerList)
        {
            if (hexBlockContainerGo.activeInHierarchy)
            {
                var hexBlockContainer = hexBlockContainerGo.GetComponent<HexBlockContainer>();
                hexBlockContainerMatrix[hexBlockContainer.x, hexBlockContainer.y] = hexBlockContainer;

                if (!_highestHexBlockContainerEachColumnList.Any(hsdf => hsdf.x == hexBlockContainer.x))
                {
                    _highestHexBlockContainerEachColumnList.Add(hexBlockContainer);
                }
                else
                {
                    var highestContainer = _highestHexBlockContainerEachColumnList.Find(h => h.x == hexBlockContainer.x);
                    var highestContainerIndex = _highestHexBlockContainerEachColumnList.IndexOf(highestContainer);
                    if (highestContainer.y > hexBlockContainer.y)//y좌표가 작을수록 높이 있는 블럭이다.
                    {
                        _highestHexBlockContainerEachColumnList[highestContainerIndex] = hexBlockContainer;
                    }
                }
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
            Destroy(hexBlock.gameObject); //프리셋으로 저장했던거 파괴 후 오브젝트 풀링으로 재생성
            PoolableManager.Instance.InstantiateAsync<HexBlock>(EPrefab.HexBlock, transform.position, Vector3.one, parentTransform: BlockEditor.Instance.transform).ContinueWithNullCheck(x =>
            {
                x.Init(hexBlockPresetColor, hexBlockPresetType);
                x.SetHexBlockContainerWithMove(this, 1000f);
            });
        }
    }



    private async UniTask SortBlocksAndGenerateNewBlocks()
    {
        List<int> GetExistEmptyColumnList()
        {
            List<int> existEmptyColumnList = new List<int>();
            for (int i = 0; i < BlockEditor.Instance.width; i++)
            {
                var emptyTop = GetTopEmptyHexBlockContainerInSameColum(i);
                if (ReferenceEquals(emptyTop, null)) //빈공간이 없다
                {
                    continue;
                }
                else
                {
                    existEmptyColumnList.Add(i);
                }
            }
            return existEmptyColumnList;
        }
        List<int> existEmptyColumnList = GetExistEmptyColumnList();

        //기존 블럭이 동시에 흘러 내려가기
        //x=0, y =  11부터 시작해서 만약 아래칸이 비어있다면, 비어있지 않은 칸을 찾을때까지 수직아아래로 내려가기.
        var moveTaskeList = new List<UniTask>();
        foreach (var emptyXIndex in existEmptyColumnList)
        {
            while (true)
            {
                var emptyTop = GetTopEmptyHexBlockContainerInSameColum(emptyXIndex);
                var nextBlock = GetEmptyUpperHexBlockContainerInSameColum(emptyTop);
                if (ReferenceEquals(nextBlock, null))
                {
                    break;
                }
                moveTaskeList.Add(nextBlock.hexBlock.SetHexBlockContainerWithMove(emptyTop, 0.2f, isTimeBase: true));
            }
        }
        await UniTask.WhenAll(moveTaskeList);
        moveTaskeList.Clear();
        //입구에서 흘러나오는 블럭에 방해되는 블럭들이 있다면, 해당 방향으로 떨어진다.


        await UniTask.WhenAll(moveTaskeList);
    }
   
    private static List<HexBlockContainer> GetNeighborContainerBlockList(HexBlockContainer hexBlockContainer)
    {
        var neighborList = new List<HexBlockContainer>();
        foreach (var dir in dirList)
        {
            int neighborIndexX = hexBlockContainer.x + dir.x;
            int neighborIndexY = hexBlockContainer.y + dir.y;
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
        return neighborList;
    }
    public static List<HexBlockContainer> GetBombAreaContainerList(HexBlockContainer hexBlockContainer, bool isLargeArea)
    {
        var TNTAreaList = new List<HexBlockContainer>();
        var tntDirList = new List<(int x, int y)>() { (0, 2), (0, -2), (1, 1), (-1, -1), (1, -1), (-1, 1) , (2,-2),(2,0),(2,2) , (-2, -2), (-2, 0), (-2, 2) };
        var tntLargeDirList = new List<(int x, int y)>() { (-2, -4), (-1, -3), (0, -4), (1, -3), (2, -4), (2, 4), (1, 3), (0, 4), (-1, 3), (-2, 4) };

        var dirList = tntDirList;
        if(isLargeArea)
        {
            dirList.AddRange(tntLargeDirList);
        }
        foreach (var dir in dirList)
        {
            int neighborIndexX = hexBlockContainer.x + dir.x;
            int neighborIndexY = hexBlockContainer.y + dir.y;
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
            TNTAreaList.Add(neighborHexBlockContainer);
        }
        return TNTAreaList;
    }
   
    private static void Union(HexBlockContainer hexBlockContainer, UnionFind unionFind)
    {
        bool IsCanUnion(HexBlock standardContainer, HexBlock neighborhexBlock)
        {
            if (standardContainer.eColor == EColor.none || neighborhexBlock.eColor == EColor.none )
            {
                return false;
            }
            if (standardContainer.eColor == neighborhexBlock.eColor )
            {
                return true;
            }
            return false;
        }
        var neighborList = GetNeighborContainerBlockList(hexBlockContainer);
        var standardHexBlockContainerIndex = CollisionDetectManager.Instance.hexBlockContainerList.IndexOf(hexBlockContainer);
        foreach (var neighborHexBlockContainer in neighborList)
        {
            var neighborHexBlockContainerIndex = CollisionDetectManager.Instance.hexBlockContainerList.IndexOf(neighborHexBlockContainer);

            if ((unionFind.Find(standardHexBlockContainerIndex) != unionFind.Find(neighborHexBlockContainerIndex)) && IsCanUnion(hexBlockContainer.hexBlock, neighborHexBlockContainer.hexBlock) )
            {
                unionFind.Union(standardHexBlockContainerIndex, neighborHexBlockContainerIndex);
                Union(neighborHexBlockContainer, unionFind);
            }
        }
    }
    private static bool IsNeighbor(HexBlock hexBlockA, HexBlock hexBlockB)
    {
        if ((Math.Abs(hexBlockA.x - hexBlockB.x) + Math.Abs(hexBlockA.y - hexBlockB.y)) == 2 && Math.Abs(hexBlockA.y - hexBlockB.y) != 2)
        {
            return true;
        }
        return false;
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

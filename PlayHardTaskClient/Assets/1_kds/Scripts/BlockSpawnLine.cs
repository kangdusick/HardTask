using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using SRF;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
[RequireComponent(typeof(HexBlockContainer))]
public class BlockSpawnLine : MonoBehaviour
{
    [SerializeField] private List<Vector2> spawnLineList = new();
    public List<(int x, int y)> spawnLineIndexList = new();
    private List<HexBlockContainer> _spawnLineHexBlockContainerList = new();
    private HexBlockContainer _spawnPointHexBlockContainer;
    private bool _isSetSpawnLineDone;
    const float newBlockMoveSpeed = 600f;
    private void Awake()
    {
        _spawnPointHexBlockContainer = GetComponent<HexBlockContainer>();
        SetSpawnLineIndexList();
    }
    private async void Start()
    {
        await UniTask.DelayFrame(1);
        PushAndSpawnBlocksInLine();
    }
    private void SetSpawnLineIndexList()
    {
        if(_isSetSpawnLineDone)
        {
            return;
        }
        _isSetSpawnLineDone = true;
        spawnLineIndexList.Clear();
        foreach (var item in spawnLineList)
        {
            int x = Mathf.RoundToInt(item.x);
            int y = Mathf.RoundToInt(item.y);
            spawnLineIndexList.Add((x, y));
            if(Application.isPlaying)
            {
                _spawnLineHexBlockContainerList.Add(HexBlockContainer.hexBlockContainerMatrix[x, y]);
            }
        }
    }
    private async UniTask PushAndSpawnBlocksInLine()
    {
        List<UniTask> moveTaskList= new List<UniTask>();
        while (true)  //스폰 라인에 빈 공간이 있으면 한칸씩 이동 후 구슬 하나 생성
        {
            moveTaskList.Clear();
            int headIndex = FindHeadIndexFromSpawnPointInLine();
            if(headIndex == _spawnLineHexBlockContainerList.Count -1)
            {
                break; //라인이 꽉 차있다.
            }
            Debug.Log(headIndex);

            for (int i = headIndex; i >= 0; i--)
            {
                moveTaskList.Add(_spawnLineHexBlockContainerList[i].hexBlock.SetHexBlockContainerWithMove(_spawnLineHexBlockContainerList[i + 1], newBlockMoveSpeed));
            }


            var spawnedBlock = PoolableManager.Instance.Instantiate<HexBlock>(EPrefab.HexBlock, _spawnPointHexBlockContainer.transform.position);
            spawnedBlock.Init(HexBlockContainer.EColorList.Random(), EBlockType.normal);
            moveTaskList.Add(spawnedBlock.SetHexBlockContainerWithMove(_spawnLineHexBlockContainerList[0], newBlockMoveSpeed));
            await UniTask.WhenAll(moveTaskList);
        }
    }
    private int FindHeadIndexFromSpawnPointInLine() //index가 음수면 스폰 포인트와 연결된 구슬이 없다는 뜻
    {
        for(int i = -1; i<_spawnLineHexBlockContainerList.Count-1;i++)
        {
            if (ReferenceEquals(_spawnLineHexBlockContainerList[i+1].hexBlock,null))
            {
                return i;
            }
        }
        return _spawnLineHexBlockContainerList.Count-1;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // 에디터에서 OnValidate가 호출될 때 기즈모를 그리도록 요청
        _isSetSpawnLineDone = false;
        SceneView.RepaintAll();
    }

    private void OnDrawGizmos()
    {
        // 기즈모 색상 설정
        Gizmos.color = Color.red;
        HexBlockContainer[,] hexBlockContainerGrid = new HexBlockContainer[30, 100];
        foreach (var item in GameObject.FindGameObjectsWithTag(ETag.HexBlockContainer.ToString()))
        {
            var hexBlockContainer = item.GetComponent<HexBlockContainer>();
            hexBlockContainerGrid[hexBlockContainer.x, hexBlockContainer.y] = hexBlockContainer;
        }
        SetSpawnLineIndexList();
        // 스폰 라인 리스트에 있는 점들을 순차적으로 그리기
        for (int i = 0; i < spawnLineList.Count - 1; i++)
        {
            var start = hexBlockContainerGrid[spawnLineIndexList[i].x, spawnLineIndexList[i].y].transform.position;
            var end = hexBlockContainerGrid[spawnLineIndexList[i+1].x, spawnLineIndexList[i+1].y].transform.position;

            // 2D 좌표를 3D로 변환하여 기즈모 그리기
            Gizmos.DrawLine(new Vector3(start.x, start.y, 0), new Vector3(end.x, end.y, 0));
        }
    }
#endif
}

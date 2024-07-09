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
    private HexBlockContainer _hexBlockContainer;
    private void Awake()
    {
        _hexBlockContainer = GetComponent<HexBlockContainer>();
        SetSpawnLineIndexList();
    }
    private void Start()
    {
        SpawnBlocksInLine(16);
    }
    private void SetSpawnLineIndexList()
    {
        spawnLineIndexList.Clear();
        foreach (var item in spawnLineList)
        {
            spawnLineIndexList.Add((Mathf.RoundToInt(item.x), Mathf.RoundToInt(item.y)));
        }
    }
    private void SpawnBlocksInLine(int blockCnt) //양옆 16개씩 스폰
    {
        foreach (var item in spawnLineIndexList)
        {
            var hexBlock = PoolableManager.Instance.Instantiate<HexBlock>(EPrefab.HexBlock);
            hexBlock.Init(HexBlockContainer.EColorList.Random(), EBlockType.normal);
            hexBlock.SetHexBlockContainerWithMove(HexBlockContainer.hexBlockContainerMatrix[item.x, item.y], 1000f);
        }
    }
#if UNITY_EDITOR
    private void OnValidate()
    {
        // 에디터에서 OnValidate가 호출될 때 기즈모를 그리도록 요청
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

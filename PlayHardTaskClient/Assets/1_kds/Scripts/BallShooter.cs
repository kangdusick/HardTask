using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sirenix.OdinInspector;
using SRF;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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

public class BallShooter : MonoBehaviour
{
    public static BallShooter Instance;
    [SerializeField] private RectTransform _shootingStartPoint;
    [SerializeField] private RectTransform ballDestroierRect;
    [SerializeField] private LineRenderer _shootingLineRenderer;
    private HexBlockContainer _destineHexBlockContainer = null;
    private List<Vector3> shootingBallMovingRoute = new();
    private const float shootingBallSpeed = 1200f;
    public bool isWhileBallShooterRoutine;
    [HideInInspector] public List<HexBlock> prepareBallList = new();
    static readonly float[] angleList = { 90f, -30f, -150f, -270f };
    private List<HexBlockContainer> _enabledHintBlockList = new(); 
    private void Awake()
    {
        Instance = this;
        isWhileBallShooterRoutine = false;
        _shootingLineRenderer.gameObject.SetActive(false);
        TouchManager.Instance.OnTouchIng += SetDestineHexBlockContainer;
        TouchManager.Instance.OnTouchUp += ShootingBall;
        PrefareBall();
    }
    public async void FillNeroOrbDirectly()
    {
        isWhileBallShooterRoutine = true;
        var secrifiedBall = prepareBallList[0];
        secrifiedBall.transform.DOMove(NeroOrbContainer.Instance.transform.position,0.25f);
        await UniTask.Delay(250);
        prepareBallList.Remove(secrifiedBall);
        PoolableManager.Instance.Destroy(secrifiedBall.gameObject);
        NeroOrbContainer.Instance.RemainNeroOrbCount -= 10;
        isWhileBallShooterRoutine = false;
        PrefareBall();
        Player.Instance.TurnEnd();
    }
    public void PrefareBall(bool isNeroOrb = false)
    {
        while (prepareBallList.Count<2)
        {
            HexBlock newBall = null;
            if (isNeroOrb)
            {
                newBall = PoolableManager.Instance.Instantiate<HexBlock>(EPrefab.NeroOrb, _shootingStartPoint.position);
                newBall.Init(EColor.none, EBlockType.bomb_Range2_neroOrb);
            }
            else
            {
                newBall = PoolableManager.Instance.Instantiate<HexBlock>(EPrefab.HexBlock, _shootingStartPoint.position);
                newBall.Init(HexBlockContainer.EColorList.Random(), EBlockType.normal);
            }
            newBall.transform.localScale = Vector3.zero;
            newBall.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
            prepareBallList.Insert(0, newBall);
            RotateBall();
        }
    }
    public void RotateBall()
    {
        // 새로운 리스트 생성하여 회전된 공들의 순서를 저장
        List<HexBlock> rotatedList = new List<HexBlock>(prepareBallList.Count);

        // 새로운 리스트에 회전된 순서로 공들을 추가
        for (int i = 0; i < prepareBallList.Count; i++)
        {
            rotatedList.Add(null);
        }

        for (int i = 0; i < prepareBallList.Count; i++)
        {
            if (i == prepareBallList.Count - 1)
            {
                prepareBallList[i].RotateAroundCircle(angleList[i], angleList[3]);
                rotatedList[0] = prepareBallList[i];
            }
            else
            {
                prepareBallList[i].RotateAroundCircle(angleList[i], angleList[i + 1]);
                rotatedList[i + 1] = prepareBallList[i];
            }
        }

        // 새로운 순서로 원래 리스트를 업데이트
        for (int i = 0; i < prepareBallList.Count; i++)
        {
            prepareBallList[i] = rotatedList[i];
        }
    }
    void SetDestineHexBlockContainer(Vector2 screenPos) //마우스 클릭 중에 조준선이 활성화되며 구슬이 어디에 도착할지 표시해준다.
    {
        if (!GameManager.Instance.IsCanMouseClick)
        {
            return;
        }

        shootingBallMovingRoute.Clear();
        shootingBallMovingRoute.Add(_shootingStartPoint.position);
        EnableDestinePositionHint(false);

        var laserDirection = TouchManager.Instance.mouseWorldPos - _shootingStartPoint.transform.position;
        laserDirection.z = 0f;

        var hit = GetNearestRaycastHit(_shootingStartPoint.transform.position, laserDirection, new List<ELayers>() { ELayers.Wall, ELayers.HexBlockContainer });
        if (!ReferenceEquals(hit.collider, null))
        {
            shootingBallMovingRoute.Add(hit.point);
            if (hit.collider.gameObject.layer == (int)ELayers.Wall) //벽과 충돌한 경우 한번 반사된다.
            {
                // 반사된 레이저 발사
                Vector3 hitPoint = hit.point;
                Vector3 normal = hit.normal;
                Vector3 incomingVec = laserDirection;
                Vector3 reflectVec = Vector3.Reflect(incomingVec, normal);

                var hitByReflect = GetNearestRaycastHit(hitPoint, reflectVec, new List<ELayers>() { ELayers.HexBlockContainer });

                if (!ReferenceEquals(hitByReflect.collider, null))
                {
                    shootingBallMovingRoute.Add(hitByReflect.point);
                    _destineHexBlockContainer = GetBallDestineContainer(hitByReflect);
                }
            }
            else //구슬이 있는 블럭인 경우 해당 구슬과 인접한 가장 가까운 빈 구역을 표시해준다.
            {
                _destineHexBlockContainer = GetBallDestineContainer(hit);
            }
        }

        if (!ReferenceEquals(_destineHexBlockContainer, null))
        {
            Player.Instance.ChangeIdleAnim(EGangAnimation.GangDungeon_AnubisCasting);
            shootingBallMovingRoute.Add(_destineHexBlockContainer.transform.position);
            EnableDestinePositionHint(true);
        }
        else
        {
            Player.Instance.ChangeIdleAnim(EGangAnimation.GangDungeon_AnubisIdle);
        }
    }
    private void EnableDestinePositionHint(bool isEnable)
    {
        if (!ReferenceEquals(_destineHexBlockContainer, null))
        {
            if (isEnable)
            {
                _destineHexBlockContainer.EnableHintEffect(true);
                _enabledHintBlockList.Add(_destineHexBlockContainer);

                if (prepareBallList[0].eBlockType == EBlockType.bomb_Range2_neroOrb)
                {
                    var neroOrbRange = _destineHexBlockContainer.GetNeighborContainerBlockList(2);
                    foreach (var item in neroOrbRange)
                    {
                        item.EnableHintEffect(true);
                        _enabledHintBlockList.Add(item);
                    }
                   
                }

                _shootingLineRenderer.gameObject.SetActive(true);
                _shootingLineRenderer.positionCount = shootingBallMovingRoute.Count - 1;
                _shootingLineRenderer.SetPositions(shootingBallMovingRoute.GetRange(0, shootingBallMovingRoute.Count - 1).ToArray());

            }
            else
            {
                _shootingLineRenderer.gameObject.SetActive(false);
                foreach (var item in _enabledHintBlockList)
                {
                    item.EnableHintEffect(false);
                }
                _enabledHintBlockList.Clear();
                _destineHexBlockContainer = null;

            }
        }
    }
    private async void ShootingBall(Vector2 screenPos)
    {
        if (!GameManager.Instance.IsCanMouseClick)
        {
            return;
        }
        if (!ReferenceEquals(_destineHexBlockContainer, null))
        {
            var shootingBall = prepareBallList[0];
            if(shootingBall.eBlockType == EBlockType.bomb_Range2_neroOrb)
            {
                NeroOrbContainer.Instance.EnableNeroOrbContainer(true);
            }
            Player.Instance.currentIdleAnim = EGangAnimation.GangDungeon_AnubisIdle.OriginName();
            Player.Instance.SetAnim(EGangAnimation.GangDungeon_AnubisCastDone);
            prepareBallList.Remove(shootingBall);
            isWhileBallShooterRoutine = true;
            _shootingLineRenderer.gameObject.SetActive(false);
            var bossBlock = IsBossAttackDirectly();
            if (bossBlock)
            {
                await shootingBall.SetHexBlockContainerWithMove(_destineHexBlockContainer, shootingBallSpeed, shootingBallMovingRoute);
                await shootingBall.DirectDamageToBoss(bossBlock);
            }
            else
            {
                await shootingBall.SetHexBlockContainerWithMove(_destineHexBlockContainer, shootingBallSpeed, shootingBallMovingRoute);
                await FindMatchAndDestroyBalls(shootingBall);
            }
            EnableDestinePositionHint(false);
            PrefareBall();
            isWhileBallShooterRoutine = false;
            Player.Instance.TurnEnd();
        }
    }
    private HexBlock IsBossAttackDirectly()
    {
        foreach (var item in _destineHexBlockContainer.GetNeighborContainerBlockList())
        {
            if (!ReferenceEquals(item.hexBlock, null) && item.hexBlock.eBlockType == EBlockType.boss)
            {
                return item.hexBlock;
            }
        }
        return null;
    }
    private async UniTask FindMatchAndDestroyBalls(HexBlock shootedBall)
    {
        UnionFind unionFindSameColor = new UnionFind(HexBlockContainer.hexBlockContainerList.Count + 1);
        HexBlockContainer shootedBallContainer = shootedBall.hexBlockContainer;
        //shootedBall과 같은 색상의 볼이 3개 이상이면 파괴
        UnionSameColorHexBlocks(shootedBallContainer, unionFindSameColor);
        var unionedHexBlockList = GetUnionedHexBlockList(shootedBall, unionFindSameColor);
        if (unionedHexBlockList.Count >= 3)
        {
            foreach (var item in unionedHexBlockList)
            {
                item.Damaged();
            }
        }

        var checkBombList = shootedBallContainer.GetNeighborContainerBlockList(1); //발사된 곳 주변에 폭탄블럭이 있다면 연쇄폭파
        checkBombList.Add(shootedBallContainer);
        foreach (var item in checkBombList)
        {
            if(!ReferenceEquals(item.hexBlock,null) && item.hexBlock.eBlockType.ToString().Contains("bomb"))
            {
                item.hexBlock.Damaged();
            }
        }
        //attatch포인트와 연결되어있지 않은 그룹 모두 낙하파괴
        List<HexBlock> destroyWithFall = new();
        UnionFind unionFindAttatchmentPoint = new UnionFind(HexBlockContainer.hexBlockContainerList.Count + 1);
        foreach (var item in HexBlockContainer.attatchmentHexBlockList)
        {
            UnionAroundAttatchmentPoint(item.hexBlockContainer, unionFindAttatchmentPoint);
        }
        for (int i = 0; i < HexBlockContainer.hexBlockContainerList.Count; i++)
        {
            HexBlock hexBlock = HexBlockContainer.hexBlockContainerList[i].hexBlock;
            if (unionFindAttatchmentPoint.GetSize(i) <= 1 && !ReferenceEquals(hexBlock, null) && !hexBlock.IsCantDestroyAndMove)
            {
                destroyWithFall.Add(hexBlock);
            }
        }

        await BallDestroyer.Instance.DestroyWithBallDestroyer(destroyWithFall);
    }
    private List<HexBlock> GetUnionedHexBlockList(HexBlock standardHexBlock, UnionFind unionFind)
    {
        List<HexBlock> unionedHexBlockList = new();
        var unionedIndexList = unionFind.GetUnionList(HexBlockContainer.hexBlockContainerList.IndexOf(standardHexBlock.hexBlockContainer));
        foreach (var item in unionedIndexList)
        {
            unionedHexBlockList.Add(HexBlockContainer.hexBlockContainerList[item].hexBlock);
        }
        return unionedHexBlockList;
    }
    private void UnionAroundAttatchmentPoint(HexBlockContainer hexBlockContainer, UnionFind unionFind)
    {
        var neighborList = hexBlockContainer.GetNeighborContainerBlockList();
        var standardHexBlockContainerIndex = HexBlockContainer.hexBlockContainerList.IndexOf(hexBlockContainer);
        foreach (var neighborHexBlockContainer in neighborList)
        {
            if (ReferenceEquals(neighborHexBlockContainer.hexBlock, null))
            {
                continue;
            }
            var neighborHexBlockContainerIndex = HexBlockContainer.hexBlockContainerList.IndexOf(neighborHexBlockContainer);
            if ((unionFind.Find(standardHexBlockContainerIndex) != unionFind.Find(neighborHexBlockContainerIndex)))
            {
                unionFind.Union(standardHexBlockContainerIndex, neighborHexBlockContainerIndex);
                UnionAroundAttatchmentPoint(neighborHexBlockContainer, unionFind);
            }
        }
    }
    private void UnionSameColorHexBlocks(HexBlockContainer hexBlockContainer, UnionFind unionFind)
    {
        bool IsCanUnion(HexBlock standardContainer, HexBlock neighborhexBlock)
        {
            if (standardContainer.eColor == EColor.none || neighborhexBlock.eColor == EColor.none)
            {
                return false;
            }
            if (standardContainer.eColor == neighborhexBlock.eColor)
            {
                return true;
            }
            return false;
        }
        var neighborList = hexBlockContainer.GetNeighborContainerBlockList();
        var standardHexBlockContainerIndex = HexBlockContainer.hexBlockContainerList.IndexOf(hexBlockContainer);
        foreach (var neighborHexBlockContainer in neighborList)
        {
            if (ReferenceEquals(neighborHexBlockContainer.hexBlock, null))
            {
                continue;
            }
            var neighborHexBlockContainerIndex = HexBlockContainer.hexBlockContainerList.IndexOf(neighborHexBlockContainer);

            if ((unionFind.Find(standardHexBlockContainerIndex) != unionFind.Find(neighborHexBlockContainerIndex)) && IsCanUnion(hexBlockContainer.hexBlock, neighborHexBlockContainer.hexBlock))
            {
                unionFind.Union(standardHexBlockContainerIndex, neighborHexBlockContainerIndex);
                UnionSameColorHexBlocks(neighborHexBlockContainer, unionFind);
            }
        }
    }
    private HexBlockContainer GetBallDestineContainer(RaycastHit2D raycastHit2D)
    {
        var detectedContainer = raycastHit2D.collider.gameObject.GetCashComponent<HexBlockContainer>();
        foreach (var item in detectedContainer.GetNeighborContainerBlockList())
        {
            if (ReferenceEquals(item.hexBlock, null) &&
                GameUtil.DistanceSquare2D(item.transform.position, raycastHit2D.point) <= (HexBlockContainer.hexHeight / 2f) * (HexBlockContainer.hexHeight / 2f))
            {
                return item;
            }
        }
        return null;
    }
    private RaycastHit2D GetNearestRaycastHit(Vector3 from, Vector3 dir, List<ELayers> detectLayerList)
    {
        var hits = Physics2D.RaycastAll(from, dir.normalized, 2000f);

        List<RaycastHit2D> hitsList = new();
        foreach (var hit in hits)
        {
            // 충돌한 경우
            if (hit.collider != null)
            {
                var eLayer = (ELayers)hit.collider.gameObject.layer;
                switch (eLayer)
                {
                    case ELayers.Wall:
                        if (detectLayerList.Contains(eLayer))
                        {
                            hitsList.Add(hit);
                        }
                        break;
                    case ELayers.HexBlockContainer:
                        if (detectLayerList.Contains(eLayer))
                        {
                            var hexBlockContainer = hit.collider.gameObject.GetCashComponent<HexBlockContainer>();
                            if (!ReferenceEquals(hexBlockContainer.hexBlock, null))
                            {
                                hitsList.Add(hit);
                            }
                        }
                        break;
                }
            }
        }
        hitsList.OrderByDescending(x => GameUtil.DistanceSquare2D(x.point, from));
        return hitsList.FirstOrDefault();
    }
}

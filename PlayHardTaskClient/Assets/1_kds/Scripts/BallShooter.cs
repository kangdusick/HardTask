using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallShooter : MonoBehaviour
{
    [SerializeField] RectTransform shootingStartPoint;
    private void Awake()
    {
        //TouchManager.Instance.OnTouchIng+= ShootLaser;
    }
    void Update()
    {
        // 레이저를 쏘는 기능
        ShootLaser();
    }

    void ShootLaser()
    {
        //// 레이저 발사
        //RaycastHit2D hit = Physics2D.Raycast(shootingStartPoint.position, laserDirection, laserDistance);

        //// 충돌한 경우
        //if (hit.collider != null)
        //{
        //    // 충돌 지점의 위치를 출력
        //    Debug.Log("Hit point: " + hit.point);

        //    // 충돌 지점을 시각적으로 표시
        //    Debug.DrawLine(shootingStartPoint.position, hit.point, Color.red);
        //}
        //else
        //{
        //    // 충돌하지 않은 경우 레이저의 최대 거리까지 선을 그림
        //    Debug.DrawLine(shootingStartPoint.position, shootingStartPoint.position + (Vector3)laserDirection * laserDistance, Color.green);
        //}
    }
}

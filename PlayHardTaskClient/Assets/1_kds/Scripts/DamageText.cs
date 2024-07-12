using TMPro;
using UnityEngine;
public class DamageText : MonoBehaviour
{
    [SerializeField] private TMP_Text dmgText;
    public void Init(double damage, Color textColor)
    {
        if(damage<0.001f)
        {
            PoolableManager.Instance.Destroy(gameObject);
            return;
        }
        transform.localScale= Vector3.one;
        transform.position = new Vector3(transform.position.x, transform.position.y + 1f, 0f);
        dmgText.text = damage.KMBTUnit();
        dmgText.color = textColor;
        bool isMoveRight = Random.Range(0, 2) == 0;
        float destPosx;
        if (isMoveRight)
        {
            destPosx = transform.position.x + Random.Range(50f, 150f);

        }
        else
        {
            destPosx = transform.position.x + Random.Range(-150f, -50f);
        }

        GameUtil.Instance.ParabolaMoveForParallel(transform, 
            new Vector3(destPosx, transform.position.y + Random.Range(50f, 150f), 0f),
            isMoveRight ? -60f : 60f,
            0.5f,
            () => { PoolableManager.Instance.Destroy(gameObject); },
            false,false,true);
    }
}

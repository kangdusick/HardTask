using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ToastMessage : MonoBehaviour
{
    [SerializeField] private TMP_Text _messageText;
    [SerializeField] private RectTransform _rect;
    [SerializeField] private DOTweenAnimation _doMoveAnim;
    [SerializeField] private DOTweenAnimation _doScaleAnim;
    public void Init(string message)
    {
        _messageText.text = message;
        _rect.SetParent(MainCanvas.Instance.transform);
        _rect.localPosition = Vector3.zero;
        _rect.localScale = Vector3.one;
        _doMoveAnim.DORestart();
        _doScaleAnim.DORestart();
        LayoutRebuilder.ForceRebuildLayoutImmediate(_rect);
    }
    public void OnAnaimationDone()
    {
        PoolableManager.Instance.Destroy(gameObject);
    }
}

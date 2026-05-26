using UnityEngine;
using TMPro;
using DG.Tweening;

public class DamagePopup : MonoBehaviour
{
    [SerializeField] private TextMeshPro textMesh;

    private void LateUpdate()
    {
        if (Camera.main != null)
        {
            transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward, Camera.main.transform.up);
        }
    }

    public void Setup(float damageAmount, Color color)
    {
        textMesh.text = damageAmount.ToString("F0");
        textMesh.color = color;

        transform.localScale = Vector3.zero;
        transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack);

        transform.DOMoveY(transform.position.y + 1.5f, 0.8f);
        textMesh.DOFade(0, 0.8f).SetDelay(0.2f).OnComplete(() => Destroy(gameObject));
    }
}

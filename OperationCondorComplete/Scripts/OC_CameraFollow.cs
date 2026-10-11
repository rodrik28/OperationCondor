using System.Collections;
using UnityEngine;

public class OC_CameraFollow : MonoBehaviour
{
    [SerializeField] Transform target;
    [SerializeField] float smooth = 5f;
    [SerializeField] Vector3 offset = new Vector3(2.2f, 1f, -10f);
    [SerializeField] float minX = -8f, maxX = 18f;
    Vector3 shakeOffset;

    void LateUpdate()
    {
        if (target == null) return;
        Vector3 desired = target.position + offset;
        desired.x = Mathf.Clamp(desired.x, minX, maxX);
        desired.y = Mathf.Max(-0.1f, desired.y);
        desired.z = -10f;
        transform.position = Vector3.Lerp(transform.position, desired + shakeOffset, smooth * Time.unscaledDeltaTime);
    }

    public void SetTarget(Transform t) => target = t;
    public void Shake(float duration = .15f, float strength = .13f) { StopAllCoroutines(); StartCoroutine(ShakeRoutine(duration, strength)); }
    IEnumerator ShakeRoutine(float d, float s)
    {
        float t=0;
        while (t < d) { t += Time.unscaledDeltaTime; shakeOffset = (Vector3)Random.insideUnitCircle * s; yield return null; }
        shakeOffset = Vector3.zero;
    }
}

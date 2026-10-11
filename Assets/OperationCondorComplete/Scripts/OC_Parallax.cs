using UnityEngine;
public class OC_Parallax : MonoBehaviour
{
    [SerializeField] Transform cam;
    [SerializeField, Range(0f,1f)] float factor = .2f;
    Vector3 start;
    void Start(){ start = transform.position; if (cam == null && Camera.main != null) cam = Camera.main.transform; }
    void LateUpdate(){ if (cam != null) transform.position = new Vector3(start.x + cam.position.x * factor, start.y + cam.position.y * factor * .08f, start.z); }
    public void Configure(Transform c, float f){ cam=c; factor=f; }
}

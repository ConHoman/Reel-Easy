using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float smooth = 0.1f;

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 pos = new Vector3(target.position.x, target.position.y, transform.position.z);
        transform.position = Vector3.Lerp(transform.position, pos, smooth);
    }
}
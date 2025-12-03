using UnityEngine;

public class Billboard : MonoBehaviour
{
    [SerializeField]
    private Camera _camera;

    void Start()
    {
        _camera = Camera.main;
    }

    void LateUpdate()
    {
        Vector3 dir = _camera.transform.position - transform.position;
        dir.y = 0; // запрещаем наклон
        transform.rotation = Quaternion.LookRotation(dir);
        transform.Rotate(0f, 180f, 0f);
    }
}

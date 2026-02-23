using System.Collections;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public GameObject Target;
    public Vector3 Offset = Vector3.zero;
    public Vector3 Dir = new Vector3(0, 1, 0);
    public float Distance = 10;

    private void Awake()
    {
        GetComponent<Camera>().clearFlags = CameraClearFlags.SolidColor;
    }

    IEnumerator Start()
    {
        yield return null;

        while (true)
        {
            transform.position = Target.transform.position + Dir.normalized * Distance + Offset;
            transform.LookAt(Target.transform);
            yield return null;
        }
    }
}

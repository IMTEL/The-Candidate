using UnityEngine;

public class ResetPosition : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        Vector3 position= transform.position;
        position.x = 0;
        position.z = 0;
        transform.position = position;
    }
}

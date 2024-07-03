using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCamera : MonoBehaviour
{
    [Header("----------Move")]
    public Transform target;
    public float speed;

    void LateUpdate()
    {
        transform.position = Vector3.MoveTowards(transform.position, target.position + Vector3.up * 1.5f, Time.deltaTime * speed);
        transform.position = new Vector3(transform.position.x, transform.position.y, -10f);
    }
}

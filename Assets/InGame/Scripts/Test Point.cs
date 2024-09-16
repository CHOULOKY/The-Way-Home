using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestPoint : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            GameManager.Instance.GameExit();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) {
            GameManager.Instance.savePoint = this.transform.position;
        }
    }
}

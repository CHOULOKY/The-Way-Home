using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

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
            PhotonView PV = gameObject.GetComponent<PhotonView>();
            PV.RPC("AssignPoint", RpcTarget.All, this.transform.position.x, this.transform.position.y);
        }
    }
    [PunRPC]
    private void AssignPoint(float _x, float _y)
    {
        GameManager.Instance.savePoint = new Vector2(_x, _y);
    }
}

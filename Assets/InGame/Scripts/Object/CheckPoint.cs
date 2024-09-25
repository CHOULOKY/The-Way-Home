using UnityEngine;
using Photon.Pun;

public class CheckPoint : MonoBehaviour
{
    [Header("CheckPoint")]
    public int pointNumber;

    private void Update()
    {
        // Test Code
        /*
        if (Input.GetKeyDown(KeyCode.Escape))
            GameManager.Instance.GameFail();
        */
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
            OnTriggerExit2D(collision);
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) {
            if (!GameManager.Instance || GameManager.Instance.saveNumber > this.pointNumber) return;
            PhotonView PV = gameObject.GetComponent<PhotonView>();
            PV.RPC("AssignPoint", RpcTarget.All, this.pointNumber, this.transform.position.x, this.transform.position.y);
        }
    }
    [PunRPC]
    private void AssignPoint(int _number, float _x, float _y)
    {
        GameManager.Instance.saveNumber = _number;
        GameManager.Instance.savePoint = new Vector2(_x, _y);
    }
}

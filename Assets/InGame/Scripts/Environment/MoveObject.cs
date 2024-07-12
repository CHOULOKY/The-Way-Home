using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveObject : MonoBehaviour
{
    [Header("----------Attributes")]
    private Rigidbody2D rigid;

    [Header("----------Hit")]
    public bool isETCObj;
    public float knockPower;

    [Header("----------Photon")]
    private PhotonView PV;


    void Awake()
    {
        // Attributes
        rigid = GetComponent<Rigidbody2D>();

        // Photon
        PV = GetComponent<PhotonView>();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        OnCollisionStay2D(collision);
    }
    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player")) {
            Player player = collision.gameObject.GetComponent<Player>();

            #region Exception
            if (!isETCObj) return;
            if (player.isHurt || player.isDeath) return;
            if (!player.PV.IsMine) return;
            #endregion

            PV.RequestOwnership();
            Vector2 hittedDir = (this.transform.position - player.transform.position).normalized;
            this.rigid.AddForce(hittedDir * knockPower, ForceMode2D.Impulse);
        }
    }


    public void Hitted(Player player)
    {
        if (isETCObj) return;

        PV.RequestOwnership();
        Vector2 hittedDir = (this.transform.position - player.transform.position).normalized;
        this.rigid.AddForce(hittedDir * knockPower, ForceMode2D.Impulse);
    }
}

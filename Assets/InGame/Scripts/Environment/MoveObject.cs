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
    public float knockPower;
    public string effectName;
    private ParticleSystem hitEffect;

    [Header("----------Photon")]
    private PhotonView PV;


    void Awake()
    {
        // Attributes
        rigid = GetComponent<Rigidbody2D>();

        // Photon
        PV = GetComponent<PhotonView>();
    }


    public void Hitted(Player player)
    {
        PV.RequestOwnership();
        PV.RPC("PlayHittedEffect", RpcTarget.All);

        Vector2 hittedDir = (this.transform.position - player.transform.position).normalized;
        this.rigid.AddForce(hittedDir * knockPower, ForceMode2D.Impulse);
    }
    [PunRPC]
    void PlayHittedEffect()
    {
        if (!hitEffect)
            hitEffect = PhotonNetwork.Instantiate(effectName, transform.position, Quaternion.identity).GetComponent<ParticleSystem>();
        hitEffect.transform.position = transform.position;
        hitEffect.transform.localScale = new Vector2(Random.Range(0.4f, 1f), Random.Range(0.4f, 1f));
        hitEffect.gameObject.SetActive(true);
        hitEffect.Play();
    }
}

using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveObject : MonoBehaviour
{
    [Header("----------Attributes")]
    private Rigidbody2D rigid;
    private Animator animator;

    [Header("----------Hit")]
    public float knockPower;
    public string effectName;
    private ParticleSystem hitEffect;
    public bool isDestroyObj;

    [Header("----------Photon")]
    private PhotonView PV;


    void Awake()
    {
        // Attributes
        rigid = GetComponent<Rigidbody2D>();
        if (isDestroyObj) animator = GetComponent<Animator>();

        // Photon
        PV = GetComponent<PhotonView>();
    }


    public void Hitted(Player player)
    {
        if (!PV.IsMine) PV.RequestOwnership();
        PV.RPC("PlayHittedEffect", RpcTarget.All);

        if (isDestroyObj) {
            PV.RPC("SetRPCTrg", RpcTarget.All, "destroyTrg");
        }
        else {
            Vector2 hittedDir = (this.transform.position - player.transform.position).normalized;
            this.rigid.AddForce(hittedDir * knockPower, ForceMode2D.Impulse);
        }
    }
    public void DestroyObject()
    {
        gameObject.SetActive(false);
    }
    [PunRPC]
    private void PlayHittedEffect()
    {
        if (!hitEffect)
            hitEffect = PhotonNetwork.Instantiate(effectName, transform.position, Quaternion.identity).GetComponent<ParticleSystem>();
        hitEffect.transform.position = transform.position;
        hitEffect.transform.localScale = new Vector2(Random.Range(0.4f, 1f), Random.Range(0.4f, 1f));
        hitEffect.gameObject.SetActive(true);
        hitEffect.Play();
    }

    [PunRPC]
    private void SetRPCTrg(string _str)
    {
        animator.SetTrigger(_str);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Object : MonoBehaviour
{
    [Header("Component")]
    private Rigidbody2D rigid;
    private PhotonView PV;

    [Header("Hurt")]
    public float knockPower;
    public string effectName;
    private ParticleSystem hitEffect;
    public bool isDestroyObj;


    void Awake()
    {
        // Component
        rigid = GetComponent<Rigidbody2D>();
        PV = GetComponent<PhotonView>();
    }


    public void HurtByPlayer(GameObject _player)
    {
        if (!PV.IsMine) PV.RequestOwnership();
        PV.RPC("PlayHurtEffect", RpcTarget.All);

        if (isDestroyObj) {
            PV.RPC("SetAnimTrg", RpcTarget.All, "destroyTrg");
        }
        else {
            Vector2 hittedDir = (this.transform.position - _player.transform.position).normalized;
            this.rigid.AddForce(hittedDir * knockPower, ForceMode2D.Impulse);
        }
    }
    public void DestroyObject()
    {
        Destroy(this);
    }
    [PunRPC]
    private void PlayHurtEffect()
    {
        if (!hitEffect)
            hitEffect = PhotonNetwork.Instantiate(effectName, transform.position, Quaternion.identity).GetComponent<ParticleSystem>();
        hitEffect.transform.position = transform.position;
        hitEffect.transform.localScale = new Vector2(Random.Range(0.4f, 1f), Random.Range(0.4f, 1f));
        hitEffect.gameObject.SetActive(true);
        hitEffect.Play();
    }

    [PunRPC]
    private void SetAnimTrg(string _str)
    {
        this.GetComponent<Animator>().SetTrigger(_str);
    }
}

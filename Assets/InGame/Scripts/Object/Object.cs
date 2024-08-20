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
    public bool isDestroyObj;
    private ParticleSystem hurtEffect;


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
        if (!hurtEffect)
            hurtEffect = PhotonNetwork.Instantiate(effectName, transform.position, Quaternion.identity).GetComponent<ParticleSystem>();
        hurtEffect.transform.position = transform.position;
        float effectSize = this.transform.localScale.x;
        hurtEffect.transform.localScale =
            new Vector2(Random.Range(effectSize * 0.4f, effectSize), Random.Range(effectSize * 0.4f, effectSize));
        hurtEffect.gameObject.SetActive(true);
        hurtEffect.Play();
    }

    [PunRPC]
    private void SetAnimTrg(string _str)
    {
        this.GetComponent<Animator>().SetTrigger(_str);
    }
}

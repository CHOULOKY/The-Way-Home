using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trap : MonoBehaviour
{
    [Header("Component")]
    private Rigidbody2D rigid;
    private PhotonView PV;

    [Header("Attack")]
    public float attackPower;

    [Header("Hurt")]
    public bool isHurtTrap;
    public float knockPower;
    public string effectName;
    private ParticleSystem hurtEffect;

    private void Awake()
    {
        // Component
        rigid = GetComponent<Rigidbody2D>();
        PV = GetComponent<PhotonView>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        OnCollisionStay2D(collision);
    }
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player")) {
            collision.gameObject.GetComponent<Player>().HurtByMonster(this.gameObject, attackPower);
        }
        else if (collision.gameObject.CompareTag("Monster")) {
            collision.gameObject.GetComponent<Monster>().HurtByPlayer(this.gameObject, attackPower);
        }
    }

    public void HurtByPlayer(GameObject _player)
    {
        if (!PV.IsMine) PV.RequestOwnership();
        PV.RPC("PlayHurtEffect", RpcTarget.All);

        Vector2 hittedDir = (this.transform.position - _player.transform.position).normalized;
        this.rigid.AddForce(hittedDir * knockPower, ForceMode2D.Impulse);
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
}

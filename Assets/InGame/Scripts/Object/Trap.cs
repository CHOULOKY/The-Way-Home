using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trap : MonoBehaviour
{
    [Header("Component")]
    private Rigidbody2D rigid;
    private PhotonView photonView;

    [Header("Attack")]
    public float attackPower;

    [Header("Hurt")]
    public bool isHurtTrap;
    public float knockPower;
    public string effectName;
    private ParticleSystem hurtEffect;


    private void Awake()
    {
        // Component initialization
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        rigid = GetComponent<Rigidbody2D>();
        photonView = GetComponent<PhotonView>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        OnCollisionStay2D(collision);
    }
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player")) {
            collision.gameObject.GetComponent<Player>().HurtByMonster(gameObject, attackPower);
        }
        else if (collision.gameObject.CompareTag("Monster")) {
            collision.gameObject.GetComponent<Monster>().HurtByPlayer(gameObject, attackPower);
        }
    }

    public void HurtByPlayer(GameObject player)
    {
        if (!photonView.IsMine) {
            photonView.RequestOwnership();
        }

        photonView.RPC(nameof(PlayHurtEffect), RpcTarget.All);
        ApplyKnockback(player);
    }

    private void ApplyKnockback(GameObject player)
    {
        Vector2 knockbackDirection = (transform.position - player.transform.position).normalized;
        knockbackDirection = new Vector2(Mathf.Sign(knockbackDirection.x), 1);
        rigid.AddForce(knockbackDirection * knockPower, ForceMode2D.Impulse);
    }

    [PunRPC]
    private void PlayHurtEffect()
    {
        if (hurtEffect == null) {
            hurtEffect = PhotonNetwork.Instantiate(effectName, transform.position, Quaternion.identity).GetComponent<ParticleSystem>();
        }

        ConfigureHurtEffect();
        hurtEffect.Play();
    }
    private void ConfigureHurtEffect()
    {
        hurtEffect.transform.position = transform.position;
        float effectSize = transform.localScale.x;
        hurtEffect.transform.localScale = new Vector2(Random.Range(effectSize * 0.4f, effectSize), Random.Range(effectSize * 0.4f, effectSize));
        hurtEffect.gameObject.SetActive(true);
    }
}

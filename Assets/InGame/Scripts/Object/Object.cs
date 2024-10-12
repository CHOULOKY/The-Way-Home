using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Object : MonoBehaviour
{
    [Header("Component")]
    private Rigidbody2D rigid;
    private PhotonView photonView;

    [Header("Hurt")]
    public float knockPower;
    public string effectName;
    public bool isDestroyObj;
    private ParticleSystem hurtEffect;


    private void Awake()
    {
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        rigid = GetComponent<Rigidbody2D>();
        photonView = GetComponent<PhotonView>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (IsCollisionWithRigidBody(collision.collider)) return;

        if (collision.collider.CompareTag("Player") || collision.collider.CompareTag("Monster")) {
            rigid.velocity = new Vector2(rigid.velocity.x * 0.25f, rigid.velocity.y * 0.5f);
        }
    }
    private bool IsCollisionWithRigidBody(Collider2D collider)
    {
        Rigidbody2D rigidBody = collider.GetComponent<Rigidbody2D>();
        return rigidBody &&
               (rigidBody.bodyType == RigidbodyType2D.Static || rigidBody.bodyType == RigidbodyType2D.Dynamic);
    }

    public void HurtByPlayer(GameObject player)
    {
        if (!photonView.IsMine) photonView.RequestOwnership();
        photonView.RPC(nameof(PlayHurtEffect), RpcTarget.All);

        if (isDestroyObj) {
            photonView.RPC(nameof(SetAnimTrigger), RpcTarget.All, "destroyTrg");
            SoundManager.instance.PlaySfx(SoundManager.Sfx.Melee);
        } else {
            ApplyKnockback(player);
        }
    }
    private void ApplyKnockback(GameObject player)
    {
        Vector2 hitDirection = (transform.position - player.transform.position).normalized;
        rigid.AddForce(hitDirection * knockPower, ForceMode2D.Impulse);
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

    public void DestroyObject()
    {
        Destroy(this.gameObject);
    }


    [PunRPC]
    private void SetAnimTrigger(string triggerName)
    {
        GetComponent<Animator>().SetTrigger(triggerName);
    }
}

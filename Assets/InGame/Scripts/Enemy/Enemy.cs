using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("----------Attributes")]
    private Rigidbody2D rigid;

    [Header("----------Attack")]
    public int attackPower;

    [Header("----------Hit")]
    public float knockPower;
    public string effectName;
    private ParticleSystem hitEffect;

    [Header("----------Photon")]
    private PhotonView PV;
    private Vector3 curPos;


    void Awake()
    {
        // Attributes
        rigid = GetComponent<Rigidbody2D>();

        // Photon
        PV = GetComponent<PhotonView>();
    }

    void OnEnable()
    {
        // Enemy initialization
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
            if (player.isHurt || player.isDeath) return;
            else if (!player.PV.IsMine) return;
            #endregion

            player.isHurt = true;
            player.animator.SetBool("isHurt", true);
            player.animator.SetTrigger("hurtTrigger");
            StartCoroutine(player.HurtRoutine());

            player.rigid.constraints = RigidbodyConstraints2D.FreezeRotation;
            Vector2 knockDir = (player.rigid.position - this.rigid.position);
            player.rigid.velocity = Vector2.zero;
            player.rigid.AddForce(knockDir * knockPower, ForceMode2D.Impulse);

            player.stat.health -= this.attackPower;
        }
    }


    public void Hitted(Player player)
    {
        PV.RequestOwnership();
        PV.RPC("PlayHitParticleEffect", RpcTarget.All);

        Vector2 hittedDir = (this.transform.position - player.transform.position).normalized;
        this.rigid.AddForce(hittedDir * knockPower, ForceMode2D.Impulse);
    }
    [PunRPC]
    void PlayHitParticleEffect()
    {
        if (!hitEffect)
            hitEffect = PhotonNetwork.Instantiate(effectName, transform.position, Quaternion.identity).GetComponent<ParticleSystem>();
        hitEffect.transform.position = transform.position;
        hitEffect.transform.localScale = new Vector2(Random.Range(0.4f, 1f), Random.Range(0.4f, 1f));
        hitEffect.gameObject.SetActive(true);
        hitEffect.Play();
    }
}

using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trap : MonoBehaviour
{
    [Header("Attack")]
    public float attackPower;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        OnCollisionStay2D(collision);
    }
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player")) {
            collision.gameObject.GetComponent<Player>().HurtByMonster(this.gameObject, attackPower);
        }
    }

    /*
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
    */
}

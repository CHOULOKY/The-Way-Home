using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Player;
using UnityEngine.TextCore.Text;
using UnityEngine.UIElements;

public class Enemy : MonoBehaviour, IPunObservable
{
    [Header("----------Attributes")]
    private Rigidbody2D rigid;
    private Animator animator;

    [Header("----------Enemy State")]
    public EnemyStat stat;
    public bool isHurt;
    public bool isDeath;

    [Header("----------Move")]
    public int inputX;

    [Header("----------Attack")]
    [Tooltip("If true, the attack is done with a collider. If not, it is done with a trigger.")]
    public bool isCollAtk;

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
        animator = GetComponent<Animator>();

        // Photon
        PV = GetComponent<PhotonView>();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        #region Exception
        if (isHurt || isDeath) return;
        if (!isCollAtk) return;
        #endregion

        OnCollisionStay2D(collision);
    }
    void OnCollisionStay2D(Collision2D collision)
    {
        #region Exception
        if (isHurt || isDeath) return;
        if (!isCollAtk) return;
        #endregion

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

            player.stat.health -= stat.attackPower;
        }
    }

    void Start()
    {
        StartCoroutine(StartXRoutine());
    }

    void Update()
    {
        #region Exception
        if (isHurt || isDeath) return;
        if (isCollAtk) return;
        #endregion

        #region Flip
        ControlFlip(inputX);
        #endregion

        #region Move
        transform.Translate(Mathf.Abs(inputX) * Vector2.right * stat.moveSpeed * Time.deltaTime);
        #endregion

        #region Attack

        #endregion

        #region Animator Parameter
        animator.SetFloat("xMove", Mathf.Abs(inputX));
        animator.SetFloat("yMove", rigid.velocity.y);
        #endregion
    }


    private IEnumerator StartXRoutine()
    {
        inputX = Random.Range(-1, 2);

        yield return new WaitForSeconds(7f);

        StartCoroutine(StartXRoutine());
    }

    private void ControlFlip(int _inputX)
    {
        // FlipX
        if (_inputX > 0)
            transform.eulerAngles = Vector3.zero;
        else if (_inputX < 0)
            transform.eulerAngles = new Vector3(0, 180, 0);
    }

    public void Hitted(Player player)
    {
        #region Exception
        if (isDeath) return;
        if (isCollAtk) return;
        #endregion

        PV.RequestOwnership();
        PV.RPC("PlayHitParticleEffect", RpcTarget.All);

        StartCoroutine(HurtRoutine());
        Vector2 hittedDir = (this.transform.position - player.transform.position).normalized;
        this.rigid.AddForce(hittedDir * knockPower, ForceMode2D.Impulse);
    }
    [PunRPC]
    private void PlayHitParticleEffect()
    {
        if (!hitEffect)
            hitEffect = PhotonNetwork.Instantiate(effectName, transform.position, Quaternion.identity).GetComponent<ParticleSystem>();
        hitEffect.transform.position = transform.position;
        hitEffect.transform.localScale = new Vector2(Random.Range(0.4f, 1f), Random.Range(0.4f, 1f));
        hitEffect.gameObject.SetActive(true);
        hitEffect.Play();
    }
    private IEnumerator HurtRoutine()
    {
        isHurt = true;
        animator.SetBool("isHurt", isHurt);

        yield return new WaitForSeconds(0.25f);

        isHurt = false;
        animator.SetBool("isHurt", isHurt);
    }


    #region Photon
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting) {
            stream.SendNext(inputX);
            stream.SendNext(isHurt);
        }
        else {
            inputX = (int)stream.ReceiveNext();
            isHurt = (bool)stream.ReceiveNext();
        }
    }
    #endregion
}


[System.Serializable]
public class EnemyStat
{
    [Header("Health stat")]
    public int maxHealth;
    public int health;

    [Header("Move stat")]
    public float moveSpeed;

    [Header("Attack stat")]
    public int attackPower;
    [Tooltip("Second basis")] public float attackSpeed;
}

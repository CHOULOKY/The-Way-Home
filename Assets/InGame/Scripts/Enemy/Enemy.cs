using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Player;
using UnityEngine.TextCore.Text;
using UnityEngine.UIElements;
using UnityEngine.Windows;

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
    bool isGround;

    [Header("----------Slope")]
    private Transform groundPos; // Must position child's 0
    private Transform frontPos; // Must position child's 1
    private float groundRadius;
    private float slopeDistance;
    private RaycastHit2D slopeHit;
    private RaycastHit2D frontHit;
    private float maxAngle;
    private float angle;
    private Vector2 perp;
    public bool isSlope;

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

        // Slope
        groundPos = transform.GetChild(0);
        frontPos = transform.GetChild(1);

        // Photon
        PV = GetComponent<PhotonView>();
    }

    void OnEnable()
    {
        // Slope
        groundRadius = 0.1f;
        slopeDistance = 1;
        maxAngle = 60;
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
        if (PhotonNetwork.InRoom)
            PV.RPC("ControlFlip", RpcTarget.AllBuffered, inputX, isSlope);
        #endregion

        #region Check - Ground
        GroundChk();
        #endregion

        #region Check - Slope
        slopeHit = Physics2D.Raycast(groundPos.position, Vector2.down, slopeDistance,
            LayerMask.GetMask("Ground", "Front Object"));
        frontHit = Physics2D.Raycast(frontPos.position, transform.right, 0.1f,
            LayerMask.GetMask("Ground", "Front Object"));

        if ((slopeHit || frontHit)) {
            if (frontHit)
                SlopeChk(frontHit);
            else if (slopeHit)
                SlopeChk(slopeHit);

            // Check angle and perp
            /*
             Debug.DrawLine(slopeHit.point, slopeHit.point + slopeHit.normal, Color.red);
             Debug.DrawLine(slopeHit.point, slopeHit.point + perp, Color.red);
             Debug.DrawLine(frontHit.point, frontHit.point + frontHit.normal, Color.green);
             Debug.DrawLine(frontHit.point, frontHit.point + perp, Color.green);
            */
        }
        #endregion

        #region Move
        if (PV.IsMine)
            Move();
        #endregion

        #region Attack

        #endregion

        #region Animator Parameter
        if (animator != null && PV.IsMine) {
            animator.SetFloat("xMove", Mathf.Abs(inputX));
            animator.SetFloat("yMove", rigid.velocity.y);
        }
        #endregion
    }


    private IEnumerator StartXRoutine()
    {
        inputX = Random.Range(-1, 2);

        yield return new WaitForSeconds(Random.Range(3f, 10f));

        StartCoroutine(StartXRoutine());
    }

    [PunRPC]
    private void ControlFlip(int _inputX, bool _isSlope)
    {
        // FlipX
        if (_inputX > 0)
            transform.eulerAngles = Vector3.zero;
        else if (_inputX < 0)
            transform.eulerAngles = new Vector3(0, 180, 0);

        // FlipZ (on the slope)
        if (_inputX == 0 && _isSlope)
            rigid.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
        else
            rigid.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    private void GroundChk()
    {
        isGround = Physics2D.OverlapCircle(groundPos.position, groundRadius,
            LayerMask.GetMask("Ground", "Front Object"));
    }
    private void SlopeChk(RaycastHit2D hit)
    {
        angle = Vector2.Angle(hit.normal, Vector2.up);
        perp = Vector2.Perpendicular(hit.normal).normalized;

        if (angle != 0) isSlope = true;
        else isSlope = false;
    }

    private void DeathChk()
    {
        if (stat.health <= 0) {
            isDeath = true;
            animator.SetBool("isDeath", true);
        }
    }

    private void Move()
    {
        // Translate Move
        if (inputX != 0) {
            if (isSlope && isGround && angle < maxAngle) {
                rigid.velocity = Vector2.zero;
                if (inputX > 0)
                    transform.Translate(new Vector2(Mathf.Abs(inputX) * -perp.x * stat.moveSpeed * Time.deltaTime,
                        Mathf.Abs(inputX) * -perp.y * stat.moveSpeed * Time.deltaTime));
                else if (inputX < 0)
                    transform.Translate(new Vector2(Mathf.Abs(inputX) * -perp.x * stat.moveSpeed * Time.deltaTime,
                        Mathf.Abs(inputX) * perp.y * stat.moveSpeed * Time.deltaTime));
            }
            else
                transform.Translate(Mathf.Abs(inputX) * Vector2.right * stat.moveSpeed * Time.deltaTime);
        }
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
            stream.SendNext(isHurt);
        }
        else {
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

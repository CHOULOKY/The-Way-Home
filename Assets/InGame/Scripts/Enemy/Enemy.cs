using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Player;
using UnityEngine.TextCore.Text;
using UnityEngine.UIElements;
using Unity.Burst.CompilerServices;

public class Enemy : MonoBehaviour, IPunObservable
{
    [Header("----------Attributes")]
    public Rigidbody2D rigid;
    private Animator animator;

    [Header("----------Enemy State")]
    public EnemyStat stat;
    public bool isHurt;
    public bool isDeath;

    [Header("----------Move")]
    public int inputX;
    public RaycastHit2D wallHit;

    [Header("----------Slope")]
    private Transform frontPos; // Must position child's 0
    private Transform groundPos; // Must position child's 1
    private float groundRadius;
    private float slopeDistance;
    private RaycastHit2D slopeHit;
    private RaycastHit2D frontHit;
    private float maxAngle;
    private float angle;
    private Vector2 perp;
    public bool isGround;
    public bool isSlope;

    [Header("----------Search")]
    public float searchDistance;
    public Vector2 searchBox;
    private RaycastHit2D searchHit;
    private float searchTimer;

    [Header("----------Attack")]
    public float attackDistance;
    public Vector2 attackBox;
    private RaycastHit2D[] attackHits;
    [Tooltip("If true, the attack is done with a collider. If not, it is done with a trigger.")]
    public bool isCollAtk;
    public bool isAttacking;

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
        frontPos = transform.GetChild(0);
        groundPos = transform.GetChild(1);

        // Photon
        PV = GetComponent<PhotonView>();
    }

    void OnEnable()
    {
        #region Exception
        if (isCollAtk) return;
        #endregion

        // Attributes
        rigid.simulated = true;
        this.gameObject.SetActive(true);

        // Enemy Status
        stat.health = stat.maxHealth;
        isDeath = false;

        // Slope
        groundRadius = 0.1f;
        slopeDistance = 1;
        maxAngle = 60;

        // Start Function
        StartCoroutine(SetXRoutine());
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
            collision.gameObject.GetComponent<Player>().Hitted(this);
        }
    }

    void Update()
    {
        #region Check - Death
        DeathChk();
        #endregion

        #region Exception
        if (isHurt || isDeath) return;
        if (isCollAtk) return;
        #endregion

        #region Flip
        if (PhotonNetwork.InRoom && PV.IsMine && !isAttacking)
            PV.RPC("ControlFlip", RpcTarget.AllBuffered, inputX, isSlope);
        #endregion

        #region Check - Ground
        GroundChk();
        #endregion

        #region Search
        SearchPlayer();
        #endregion

        #region Check - Wall
        WallChk();
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
        if (isAttacking) inputX = 0;

        if (PV.IsMine)
            Move();
        #endregion

        #region Attack
        Attack();
        #endregion

        #region Animator Parameter
        if (animator != null && PV.IsMine) {
            animator.SetFloat("xMove", Mathf.Abs(inputX));
            animator.SetFloat("yMove", rigid.velocity.y);
        }
        #endregion
    }


    private IEnumerator SetXRoutine()
    {
        inputX = Random.Range(-1, 2);

        yield return new WaitForSeconds(Random.Range(3f, 10f));

        StartCoroutine(SetXRoutine());
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
    private void WallChk()
    {
        wallHit = Physics2D.Raycast(rigid.position, Vector2.right * inputX, 0.5f,
            LayerMask.GetMask("Ground", "Front Object"));
        if (wallHit && (wallHit.collider.CompareTag("Ground") || wallHit.collider.CompareTag("Stop Object")))
            inputX *= searchHit ? 0 : Random.Range(-1, 1);
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
        if (stat.health <= 0 && !isDeath) {
            isDeath = true;
            rigid.simulated = false;
            animator.SetTrigger("deathTrg");

            StartCoroutine(DeathRoutine());
        }
    }
    private IEnumerator DeathRoutine()
    {
        yield return new WaitForSeconds(8);

        this.gameObject.SetActive(false);
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

    private void SearchPlayer()
    {
        RaycastHit2D _searchHit = Physics2D.BoxCast((Vector2)transform.position, searchBox, 0,
            transform.rotation.eulerAngles.y == 180 ? Vector2.left : Vector2.right,
            searchDistance, LayerMask.GetMask("Player"));

        if (_searchHit) {
            searchHit = _searchHit;
            searchTimer = 0;
        }
        else {
            searchTimer += searchTimer > 10f ? 0 : Time.deltaTime;
            if (searchTimer > 5f) searchHit = _searchHit;
        }
        if (searchHit) {
            float dirX = searchHit.transform.position.x - transform.position.x;
            float dirY = searchHit.transform.position.y - transform.position.y;
            inputX = (int)Mathf.Sign(dirX);
            if (Mathf.Abs(dirX) < 0.5f) inputX = 0;
        }
    }

    private void Attack()
    {
        RaycastHit2D _attackHit = Physics2D.BoxCast((Vector2)transform.position, attackBox, 0,
                transform.rotation.eulerAngles.y == 180 ? Vector2.left : Vector2.right,
                attackDistance, LayerMask.GetMask("Player"));

        if (_attackHit) {
            if (!isAttacking) {
                isAttacking = true;
                StartCoroutine(AttackRoutine());
            }
        }
    }
    private IEnumerator AttackRoutine()
    {
        // ! anim
        // Debug.Log("Player Search in Attack Box!");

        yield return new WaitForSeconds(1.5f);

        if (!isHurt) {
            attackHits = Physics2D.BoxCastAll((Vector2)transform.position, attackBox, 0,
            transform.rotation.eulerAngles.y == 180 ? Vector2.left : Vector2.right,
            attackDistance, LayerMask.GetMask("Player"));

            if (attackHits != null) {
                foreach (var hit in attackHits) {
                    hit.collider.GetComponent<Player>().Hitted(this);
                    // Debug.Log("Attack in Attack Box!");
                }
            }
            animator.SetTrigger("attackTrg");

            yield return new WaitForSeconds(1f);

            isAttacking = false;
        }
        else {
            isAttacking = true;
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

        this.stat.health -= player.stat.attackPower;
    }
    private IEnumerator HurtRoutine()
    {
        isHurt = true;
        animator.SetBool("isHurt", isHurt);

        yield return new WaitForSeconds(0.25f);

        isHurt = false;
        animator.SetBool("isHurt", isHurt);
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


    private void OnDrawGizmos()
    {
        // Check search box
        Gizmos.color = Color.red;
        if (transform.rotation.eulerAngles.y == 180)
            Gizmos.DrawWireCube(transform.position + Vector3.left * searchDistance, searchBox);
        else
            Gizmos.DrawWireCube(transform.position + Vector3.right * searchDistance, searchBox);
        

        // Check attack box
        Gizmos.color = Color.green;
        if (transform.rotation.eulerAngles.y == 180)
            Gizmos.DrawWireCube(transform.position + Vector3.left * attackDistance, attackBox);
        else
            Gizmos.DrawWireCube(transform.position + Vector3.right * attackDistance, attackBox);
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
}

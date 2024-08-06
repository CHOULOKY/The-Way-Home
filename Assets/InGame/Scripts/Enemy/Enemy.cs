using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Player;
using UnityEngine.TextCore.Text;
using UnityEngine.UIElements;
using Unity.Burst.CompilerServices;
using static Enemy;

public class Enemy : ObjFunc, IPunObservable
{
    [Header("----------Attributes")]
    public Rigidbody2D rigid;
    private Animator animator;

    public enum CurState { Move, Attack, Hurt, Death };
    [Header("----------Enemy State")]
    public EnemyStat stat;
    public CurState curState;
    public bool isDeath;

    [Header("----------Move")]
    public float inputX;
    public bool isWall;

    [Header("----------Slope")]
    private Transform frontPos; // Must position child's 0
    private Transform groundPos; // Must position child's 1
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
    public float curAttackTime;
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
        if (!isCollAtk) {
            frontPos = transform.GetChild(0);
            groundPos = transform.GetChild(1);
        }

        // Photon
        if (!isCollAtk) {
            PV = GetComponent<PhotonView>();
        }
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
        curState = CurState.Move;
        stat.health = stat.maxHealth;
        isDeath = false;

        // Start Function
        StartCoroutine(SetXRoutine());
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        #region Exception
        if (!isCollAtk) return;
        #endregion

        OnCollisionStay2D(collision);
    }
    void OnCollisionStay2D(Collision2D collision)
    {
        #region Exception
        if (!isCollAtk) return;
        #endregion

        if (collision.gameObject.CompareTag("Player")) {
            collision.gameObject.GetComponent<Player>().Hitted(this);
        }
    }

    void Update()
    {
        #region Exception
        if (isCollAtk) return;
        if (!PhotonNetwork.IsConnected || !PhotonNetwork.InRoom || !PV.IsMine) return;
        if (isDeath) return;
        #endregion

        #region Check - Death
        isDeath = DeathChk(stat.health, isDeath);
        if (isDeath) curState = CurState.Death;
        #endregion

        #region FSM
        switch (curState) {
            case CurState.Move:
                #region Check - Ground, Slope, Wall
                isGround = GroundChk(groundPos.position, 0.1f, new string[] { "Ground", "Front Object" });

                (angle, perp, isSlope) = SlopeChk(rigid, groundPos.position, frontPos.position,
                    new string[] { "Ground", "Front Object" });

                isWall = WallChk(rigid, rigid.position, 0.5f, new string[] { "Ground", "Front Object" });
                #endregion

                #region Search
                SearchPlayer();
                if (searchHit) {
                    float dirX = searchHit.transform.position.x - transform.position.x;
                    inputX = (int)Mathf.Sign(dirX);
                    if (Mathf.Abs(dirX) < 0.5f) inputX = 0;
                }
                #endregion

                #region Move
                if (isWall) {
                    if (searchHit) {
                        if (rigid.transform.eulerAngles.y == 180)
                            inputX = searchHit.collider.transform.position.x > rigid.position.x ? 1 : 0;
                        else {
                            inputX = searchHit.collider.transform.position.x > rigid.position.x ? 0 : -1;
                        }
                    }
                    else {
                        if (rigid.transform.eulerAngles.y == 180 && inputX == -1) {
                            inputX = 1;
                        }
                        else if (inputX == 1) {
                            inputX = -1;
                        }
                    }
                }
                
                Move(rigid, inputX, stat.moveSpeed, perp, isGround, false, isSlope, angle < 60);
                #endregion

                #region Flip
                ControlFlip(rigid, inputX, isSlope);
                #endregion

                #region Change State
                if (CanAttack()) curState = CurState.Attack;
                #endregion
                break;
            case CurState.Attack:
                #region Attack
                inputX = 0;
                curAttackTime += Time.deltaTime;

                if (curAttackTime > stat.attackSpeed) {
                    curAttackTime = 0;
                    StartCoroutine(AttackRoutine());
                }
                #endregion

                #region Flip
                ControlFlip(rigid, inputX, isSlope);
                #endregion
                break;
            case CurState.Hurt:
                curAttackTime = 0;
                break;
            case CurState.Death:
                rigid.simulated = false;
                PV.RPC("SetRPCTrg", RpcTarget.All, "deathTrg");

                StartCoroutine(DeathRoutine());
                break;
        }
        #endregion

        #region Animator Parameter
        if (animator != null) {
            PV.RPC("SetRPCFloat", RpcTarget.All, "xMove", Mathf.Abs(inputX));
            PV.RPC("SetRPCFloat", RpcTarget.All, "yMove", rigid.velocity.y);
        }
        #endregion
    }


    private IEnumerator SetXRoutine()
    {
        inputX = Random.Range(-1, 2);

        yield return new WaitForSeconds(Random.Range(3f, 10f));

        StartCoroutine(SetXRoutine());
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
    }

    private bool CanAttack()
    {
        RaycastHit2D _attackHit = Physics2D.BoxCast((Vector2)transform.position, attackBox, 0,
                transform.rotation.eulerAngles.y == 180 ? Vector2.left : Vector2.right,
                attackDistance, LayerMask.GetMask("Player"));

        return _attackHit;
    }
    private IEnumerator AttackRoutine()
    {
        // ! anim
        // Debug.Log("Player Search in Attack Box!");

        RaycastHit2D[] _attackHits = null;
        _attackHits = Physics2D.BoxCastAll((Vector2)transform.position, attackBox, 0,
        transform.rotation.eulerAngles.y == 180 ? Vector2.left : Vector2.right,
        attackDistance, LayerMask.GetMask("Player"));

        if (_attackHits != null) {
            foreach (var hit in _attackHits) {
                PV.RPC("Attack", RpcTarget.All,
                    hit.collider.gameObject.GetComponent<PhotonView>().ViewID);
                // Debug.Log("Attack in Attack Box!");
            }
        }
        PV.RPC("SetRPCTrg", RpcTarget.All, "attackTrg");

        yield return new WaitForSeconds(1f);

        curState = CurState.Move;
        curAttackTime = 0;
    }
    [PunRPC]
    private void Attack(int viewID)
    {
        PhotonView targetView = PhotonView.Find(viewID);
        if (targetView != null) {
            GameObject target = targetView.gameObject;
            target.GetComponent<Player>().Hitted(this);
        }
    }

    public void Hitted(Player player)
    {
        #region Exception
        if (isDeath) return;
        if (isCollAtk) return;
        #endregion

        if (!PV.IsMine) PV.RequestOwnership();
        PV.RPC("PlayHitParticleEffect", RpcTarget.All);

        StartCoroutine(HurtRoutine());
        Vector2 hittedDir = (this.transform.position - player.transform.position).normalized;
        this.rigid.AddForce(hittedDir * knockPower, ForceMode2D.Impulse);

        this.stat.health -= player.stat.attackPower;
    }
    private IEnumerator HurtRoutine()
    {
        curState = CurState.Hurt;
        PV.RPC("SetRPCBool", RpcTarget.All, "isHurt", true);

        yield return new WaitForSeconds(0.25f);

        curState = CurState.Move;
        PV.RPC("SetRPCBool", RpcTarget.All, "isHurt", false);
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

    private IEnumerator DeathRoutine()
    {
        yield return new WaitForSeconds(8);

        this.gameObject.SetActive(false);
    }

    #region SetAnim
    [PunRPC]
    private void SetRPCFloat(string _str, float _value)
    {
        animator.SetFloat(_str, _value);
    }

    [PunRPC]
    private void SetRPCBool(string _str, bool _value)
    {
        animator.SetBool(_str, _value);
    }
    [PunRPC]
    private void SetRPCTrg(string _str)
    {
        animator.SetTrigger(_str);
    }
    #endregion


    private void OnDrawGizmos()
    {
        /*
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
        */
    }


    #region Photon
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting) {
            stream.SendNext(rigid.constraints);
            stream.SendNext(stat.health);
        }
        else {
            rigid.constraints = (RigidbodyConstraints2D)stream.ReceiveNext();
            stat.health = (float)stream.ReceiveNext();
        }
    }
    #endregion
}


[System.Serializable]
public class EnemyStat
{
    [Header("Health stat")]
    public float maxHealth;
    public float health;

    [Header("Move stat")]
    public float moveSpeed;

    [Header("Attack stat")]
    public float attackPower;
    public float attackSpeed;
}

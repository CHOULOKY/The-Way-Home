using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.Image;
using UnityEngine.UIElements;
using Unity.VisualScripting;
using UnityEngine.Experimental.Rendering;
using Photon.Realtime;
using Photon.Pun;

public class Player : ObjFunc, IPunObservable
{
    #region Variables
    public enum PlayerCharacter { Girl, Robot };

    [Header("----------Attributes")]
    private Rigidbody2D rigid;
    private SpriteRenderer spriteRenderer;
    private Animator animator;

    [Header("----------Player State")]
    public PlayerCharacter character;
    public PlayerStat stat;
    public bool isHurt;
    public bool isDeath;

    [Header("----------Move")]
    private float inputX;
    private float wallDistance;
    public bool isWall;

    [Header("----------Jump")]
    private Transform groundPos; // Must position child's 1
    private RaycastHit2D trampleHit;
    public bool isGround;
    public bool isJump;
    public bool isGliding;
    public bool isDucking;

    [Header("----------Slope")]
    private Transform frontPos; // Must position child's 0
    private float angle;
    private Vector2 perp;
    public bool isSlope;

    [Header("----------Attack")]
    private float attackDistance;
    private Vector2 gAttackBox;
    private Vector2 aAttackBox;
    private float curAttackTimer;
    public bool isJAttack;
    public bool isChopping;

    [Header("----------Hit")]
    private int knockPower;
    private float knockTime;

    [Header("----------Effect")]
    public string jumpName;
    private ParticleSystem jumpEffect;
    public string hitName;
    private ParticleSystem hitEffect;
    public string ChoppingName;
    private ParticleSystem choppingEffect;

    [Header("----------Photon")]
    public PhotonView PV;
    private Vector3 curPos;
    #endregion


    void Awake()
    {
        // Attributes
        rigid = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        // Jump
        groundPos = transform.GetChild(1);
        
        // Slope
        frontPos = transform.GetChild(0);

        // Photon
        PV = GetComponent<PhotonView>();
    }

    void OnEnable()
    {
        // Player initialization
        switch (character) { // -1 means undecided
            case PlayerCharacter.Girl:
                // Health
                stat.maxHealth = 100;
                stat.health = stat.maxHealth;
                // Move
                stat.moveSpeed = 5;
                wallDistance = 0.3f;
                // Jump
                stat.jumpPower = 8;
                // Attack
                stat.attackPower = 10;
                stat.attackSpeed = 0.6f;
                // Attack Gizmos
                attackDistance = 0.3f;
                gAttackBox = new Vector2(1.4f, 1.6f);
                aAttackBox = new Vector2(2.0f, 2.2f);
                // Hit
                knockPower = 6;
                knockTime = 0.7f;
                break;

            case PlayerCharacter.Robot:
                // Health
                stat.maxHealth = 100;
                stat.health = stat.maxHealth;
                // Move
                stat.moveSpeed = 5;
                wallDistance = 0.2f;
                // Jump
                stat.jumpPower = 8f;
                // Attack
                stat.attackPower = 10;
                stat.attackSpeed = 0.6f;
                // Attack Gizmos
                attackDistance = 0.2f;
                gAttackBox = new Vector2(1.25f, 1.35f);
                // Hit
                knockPower = 6;
                knockTime = 0.7f;
                break;
        }
    }

    void Update()
    {
        #region Exception
        if (!PV.IsMine) {
            if ((transform.position - curPos).sqrMagnitude >= 100) transform.position = curPos;
            else transform.position = Vector3.Lerp(transform.position, curPos, Time.deltaTime * 30);
            return;
        }
        if (isHurt || isDeath) return;
        #endregion

        #region Check - Death
        isDeath = DeathChk(stat.health, isDeath);
        if(isDeath) {
            SetAnimBool(PV, animator, "isDeath", true);
            GameManager.Instance.isFail = true;
        }
        #endregion

        #region X-Axis
        if (!isDucking)
            inputX = Input.GetAxisRaw("Horizontal");
        #endregion

        #region Flip
        PV.RPC("ControlFlip", RpcTarget.AllBuffered, null, inputX, isSlope);
        #endregion

        #region Check - Ground, Slope, Wall
        isGround = GroundChk(groundPos.position, 0.1f, new string[] { "Ground", "Front Object" });

        if (!isDucking)
            (angle, perp, isSlope) = SlopeChk(rigid, groundPos.position, frontPos.position,
                new string[] { "Ground", "Front Object" });

        isWall = WallChk(rigid, transform.position, wallDistance, new string[] { "Ground", "Front Object", "Enemy" });
        #endregion

        #region Ducking
        if (isGround)
            Ducking(new KeyCode[] { KeyCode.S, KeyCode.DownArrow });
        #endregion

        #region Move
        if (isWall)
            inputX = 0; // Wall Stop
        if (curAttackTimer > Mathf.Epsilon)
            inputX = inputX * 0.4f; // Attack Slow
        if (character == PlayerCharacter.Robot && isChopping)
            inputX = 0; // Robot Chopping Stop

        Move(rigid, inputX, stat.moveSpeed, perp, isGround, isJump, isSlope, angle < 60);
        #endregion

        #region Jump
        Jump(rigid, stat.jumpPower, isGround, ref isJump, new KeyCode[] { KeyCode.W, KeyCode.UpArrow });
        Trampling();
        #endregion

        #region DownJump
        if (isGround)
            DownJump(this.gameObject, new KeyCode[] { KeyCode.S, KeyCode.DownArrow }, new KeyCode[] { KeyCode.Space }, 0.25f);
        #endregion

        #region Gliding, Chopping
        if (character == PlayerCharacter.Girl)
            Gliding();
        if (character == PlayerCharacter.Robot)
            ChoppingDown();
        #endregion

        #region Attack
        if (isGround) isJAttack = false;
        curAttackTimer = curAttackTimer > Mathf.Epsilon ? curAttackTimer - Time.deltaTime : 0;
        if (Input.GetButton("Fire1") && curAttackTimer <= Mathf.Epsilon)
            Attack();
        #endregion

        #region Animator Parameter
        SetAnimFloat(PV, animator, "xMove", Mathf.Abs(inputX));
        SetAnimFloat(PV, animator, "yMove", rigid.velocity.y);
        SetAnimBool(PV, animator, "isGround", isGround);
        SetAnimBool(PV, animator, "isJump", isJump);
        SetAnimBool(PV, animator, "isDucking", isDucking);
        SetAnimBool(PV, animator, "isHurt", isHurt);
        if (character == PlayerCharacter.Robot)
            SetAnimBool(PV, animator, "isChopping", isChopping);
        #endregion
    }


    [PunRPC]
    protected override void ControlFlip(Rigidbody2D _rigid, float _inputX, bool _isSlope)
    {
        // FlipX
        if (_inputX > 0)
            transform.eulerAngles = Vector3.zero;
        else if (_inputX < 0)
            transform.eulerAngles = new Vector3(0, 180, 0);

        // FlipZ (on the slope)
        if (_isSlope && _inputX == 0)
            rigid.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
        else
            rigid.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    private void Ducking(KeyCode[] _keys)
    {
        foreach (var _key in _keys) {
            if (Input.GetKey(_key)) {
                inputX = 0;
                isDucking = true;
                SetAnimTrg(PV, animator, "duckingTrigger");
            }
            else isDucking = false;
        }
    }

    private void Trampling()
    {
        trampleHit = Physics2D.Raycast(rigid.position + Vector2.down * 0.5f, Vector2.down, 0.1f,
            LayerMask.GetMask("Enemy"));
        // Debug.DrawRay(rigid.position + Vector2.down * 0.5f, Vector2.down * 0.25f, Color.white);

        if (trampleHit) {
            Enemy enemy = trampleHit.transform.GetComponent<Enemy>();
            if (enemy.isCollAtk == false && this.rigid.position.y > enemy.rigid.position.y) {
                this.rigid.velocity = new Vector2(rigid.velocity.x, 0);
                this.rigid.AddForce(Vector2.up * (stat.jumpPower * 0.7f), ForceMode2D.Impulse);
            }
        }
    }

    private void Gliding()
    {
        if ((isJump || !isGround)) {
            if (Input.GetKeyDown(KeyCode.Space)) {
                rigid.drag = 20;
                isGliding = true;
                SetAnimBool(PV, animator, "isGliding", true);
            }
            else if (Input.GetKey(KeyCode.Space)) {
                rigid.drag = 20;
                if (!isGliding) {
                    isGliding = true;
                    SetAnimBool(PV, animator, "isGliding", true);
                }
            }
            else if (Input.GetKeyUp(KeyCode.Space)) {
                rigid.drag = 0;
                isGliding = false;
                SetAnimBool(PV, animator, "isGliding", false);
            }
        }
        else if (isGliding || rigid.drag != 0) {
            rigid.drag = 0;
            isGliding = false;
            SetAnimBool(PV, animator, "isGliding", false);
        }
    }

    private void ChoppingDown()
    {
        if (!isChopping && !isGround && Input.GetKeyDown(KeyCode.Space)) {
            isChopping = true;
            SetAnimTrg(PV, animator, "choppingTrigger");

            rigid.velocity = Vector2.zero;
            rigid.gravityScale = 4.0f;
        }
        else if ((isGround || trampleHit) && rigid.gravityScale != 1.5f) {
            isChopping = false;
            rigid.gravityScale = 1.5f;
        }
    }

    private void Attack()
    {
        RaycastHit2D[] attackHits = null;
        switch (character) {
            case PlayerCharacter.Girl:
                if (isGround) {
                    SetAnimTrg(PV, animator, "gAttackTrigger");

                    rigid.velocity = Vector2.zero;
                    attackHits = Physics2D.BoxCastAll(rigid.position, gAttackBox, 0,
                        transform.rotation.eulerAngles.y == 180 ? Vector2.left : Vector2.right,
                        attackDistance, LayerMask.GetMask("Enemy", "Front Object"));
                }
                else if (!isJAttack) {
                    isJAttack = true;
                    SetAnimTrg(PV, animator, "aAttackTrigger");

                    rigid.velocity = Vector2.zero;
                    rigid.AddForce(Vector2.up * 3.5f, ForceMode2D.Impulse);
                    attackHits = Physics2D.BoxCastAll(rigid.position, aAttackBox, 0,
                        Vector2.zero, 0, LayerMask.GetMask("Enemy", "Front Object"));
                }
                curAttackTimer = stat.attackSpeed;
                break;
            case PlayerCharacter.Robot:
                if (isGround) {
                    SetAnimTrg(PV, animator, "gAttackTrigger");

                    rigid.velocity = Vector2.zero;
                    attackHits = Physics2D.BoxCastAll(rigid.position, gAttackBox, 0,
                        transform.rotation.eulerAngles.y == 180 ? Vector2.left : Vector2.right,
                        attackDistance, LayerMask.GetMask("Enemy", "Front Object"));
                }
                else if (!isJAttack) {
                    isJAttack = true;
                    SetAnimTrg(PV, animator, "gAttackTrigger");

                    rigid.velocity = Vector2.zero;
                    rigid.AddForce(Vector2.up * 3.5f, ForceMode2D.Impulse);
                    attackHits = Physics2D.BoxCastAll(rigid.position, gAttackBox, 0,
                        transform.rotation.eulerAngles.y == 180 ? Vector2.left : Vector2.right,
                        attackDistance, LayerMask.GetMask("Enemy", "Front Object"));
                }
                curAttackTimer = stat.attackSpeed;
                break;
        }

        if (attackHits == null) return;
        foreach (var enemy in attackHits) {
            // Debug.Log(LayerMask.LayerToName(enemy.collider.gameObject.layer)); // Check
            switch (LayerMask.LayerToName(enemy.collider.gameObject.layer)) {
                case "Front Object":
                    AttackObject(enemy);
                    break;
                case "Enemy":
                    AttackEnemy(enemy);
                    break;
            }
        }
    }
    private void AttackObject(RaycastHit2D enemy)
    {
        // Debug.Log(enemy.collider.gameObject.name); // Check

        if (enemy.transform.GetComponent<MoveObject>())
            enemy.transform.GetComponent<MoveObject>().Hitted(this);
    }
    private void AttackEnemy(RaycastHit2D enemy)
    {
        // Debug.Log(enemy.collider.gameObject.name); // Check

        if (enemy.transform.GetComponent<Enemy>())
            enemy.transform.GetComponent<Enemy>().Hitted(this);
    }

    public void Hitted(Enemy _enemy)
    {
        #region Exception
        if (isHurt || isDeath) return;
        else if (!PV.IsMine) return;
        #endregion

        StartCoroutine(HittedRoutine(_enemy));
    }
    public IEnumerator HittedRoutine(Enemy _enemy)
    {
        #region Hit Effect
        PV.RPC("PlayHittedEffect", RpcTarget.All);
        #endregion

        isHurt = true;
        SetAnimBool(PV, animator, "isHurt", true);
        SetAnimTrg(PV, animator, "hurtTrigger");
        if (character == PlayerCharacter.Robot) {
            isChopping = false;
            rigid.gravityScale = 1.5f;
        }

        rigid.constraints = RigidbodyConstraints2D.FreezeRotation;
        Vector2 knockDir = (this.rigid.position - _enemy.rigid.position);
        rigid.velocity = Vector2.zero;
        rigid.AddForce(knockDir * knockPower, ForceMode2D.Impulse);

        yield return new WaitForSeconds(knockTime);
        
        isHurt = false;
        SetAnimBool(PV, animator, "isHurt", false);
        stat.health -= _enemy.stat.attackPower;
    }
    [PunRPC]
    public void PlayHittedEffect()
    {
        if (!hitEffect)
            hitEffect = PhotonNetwork.Instantiate(hitName, transform.position, Quaternion.identity).GetComponent<ParticleSystem>();
        hitEffect.transform.position = transform.position;
        hitEffect.transform.localScale = new Vector2(Random.Range(0.4f, 1f), Random.Range(0.4f, 1f));
        hitEffect.gameObject.SetActive(true);
        hitEffect.Play();
    }


    private void OnDrawGizmos()
    {
        // Check Wall
        /*
        Gizmos.color = Color.red;
        if (transform.rotation.eulerAngles.y == 180)
            Gizmos.DrawWireCube(rigid.position + new Vector2(-1, -1.2f) * wallDistance, wallBox);
        else
            Gizmos.DrawWireCube(rigid.position + new Vector2(1, -1.2f) * wallDistance, wallBox);
        */

        // Check Jump
        /*
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(rigid.position + Vector2.down * ground_rayLength, ground_boxSize);
        */

        // Check Down
        /*
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(groundPos.position, groundBox);
        */

        // Check Attack
        /*
        Gizmos.color = Color.red;
        if (transform.rotation.eulerAngles.y == 180)
            Gizmos.DrawWireCube(rigid.position + Vector2.left * attackDistance, gAttackBox);
        else
            Gizmos.DrawWireCube(rigid.position + Vector2.right * attackDistance, gAttackBox);
        Gizmos.DrawWireCube(rigid.position, aAttackBox);
        Gizmos.DrawWireCube(rigid.position + Vector2.down * attackDistance, Vector2.one);
        */
    }


    #region Photon
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting) {
            stream.SendNext(transform.position);
            stream.SendNext(stat.health);
        }
        else {
            curPos = (Vector3)stream.ReceiveNext();
            stat.health = (int)stream.ReceiveNext();
        }
    }
    #endregion
}


[System.Serializable]
public class PlayerStat
{
    [Header("Health stat")]
    public int maxHealth;
    public int health;

    [Header("Move stat")]
    public int moveSpeed;

    [Header("Jump stat")]
    public float jumpPower;

    [Header("Attack stat")]
    public int attackPower;
    [Tooltip("Second basis")] public float attackSpeed;
}


/*

 * 수정해야 할 사항들
 - 

 */

/*

 * 경사면 참고 링크
 - https://www.youtube.com/watch?v=A6IkXiP_ing
 - https://daekyoulibrary.tistory.com/entry/Charon-3-%EA%B2%BD%EC%82%AC%EB%A1%9CSlope-%EC%A7%80%ED%98%95-%EC%9D%B4%EB%8F%99-%EA%B5%AC%ED%98%84%ED%95%98%EA%B8%B0

*/

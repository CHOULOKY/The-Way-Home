using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.Image;
using UnityEngine.UIElements;
using Unity.VisualScripting;
using UnityEngine.Experimental.Rendering;
using Photon.Realtime;
using Photon.Pun;

public class Player : MonoBehaviour, IPunObservable
{
    #region Variables
    public enum PlayerCharacter { Girl, Robot };

    [Header("----------Attributes")]
    public Rigidbody2D rigid;
    private SpriteRenderer spriteRenderer;
    public Animator animator;

    [Header("----------Player State")]
    public PlayerCharacter character;
    public PlayerStat stat; // Overall
    public bool isHurt; // Hurt
    public bool isDeath; // Death

    [Header("----------Move")]
    private float inputX;
    private float wallDistance;
    private Vector2 wallBox;
    private RaycastHit2D wallHit;

    [Header("----------Jump")]
    private Transform groundPos; // Must position child's 0
    private float groundRadius;
    private RaycastHit2D groundHit;
    public bool isGround;
    public bool isJump;
    public bool isGliding;

    [Header("----------Slope")]
    private Transform frontPos; // Must position child's 1
    private float slopeDistance;
    private RaycastHit2D slopeHit;
    private RaycastHit2D frontHit;
    private float maxAngle;
    private float angle;
    private Vector2 perp;
    public bool isSlope;

    [Header("----------Attack")]
    private float attackDistance;
    private Vector2 gAttackBox;
    private Vector2 aAttackBox;
    private RaycastHit2D[] enemyHits;
    private float curAttackTimer;
    private bool isJAttack;

    [Header("----------Hit")]
    public int knockPower;
    private float knockTime;

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
        groundPos = transform.GetChild(0);
        
        // Slope
        frontPos = transform.GetChild(1);

        // Photon
        PV = GetComponent<PhotonView>();
    }

    void OnEnable()
    {
        // Attributes
        rigid.constraints = RigidbodyConstraints2D.FreezeRotation;

        // Player initialization
        switch (character) { // -1 means undecided
            case PlayerCharacter.Girl:
                // Health
                stat.maxHealth = 100;
                stat.health = stat.maxHealth;
                // Move
                stat.moveSpeed = 5;
                wallDistance = 0.3f;
                wallBox = new Vector2(0.05f, 0.6f);
                // Slope
                slopeDistance = 1;
                maxAngle = 60;
                // Jump
                stat.jumpPower = 8;
                groundRadius = 0.1f;
                // Attack
                stat.attackPower = 10;
                stat.attackSpeed = 0.6f;
                // Attack Gizmos
                attackDistance = 0.3f;
                gAttackBox = new Vector2(1.4f, 1.6f);
                aAttackBox = new Vector2(2.0f, 2.2f);
                // Hit
                knockPower = 7;
                knockTime = 0.7f;
                break;

            case PlayerCharacter.Robot:
                // Health
                stat.maxHealth = 100;
                stat.health = stat.maxHealth;
                // Move
                stat.moveSpeed = 5;
                wallDistance = 0.2f;
                wallBox = new Vector2(0.05f, 0.35f);
                // Slope
                slopeDistance = 1;
                maxAngle = 60;
                // Jump
                stat.jumpPower = 6.5f;
                groundRadius = 0.1f;
                // Attack
                stat.attackPower = 10;
                stat.attackSpeed = 0.6f;
                // Attack Gizmos
                attackDistance = 0.2f;
                gAttackBox = new Vector2(1.2f, 1.3f);
                // Hit
                knockPower = 7;
                knockTime = 0.7f;
                break;
        }
    }

    void Update()
    {
        #region Check - Death
        DeathChk();
        #endregion

        #region Exception
        if (isHurt || isDeath) return;
        if (!PV.IsMine) {
            if ((transform.position - curPos).sqrMagnitude >= 100) transform.position = curPos;
            else transform.position = Vector3.Lerp(transform.position, curPos, Time.deltaTime * 30);
            return;
        }
        #endregion

        #region X-Axis
        inputX = Input.GetAxisRaw("Horizontal");
        #endregion

        #region Flip
        PV.RPC("ControlFlip", RpcTarget.AllBuffered, inputX, isSlope);
        // ControlFlip();
        #endregion

        #region Check - Ground
        GroundChk();
        #endregion

        #region Check - Slope
        slopeHit = Physics2D.Raycast(groundPos.position, Vector2.down, slopeDistance,
            LayerMask.GetMask("Ground", "Front Object"));
        frontHit = Physics2D.Raycast(frontPos.position, transform.right, 0.1f,
            LayerMask.GetMask("Ground", "Front Object"));

        if (slopeHit || frontHit) {
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

        #region Girl's Ducking
        if (character == PlayerCharacter.Girl)
            if (isGround)
                Ducking();
        #endregion

        #region Move
        switch (character) {
            case PlayerCharacter.Girl:
                if (transform.rotation.eulerAngles.y == 180)
                    wallHit = Physics2D.BoxCast(rigid.position, wallBox, 0,
                        new Vector2(-1, -0.4f), wallDistance, LayerMask.GetMask("Ground", "Front Object"));
                else
                    wallHit = Physics2D.BoxCast(rigid.position, wallBox, 0,
                        new Vector2(1, -0.4f), wallDistance, LayerMask.GetMask("Ground", "Front Object"));
                break;
            case PlayerCharacter.Robot:
                if (transform.rotation.eulerAngles.y == 180)
                    wallHit = Physics2D.BoxCast(rigid.position, wallBox, 0,
                        new Vector2(-1, -1.2f), wallDistance, LayerMask.GetMask("Ground", "Front Object"));
                else
                    wallHit = Physics2D.BoxCast(rigid.position, wallBox, 0,
                        new Vector2(1, -1.2f), wallDistance, LayerMask.GetMask("Ground", "Front Object"));
                break;
        }

        if (wallHit) {
            if (LayerMask.LayerToName(wallHit.collider.gameObject.layer) == "Ground" ||
                !wallHit.transform.GetComponent<PlatformEffector2D>())
                inputX = 0;
        }
        else if (curAttackTimer > Mathf.Epsilon) {
            inputX = inputX * 0.4f;
            if (character == PlayerCharacter.Robot && (isJump || !isGround)) inputX = inputX * 2.5f;
        }

        Move();
        #endregion

        #region Jump
        Jump();
        #endregion

        #region Gliding
        if (character == PlayerCharacter.Girl)
            Gliding();
        #endregion

        #region Attack
        curAttackTimer = curAttackTimer > Mathf.Epsilon ? curAttackTimer - Time.deltaTime : 0;
        if (isGround) isJAttack = false;
        if (Input.GetButton("Fire1") && curAttackTimer <= Mathf.Epsilon)
            Attack();
        #endregion

        #region Animator Parameter
        animator.SetFloat("xMove", Mathf.Abs(inputX));
        animator.SetFloat("yMove", rigid.velocity.y);
        animator.SetBool("isGround", isGround);
        animator.SetBool("isJump", isJump);
        #endregion
    }

    void FixedUpdate()
    {
        if (isHurt || isDeath) return;

    }


    [PunRPC]
    private void ControlFlip(float _inputX, bool _isSlope)
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
            GameManager.Instance.isFail = true;
        }
    }

    private void Ducking()
    {
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) {
            animator.SetTrigger("duckingTrigger");
            inputX = 0;
        }
    }

    private void Move()
    {
        // Translate Move
        if (inputX != 0) {
            if (isSlope && isGround && !isJump && angle < maxAngle) {
                rigid.velocity = Vector2.zero;
                if (inputX > 0)
                    transform.Translate(new Vector2(Mathf.Abs(inputX) * -perp.x * stat.moveSpeed * Time.deltaTime,
                        Mathf.Abs(inputX) * -perp.y * stat.moveSpeed * Time.deltaTime));
                else if (inputX < 0)
                    transform.Translate(new Vector2(Mathf.Abs(inputX)  * - perp.x * stat.moveSpeed * Time.deltaTime,
                        Mathf.Abs(inputX) * perp.y * stat.moveSpeed * Time.deltaTime));
            }
            else
                transform.Translate(Mathf.Abs(inputX) * Vector2.right * stat.moveSpeed * Time.deltaTime);
        }
    }

    private void Jump()
    {
        if (rigid.velocity.y <= 0) isJump = false;

        if (isGround && !isJump)
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) {
                isJump = true;
                rigid.velocity = new Vector2(rigid.velocity.x, 0);
                rigid.AddForce(Vector2.up * stat.jumpPower, ForceMode2D.Impulse);
            }
    }

    private void Gliding()
    {
        if (isJump || !isGround) {
            if (Input.GetKeyDown(KeyCode.Space)) {
                isGliding = true;
                animator.SetBool("isGliding", true);
            }
            else if (Input.GetKey(KeyCode.Space)) {
                rigid.drag = 20;
                if (!isGliding) {
                    isGliding = true;
                    animator.SetBool("isGliding", true);
                }
            }
            else if (Input.GetKeyUp(KeyCode.Space)) {
                rigid.drag = 0;
                isGliding = false;
                animator.SetBool("isGliding", false);
            }
        }
        else if (isGliding || rigid.drag != 0) {
            rigid.drag = 0;
            isGliding = false;
            animator.SetBool("isGliding", false);
        }
    }

    private void Attack()
    {
        curAttackTimer = stat.attackSpeed;
        switch(character) {
            case PlayerCharacter.Girl:
                if (isGround) {
                    animator.SetTrigger("gAttackTrigger");

                    rigid.velocity = Vector2.zero;
                    if (transform.rotation.eulerAngles.y == 180)
                        enemyHits = Physics2D.BoxCastAll(rigid.position, gAttackBox, 0,
                            Vector2.left, attackDistance, LayerMask.GetMask("Enemy", "Front Object"));
                    else
                        enemyHits = Physics2D.BoxCastAll(rigid.position, gAttackBox, 0,
                            Vector2.right, attackDistance, LayerMask.GetMask("Enemy", "Front Object"));
                }
                else if (!isJAttack) {
                    isJAttack = true;
                    animator.SetTrigger("aAttackTrigger");

                    rigid.velocity = Vector2.zero;
                    rigid.AddForce(Vector2.up * 3.5f, ForceMode2D.Impulse);
                    enemyHits = Physics2D.BoxCastAll(rigid.position, aAttackBox, 0,
                        Vector2.zero, 0, LayerMask.GetMask("Enemy", "Front Object"));
                }
                break;
            case PlayerCharacter.Robot:
                if (isJump || !isGround) break;

                animator.SetTrigger("gAttackTrigger");

                rigid.velocity = Vector2.zero;
                if (transform.rotation.eulerAngles.y == 180)
                    enemyHits = Physics2D.BoxCastAll(rigid.position, gAttackBox, 0,
                        Vector2.left, attackDistance, LayerMask.GetMask("Enemy", "Front Object"));
                else
                    enemyHits = Physics2D.BoxCastAll(rigid.position, gAttackBox, 0,
                        Vector2.right, attackDistance, LayerMask.GetMask("Enemy", "Front Object"));
                break;
        }
        
        foreach (var enemy in enemyHits) {
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

    public IEnumerator HurtRoutine()
    {
        yield return new WaitForSeconds(knockTime);

        isHurt = false;
        animator.SetBool("isHurt", false);
    }


    private bool IsAnimationPlaying(string animName)
    {
        if (animator.GetCurrentAnimatorStateInfo(0).IsName(animName))
            return true;
        else
            return false;
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

        // Check jump
        /*
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(rigid.position + Vector2.down * ground_rayLength, ground_boxSize);
        */

        // Check Attack
        /*
        Gizmos.color = Color.red;
        if (transform.rotation.eulerAngles.y == 180)
            Gizmos.DrawWireCube(rigid.position + Vector2.left * attackDistance, gAttackBox);
        else
            Gizmos.DrawWireCube(rigid.position + Vector2.right * attackDistance, gAttackBox);
        Gizmos.DrawWireCube(rigid.position, aAttackBox);
        */
    }


    #region Photon
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting) {
            stream.SendNext(transform.position);
        }
        else {
            curPos = (Vector3)stream.ReceiveNext();
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

 * �����ؾ� �� ���׵�
 - 

 */

/*

 * ���� ���� ��ũ
 - https://www.youtube.com/watch?v=A6IkXiP_ing
 - https://daekyoulibrary.tistory.com/entry/Charon-3-%EA%B2%BD%EC%82%AC%EB%A1%9CSlope-%EC%A7%80%ED%98%95-%EC%9D%B4%EB%8F%99-%EA%B5%AC%ED%98%84%ED%95%98%EA%B8%B0

*/
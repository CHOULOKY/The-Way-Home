using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.VisualScripting;
using TMPro;

public class Robot : Player, IPunObservable
{
    #region Variables
    [Header("Component")]
    private Rigidbody2D rigid;
    private PhotonView PV;

    [Header("Boolean")]
    private bool isGround;
    private bool isJump;
    private bool isWall;
    private bool isDuck;
    private bool isChop;
    private bool isJumpAttack;
    private bool isHurt;
    private bool isDeath;

    [Header("Move")]
    private float inputX;
    private Vector3 curPos;

    [Header("Jump")]
    private Transform groundPos;
    private RaycastHit2D trampleHit;
    private RaycastHit2D chopTrampleHit;

    [Header("Wall")]
    public float wallDistance;

    [Header("Attack")]
    public float attackDistance;
    public Vector2 gAttackBox;
    private float curAttackTime;
    
    [Header("Hurt")]
    public int knockPower;
    public float knockTime;

    [Header("Effect")]
    public string jumpName;
    private ParticleSystem jumpEffect;
    public string hurtName;
    private ParticleSystem hurtEffect;
    public string ChoppingName;
    private ParticleSystem choppingEffect;

    [Header("InGame UI")]
    public Image healthbar;
    public TMP_Text nicknameText;
    private Canvas symbolCanvas;
    #endregion


    private void Awake()
    {
        // Component
        rigid = GetComponent<Rigidbody2D>();
        PV = GetComponent<PhotonView>();

        // Jump
        groundPos = transform.GetChild(0);

        // InGame UI
        string nickname = string.IsNullOrEmpty(PhotonNetwork.LocalPlayer.NickName) ? "Robot" : PV.Owner.NickName;

        nicknameText.text = nickname;
        nicknameText.color = PV.IsMine ? Color.yellow : Color.cyan;

        if (PV.IsMine) {
            symbolCanvas = transform.Find("SymbolCanvas").GetComponent<Canvas>();
            symbolCanvas.gameObject.SetActive(true);
        }
    }

    private void Start()
    {
        // Status
        status.health = status.maxHealth;
    }

    private void Update()
    {
        if (!PV.IsMine) {
            if ((transform.position - curPos).sqrMagnitude >= 100) transform.position = curPos;
            else transform.position = Vector2.Lerp(transform.position, curPos, Time.deltaTime * 10);
            return;
        }
        else if (isHurt || isDeath) return;

        // Check
        if (isDeath = DeathCheck()) Death();
        isGround = GroundCheck(groundPos.position, 0.1f, new string[] { "Ground", "Object", "Platform" });
        isWall = WallCheck(transform.position, wallDistance, new string[] { "Ground", "Object", "Monster" });

        // Flip
        ControlFlip(inputX);

        // Duck
        if (isGround) Duck(new KeyCode[] { KeyCode.S, KeyCode.DownArrow });

        // Move
        inputX = Input.GetAxisRaw("Horizontal");
        Move(inputX, status.moveSpeed);

        // Jump
        Jump(status.jumpPower, new KeyCode[] { KeyCode.W, KeyCode.UpArrow });

        // Trample
        Trample(status.jumpPower * 0.7f, new string[] { "Monster" }, new string[] { "Platform" });

        // Chop
        ChopDown(new KeyCode[] { KeyCode.Space });

        // Attack
        Attack("Fire1");

        // Animator
        if (!isDuck) {
            PV.RPC("SetAnimBool", RpcTarget.All, "isGround", isGround);
            PV.RPC("SetAnimFloat", RpcTarget.All, "xMove", Mathf.Abs(inputX));
        }
        PV.RPC("SetAnimFloat", RpcTarget.All, "yMove", rigid.velocity.y);
    }

    private bool GroundCheck(Vector2 _pos, float _radius, string[] _layers)
    {
        return Physics2D.OverlapCircle(_pos, _radius, LayerMask.GetMask(_layers));
    }

    private bool WallCheck(Vector2 _pos, float _distance, string[] _layers)
    {
        return Physics2D.Raycast(_pos, rigid.transform.rotation.eulerAngles.y == 180 ? Vector2.left : Vector2.right, _distance, LayerMask.GetMask(_layers));
    }

    private void ControlFlip(float _inputX)
    {
        // FlipX
        if (_inputX > 0)
            transform.eulerAngles = Vector3.zero;
        else if (_inputX < 0)
            transform.eulerAngles = new Vector3(0, 180, 0);
    }

    private void Duck(KeyCode[] _keys)
    {
        foreach (var _key in _keys) {
            if (Input.GetKey(_key)) {
                isDuck = true;
                PV.RPC("SetAnimBool", RpcTarget.All, "isDuck", true);
                PV.RPC("SetAnimTrg", RpcTarget.All, "duckTrigger");
                break;
            }
            else {
                isDuck = false;
                PV.RPC("SetAnimBool", RpcTarget.All, "isDuck", false);
            }
        }
    }

    private void Move(float _input, float _speed)
    {
        if (isWall || isDuck || isChop) _input = 0;
        if (curAttackTime > Mathf.Epsilon) _input = _input * 0.4f;

        // Translate Move
        if (_input != 0) transform.Translate(Mathf.Abs(_input) * Vector2.right * _speed * Time.deltaTime);
    }

    private void Jump(float _jumpPower, KeyCode[] _keys)
    {
        if (rigid.velocity.y <= 0) {
            isJump = false;
            PV.RPC("SetAnimBool", RpcTarget.All, "isJump", false);
        }
        if (isGround && !isJump) {
            foreach (KeyCode _key in _keys) {
                if (Input.GetKeyDown(_key)) {
                    isJump = true;
                    rigid.velocity = new Vector2(rigid.velocity.x, 0);
                    rigid.AddForce(Vector2.up * _jumpPower, ForceMode2D.Impulse);
                    PV.RPC("SetAnimBool", RpcTarget.All, "isJump", true);
                }
            }
        }
    }

    private bool isTrampleAttack, isChopDestroy;
    private void Trample(float _tramplePower, string[] _layers, string[] _chopLayers)
    {
        trampleHit = Physics2D.Raycast(rigid.position + Vector2.down * 0.5f, Vector2.down, 0.1f, LayerMask.GetMask(_layers));
        if (isChop || chopTrampleHit) chopTrampleHit = Physics2D.Raycast(rigid.position + Vector2.down * 0.5f, Vector2.down, 0.25f, LayerMask.GetMask(_chopLayers));
        // Debug.DrawRay(rigid.position + Vector2.down * 0.5f, Vector2.down * 0.25f, Color.white);

        if (trampleHit && this.rigid.position.y > trampleHit.collider.GetComponent<Rigidbody2D>().position.y) {
            if (trampleHit.collider.GetComponent<Monster>() && !trampleHit.collider.GetComponent<Trap>()) {
                if (!isTrampleAttack) {
                    if (isChop)
                        trampleHit.collider.GetComponent<Monster>().HurtByPlayer(this.gameObject, 10);
                    else
                        trampleHit.collider.GetComponent<Monster>().HurtByPlayer(this.gameObject, 5);
                    isTrampleAttack = true;
                    Invoke("TrampleAttackRoutine", 0.05f);
                }
                this.rigid.velocity = new Vector2(rigid.velocity.x, 0);
                this.rigid.AddForce(Vector2.up * _tramplePower, ForceMode2D.Impulse);
            }
        }
        if (chopTrampleHit && this.rigid.position.y > chopTrampleHit.collider.GetComponent<Rigidbody2D>().position.y) {
            if (chopTrampleHit.collider.GetComponent<Object>() && chopTrampleHit.collider.GetComponent<Object>().isDestroyObj) {
                if (!isChopDestroy) {
                    chopTrampleHit.collider.GetComponent<Object>().HurtByPlayer(this.gameObject);
                    isChopDestroy = true;
                    Invoke("ChopDestroyRoutine", 0.3f);
                }
                this.rigid.velocity = new Vector2(rigid.velocity.x, 0);
                this.rigid.AddForce(Vector2.up * _tramplePower * 1.5f, ForceMode2D.Impulse);
            }
        }
    }
    private void TrampleAttackRoutine()
    {
        isTrampleAttack = false;
    }
    private void ChopDestroyRoutine()
    {
        isChopDestroy = false;
    }

    private void ChopDown(KeyCode[] _keys)
    {
        if (!isChop && !isGround) {
            foreach (KeyCode _key in _keys) {
                if (Input.GetKeyDown(_key)) {
                    isChop = true;
                    rigid.velocity = Vector2.zero;
                    rigid.gravityScale = 4.0f;
                    PV.RPC("SetAnimBool", RpcTarget.All, "isChop", true);
                    PV.RPC("SetAnimTrg", RpcTarget.All, "chopTrigger");
                }
            }
        }
        else if ((isGround || trampleHit || chopTrampleHit) && rigid.gravityScale != 1.5f) {
            isChop = false;
            rigid.gravityScale = 1.5f;
            PV.RPC("SetAnimBool", RpcTarget.All, "isChop", false);
        }
    }

    private void Attack(string _button)
    {
        curAttackTime = curAttackTime > 0 ? curAttackTime - Time.deltaTime : 0;
        if (curAttackTime > 0) return;
        if (isGround) isJumpAttack = false;

        if (Input.GetButtonDown(_button)) {
            RaycastHit2D[] attackHits = null;
            if (isGround) {
                PV.RPC("SetAnimTrg", RpcTarget.All, "gAttackTrigger");

                rigid.velocity = Vector2.zero;
                attackHits = Physics2D.BoxCastAll(rigid.position, gAttackBox, 0,
                    transform.rotation.eulerAngles.y == 180 ? Vector2.left : Vector2.right,
                    attackDistance, LayerMask.GetMask("Monster", "Object", "Trap"));
            }
            else if (!isJumpAttack) {
                isJumpAttack = true;
                PV.RPC("SetAnimTrg", RpcTarget.All, "gAttackTrigger");

                rigid.velocity = Vector2.zero;
                rigid.AddForce(Vector2.up * 3.5f, ForceMode2D.Impulse);
                attackHits = Physics2D.BoxCastAll(rigid.position, gAttackBox, 0,
                    transform.rotation.eulerAngles.y == 180 ? Vector2.left : Vector2.right,
                    attackDistance, LayerMask.GetMask("Monster", "Object", "Trap"));
            }
            curAttackTime = status.attackSpeed;

            if (attackHits == null) return;
            foreach (RaycastHit2D monster in attackHits) {
                // Debug.Log(LayerMask.LayerToName(monster.collider.gameObject.layer)); // Check
                switch (LayerMask.LayerToName(monster.collider.gameObject.layer)) {
                    case "Monster":
                        if (monster.transform.GetComponent<Monster>())
                            monster.transform.GetComponent<Monster>().HurtByPlayer(this.gameObject, status.attackPower);
                        break;
                    case "Object":
                        if (monster.transform.GetComponent<Object>() && !monster.transform.GetComponent<Object>().isDestroyObj)
                            monster.transform.GetComponent<Object>().HurtByPlayer(this.gameObject);
                        break;
                    case "Trap":
                        if (monster.transform.GetComponent<Trap>()) {
                            if (monster.transform.GetComponent<Trap>().isHurtTrap)
                                monster.transform.GetComponent<Trap>().HurtByPlayer(this.gameObject);
                        }
                        break;
                }
            }
        }
    }

    public override void HurtByMonster(GameObject _monster, float _attackPower)
    {
        if (isHurt || isDeath) return;

        StartCoroutine(HurtRoutine(_monster, _attackPower));
    }
    private IEnumerator HurtRoutine(GameObject _monster, float _attackPower)
    {
        PV.RPC("PlayHurtEffect", RpcTarget.All);

        isChop = false;
        rigid.gravityScale = 1.5f;
        PV.RPC("SetAnimBool", RpcTarget.All, "isChop", false);
        isHurt = true;
        PV.RPC("SetAnimBool", RpcTarget.All, "isHurt", true);
        PV.RPC("SetAnimTrg", RpcTarget.All, "hurtTrigger");

        rigid.constraints = RigidbodyConstraints2D.FreezeRotation;
        Vector2 knockDir = (this.rigid.position - _monster.GetComponent<Rigidbody2D>().position);
        rigid.velocity = Vector2.zero;
        rigid.AddForce(knockDir * knockPower, ForceMode2D.Impulse);

        status.health -= _attackPower;
        healthbar.fillAmount -= _attackPower * 0.01f;

        yield return new WaitForSeconds(knockTime);

        isHurt = false;
        PV.RPC("SetAnimBool", RpcTarget.All, "isHurt", false);
    }
    [PunRPC]
    private void PlayHurtEffect()
    {
        if (!hurtEffect)
            hurtEffect = PhotonNetwork.Instantiate(hurtName, transform.position, Quaternion.identity).GetComponent<ParticleSystem>();
        hurtEffect.transform.position = transform.position;
        float effectSize = this.transform.localScale.x;
        hurtEffect.transform.localScale =
            new Vector2(Random.Range(effectSize * 0.4f, effectSize), Random.Range(effectSize * 0.4f, effectSize));
        hurtEffect.gameObject.SetActive(true);
        hurtEffect.Play();
    }

    private bool DeathCheck()
    {
        return status.health <= 0;
    }

    private void Death()
    {
        PV.RPC("SetAnimBool", RpcTarget.All, "isDeath", true);
        GameManager.Instance.isFail = true;
    }

    #region Photon
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(status.health);
            stream.SendNext(healthbar.fillAmount);
        }
        else
        {
            curPos = (Vector3)stream.ReceiveNext();
            status.health = (float)stream.ReceiveNext();
            healthbar.fillAmount = (float)stream.ReceiveNext();
        }
    }
    #endregion
}

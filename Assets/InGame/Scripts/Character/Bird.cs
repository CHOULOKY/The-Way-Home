using BirdStates;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bird : Monster, IPunObservable
{
    private enum States { Idle, Move, Attack, Hurt, Death }
    [Header("FSM")]
    private States curState;
    private FSM<Bird> fsm;

    [Header("Component")]
    private PhotonView PV;

    [Header("Move")]
    private Transform defaultPosition;

    [Header("Search")]
    public float searchDistance;
    public Vector2 searchBox;

    [Header("Attack")]
    public float attackDistance;
    public Vector2 attackBox;

    [Header("Hurt")]
    public float knockPower;
    public string effectName;
    private ParticleSystem hurtEffect;
    [HideInInspector] public GameObject player;
    [HideInInspector] public float playerPower;


    private void Awake()
    {
        // Component
        PV = GetComponent<PhotonView>();
    }

    private void Start()
    {
        // Status
        status.health = status.maxHealth;

        // FSM
        curState = States.Idle;
        fsm = new FSM<Bird>(new IdleState(this));

        // Move
        defaultPosition = transform.GetChild(0);
        defaultPosition.parent = null;
        defaultPosition.position = this.transform.position;
    }

    private void Update()
    {
        if (!PhotonNetwork.IsConnected || !PhotonNetwork.InRoom || !PV.IsMine) return;
        
        switch (curState) {
            case States.Idle:
                if (CanSeePlayer()) {
                    if (CanAttackPlayer())
                        ChangeState(States.Attack);
                    else
                        ChangeState(States.Move);
                }
                break;
            case States.Move:
                if (CanSeePlayer()) {
                    if (CanAttackPlayer())
                        ChangeState(States.Attack);
                }
                break;
            case States.Attack:
                if (!CanAttackPlayer()) {
                    ChangeState(States.Move);
                }
                break;
            case States.Hurt:
                if (DeathCheck())
                    ChangeState(States.Death);
                else if (CanSeePlayer()) {
                    if (CanAttackPlayer())
                        ChangeState(States.Attack);
                    else
                        ChangeState(States.Move);
                }
                break;
            case States.Death:
                // 
                break;
        }
        fsm.UpdateState();
    }

    private void ChangeState(States _nextState)
    {
        curState = _nextState;
        switch (curState) {
            case States.Idle:
                fsm.ChangeState(new IdleState(this));
                break;
            case States.Move:
                fsm.ChangeState(new MoveState(this));
                break;
            case States.Attack:
                fsm.ChangeState(new AttackState(this));
                break;
            case States.Hurt:
                fsm.ChangeState(new HurtState(this));
                break;
            case States.Death:
                fsm.ChangeState(new DeathState(this));
                break;
        }
    }

    private bool CheckMoveBorder()
    {
        return ((Vector2)defaultPosition.position - (Vector2)this.transform.position).sqrMagnitude >= 169;
    }

    private bool CanSeePlayer()
    {
        return Physics2D.BoxCast((Vector2)transform.position, searchBox, 0,
            transform.rotation.eulerAngles.y == 180 ? Vector2.left : Vector2.right,
            searchDistance, LayerMask.GetMask("Player"));
    }

    private bool CanAttackPlayer()
    {
        return Physics2D.BoxCast((Vector2)transform.position, attackBox, 0,
            transform.rotation.eulerAngles.y == 180 ? Vector2.left : Vector2.right,
            attackDistance, LayerMask.GetMask("Player"));
    }

    public override void HurtByPlayer(GameObject _player, float _attackPower)
    {
        if (curState == States.Death) return;
        if (!PV.IsMine) PV.RequestOwnership();
        player = _player;
        playerPower = _attackPower;
        ChangeState(States.Hurt);
    }

    private bool DeathCheck()
    {
        return status.health <= 0;
    }

    #region PunRPC
    public void RPCAttackPlayer()
    {
        PV.RPC("AttackPlayer", RpcTarget.All);
    }
    [PunRPC]
    private void AttackPlayer()
    {
        // ! anim
        // Debug.Log("Player Search in Attack Box!");

        RaycastHit2D[] _attackHits = Physics2D.BoxCastAll((Vector2)this.transform.position, attackBox, 0,
            this.transform.rotation.eulerAngles.y == 180 ? Vector2.left : Vector2.right,
            attackDistance, LayerMask.GetMask("Player"));

        if (_attackHits != null) {
            foreach (var hit in _attackHits) {
                PhotonView targetView = PhotonView.Find(hit.collider.gameObject.GetComponent<PhotonView>().ViewID);
                if (targetView != null) {
                    targetView.gameObject.GetComponent<Player>().HurtByMonster(this.gameObject, status.attackPower);
                }
                // Debug.Log("Attack in Attack Box!");
            }
        }
        RPCAnimTrg("attackTrg");
    }

    public void RPCHurtEffect()
    {
        PV.RPC("PlayHurtEffect", RpcTarget.All);
    }
    [PunRPC]
    private void PlayHurtEffect()
    {
        if (!hurtEffect)
            hurtEffect = PhotonNetwork.Instantiate(effectName, this.transform.position, Quaternion.identity).GetComponent<ParticleSystem>();
        hurtEffect.transform.position = this.transform.position;
        float effectSize = this.transform.localScale.x;
        hurtEffect.transform.localScale =
            new Vector2(UnityEngine.Random.Range(effectSize * 0.4f, effectSize), UnityEngine.Random.Range(effectSize * 0.4f, effectSize));
        hurtEffect.gameObject.SetActive(true);
        hurtEffect.Play();
    }

    public void RPCAnimFloat(string _str, float _value)
    {
        PV.RPC("SetAnimFloat", RpcTarget.All, _str, _value);
    }
    public void RPCAnimBool(string _str, bool _value)
    {
        PV.RPC("SetAnimBool", RpcTarget.All, _str, _value);
    }
    public void RPCAnimTrg(string _str)
    {
        PV.RPC("SetAnimTrg", RpcTarget.All, _str);
    }
    #endregion

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting) {
            stream.SendNext(status.health);
        }
        else {
            status.health = (float)stream.ReceiveNext();
        }
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
}

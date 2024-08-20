using DobermannStates;
using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dobermann : Monster
{
    private enum States { Idle, Move, Attack, Hurt, Death }
    [Header("FSM")]
    private States curState;
    private FSM<Dobermann> fsm;

    [Header("Component")]
    private PhotonView PV;

    [Header("Move")]
    public int inputX;

    [Header("Search")]
    public float searchDistance;
    public Vector2 searchBox;

    [Header("Attack")]
    public float attackDistance;
    public Vector2 attackBox;

    [Header("Hurt")]
    public float knockPower;
    public string effectName;
    [HideInInspector] public GameObject player;
    [HideInInspector] public float playerPower;


    private void Awake()
    {
        // Component
        PV = GetComponent<PhotonView>();
    }

    private void Start()
    {
        // FSM
        curState = States.Idle;
        fsm = new FSM<Dobermann>(new IdleState(this));

        // Move
        StartCoroutine(SetXRoutine());
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
                else if (inputX != 0) ChangeState(States.Move);
                break;
            case States.Move:
                if (CanSeePlayer()) {
                    if (CanAttackPlayer())
                        ChangeState(States.Attack);
                }
                else if (inputX == 0) ChangeState(States.Idle);
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
                else if (inputX == 0)
                    ChangeState(States.Idle);
                else
                    ChangeState(States.Move);
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

    private IEnumerator SetXRoutine()
    {
        inputX = UnityEngine.Random.Range(-1, 2);

        yield return new WaitForSeconds(UnityEngine.Random.Range(3f, 10f));

        StartCoroutine(SetXRoutine());
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

    private void OnDisable()
    {
        // Coroutine
        StopCoroutine(SetXRoutine());
    }
}

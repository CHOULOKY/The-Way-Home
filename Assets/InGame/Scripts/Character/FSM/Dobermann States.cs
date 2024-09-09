using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Windows;
using Photon.Pun;
using Unity.Burst.CompilerServices;
using Photon.Realtime;
using static System.Net.WebRequestMethods;

namespace DobermannStates
{
    public class IdleState : BaseState<Dobermann>
    {
        [Header("Component")]
        private Rigidbody2D rigid;

        public IdleState(Dobermann _monster) : base(_monster) { }

        public override void OnStateEnter()
        {
            // Component
            rigid = monster.GetComponent<Rigidbody2D>();

            // Animator
            monster.RPCAnimFloat("xMove", 0);
        }

        public override void OnStateUpdate()
        {
            // Component
            if (!rigid) {
                rigid = monster.GetComponent<Rigidbody2D>();
                return;
            }

            // Animator
            monster.RPCAnimFloat("yMove", rigid.velocity.y);
        }

        public override void OnStateExit()
        {
            // 
        }
    }

    public class MoveState : BaseState<Dobermann>
    {
        [Header("Component")]
        private Rigidbody2D rigid;

        [Header("Check")]
        private bool isWall;
        private bool isCliff;

        [Header("Move")]
        private float inputX;

        public MoveState(Dobermann _monster) : base(_monster) { }

        public override void OnStateEnter()
        {
            // Component
            rigid = monster.GetComponent<Rigidbody2D>();
        }

        public override void OnStateUpdate()
        {
            // Component
            if (!rigid) {
                rigid = monster.GetComponent<Rigidbody2D>();
                return;
            }

            // Check
            isWall = WallCheck(rigid.position, 0.5f, new string[] { "Ground", "Object", "Player" });
            isCliff = FallCheck(rigid.position, 0.4f, 1, new string[] { "Ground", "Platform" });
            
            // Flip
            inputX = monster.inputX;
            monster.inputX = inputX = ControlFlip(inputX);

            // Move
            Move(inputX, monster.status.moveSpeed);

            // Animator
            monster.RPCAnimFloat("xMove", Mathf.Abs((float)inputX));
            monster.RPCAnimFloat("yMove", rigid.velocity.y);
        }

        public override void OnStateExit()
        {
            // Flip
            ControlFlip(0);

            // Animator
            monster.RPCAnimFloat("xMove", 0);
        }

        private bool WallCheck(Vector2 _pos, float _distance, string[] _layers)
        {
            return Physics2D.Raycast(_pos, rigid.transform.rotation.eulerAngles.y == 180 ? Vector2.left : Vector2.right, _distance, LayerMask.GetMask(_layers));
        }

        private bool FallCheck(Vector2 _pos, float _start, float _distance, string[] _layers)
        {
            _pos = rigid.transform.rotation.eulerAngles.y == 180 ?
                new Vector2(_pos.x - _start, _pos.y) : new Vector2(_pos.x + _start, _pos.y);
            // Debug.DrawRay(_pos, Vector3.down, new Color(0, 1, 0));
            return !Physics2D.Raycast(_pos, Vector3.down, _distance, LayerMask.GetMask(_layers));
        }

        private float ControlFlip(float _input)
        {
            RaycastHit2D _player = CanSeePlayer();
            if (_player) {
                float dirX = _player.transform.position.x - rigid.transform.position.x;
                _input = (int)Mathf.Sign(dirX);
                if (Mathf.Abs(dirX) < 0.25f || isWall || isCliff) _input = 0;
            }
            else if (isWall || isCliff) _input = -_input;

            if (_input > 0)
                rigid.transform.eulerAngles = Vector3.zero;
            else if (_input < 0)
                rigid.transform.eulerAngles = new Vector3(0, 180, 0);

            return _input;
        }

        private RaycastHit2D CanSeePlayer()
        {
            return Physics2D.BoxCast((Vector2)rigid.transform.position, monster.searchBox, 0,
                rigid.transform.rotation.eulerAngles.y == 180 ? Vector2.left : Vector2.right,
                monster.searchDistance, LayerMask.GetMask("Player"));
        }

        private void Move(float _input, float _speed)
        {
            // Translate Move
            rigid.transform.Translate(Mathf.Abs(_input) * Vector2.right * _speed * Time.deltaTime);
        }
    }

    public class AttackState : BaseState<Dobermann>
    {
        [Header("----------Attack")]
        private float curAttackTime;

        public AttackState(Dobermann _monster) : base(_monster) { }
        
        public override void OnStateEnter()
        {
            // 
        }
        public override void OnStateUpdate()
        {
            curAttackTime += Time.deltaTime;
            if (curAttackTime > monster.status.attackSpeed) {
                curAttackTime = 0;
                monster.RPCAttackPlayer();
            }
        }

        public override void OnStateExit()
        {
            // 
        }
    }

    public class HurtState : BaseState<Dobermann>
    {
        [Header("Component")]
        private Rigidbody2D rigid;

        public HurtState(Dobermann _monster) : base(_monster) { }

        public override void OnStateEnter()
        {
            // Component
            rigid = monster.GetComponent<Rigidbody2D>();

            // Hurt
            monster.RPCHurtEffect();
            monster.RPCAnimTrg("hurtTrg");
            Vector2 hittedDir = (rigid.transform.position - monster.player.transform.position).normalized;
            rigid.AddForce(hittedDir * monster.knockPower, ForceMode2D.Impulse);
            monster.status.health -= monster.playerPower;
        }

        public override void OnStateUpdate()
        {
            // 
        }

        public override void OnStateExit()
        {
            // 
        }
    }

    public class DeathState : BaseState<Dobermann>
    {
        public DeathState(Dobermann _monster) : base(_monster) { }

        public override void OnStateEnter()
        {
            monster.gameObject.layer = LayerMask.NameToLayer("Default");
            monster.RPCAnimTrg("deathTrg");
            monster.DestroyMonster(monster.gameObject, 8f);
        }
        public override void OnStateUpdate()
        {
            // 
        }

        public override void OnStateExit()
        {
            // 
        }
    }
}

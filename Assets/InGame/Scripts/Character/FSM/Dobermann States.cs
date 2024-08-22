using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Windows;
using Photon.Pun;
using Unity.Burst.CompilerServices;
using Photon.Realtime;

namespace DobermannStates
{
    public class IdleState : BaseState<Dobermann>
    {
        [Header("Component")]
        private Rigidbody2D rigid;
        private PhotonView PV;

        public IdleState(Dobermann _monster) : base(_monster) { }

        public override void OnStateEnter()
        {
            // Component
            rigid = monster.GetComponent<Rigidbody2D>();
            PV = monster.GetComponent<PhotonView>();

            // Animator
            PV.RPC("SetAnimFloat", RpcTarget.All, "xMove", 0);
        }

        public override void OnStateUpdate()
        {
            // Animator
            PV.RPC("SetAnimFloat", RpcTarget.All, "yMove", rigid.velocity.y);
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
        private PhotonView PV;

        [Header("Check")]
        private UnityEngine.Transform grondPos;

        [Header("Move")]
        private int inputX;

        public MoveState(Dobermann _monster) : base(_monster) { }

        public override void OnStateEnter()
        {
            // Component
            rigid = monster.GetComponent<Rigidbody2D>();
            PV = monster.GetComponent<PhotonView>();

            // Check
            grondPos = monster.transform.GetChild(0);
        }

        public override void OnStateUpdate()
        {
            // Move
            inputX = monster.inputX;
            Move(inputX, monster.status.moveSpeed);
            monster.inputX = inputX;
            
            // Flip
            PV.RPC("ControlFlip", RpcTarget.AllBuffered, inputX);

            // Animator
            PV.RPC("SetAnimFloat", RpcTarget.All, "xMove", Mathf.Abs((float)inputX));
            PV.RPC("SetAnimFloat", RpcTarget.All, "yMove", rigid.velocity.y);
        }

        public override void OnStateExit()
        {
            // Flip
            PV.RPC("ControlFlip", RpcTarget.All, 0);

            // Animator
            PV.RPC("SetAnimFloat", RpcTarget.All, "xMove", 0);
        }

        private bool GroundCheck(Vector2 _pos, float _radius, string[] _layers)
        {
            return Physics2D.OverlapCircle(_pos, _radius, LayerMask.GetMask(_layers));
        }

        [PunRPC]
        private void ControlFlip(int _inputX)
        {
            // FlipX
            if (_inputX > 0)
                rigid.transform.eulerAngles = Vector3.zero;
            else if (_inputX < 0)
                rigid.transform.eulerAngles = new Vector3(0, 180, 0);
        }

        private RaycastHit2D CanSeePlayer()
        {
            return Physics2D.BoxCast((Vector2)rigid.transform.position, monster.searchBox, 0,
                rigid.transform.rotation.eulerAngles.y == 180 ? Vector2.left : Vector2.right,
                monster.searchDistance, LayerMask.GetMask("Player"));
        }

        private void Move(float _input, float _speed)
        {
            RaycastHit2D _player = CanSeePlayer();
            if (_player) {
                float dirX = _player.transform.position.x - rigid.transform.position.x;
                inputX = (int)Mathf.Sign(dirX);
                if (Mathf.Abs(dirX) < 0.4f) inputX = 0;
            }

            // Translate Move
            if (_input != 0) rigid.transform.Translate(Mathf.Abs(_input) * Vector2.right * _speed * Time.deltaTime);
        }
    }

    public class AttackState : BaseState<Dobermann>
    {
        [Header("Component")]
        private Rigidbody2D rigid;
        private PhotonView PV;

        [Header("----------Attack")]
        private float curAttackTime;

        public AttackState(Dobermann _monster) : base(_monster) { }
        
        public override void OnStateEnter()
        {
            // Component
            rigid = monster.GetComponent<Rigidbody2D>();
            PV = monster.GetComponent<PhotonView>();
        }
        public override void OnStateUpdate()
        {
            curAttackTime += Time.deltaTime;
            if (curAttackTime > monster.status.attackSpeed) {
                curAttackTime = 0;
                PV.RPC("AttackPlayer", RpcTarget.All);
            }
        }

        public override void OnStateExit()
        {
            // 
        }

        [PunRPC]
        private void AttackPlayer()
        {
            // ! anim
            // Debug.Log("Player Search in Attack Box!");

            RaycastHit2D[] _attackHits = Physics2D.BoxCastAll((Vector2)rigid.transform.position, monster.attackBox, 0,
                rigid.transform.rotation.eulerAngles.y == 180 ? Vector2.left : Vector2.right,
                monster.attackDistance, LayerMask.GetMask("Player"));

            if (_attackHits != null) {
                foreach (var hit in _attackHits) {
                    PhotonView targetView = PhotonView.Find(hit.collider.gameObject.GetComponent<PhotonView>().ViewID);
                    if (targetView != null) {
                        targetView.gameObject.GetComponent<Player>().HurtByMonster(monster.gameObject, monster.status.attackPower);
                    }
                    // Debug.Log("Attack in Attack Box!");
                }
            }
            PV.RPC("SetAnimTrg", RpcTarget.All, "attackTrg");
        }

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
    }

    public class HurtState : BaseState<Dobermann>
    {
        [Header("Component")]
        private Rigidbody2D rigid;
        private PhotonView PV;

        [Header("Effect")]
        private ParticleSystem hurtEffect;

        public HurtState(Dobermann _monster) : base(_monster) { }

        public override void OnStateEnter()
        {
            // Component
            rigid = monster.GetComponent<Rigidbody2D>();
            PV = monster.GetComponent<PhotonView>();
            
            // Hurt
            PV.RPC("PlayHurtEffect", RpcTarget.All);
            PV.RPC("SetAnimTrg", RpcTarget.All, "hurtTrg");
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

        [PunRPC]
        private void PlayHurtEffect()
        {
            if (!hurtEffect)
                hurtEffect = PhotonNetwork.Instantiate(monster.effectName, monster.transform.position, Quaternion.identity).GetComponent<ParticleSystem>();
            hurtEffect.transform.position = monster.transform.position;
            float effectSize = rigid.transform.localScale.x;
            hurtEffect.transform.localScale =
                new Vector2(Random.Range(effectSize * 0.4f, effectSize), Random.Range(effectSize * 0.4f, effectSize));
            hurtEffect.gameObject.SetActive(true);
            hurtEffect.Play();
        }
    }

    public class DeathState : BaseState<Dobermann>
    {
        public DeathState(Dobermann _monster) : base(_monster) { }

        public override void OnStateEnter()
        {
            monster.GetComponent<Rigidbody2D>().simulated = false;
            monster.GetComponent<PhotonView>().RPC("SetAnimTrg", RpcTarget.All, "deathTrg");
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

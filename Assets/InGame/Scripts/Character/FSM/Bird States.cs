using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BirdStates
{
    public class IdleState : BaseState<Bird>
    {
        public IdleState(Bird _monster) : base(_monster) { }

        public override void OnStateEnter()
        {
            // Animator
            monster.RPCAnimFloat("xMove", 0);
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

    public class MoveState : BaseState<Bird>
    {
        [Header("Component")]
        private Rigidbody2D rigid;

        [Header("Move")]
        private Vector2 inputVec;
        private List<Node> pathToPlayer;
        private int currentPathIndex;

        public MoveState(Bird _monster) : base(_monster) { }

        public override void OnStateEnter()
        {
            // Component
            rigid = monster.GetComponent<Rigidbody2D>();

            // Move
            monster.InvokeRepeating("Move", 0f, 2f);
        }

        public override void OnStateUpdate()
        {
            // Component
            if (!rigid) {
                rigid = monster.GetComponent<Rigidbody2D>();
                return;
            }
        }

        public override void OnStateExit()
        {
            // Animator
            monster.RPCAnimFloat("xMove", 0);
            monster.RPCAnimFloat("yMove", 0);
        }

        private void ControlFlip(float _input)
        {
            if (_input > 0)
                rigid.transform.eulerAngles = Vector3.zero;
            else if (_input < 0)
                rigid.transform.eulerAngles = new Vector3(0, 180, 0);
        }

        private RaycastHit2D CanSeePlayer()
        {
            return Physics2D.BoxCast((Vector2)rigid.transform.position, monster.searchBox, 0,
                rigid.transform.rotation.eulerAngles.y == 180 ? Vector2.left : Vector2.right,
                monster.searchDistance, LayerMask.GetMask("Player"));
        }

        private void Move()
        {
            RaycastHit2D hit = CanSeePlayer();
            if (hit.collider != null) {
                // �÷��̾ �������� ��
                Vector2Int start = new Vector2Int(Mathf.RoundToInt(monster.transform.position.x), Mathf.RoundToInt(monster.transform.position.y));
                Vector2Int target = new Vector2Int(Mathf.RoundToInt(hit.collider.transform.position.x), Mathf.RoundToInt(hit.collider.transform.position.y));

                pathToPlayer = GameManager.Instance.astarManager.PathFinding(start, target); // ��� ���

                if (pathToPlayer != null && pathToPlayer.Count > 0) {
                    currentPathIndex = 0; // ��� ���������� �ʱ�ȭ
                    monster.StartCoroutine(FollowPath()); // ��θ� ���� �̵��ϴ� �ڷ�ƾ ����
                }
            }
        }
        private IEnumerator FollowPath()
        {
            while (currentPathIndex < pathToPlayer.Count) {
                Vector2 targetPosition = new Vector2(pathToPlayer[currentPathIndex].x, pathToPlayer[currentPathIndex].y);
                // targetPosition = new Vector2(rigid.transform.rotation.eulerAngles.y == 180 ? -targetPosition.x : targetPosition.x, targetPosition.y);
                ControlFlip(targetPosition.x);
                monster.RPCAnimFloat("xMove", Mathf.Abs((float)targetPosition.x));
                monster.RPCAnimFloat("yMove", (float)targetPosition.y);

                // ���� ��ġ�� Ÿ�� ��� ������ �Ÿ��� ������ ���� ���� �̵�
                while (Vector2.Distance(monster.transform.position, targetPosition) > 0.1f) {
                    monster.transform.position = Vector2.MoveTowards(monster.transform.position, targetPosition, Time.deltaTime * monster.status.moveSpeed);
                    yield return null; // ���� �����ӱ��� ���
                }

                currentPathIndex++; // ���� ���� �̵�
            }
        }
    }

    public class AttackState : BaseState<Bird>
    {
        [Header("----------Attack")]
        private float curAttackTime;

        public AttackState(Bird _monster) : base(_monster) { }

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

    public class HurtState : BaseState<Bird>
    {
        [Header("Component")]
        private Rigidbody2D rigid;

        public HurtState(Bird _monster) : base(_monster) { }

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

    public class DeathState : BaseState<Bird>
    {
        public DeathState(Bird _monster) : base(_monster) { }

        public override void OnStateEnter()
        {
            monster.GetComponent<Rigidbody2D>().gravityScale = 1;
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

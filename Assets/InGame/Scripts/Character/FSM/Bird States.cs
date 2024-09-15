using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BirdStates
{
    public class IdleState : BaseState<Bird>
    {
        [Header("Component")]
        private Rigidbody2D rigid;

        public IdleState(Bird _monster) : base(_monster) { }

        public override void OnStateEnter()
        {
            // Component
            rigid = monster.GetComponent<Rigidbody2D>();

            // Animator
            monster.RPCAnimFloat("xMove", 0);
            monster.RPCAnimFloat("yMove", 0);
        }

        public override void OnStateUpdate()
        {
            // Component
            if (!rigid) {
                rigid = monster.GetComponent<Rigidbody2D>();
                return;
            }

            // Flip
            ControlFlip(monster.inputX);
        }

        public override void OnStateExit()
        {
            // 
        }

        private void ControlFlip(float _input)
        {
            if (_input > 0)
                rigid.transform.eulerAngles = Vector3.zero;
            else if (_input < 0)
                rigid.transform.eulerAngles = new Vector3(0, 180, 0);
        }
    }

    public class MoveState : BaseState<Bird>
    {
        [Header("Component")]
        private Rigidbody2D rigid;

        [Header("Move")]
        private List<Node> pathToPlayer;
        private int currentPathIndex;
        private float curRoutineTime;
        private bool isFollowPath;

        public MoveState(Bird _monster) : base(_monster) { }

        public override void OnStateEnter()
        {
            // Component
            rigid = monster.GetComponent<Rigidbody2D>();

            // Move
            PathCheck();
        }

        public override void OnStateUpdate()
        {
            // Component
            if (!rigid) {
                rigid = monster.GetComponent<Rigidbody2D>();
                return;
            }
            
            curRoutineTime += Time.deltaTime;
            if (curRoutineTime > 1f) {
                curRoutineTime = 0;
                PathCheck();
            }

            if (isFollowPath) {
                FollowPath();
            }
        }

        public override void OnStateExit()
        {
            // Move
            curRoutineTime = 0;
            isFollowPath = false;
        }

        private void ControlFlip(float _input)
        {
            if (_input > 0)
                rigid.transform.eulerAngles = Vector3.zero;
            else if (_input < 0)
                rigid.transform.eulerAngles = new Vector3(0, 180, 0);
        }

        private void Test()
        {
            /*
            RaycastHit2D hit = monster.CanSeePlayer();
            if (hit.collider != null) {
                // 플레이어를 감지했을 때
                Vector2Int start = new Vector2Int(Mathf.RoundToInt(monster.transform.position.x), Mathf.RoundToInt(monster.transform.position.y));
                Vector2Int target = new Vector2Int(Mathf.RoundToInt(hit.collider.transform.position.x), Mathf.RoundToInt(hit.collider.transform.position.y));

                // pathToPlayer = GameManager.Instance.astarManager.PathFinding(start, target, true, true);
                pathToPlayer = GameObject.FindAnyObjectByType<AStarManager>().PathFinding(start, target, true, true);

                for (int i = 0; i < pathToPlayer.Count; i++) Debug.Log(i + "번째는 " + pathToPlayer[i].x + ", " + pathToPlayer[i].y);
            }
            */
        }

        private void PathCheck()
        {
            RaycastHit2D hit = monster.CanSeePlayer();
            if (hit.collider != null) {
                // 플레이어를 감지했을 때
                Vector2Int start = new Vector2Int(Mathf.RoundToInt(monster.transform.position.x), Mathf.RoundToInt(monster.transform.position.y));
                Vector2Int target = new Vector2Int(Mathf.RoundToInt(hit.collider.transform.position.x), Mathf.RoundToInt(hit.collider.transform.position.y));

                // pathToPlayer = GameManager.Instance.astarManager.PathFinding(start, target, true, true);
                pathToPlayer = GameObject.FindAnyObjectByType<AStarManager>().PathFinding(start, target, true, true); // 경로 계산

                if (pathToPlayer != null && pathToPlayer.Count > 0) {
                    currentPathIndex = 0; // 경로 시작점으로 초기화
                    isFollowPath = true; // 경로를 따라 이동하는 루틴 시작
                }
                else {
                    isFollowPath = false;
                }
            }
        }
        private void FollowPath()
        {
            if (currentPathIndex < pathToPlayer.Count) {
                Vector2 targetPosition = new Vector2(pathToPlayer[currentPathIndex].x, pathToPlayer[currentPathIndex].y);

                float dirX = rigid.position.x < targetPosition.x ? 1 : -1;
                ControlFlip(dirX);
                monster.RPCAnimFloat("xMove", Mathf.Abs(dirX));
                monster.RPCAnimFloat("yMove", (float)targetPosition.y);

                // 현재 위치와 타겟 노드 사이의 거리가 작으면 다음 노드로 이동
                if (Vector2.Distance(monster.transform.position, targetPosition) > 0.1f) {
                    monster.transform.position = Vector2.MoveTowards(monster.transform.position, targetPosition, Time.deltaTime * monster.status.moveSpeed);
                    // yield return null; // 다음 프레임까지 대기
                }
                else {
                    currentPathIndex++; // 다음 노드로 이동
                }
            }
            else {
                isFollowPath = false;
            }
        }
    }

    public class BackState : BaseState<Bird>
    {
        [Header("Component")]
        private Rigidbody2D rigid;

        [Header("Move")]
        private Transform defaultPosition;
        private List<Node> pathToDefault;
        private int currentPathIndex;
        private bool isFollowPath;

        public BackState(Bird _monster) : base(_monster) { }

        public override void OnStateEnter()
        {
            // Component
            rigid = monster.GetComponent<Rigidbody2D>();

            // Move
            PathCheck();
        }

        public override void OnStateUpdate()
        {
            // Component
            if (!rigid) {
                rigid = monster.GetComponent<Rigidbody2D>();
                return;
            }

            if (pathToDefault == default) {
                PathCheck();
            }

            if (isFollowPath) {
                FollowPath();
            }
        }

        public override void OnStateExit()
        {
            // Move
            isFollowPath = false;
        }

        private void ControlFlip(float _input)
        {
            if (_input > 0)
                rigid.transform.eulerAngles = Vector3.zero;
            else if (_input < 0)
                rigid.transform.eulerAngles = new Vector3(0, 180, 0);
        }

        private void PathCheck()
        {
            if (!defaultPosition) defaultPosition = monster.defaultPosition;
            else {
                Vector2Int start = new Vector2Int(Mathf.RoundToInt(monster.transform.position.x), Mathf.RoundToInt(monster.transform.position.y));
                Vector2Int target = new Vector2Int(Mathf.RoundToInt(defaultPosition.position.x), Mathf.RoundToInt(defaultPosition.position.y));

                // pathToDefault = GameManager.Instance.astarManager.PathFinding(start, target, true, true);
                pathToDefault = GameObject.FindAnyObjectByType<AStarManager>().PathFinding(start, target, true, true);

                if (pathToDefault != null && pathToDefault.Count > 0) {
                    currentPathIndex = 0; // 경로 시작점으로 초기화
                    isFollowPath = true; // 경로를 따라 이동하는 루틴 시작
                }
                else {
                    isFollowPath = false;
                }
            }
        }
        private void FollowPath()
        {
            if (currentPathIndex < pathToDefault.Count) {
                Vector2 targetPosition = new Vector2(pathToDefault[currentPathIndex].x, pathToDefault[currentPathIndex].y);

                float dirX = rigid.position.x < targetPosition.x ? 1 : -1;
                ControlFlip(dirX);
                monster.RPCAnimFloat("xMove", Mathf.Abs(dirX));
                monster.RPCAnimFloat("yMove", (float)targetPosition.y);

                // 현재 위치와 타겟 노드 사이의 거리가 작으면 다음 노드로 이동
                if (Vector2.Distance(monster.transform.position, targetPosition) > 0.1f) {
                    monster.transform.position = Vector2.MoveTowards(monster.transform.position, targetPosition, Time.deltaTime * monster.status.moveSpeed);
                    // yield return null; // 다음 프레임까지 대기
                }
                else {
                    currentPathIndex++; // 다음 노드로 이동
                }
            }
            else {
                isFollowPath = false;
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

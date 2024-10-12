using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BirdStates
{
    public abstract class BirdState : BaseState<Bird>
    {
        protected Rigidbody2D rigid;

        public BirdState(Bird _monster) : base(_monster) { }

        public override void OnStateEnter()
        {
            if (!rigid)
                rigid = monster.GetComponent<Rigidbody2D>();
        }

        public override void OnStateUpdate()
        {
            if (!rigid) {
                rigid = monster.GetComponent<Rigidbody2D>();
                return;
            }
        }
    }

    public class IdleState : BirdState
    {
        public IdleState(Bird _monster) : base(_monster) { }

        public override void OnStateEnter()
        {
            base.OnStateEnter();

            monster.RPCAnimFloat("xMove", 0);
            monster.RPCAnimFloat("yMove", 0);
        }

        public override void OnStateUpdate()
        {
            base.OnStateUpdate();

            // Flip
            ControlFlip(monster.inputX);
        }

        public override void OnStateExit() { }

        private void ControlFlip(float _input)
        {
            if (_input > 0)
                rigid.transform.eulerAngles = Vector3.zero;
            else if (_input < 0)
                rigid.transform.eulerAngles = new Vector3(0, 180, 0);
        }
    }

    public class MoveState : BirdState
    {
        [Header("Move")]
        private List<Node> pathToPlayer;
        private int currentPathIndex;
        private float curRoutineTime;
        private bool isFollowPath;

        public MoveState(Bird _monster) : base(_monster) { }

        public override void OnStateEnter()
        {
            base.OnStateEnter();

            // Move
            PathCheck();
        }

        public override void OnStateUpdate()
        {
            base.OnStateUpdate();

            curRoutineTime += Time.deltaTime;
            if (curRoutineTime > 0.75f) {
                curRoutineTime = 0;
                PathCheck();
            }

            if (isFollowPath) {
                FollowPath();
            }
        }

        public override void OnStateExit()
        {
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

        /*
        private void Test()
        {
            RaycastHit2D hit = monster.CanSeePlayer();
            if (hit.collider != null) {
                // 플레이어를 감지했을 때
                Vector2Int start = new Vector2Int(Mathf.RoundToInt(monster.transform.position.x), Mathf.RoundToInt(monster.transform.position.y));
                Vector2Int target = new Vector2Int(Mathf.RoundToInt(hit.collider.transform.position.x), Mathf.RoundToInt(hit.collider.transform.position.y));

                // pathToPlayer = GameManager.Instance.astarManager.PathFinding(start, target, true, true);
                pathToPlayer = GameObject.FindAnyObjectByType<AStarManager>().PathFinding(start, target, true, true);

                for (int i = 0; i < pathToPlayer.Count; i++) Debug.Log(i + "번째는 " + pathToPlayer[i].x + ", " + pathToPlayer[i].y);
            }
        }
        */

        private void PathCheck()
        {
            RaycastHit2D hit = monster.CanSeePlayer();
            if (hit.collider != null) {
                Vector2Int start = new(Mathf.RoundToInt(monster.transform.position.x), Mathf.RoundToInt(monster.transform.position.y));
                Vector2Int target = new(Mathf.RoundToInt(hit.collider.transform.position.x), Mathf.RoundToInt(hit.collider.transform.position.y));

                pathToPlayer = GameObject.FindAnyObjectByType<AStarManager>().PathFinding(start, target, true, true);

                isFollowPath = pathToPlayer != null && pathToPlayer.Count > 0;
                currentPathIndex = 0;
            }
        }
        private void FollowPath()
        {
            if (currentPathIndex < pathToPlayer.Count) {
                Vector2 targetPosition = new(pathToPlayer[currentPathIndex].x, pathToPlayer[currentPathIndex].y);
                float dirX = rigid.position.x < targetPosition.x ? 1 : -1;
                ControlFlip(dirX);

                monster.RPCAnimFloat("xMove", Mathf.Abs(dirX));
                monster.RPCAnimFloat("yMove", targetPosition.y);

                if (Vector2.Distance(monster.transform.position, targetPosition) > 0.1f) {
                    monster.transform.position = Vector2.MoveTowards(monster.transform.position, targetPosition, Time.deltaTime * monster.status.moveSpeed);
                } else {
                    currentPathIndex++;
                }
            } else {
                isFollowPath = false;
            }
        }
    }

    public class BackState : BirdState
    {
        [Header("Move")]
        private Transform defaultPosition;
        private List<Node> pathToDefault;
        private int currentPathIndex;
        private bool isFollowPath;

        public BackState(Bird _monster) : base(_monster) { }

        public override void OnStateEnter()
        {
            base.OnStateEnter();

            // Move
            PathCheck();
        }

        public override void OnStateUpdate()
        {
            base.OnStateUpdate();

            if (isFollowPath) {
                FollowPath();
            }
        }

        public override void OnStateExit()
        {
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

            Vector2Int start = new(Mathf.RoundToInt(monster.transform.position.x), Mathf.RoundToInt(monster.transform.position.y));
            Vector2Int target = new(Mathf.RoundToInt(defaultPosition.position.x), Mathf.RoundToInt(defaultPosition.position.y));

            pathToDefault = GameObject.FindAnyObjectByType<AStarManager>().PathFinding(start, target, true, true);
            isFollowPath = pathToDefault != null && pathToDefault.Count > 0;
            currentPathIndex = 0;
        }
        private void FollowPath()
        {
            if (currentPathIndex < pathToDefault.Count) {
                Vector2 targetPosition = new(pathToDefault[currentPathIndex].x, pathToDefault[currentPathIndex].y);
                float dirX = rigid.position.x < targetPosition.x ? 1 : -1;
                ControlFlip(dirX);

                monster.RPCAnimFloat("xMove", Mathf.Abs(dirX));
                monster.RPCAnimFloat("yMove", targetPosition.y);

                if (Vector2.Distance(monster.transform.position, targetPosition) > 0.1f) {
                    monster.transform.position = Vector2.MoveTowards(monster.transform.position, targetPosition, Time.deltaTime * monster.status.moveSpeed);
                } else {
                    currentPathIndex++;
                }
            } else {
                isFollowPath = false;
            }
        }
    }

    public class AttackState : BirdState
    {
        private float curAttackTime;

        public AttackState(Bird _monster) : base(_monster) { }

        public override void OnStateEnter() { }

        public override void OnStateUpdate()
        {
            curAttackTime += Time.deltaTime;
            if (curAttackTime > monster.status.attackSpeed) {
                curAttackTime = 0;
                monster.RPCAttackPlayer();
            }
        }

        public override void OnStateExit() { }
    }

    public class HurtState : BirdState
    {
        public HurtState(Bird _monster) : base(_monster) { }

        public override void OnStateEnter()
        {
            base.OnStateEnter();

            monster.RPCHurtEffect();
            monster.RPCAnimTrg("hurtTrg");
            SoundManager.instance.PlaySfx(SoundManager.Sfx.Melee);
            Vector2 hittedDir = (rigid.transform.position - monster.player.transform.position).normalized;
            rigid.AddForce(hittedDir * monster.knockPower, ForceMode2D.Impulse);
            monster.status.health -= monster.playerPower;
        }

        public override void OnStateUpdate() { }

        public override void OnStateExit() { }
    }

    public class DeathState : BirdState
    {
        public DeathState(Bird _monster) : base(_monster) { }

        public override void OnStateEnter()
        {
            base.OnStateEnter();
            rigid.velocity = Vector2.zero;
            rigid.gravityScale = 1;
            monster.gameObject.layer = LayerMask.NameToLayer("Default");
            monster.gameObject.tag = "Untagged";
            monster.RPCAnimTrg("deathTrg");
            monster.DestroyMonster(monster.gameObject, 8f);
        }
        public override void OnStateUpdate() { }

        public override void OnStateExit() { }
    }
}

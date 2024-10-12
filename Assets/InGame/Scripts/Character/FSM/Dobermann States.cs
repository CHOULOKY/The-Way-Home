using UnityEngine;

namespace DobermannStates
{
    public abstract class DobermannState : BaseState<Dobermann>
    {
        protected Rigidbody2D rigid;

        public DobermannState(Dobermann _monster) : base(_monster) { }

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

    public class IdleState : DobermannState
    {
        public IdleState(Dobermann _monster) : base(_monster) { }

        public override void OnStateEnter()
        {
            base.OnStateEnter();
            monster.RPCAnimFloat("xMove", 0);
        }

        public override void OnStateUpdate()
        {
            base.OnStateUpdate();

            if (rigid)
                monster.RPCAnimFloat("yMove", rigid.velocity.y);
        }

        public override void OnStateExit() { }
    }

    public class MoveState : DobermannState
    {
        [Header("Check")]
        private bool isWall;
        private bool isCliff;

        [Header("Move")]
        private float inputX;

        public MoveState(Dobermann _monster) : base(_monster) { }

        public override void OnStateEnter()
        {
            base.OnStateEnter();
        }

        public override void OnStateUpdate()
        {
            base.OnStateUpdate();

            isWall = ObjectCheck(rigid.position, 0.5f, "Object");
            isWall = WallCheck(rigid.position, 0.5f, "Ground", "Object");
            isCliff = CheckCliff(rigid.position, 0.4f, 1, "Ground", "Platform");

            monster.inputX = inputX = ControlFlip(monster.inputX);

            Move(inputX, monster.status.moveSpeed);

            UpdateAnimator(Mathf.Abs(inputX), rigid.velocity.y);
        }

        public override void OnStateExit()
        {
            ControlFlip(0);
            UpdateAnimator(0, 0);
        }

        private bool ObjectCheck(Vector2 _pos, float _distance, params string[] _layers)
        {
            return Physics2D.Raycast(_pos, (rigid.transform.rotation.eulerAngles.y == 180 ? Vector2.left : Vector2.right) * 0.5f + Vector2.down * 0.7f,
                _distance, LayerMask.GetMask(_layers));
        }
        private bool WallCheck(Vector2 _pos, float _distance, params string[] _layers)
        {
            if (Physics2D.Raycast(_pos, (rigid.transform.rotation.eulerAngles.y == 180 ? Vector2.left : Vector2.right),
                _distance, LayerMask.GetMask(_layers))) return true;
            return isWall;
        }

        private bool CheckCliff(Vector2 _pos, float _start, float _distance, params string[] _layers)
        {
            _pos = rigid.transform.rotation.eulerAngles.y == 180 ?
                new Vector2(_pos.x - _start, _pos.y) : new Vector2(_pos.x + _start, _pos.y);
            // Debug.DrawRay(_pos, Vector3.down, new Color(0, 1, 0));
            return !Physics2D.Raycast(_pos, Vector3.down, _distance, LayerMask.GetMask(_layers));
        }

        private void UpdateAnimator(float xMove, float yMove)
        {
            monster.RPCAnimFloat("xMove", xMove);
            monster.RPCAnimFloat("yMove", yMove);
        }

        private float ControlFlip(float _input)
        {
            RaycastHit2D _player = monster.CanSeePlayer();
            if (_player) {
                float dirX = _player.transform.position.x - rigid.transform.position.x;
                _input = Mathf.Sign(dirX);
                if (Mathf.Abs(dirX) < 0.25f) _input = 0;
                else if (isWall || isCliff) {
                    if (rigid.transform.rotation.eulerAngles.y == 180 && dirX > 0) _input = 1;
                    else if (rigid.transform.rotation.eulerAngles.y != 180 && dirX < 0) _input = -1;
                    else _input = 0;
                }
            }
            else if (isWall || isCliff) _input = -_input;

            if (_input > 0)
                rigid.transform.eulerAngles = Vector3.zero;
            else if (_input < 0)
                rigid.transform.eulerAngles = new Vector3(0, 180, 0);

            return _input;
        }

        private void Move(float _input, float _speed)
        {
            // Translate Move
            rigid.transform.Translate(Mathf.Abs(_input) * Vector2.right * _speed * Time.deltaTime);
        }
    }

    public class AttackState : DobermannState
    {
        [Header("----------Attack")]
        private float curAttackTime;

        public AttackState(Dobermann _monster) : base(_monster) { }
        
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

    public class HurtState : DobermannState
    {
        public HurtState(Dobermann _monster) : base(_monster) { }

        public override void OnStateEnter()
        {
            base.OnStateEnter();
            monster.RPCHurtEffect();
            monster.RPCAnimTrg("hurtTrg");
            SoundManager.instance.PlaySfx(SoundManager.Sfx.Melee);
            ApplyKnockBack();
            monster.status.health -= monster.playerPower;
        }

        public override void OnStateUpdate() { }

        public override void OnStateExit() { }

        private void ApplyKnockBack()
        {
            Vector2 hittedDir = (rigid.transform.position - monster.player.transform.position).normalized;
            rigid.AddForce(hittedDir * monster.knockPower, ForceMode2D.Impulse);
        }
    }

    public class DeathState : DobermannState
    {
        public DeathState(Dobermann _monster) : base(_monster) { }

        public override void OnStateEnter()
        {
            base.OnStateEnter();
            rigid.velocity = Vector2.zero;
            monster.gameObject.layer = LayerMask.NameToLayer("Default");
            monster.gameObject.tag = "Untagged";
            monster.RPCAnimTrg("deathTrg");
            monster.DestroyMonster(monster.gameObject, 8f);
        }
        public override void OnStateUpdate() { }

        public override void OnStateExit() { }
    }
}

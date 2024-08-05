using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UIElements;
using static Player;

public class ObjFunc : MonoBehaviour
{
    #region Move
    protected virtual void Move(Rigidbody2D _rigid, float _input, float _speed, Vector2 _perp, bool _isGround, bool _isJump, bool _isSlope, bool _isAngle)
    {
        // Translate Move
        if (_input != 0) {
            if (_isSlope && _isGround && !_isJump && _isAngle) {
                _rigid.velocity = Vector2.zero;
                if (_input > 0)
                    transform.Translate(new Vector2(Mathf.Abs(_input) * -_perp.x * _speed * Time.deltaTime,
                        Mathf.Abs(_input) * -_perp.y * _speed * Time.deltaTime));
                else if (_input < 0)
                    transform.Translate(new Vector2(Mathf.Abs(_input) * -_perp.x * _speed * Time.deltaTime,
                        Mathf.Abs(_input) * _perp.y * _speed * Time.deltaTime));
            }
            else
                transform.Translate(Mathf.Abs(_input) * Vector2.right * _speed * Time.deltaTime);
        }
    }
    #endregion

    #region Jump
    protected virtual void Jump(Rigidbody2D _rigid, float _power, bool _isGround, ref bool _isJump, KeyCode[] _keys)
    {
        if (_rigid.velocity.y <= 0) _isJump = false;

        if (_isGround && !_isJump) {
            foreach (var _key in _keys) {
                if (Input.GetKeyDown(_key)) {
                    _isJump = true;
                    _rigid.velocity = new Vector2(_rigid.velocity.x, 0);
                    _rigid.AddForce(Vector2.up * _power, ForceMode2D.Impulse);
                }
            }
        }
    }

    protected virtual void DownJump(GameObject _object, KeyCode[] _firstKeys, KeyCode[] _secondKeys, float _offTime)
    {
        StartCoroutine(DonwJumpRoutine(_object, _firstKeys, _secondKeys, _offTime));
    }
    protected virtual IEnumerator DonwJumpRoutine(GameObject _object, KeyCode[] _firstKeys, KeyCode[] _secondKeys, float _offTime)
    {
        foreach (var _first in _firstKeys) {
            foreach (var _second in _secondKeys) {
                if (Input.GetKey(_first) && Input.GetKeyDown(_second)) {
                    _object.layer = LayerMask.NameToLayer("Back Object");

                    yield return new WaitForSeconds(_offTime);

                    _object.layer = LayerMask.NameToLayer(_object.tag);
                }
            }
        }
    }
    #endregion

    #region Flip
    protected virtual void ControlFlip(Rigidbody2D _rigid, float _inputX, bool _isSlope)
    {
        // FlipX
        if (_inputX > 0)
            _rigid.transform.eulerAngles = Vector3.zero;
        else if (_inputX < 0)
            _rigid.transform.eulerAngles = new Vector3(0, 180, 0);

        // FlipZ (on the slope)
        if (_isSlope && _inputX == 0)
            _rigid.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
        else
            _rigid.constraints = RigidbodyConstraints2D.FreezeRotation;
    }
    #endregion

    #region Check
    protected virtual bool GroundChk(Vector2 _pos, float _radius, string[] _layers)
    {
        return Physics2D.OverlapCircle(_pos, _radius, LayerMask.GetMask(_layers));
    }

    protected virtual (float, Vector2, bool) SlopeChk(Rigidbody2D _rigid, Vector2 _downPos, Vector2 _frontPos, string[] _layers)
    {
        RaycastHit2D _slopeHit = Physics2D.Raycast(_downPos, Vector2.down, 0.25f, LayerMask.GetMask(_layers));
        RaycastHit2D _frontHit = Physics2D.Raycast(_frontPos, _rigid.transform.rotation.eulerAngles.y == 180 ? Vector2.left : Vector2.right, 0.1f, LayerMask.GetMask(_layers));
        // Debug.DrawRay(_frontPos, (_rigid.transform.rotation.eulerAngles.y == 180 ? Vector2.left : Vector2.right) * 0.1f, Color.white);

        if (_frontHit || _slopeHit) {
            float _angle = 0;
            Vector2 _perp = Vector2.zero;
            if (_frontHit) {
                _angle = Vector2.Angle(_frontHit.normal, Vector2.up);
                _perp = Vector2.Perpendicular(_frontHit.normal).normalized;
            }
            else if (_slopeHit) {
                _angle = Vector2.Angle(_slopeHit.normal, Vector2.up);
                _perp = Vector2.Perpendicular(_slopeHit.normal).normalized;
            }

            if (_angle != 0) return (_angle, _perp, true);
            else return (_angle, _perp, false);
        }
        else {
            return (0, Vector2.zero, false);
        }
    }

    protected virtual bool WallChk(Rigidbody2D _rigid, Vector2 _pos, float _distance, string[] _layers)
    {
        RaycastHit2D _wallHit = Physics2D.Raycast(_pos, _rigid.transform.rotation.eulerAngles.y == 180 ? Vector2.left : Vector2.right, _distance, LayerMask.GetMask(_layers));

        if (_wallHit) {
            foreach (string _layer in _layers) {
                if (_layer == "Front Object") {
                    if (_wallHit.collider.CompareTag("Stop Object")) {
                        return true;
                    }
                    continue;
                }
                else if (_layer == "Enemy") {
                    Enemy enemy = _wallHit.collider.GetComponent<Enemy>();
                    if (enemy != null && !enemy.isCollAtk) {
                        return true;
                    }
                    continue;
                }
                if (_wallHit.collider.CompareTag(_layer)) {
                    return true;
                }
            }
        }
        return false;
    }

    protected virtual bool DeathChk(float _health, bool _isDeath)
    {
        if (_health <= 0 && !_isDeath) return true;
        return false;
    }
    #endregion

    #region SetAnim
    protected virtual void SetAnimFloat(PhotonView _PV, Animator _animator, string _str, float _value)
    {
        if (_PV.IsMine || !_PV)
            _animator.SetFloat(_str, _value);
    }

    protected virtual void SetAnimBool(PhotonView _PV, Animator _animator, string _str, bool _value)
    {
        if (_PV.IsMine || !_PV)
            _animator.SetBool(_str, _value);
    }

    protected virtual void SetAnimTrg(PhotonView _PV, Animator _animator, string _str)
    {
        if (_PV.IsMine || !_PV)
            _animator.SetTrigger(_str);
    }
    #endregion
}

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
using UnityEngine.Windows;

public class ObjFunc : MonoBehaviour
{
    #region Move

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
        if (_inputX == 0 && _isSlope)
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

    protected virtual (float, Vector2, bool) SlopeChk(Rigidbody2D _rigid, Vector2 _groundPos, Vector2 _frontPos, float _distance, string[] _layers)
    {
        RaycastHit2D _slopeHit = Physics2D.Raycast(_groundPos, Vector2.down, _distance, LayerMask.GetMask(_layers));
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

    /*
    protected virtual bool WallChk(Vector2 _pos, float _distance, string[] _layers)
    {
        Physics2D.Raycast(_pos, Vector2.right, _distance, LayerMask.GetMask(_layers));


        switch (character) {
            case PlayerCharacter.Girl:
                wallHits = Physics2D.BoxCastAll(rigid.position, wallBox, 0,
                    transform.rotation.eulerAngles.y == 180 ? new Vector2(-1, -0.4f) : new Vector2(1, -0.4f),
                    wallDistance, LayerMask.GetMask("Ground", "Front Object", "Enemy"));
                break;
            case PlayerCharacter.Robot:
                wallHits = Physics2D.BoxCastAll(rigid.position, wallBox, 0,
                    transform.rotation.eulerAngles.y == 180 ? new Vector2(-1, -1.2f) : new Vector2(1, -1.2f),
                    wallDistance, LayerMask.GetMask("Ground", "Front Object", "Enemy"));
                break;
        }

        foreach (RaycastHit2D wallHit in wallHits) {
            if (wallHit.collider.CompareTag("Ground") || wallHit.collider.CompareTag("Stop Object"))
                inputX = 0;
            else if (wallHit.collider.CompareTag("Enemy") && !wallHit.collider.GetComponent<Enemy>().isCollAtk)
                inputX = 0;
        }
        Debug.DrawRay(rigid.position, Vector2.right * 0.3f, Color.white);
        Debug.DrawRay(rigid.position, Vector2.left * 0.3f, Color.white);
    }
    */

    protected virtual bool DeathChk<T>(T _health) where T : IComparable<T>
    {
        if (_health.CompareTo(default(T)) <= 0) return true;
        return false;
    }
    #endregion

    #region SetAnim
    protected virtual void SetAnimFloat(PhotonView _PV, Animator _animator, string _str, float _value)
    {
        if (_PV.IsMine || _PV == null)
            _animator.SetFloat(_str, _value);
    }

    protected virtual void SetAnimBool(PhotonView _PV, Animator _animator, string _str, bool _value)
    {
        if (_PV.IsMine || _PV == null)
            _animator.SetBool(_str, _value);
    }

    protected virtual void SetAnimTrg(PhotonView _PV, Animator _animator, string _str)
    {
        if (_PV.IsMine || _PV == null)
            _animator.SetTrigger(_str);
    }
    #endregion
}

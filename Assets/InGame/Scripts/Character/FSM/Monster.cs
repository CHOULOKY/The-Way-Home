using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Monster : MonoBehaviour
{
    [Header("Status")]
    public MonsterStatus status;

    public abstract void HurtByPlayer(GameObject _player, float _attackPower);

    public void DestroyMonster(GameObject _monster, float _time=0)
    {
        Destroy(_monster, _time);
    }

    #region SetAnim
    [PunRPC]
    public void SetAnimFloat(string _str, float _value)
    {
        this.GetComponent<Animator>().SetFloat(_str, _value);
    }
    [PunRPC]
    public void SetAnimBool(string _str, bool _value)
    {
        this.GetComponent<Animator>().SetBool(_str, _value);
    }
    [PunRPC]
    public void SetAnimTrg(string _str)
    {
        this.GetComponent<Animator>().SetTrigger(_str);
    }
    #endregion
}

[System.Serializable]
public class MonsterStatus
{
    [Header("Health")]
    public float maxHealth;
    public float health;

    [Header("Move")]
    public float moveSpeed;

    [Header("Attack")]
    public float attackPower;
    public float attackSpeed;
}

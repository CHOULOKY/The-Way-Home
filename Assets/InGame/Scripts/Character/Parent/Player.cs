using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.Image;
using UnityEngine.UIElements;
using Unity.VisualScripting;
using UnityEngine.Experimental.Rendering;
using Photon.Realtime;
using Photon.Pun;
using System.Threading;

public abstract class Player : MonoBehaviour
{
    [Header("Status")]
    public PlayerStatus status;

    public abstract void HurtByMonster(GameObject _monster, float _attackPower);

    protected void DestroyPlayer(GameObject player, float delay = 0)
    {
        Destroy(player, delay);
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
public class PlayerStatus
{
    [Header("Health stat")]
    public float maxHealth;
    public float health;

    [Header("Move stat")]
    public float moveSpeed;

    [Header("Jump stat")]
    public float jumpPower;

    [Header("Attack stat")]
    public float attackPower;
    [Tooltip("Second basis")] public float attackSpeed;
}

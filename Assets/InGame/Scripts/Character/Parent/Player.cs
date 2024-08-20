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
    [SerializeField] public PlayerStatus status;

    public abstract void HurtByMonster(GameObject _monster, float _attackPower);

    protected void DestroyPlayer(GameObject _player, float _value = 0)
    {
        Destroy(_player, _value);
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


/*

 * 수정해야 할 사항들
 - 

 */

/*

 * 경사면 참고 링크
 - https://www.youtube.com/watch?v=A6IkXiP_ing
 - https://daekyoulibrary.tistory.com/entry/Charon-3-%EA%B2%BD%EC%82%AC%EB%A1%9CSlope-%EC%A7%80%ED%98%95-%EC%9D%B4%EB%8F%99-%EA%B5%AC%ED%98%84%ED%95%98%EA%B8%B0

*/

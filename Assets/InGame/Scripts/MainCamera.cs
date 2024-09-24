using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCamera : MonoBehaviour
{
    [Header("----------Move")]
    private Player player;
    public float speed;

    [Header("----------Set Retry")]
    private int curcount = 0;
    public int limitcount = 60;

    private static MainCamera instance;

    private void Awake()
    {
        transform.position = Vector3.zero;
    }

    void LateUpdate()
    {
        if (!player) return;

        transform.position = Vector3.MoveTowards(transform.position,
            player.transform.position + Vector3.up * 1.5f, Time.deltaTime * speed);
        transform.position = new Vector3(transform.position.x, transform.position.y, -10f);
    }


    public void StartSet()
    {
        if (curcount > limitcount) {
            Debug.LogWarning("* MainCamera: Player Search Failed!");
            GameManager.Instance.GameQuit();
            return;
        }
        else if (player) {
            Debug.LogWarning("* MainCamera: Player Already Found!");
            curcount = 0;
            return;
        }
        curcount++;

        Player[] targets = FindObjectsOfType<Player>();
        if (targets.Length > 0)
            foreach (Player target in targets) {
                if (target.GetComponent<PhotonView>().IsMine) {
                    Debug.Log("-> MainCamera: Player Found");
                    curcount = 0;
                    player = target;
                    transform.position =
                        new Vector3(player.transform.position.x, player.transform.position.y + 1.5f, -10f);
                }
            }
        else {
            if (curcount % 10 == 0)
                Debug.Log("-> MainCamera: Number of Player Search Attempts " + curcount);
            StartCoroutine(RetryRoutine());
        }
    }
    private IEnumerator RetryRoutine()
    {
        yield return new WaitForSecondsRealtime(0.5f);

        StartSet();
    }
}

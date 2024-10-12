using Photon.Pun;
using System.Collections;
using UnityEngine;

public class MainCamera : MonoBehaviour
{
    [Header("----------Move")]
    public float speed;

    [Header("----------Set Retry")]
    private int currentAttemptCount = 0;
    public int maxAttempts = 60;

    private Player player;

    private void Awake()
    {
        transform.position = Vector3.zero;
    }

    void LateUpdate()
    {
        if (player != null) {
            MoveCameraToPlayer();
        }
    }

    private void MoveCameraToPlayer()
    {
        Vector3 targetPosition = player.transform.position + Vector3.up * 1.5f;
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, Time.deltaTime * speed);
        transform.position = new Vector3(transform.position.x, transform.position.y, -10f);
    }


    public void StartSet()
    {
        if (player != null) {
            currentAttemptCount = 0;
            return;
        }

        if (currentAttemptCount > maxAttempts) {
            Debug.LogError("* MainCamera: Player Search Failed!");
            GameManager.Instance.QuitGame();
            return;
        }

        FindPlayer();
    }

    private void FindPlayer()
    {
        Player[] targets = FindObjectsOfType<Player>();

        if (targets.Length > 0) {
            foreach (Player target in targets) {
                if (target.GetComponent<PhotonView>().IsMine) {
                    player = target;
                    SetCameraPositionToPlayer();
                    return;
                }
            }
        } else {
            LogAttemptCount();
            StartCoroutine(RetryRoutine());
        }
    }

    private void SetCameraPositionToPlayer()
    {
        currentAttemptCount = 0;
        transform.position = new Vector3(player.transform.position.x, player.transform.position.y + 1.5f, -10f);
    }
    private void LogAttemptCount()
    {
        currentAttemptCount++;
        if (currentAttemptCount % 10 == 0) {
            Debug.LogWarning($"-> MainCamera: Number of Player Search Attempts {currentAttemptCount / 2}");
        }
    }

    private IEnumerator RetryRoutine()
    {
        yield return new WaitForSecondsRealtime(0.5f);
        StartSet();
    }
}

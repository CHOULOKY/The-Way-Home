using UnityEngine;
using Photon.Pun;

public class CheckPoint : MonoBehaviour
{
    [Header("CheckPoint")]
    public int pointNumber;

    [Header("Effect")]
    public string checkName;
    private ParticleSystem checkEffect;

    private void Update()
    {
        // Test Code
        if (Input.GetKeyDown(KeyCode.Escape)) {
            GameManager.Instance.HandleGameFailure();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) {
            OnTriggerExit2D(collision);
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) {
            if (GameManager.Instance == null || GameManager.Instance.saveNumber >= pointNumber) return;
            PhotonView PV = GetComponent<PhotonView>();
            PV.RPC(nameof(AssignPoint), RpcTarget.All, pointNumber, transform.position.x, transform.position.y);
        }
    }
    [PunRPC]

    private void AssignPoint(int pointNumber, float x, float y)
    {
        GameManager.Instance.saveNumber = pointNumber;
        GameManager.Instance.savePoint = new Vector2(x, y);

        PlayCheckEffect();
    }

    private void PlayCheckEffect()
    {
        Vector2 effectPosition = GetBottomPosition(gameObject) + Vector2.up;

        if (checkEffect == null) {
            checkEffect = PhotonNetwork.Instantiate(checkName, transform.position, Quaternion.identity).GetComponent<ParticleSystem>();
        }

        checkEffect.transform.position = effectPosition;
        checkEffect.gameObject.SetActive(true);
        checkEffect.Play();
    }

    private Vector2 GetBottomPosition(GameObject _object)
    {
        if (_object.TryGetComponent<Collider2D>(out Collider2D collider)) {
            Bounds bounds = collider.bounds;
            return new Vector2(bounds.center.x, bounds.min.y);
        }
        return default;
    }
}

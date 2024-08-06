using Photon.Pun;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class TestCode : MonoBehaviourPunCallbacks
{
    private static TestCode instance = null;
    public MainCamera mainCamera;
    string selectedCharacter = (string)PhotonNetwork.LocalPlayer.CustomProperties["selectedCharacter"];

    public PhotonView PV;
    private bool isHostReady = false;

    private void Awake()
    {
        Screen.SetResolution(1280, 720, false);

        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
        mainCamera = FindObjectOfType<MainCamera>();
    }

    private void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(HostSpawnRoutine());
        }
        else
        {
            StartCoroutine(GuestSpawnRoutine());
        }
    }

    private IEnumerator HostSpawnRoutine()
    {
        // The host creates the character right away
        yield return new WaitForSeconds(0.5f);
        Spawn(selectedCharacter);

        // The host is ready
        PV.RPC("HostReadyRPC", RpcTarget.Others);
    }

    private IEnumerator GuestSpawnRoutine()
    {
        // Guest waits until the host is ready
        yield return new WaitUntil(() => isHostReady);
        Spawn(selectedCharacter);
    }

    [PunRPC]
    private void HostReadyRPC()
    {
        isHostReady = true;
    }

    public void Spawn(string selectedCharacter)
    {
        if (selectedCharacter == "Girl")
        {
            PhotonNetwork.Instantiate("Player Girl", new Vector3(1, -0.5f, 0), Quaternion.identity);
        }
        else if (selectedCharacter == "Robot")
        {
            PhotonNetwork.Instantiate("Player Robot", new Vector3(-1, -0.5f, 0), Quaternion.identity);
        }

        Debug.Log("-> MainCamera: Player Found");
        mainCamera.StartSet();
    }
}

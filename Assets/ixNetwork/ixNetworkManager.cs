using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class ixNetworkManager : MonoBehaviourPunCallbacks
{
    public static ixNetworkManager instance;
    public bool bListUsers = false;
    //public string RemotePlayerObjectName = "RemotePlayer";
   // UNetworkPlayer np;

    // Start is called before the first frame update
    void Start()
    {
        PhotonNetwork.ConnectUsingSettings();


    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.NickName = "GUEST " + Random.Range(1, 1000);
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.PublishUserId = true;
        roomOptions.IsVisible = false;
        roomOptions.MaxPlayers = 20;
        bool success = PhotonNetwork.JoinOrCreateRoom("iosRGBD", roomOptions, TypedLobby.Default);
        Debug.Log("connected to masterserver with " + success);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Room Max number: " + PhotonNetwork.CurrentRoom.MaxPlayers);
        Player[] player = PhotonNetwork.PlayerList;

        for (int i = 0; i < player.Length; i++)
        {
            Debug.Log((i).ToString() + " : " + player[i].NickName + " ID = " + player[i].UserId);
        }


        //// Network Instantiate the object used to represent our player. This will have a View on it and represent the player         
        //GameObject p = PhotonNetwork.Instantiate(
        //RemotePlayerObjectName, new Vector3(0f, 0f, 0f), Quaternion.identity, 0);
        //np = p.GetComponent<UNetworkPlayer>();
        //if (np)
        //{
        //    np.transform.name = "MyRemotePlayer";
        //    np.AssignPlayerObjects();
        //}
    }

    // Update is called once per frame
    void Update()
    {
        if (bListUsers)
        {
            bListUsers = false;
            foreach (var p in PhotonNetwork.PlayerList)
            {
                Debug.Log(p.UserId);
            }
        }
        //if (np)
        //    if (!np.PlayerHeadTransform || !np.PlayerLeftHandTransform || !np.PlayerRightHandTransform)
        //        np.AssignPlayerObjects();
    }
}

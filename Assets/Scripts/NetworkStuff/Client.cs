using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using NaughtyAttributes;
using ExitGames.Client.Photon;

public class Client : MonoBehaviourPunCallbacks
{
    // public PlayerNetworkState playerNetworkState;

    public GameObject playerPrefab;
    public List<GameObject> playerSpawnPoints;


    public string gameVersion = "0.0.1";
    public string nickName;

    [ReadOnly] public bool connected;
    [ReadOnly] public bool inRoom;
    [ReadOnly] public RoomInfo room;
    [ReadOnly] public int playerRoomId;


    public int maxPlayers = 2;
    public string roomToJoinName;
    
    public List<RoomInfo> rooms;


    private OverNetworkData OND;

    //EVENTS
    private const byte GAME_LAUNCHED_EVENT = 1;
    //object[] myData = new object[] { r, g, b };
    //PhotonNetwork.RaiseEvents(SOME_EVENT, myDat, RaiseEventOptions.Default, SendOptions.SendReliable);

    //public void OnEnable()
    //{
    //    //PhotonNetwork.NetworkingClient.EventReceived += NetworkingClient_EventReceived; // Subscribe to event
    //}

    //public void OnDisable()
    //{
    //    //PhotonNetwork.NetworkingClient.EventReceived -= NetworkingClient_EventReceived; // Unsubscribe to event
    //}



    public int GetPlayerIndex()
    {
        int index = 0;
        foreach (var player in PhotonNetwork.CurrentRoom.Players)
        {
            if (player.Value.NickName == nickName) { break; }
            index++;
        }
        return index;
    }


    private void NetworkingClient_EventReceived(EventData obj)
    {
        if (obj.Code == GAME_LAUNCHED_EVENT)
        {
            //foreach (var player in PhotonNetwork.CurrentRoom.Players)
            //{
            //    Debug.Log(nickName);
            //    Debug.Log(player.Value.NickName);
            //    Debug.Log(player.Value.ActorNumber);
            //    Debug.Log(player.Key);
            //    Debug.Log(player.Value.NickName == nickName);
            //}



            GameObject playerObject = NetworkInstantiate(playerPrefab, playerSpawnPoints[playerRoomId-1].transform.position, Quaternion.identity, false);


            //playerObject.GetComponent<PhotonView>().TransferOwnership(player.Key);

            //object[] datas(object[])obj.customdata;
            //float r = (float)datas[0];
            Debug.Log("Event received");
        }
    }

    [PunRPC]
    void ChatMessage(string a, string b)
    {
        GameObject playerObject = NetworkInstantiate(playerPrefab, playerSpawnPoints[playerRoomId - 1].transform.position, Quaternion.identity, true);
        Debug.Log(string.Format("ChatMessage {0} {1}", a, b));
    }


    [ReorderableList] public List<NetworkedPrefab> networkedPrefabs = new List<NetworkedPrefab>();

    public GameObject NetworkInstantiate(GameObject obj, Vector3 position, Quaternion rotation, bool roomObject)
    {
        foreach (NetworkedPrefab networkedPrefab in networkedPrefabs)
        {
            if (networkedPrefab.Prefab == obj)
            {
                if (networkedPrefab.Path != string.Empty)
                {
                    GameObject result = null;
                    if (roomObject)
                    {
                        result = PhotonNetwork.InstantiateRoomObject(networkedPrefab.Path, position, rotation);
                    }
                    else
                    {
                        result = PhotonNetwork.Instantiate(networkedPrefab.Path, position, rotation);
                    }

                    return result;
                }
                else
                {
                    Debug.Log("Path is empty for gameobject name " + networkedPrefab.Prefab);
                    return null;
                }
            }
        }
        return null;
    }

    [System.Serializable]
    public class NetworkedPrefab
    {
        public GameObject Prefab;
        public string Path;

        public NetworkedPrefab(GameObject obj, string path)
        {
            Prefab = obj;
            Path = PrefabPath(path);
        }

        private string PrefabPath(string path) // Becase path to asset in build is defferent than in editor
        {
            int extensionLength = System.IO.Path.GetExtension(path).Length;
            int additionalLength = 10;
            int startIndex = path.ToLower().IndexOf("resource");

            if (startIndex == -1)
            {
                return string.Empty;
            }
            else
            {
                return path.Substring(startIndex + additionalLength, path.Length - (additionalLength + startIndex + extensionLength));
            }
        }
    }

    public void Start()
    {
        OND = gameObject.GetComponent<OverNetworkData>();
        nickName = "Player" + Random.Range(1000, 9999);
        rooms = new List<RoomInfo>();
        ConnectToServer();
    }





    public void ConnectToServer()
    {
        PhotonNetwork.SendRate = 40; // Default is 20
        PhotonNetwork.SerializationRate = 20; // Default is 10 (Used by PhotonSerializeViews)

        PhotonNetwork.NickName = nickName;
        PhotonNetwork.GameVersion = gameVersion;
        PhotonNetwork.ConnectUsingSettings();
        PhotonNetwork.ConnectToBestCloudServer();
        Debug.Log("Connecting to the server as: " + PhotonNetwork.LocalPlayer.NickName + " ...");
    }

    public override void OnConnectedToMaster()
    {
        //if (!PhotonNetwork.InLobby) //Needed?
        //{
        //    PhotonNetwork.JoinLobby();
        //}
        Debug.Log("Connected to the server");
        connected = true;
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log("Disconnected from the server (" + cause.ToString() + ")");
        connected = false;
    }



    public void CreateRoom()
    {
        if (PhotonNetwork.IsConnected)
        {
            string roomName = "aaa";
            //roomName = "Room" + Random.Range(0, 1000);

            foreach (RoomInfo room in rooms) //Check if this name is already taken
            {
                if (room.Name == roomName)
                {
                    CreateRoom();
                    return;
                }
            }

            RoomOptions roomOptions = new RoomOptions() { MaxPlayers = (byte)maxPlayers, PlayerTtl = 1, EmptyRoomTtl = 5 };
            PhotonNetwork.CreateRoom(roomName, roomOptions, TypedLobby.Default);
            Debug.Log("Creating room named " + roomName + " ...");
        }  

    }

    public override void OnCreatedRoom()
    {
        Debug.Log("Room created and joined");
        ToggleGameEventSubscription(true);
        playerRoomId = 1;
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log("Room creation failed " + message);
    }


    

    public void JoinRoom(string roomName)
    {
        if (roomName == null || roomName == "" || roomName.Length != 3)
        {
            Debug.Log("Wrong room name");
            return;
        }

        PhotonNetwork.JoinRoom(roomName);
        Debug.Log("Joining room ...");
    }

    public override void OnJoinedRoom()
    {
        room = PhotonNetwork.CurrentRoom;
        inRoom = true;
        Debug.Log("Joined room " + room.Name);
        ToggleGameEventSubscription(true);
        playerRoomId = PhotonNetwork.CurrentRoom.PlayerCount;
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        room = null;
        inRoom = false;

        if (returnCode == 3)
        {
            //Limit of players stuff
        }


        Debug.Log("Failed to join room (" + message + ")");
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom(true);
        Debug.Log("Leaving room ...");
    }

    public override void OnLeftRoom()
    {
        room = null;
        inRoom = false;
        Debug.Log("Room left");
    }




  

  

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        rooms = roomList;
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        Debug.Log(newPlayer.NickName + " entered the room");
        if (PhotonNetwork.IsMasterClient)
        {
            if (PhotonNetwork.CurrentRoom.PlayerCount == PhotonNetwork.CurrentRoom.MaxPlayers)
            {
                LaunchGame();
            } 
        }
        else
        {
           
        }
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
       // isGameMaster = truee;
    }

    public override void OnMasterClientSwitched(Photon.Realtime.Player newMasterClient)
    {
        //Needed?
    }



    public void ToggleGameEventSubscription(bool toggle)
    {
        if (toggle)
        {
            PhotonNetwork.NetworkingClient.EventReceived += NetworkingClient_EventReceived;
        }
        else
        {
            PhotonNetwork.NetworkingClient.EventReceived -= NetworkingClient_EventReceived;
        }
        
    }


    public void Update()
    {
        Debug.Log(OND.gameActive);
        if (OND.gameActive)
        {
           
        }
    }

    public void LaunchGame()
    {
        PhotonNetwork.CurrentRoom.IsOpen = false;
        Debug.Log("Game Launched!!!");

        // Send event to all players except self to start game

        // RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup. };

        //object[] myData = new object[] { r, g, b };

        //PhotonNetwork.RaiseEvent(GAME_LAUNCHED_EVENT, 0, RaiseEventOptions.Default, SendOptions.SendReliable);




        GameObject playerObject = NetworkInstantiate(playerPrefab, playerSpawnPoints[0].transform.position, Quaternion.identity, false);


        OND.gameActive = true;

        //PhotonView photonView = PhotonView.Get(this);
        //photonView.RPC("ChatMessage", RpcTarget.All, "jup", "and jup.");


        // PhotonView photonView = PhotonView.Get(this);
        //  photonView.RPC("ChatMessage", RpcTarget. RpcTarget.All, "jup", "and jup.");

        int i = 0;
        foreach (var player in PhotonNetwork.CurrentRoom.Players)
        {
           // Debug.Log(player.Key);
            //Debug.Log(PhotonNetwork.CurrentRoom.Players);
            //Debug.Log(PhotonNetwork.LocalPlayer);


            // PhotonView photonView = PhotonView.Get(this);
            //  photonView.RPC("ChatMessage", RpcTarget. RpcTarget.All, "jup", "and jup.");

            //GameObject playerObject = NetworkInstantiate(playerPrefab, playerSpawnPoints[i].transform.position, Quaternion.identity, true);
            //playerObject.GetComponent<PhotonView>().TransferOwnership(player.Key);
            //i++;
        }

        //// PhotonNetwork.AutomaticallySyncScene = true; // Move this to start
        //if (PhotonNetwork.IsMasterClient)
        //{
        //    //PhotonNetwork.CurrentRoom.IsOpen = false;
        //    PhotonNetwork.LoadLevel(1);

        //}
    }






    void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 200, 100), "Connected: " + connected);
        GUI.Label(new Rect(10, 30, 200, 100), "Room: " + (inRoom ? room.Name : "None"));
        GUI.Label(new Rect(10, 50, 200, 100), "MasterClient: " + PhotonNetwork.IsMasterClient);
        GUI.Label(new Rect(10, 70, 200, 100), "Player index: " + (inRoom ? playerRoomId.ToString() : "None"));
 

        Vector2 debugButtonsPivot = new Vector2(10, 100);
        Vector2 debugButtonsSize = new Vector2(120, 20);
        int debugButtonsOffset = 10;

        int i = 0;

        if (!inRoom)
        {
            if (GUI.Button(new Rect(debugButtonsPivot.x + debugButtonsSize.x * i + debugButtonsOffset * i, debugButtonsPivot.y, debugButtonsSize.x, debugButtonsSize.y), "CreateRoom"))
                CreateRoom();
            i++;
        }
        if (!inRoom)
        {
            if (GUI.Button(new Rect(debugButtonsPivot.x + debugButtonsSize.x * i + debugButtonsOffset * i, debugButtonsPivot.y, debugButtonsSize.x, debugButtonsSize.y), "JoinRoom"))
                JoinRoom(roomToJoinName);
            i++;
            roomToJoinName = GUI.TextField(new Rect(debugButtonsPivot.x + debugButtonsSize.x * i + debugButtonsOffset * i, debugButtonsPivot.y, debugButtonsSize.x, debugButtonsSize.y), roomToJoinName);
            i++;
        }

        if (inRoom)
        {
            if (GUI.Button(new Rect(debugButtonsPivot.x + debugButtonsSize.x * i + debugButtonsOffset * i, debugButtonsPivot.y, debugButtonsSize.x, debugButtonsSize.y), "LeaveRoom"))
                LeaveRoom();
            i++;     
        }

       


        string roomsActiveData = "Active rooms: \n";
        foreach (RoomInfo room in rooms)
        {
            roomsActiveData = roomsActiveData + room.Name + "\n";
        }
        GUI.TextArea(new Rect(10, 180, 300, 150), roomsActiveData);

    }

   

}


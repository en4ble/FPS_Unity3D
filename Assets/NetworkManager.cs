using UnityEngine;
using System.Collections.Generic;

public class NetworkManager : MonoBehaviour {

	public GameObject standbyCamera;
	public SpawnSpot[] spawnSpots;

	public bool offlineMode = false;
    public float respawnTimer = 0;

	bool connecting = false;
    bool hasPickedTeam = false;
    int teamID = 0;

	List<string> chatMessages;
	int maxChatMessages = 5;

    float sensitivity = 2f;
    bool invertX = false;
    bool invertY = false;
    int fov = 100;

	GameObject myPlayerGO;
    GameObject crosshair;

	// Use this for initialization
	void Start () {
		spawnSpots = GameObject.FindObjectsOfType<SpawnSpot>();
		PhotonNetwork.player.name = PlayerPrefs.GetString("Username", "Awesome Dude");
		chatMessages = new List<string>();
        crosshair = GameObject.Find("Crosshair");
	}

	void OnDestroy() {
		PlayerPrefs.SetString("Username", PhotonNetwork.player.name);
	}

	public void AddChatMessage(string m) {
		GetComponent<PhotonView>().RPC ("AddChatMessage_RPC", PhotonTargets.AllBuffered, m);
	}

    public void AddChatMessage(string m, int pTeamID)
    {
        if (pTeamID != -1)
            GetComponent<PhotonView>().RPC("AddChatMessage_RPC", PhotonTargets.AllBuffered, m, teamID);
        else
            GetComponent<PhotonView>().RPC("AddChatMessage_RPC", PhotonTargets.AllBuffered, m);
    }

    [RPC]
    void AddChatMessage_RPC(string m, int pTeamID)
    {
        while (chatMessages.Count >= maxChatMessages)
        { 
            chatMessages.RemoveAt(0);
        }

        if (pTeamID == -1 || pTeamID == teamID)
            //compare teamID -> if same team, display message | -1 -> everyone on the server gets this message
            chatMessages.Add(m);
    }

	[RPC]
	void AddChatMessage_RPC(string m) {
		while(chatMessages.Count >= maxChatMessages) {
			chatMessages.RemoveAt(0);
		}
		chatMessages.Add(m);
	}

	void Connect() {
        //version number (so you can only connect to a server with clients with the same version number)
		PhotonNetwork.ConnectUsingSettings("MultiFPS by enable 00.000.101");
	}
	 
	void OnGUI() {
		GUILayout.Label( PhotonNetwork.connectionStateDetailed.ToString() );

		if(PhotonNetwork.connected == false && connecting == false ) {
            // not yet connected - starting screen
			GUILayout.BeginArea( new Rect(0, 0, Screen.width, Screen.height) );
            GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			GUILayout.BeginVertical();
			GUILayout.FlexibleSpace();

			GUILayout.BeginHorizontal();
			GUILayout.Label("Username: ");
			PhotonNetwork.player.name = GUILayout.TextField(PhotonNetwork.player.name);
			GUILayout.EndHorizontal();

			if( GUILayout.Button("Singleplayer") ) {
				connecting = true;
				PhotonNetwork.offlineMode = true;
				OnJoinedLobby();
			}

			if( GUILayout.Button("Multiplayer") ) {
				connecting = true;
				Connect ();
			}

			GUILayout.FlexibleSpace();
			GUILayout.EndVertical();
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			GUILayout.EndArea();
		}

		if(PhotonNetwork.connected == true && connecting == false) {
            if (hasPickedTeam)
            {
                //fully connected, display chatbox
                GUILayout.BeginArea(new Rect(0, 0, Screen.width, Screen.height));
                GUILayout.BeginVertical();
                GUILayout.FlexibleSpace();

                foreach (string msg in chatMessages)
                {
                    GUILayout.Label(msg);
                }

                GUILayout.EndVertical();
                GUILayout.EndArea();
            }
            else
            {
                //player has not selected a team
                GUILayout.BeginArea(new Rect(0, 0, Screen.width, Screen.height));
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.BeginVertical();
                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Terrorists"))
                {
                    SpawnMyPlayer(1);
                }

                if (GUILayout.Button("Counterterrorists"))
                {
                    SpawnMyPlayer(2);
                }

                if (GUILayout.Button("random"))
                {
                    SpawnMyPlayer(Random.Range(1, 3));  //1 or 2
                }

                if (GUILayout.Button("Renegade"))
                {
                    SpawnMyPlayer(0);
                }

                GUILayout.FlexibleSpace();
                GUILayout.EndVertical();
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.EndArea();
            }

		}

	}

	void OnJoinedLobby() {
		Debug.Log ("OnJoinedLobby");
		PhotonNetwork.JoinRandomRoom();
	}

	void OnPhotonRandomJoinFailed() {
		Debug.Log ("OnPhotonRandomJoinFailed");
		PhotonNetwork.CreateRoom( null );
	}

	void OnJoinedRoom() {
		Debug.Log ("OnJoinedRoom");

		connecting = false;
	}

	void SpawnMyPlayer(int teamID) {
        this.teamID = teamID;
        hasPickedTeam = true;

		AddChatMessage("Spawning player: " + PhotonNetwork.player.name);

		if(spawnSpots == null) {
			Debug.LogError ("WTF?!?!?");
			return;
		}

		SpawnSpot mySpawnSpot = spawnSpots[ Random.Range (0, spawnSpots.Length) ];
		myPlayerGO = (GameObject)PhotonNetwork.Instantiate("PlayerController", mySpawnSpot.transform.position, mySpawnSpot.transform.rotation, 0);
		standbyCamera.SetActive(false);

		((MonoBehaviour)myPlayerGO.GetComponent("MouseLook")).enabled = true;
		((MonoBehaviour)myPlayerGO.GetComponent("PlayerMovement")).enabled = true;
		((MonoBehaviour)myPlayerGO.GetComponent("PlayerShooting")).enabled = true;
        ((MonoBehaviour)myPlayerGO.GetComponent("Health")).enabled = true;

        myPlayerGO.GetComponent<PhotonView>().RPC("setTeamID", PhotonTargets.AllBuffered, teamID);

        crosshair.guiTexture.enabled = true;

		myPlayerGO.transform.FindChild("Main Camera").gameObject.SetActive(true);

        UpdateSettings();

        //enable chat when spawning
        Chat chat = GameObject.FindObjectOfType<Chat>();
        chat.chatActive = true;

        //disable menu when spawning
        Menu menu = GameObject.FindObjectOfType<Menu>();
        if (menu.showMenu == true)
            menu.showMenu = false;
        UnFreeze();
	}

    public void ChangeSettings(float pSensitivity, bool pInvertX, bool pInvertY, int pFov)
    {
        sensitivity = pSensitivity;
        invertX = pInvertX;
        invertY = pInvertY;
        fov = pFov;

        UpdateSettings();
    }

    void UpdateSettings()
    {
        if(myPlayerGO != null)
        {
            //setting sensitivity
            if (sensitivity < 20f)
            {
                myPlayerGO.GetComponent<MouseLook>().sensitivityX = sensitivity;
                myPlayerGO.transform.FindChild("Main Camera").GetComponent<MouseLook>().sensitivityY = sensitivity;
            }

            //setting invert
            myPlayerGO.GetComponent<MouseLook>().invertX = invertX;
            myPlayerGO.transform.FindChild("Main Camera").GetComponent<MouseLook>().invertY = invertY;

            //setting fov
            if (fov >= 90 && fov <= 120)
                myPlayerGO.transform.FindChild("Main Camera").camera.fieldOfView = fov;
        }
    }

    public void Freeze()
    {
        if (myPlayerGO != null)
        {
            //no movement, no looking around
            ((MonoBehaviour)myPlayerGO.GetComponent("MouseLook")).enabled = false;
            myPlayerGO.transform.FindChild("Main Camera").GetComponent<MouseLook>().enabled = false;
            myPlayerGO.GetComponent<PlayerMovement>().freeze = true;
            myPlayerGO.GetComponent<PlayerMovement>().unlockCursor = true;
            myPlayerGO.GetComponent<PlayerShooting>().freeze = true;
        }
    }

    public void FreezeForChat()
    {
        if (myPlayerGO != null)
        {
            //no movement
            myPlayerGO.GetComponent<PlayerMovement>().freeze = true;
        }
    }

    public void UnFreeze()
    {
        if (myPlayerGO != null)
        {
            //re-enable movement and looking around
            ((MonoBehaviour)myPlayerGO.GetComponent("MouseLook")).enabled = true;
            myPlayerGO.transform.FindChild("Main Camera").GetComponent<MouseLook>().enabled = true;
            myPlayerGO.GetComponent<PlayerMovement>().freeze = false;
            myPlayerGO.GetComponent<PlayerMovement>().unlockCursor = false;
            myPlayerGO.GetComponent<PlayerShooting>().freeze = false;
        }
    }

    void Update()
    {
        if (respawnTimer > 0)
        {
            respawnTimer -= Time.deltaTime;

            if (respawnTimer <= 0)
            {
                //respawn Player
                SpawnMyPlayer(teamID);
            }
        }
    }

    public string GetPlayerName()
    {
        return PhotonNetwork.player.name;
    }

    public Vector3 GetMyPosition()
    {
        return myPlayerGO.transform.position;
    }
}

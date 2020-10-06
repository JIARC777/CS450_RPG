using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class Menu : MonoBehaviourPunCallbacks, ILobbyCallbacks
{
    [Header("Menu Screen References")]
    public GameObject mainScreen;
    public GameObject createRoomScreen;
    public GameObject lobbyScreen;
    public GameObject lobbyBrowserScreen;

    [Header("Main Screen")]
    public Button createRoomButton;
    public Button findRoomButton;

    [Header("Lobby")]
    public TextMeshProUGUI playerListText;
    public TextMeshProUGUI roomInfoText;
    public Button startGameButton;

    [Header("Lobby Browser")]
    public RectTransform roomListContainer;
    public GameObject roomButtonPrefab;

    private List<GameObject> roomButtons = new List<GameObject>();
    private List<RoomInfo> roomList = new List<RoomInfo>();

    // Start is called before the first frame update
    void Start()
    {
        //disabiling menu buttons to start
        createRoomButton.interactable = false;
        findRoomButton.interactable = false;
        //enable cursor since its hidden in the gameplay
        Cursor.lockState = CursorLockMode.None;

        if (PhotonNetwork.InRoom)
        {
            //go to lobby
            SetScreen(lobbyScreen);
            UpdateLobbyUI();
            // make the room visible
            PhotonNetwork.CurrentRoom.IsVisible = true;
            PhotonNetwork.CurrentRoom.IsOpen = true;
        }
    }

    void SetScreen(GameObject screen)
    {
        // turn all screens off and activate the one you have been passed rather than checking to see which ones to disable
        mainScreen.SetActive(false);
        createRoomScreen.SetActive(false);
        lobbyScreen.SetActive(false);
        lobbyBrowserScreen.SetActive(false);

        //activate
        screen.SetActive(true);
        if (screen == lobbyBrowserScreen)
            UpdateLobbyBrowserUI();
    }

    public void OnPlayerNameValueChanged(TMP_InputField playerNameInput)
    {
        PhotonNetwork.NickName = playerNameInput.text;
    }

    public override void OnConnectedToMaster()
    {
        // enable menu buttons once we connect to the server
        createRoomButton.interactable = true;
        findRoomButton.interactable = true;
    }

    // called when create room button is pressed
    public void OnCreateRoomButton()
    {
        SetScreen(createRoomScreen);
    }

    // when find room button is pressed
    public void OnFindRoomButton()
    {
        SetScreen(lobbyBrowserScreen);
    }
    
    //When Back button gets pressed
    public void OnBackButton()
    {
        SetScreen(mainScreen);
    }

    //For Create Room screen
    public void OnCreateButton(TMP_InputField roomNameInput)
    {
        NetworkManager.instance.CreateRoom(roomNameInput.text);
    }

    //For Lobby Screen
    public override void OnJoinedRoom()
    {
         SetScreen(lobbyScreen);
         photonView.RPC("UpdateLobbyUI", RpcTarget.AllBuffered);
    }

    [PunRPC]
    void UpdateLobbyUI ()
    {
        //enable or disable ability to start depending on if you are host or not
        startGameButton.interactable = PhotonNetwork.IsMasterClient;

        playerListText.text = "";
        foreach (Player player in PhotonNetwork.PlayerList)
            playerListText.text += player.NickName + "\n";
        //set room info text
        roomInfoText.text = "<b>Room Name</b> \n" + PhotonNetwork.CurrentRoom.Name;
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        UpdateLobbyUI();
    }

    public void OnStartGameButton()
    {
        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.CurrentRoom.IsVisible = false;

        NetworkManager.instance.photonView.RPC("ChangeScene", RpcTarget.All, "Game");
    }

    public void OnLeaveLobbyButton()
    {
        PhotonNetwork.LeaveRoom();
        SetScreen(mainScreen);
    }

    //populate browser with buttona
    void UpdateLobbyBrowserUI()
    {
        //disable all room buttons
        foreach (GameObject button in roomButtons)
            button.SetActive(false);
        for (int x = 0; x < roomList.Count; ++x)
        {
            //get or create button object
            GameObject button = x >= roomButtons.Count ? CreateRoomButton() : roomButtons[x];
            button.SetActive(true);

            //set room name and player count
            button.transform.Find("RoomNameText").GetComponent<TextMeshProUGUI>().text = roomList[x].Name;
            button.transform.Find("PlayerCountText").GetComponent<TextMeshProUGUI>().text = roomList[x].PlayerCount + " / " + roomList[x].MaxPlayers;

            //set the onclick event
            Button buttonComp = button.GetComponent<Button>();
            string roomName = roomList[x].Name;
            buttonComp.onClick.RemoveAllListeners();
            buttonComp.onClick.AddListener(() => { OnJoinRoomButton(roomName); });
        }
    }

    GameObject CreateRoomButton()
    {
        GameObject buttonObj = Instantiate(roomButtonPrefab, roomListContainer.transform);
        roomButtons.Add(buttonObj);
        return buttonObj;
    }

    public void OnJoinRoomButton(string roomName)
    {
        NetworkManager.instance.JoinRoom(roomName);
    }
    public void OnRefreshButton ()
    {
        Debug.Log("Button Pressed");
        UpdateLobbyBrowserUI();
    }

    public override void OnRoomListUpdate(List<RoomInfo> allRooms)
    {
        Debug.Log("Room Created");
        roomList = allRooms;

    }
}

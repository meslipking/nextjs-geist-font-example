using UnityEngine;
using System.Collections.Generic;
using System;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public static NetworkManager Instance { get; private set; }

    [Header("Network Settings")]
    [SerializeField] private string gameVersion = "1.0";
    [SerializeField] private int maxPlayersPerRoom = 2;
    [SerializeField] private float reconnectDelay = 3f;
    [SerializeField] private float matchmakingTimeout = 30f;

    [Header("Custom Properties")]
    private readonly string PLAYER_READY = "PlayerReady";
    private readonly string SELECTED_ANIMALS = "SelectedAnimals";

    public bool IsConnected => PhotonNetwork.IsConnected;
    public bool InRoom => PhotonNetwork.InRoom;
    public bool IsMaster => PhotonNetwork.IsMasterClient;

    private bool isConnecting;
    private float reconnectTimer;
    private float matchmakingTimer;

    // Events
    public event Action OnConnectedToServer;
    public event Action OnDisconnectedFromServer;
    public event Action<Player> OnPlayerJoined;
    public event Action<Player> OnPlayerLeft;
    public event Action OnJoinedRoom;
    public event Action OnLeftRoom;
    public event Action OnMatchmakingTimeout;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Initialize();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Initialize()
    {
        // Configure Photon settings
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.GameVersion = gameVersion;

        // Set up custom event handlers
        PhotonNetwork.NetworkingClient.EventReceived += OnEventReceived;
    }

    private void Update()
    {
        // Handle reconnection
        if (!PhotonNetwork.IsConnected && !isConnecting)
        {
            reconnectTimer -= Time.deltaTime;
            if (reconnectTimer <= 0)
            {
                reconnectTimer = reconnectDelay;
                Connect();
            }
        }

        // Handle matchmaking timeout
        if (isConnecting)
        {
            matchmakingTimer -= Time.deltaTime;
            if (matchmakingTimer <= 0)
            {
                CancelMatchmaking();
                OnMatchmakingTimeout?.Invoke();
            }
        }
    }

    #region Connection Methods

    public void Connect()
    {
        if (!PhotonNetwork.IsConnected && !isConnecting)
        {
            isConnecting = true;
            matchmakingTimer = matchmakingTimeout;
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public void Disconnect()
    {
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.Disconnect();
        }
    }

    public void JoinRandomRoom()
    {
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.JoinRandomRoom();
        }
    }

    public void CreateRoom()
    {
        if (PhotonNetwork.IsConnected)
        {
            RoomOptions options = new RoomOptions
            {
                MaxPlayers = maxPlayersPerRoom,
                PublishUserId = true
            };
            PhotonNetwork.CreateRoom(null, options);
        }
    }

    public void LeaveRoom()
    {
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
        }
    }

    public void CancelMatchmaking()
    {
        if (isConnecting)
        {
            isConnecting = false;
            PhotonNetwork.Disconnect();
        }
    }

    #endregion

    #region Game State Synchronization

    public void SetPlayerReady(bool ready)
    {
        if (PhotonNetwork.LocalPlayer != null)
        {
            ExitGames.Client.Photon.Hashtable properties = new ExitGames.Client.Photon.Hashtable
            {
                { PLAYER_READY, ready }
            };
            PhotonNetwork.LocalPlayer.SetCustomProperties(properties);
        }
    }

    public void SetSelectedAnimals(List<AnimalType> animals)
    {
        if (PhotonNetwork.LocalPlayer != null)
        {
            string animalData = string.Join(",", animals);
            ExitGames.Client.Photon.Hashtable properties = new ExitGames.Client.Photon.Hashtable
            {
                { SELECTED_ANIMALS, animalData }
            };
            PhotonNetwork.LocalPlayer.SetCustomProperties(properties);
        }
    }

    public bool IsPlayerReady(Player player)
    {
        return player.CustomProperties.TryGetValue(PLAYER_READY, out object ready) && (bool)ready;
    }

    public List<AnimalType> GetPlayerSelectedAnimals(Player player)
    {
        List<AnimalType> animals = new List<AnimalType>();
        if (player.CustomProperties.TryGetValue(SELECTED_ANIMALS, out object animalData))
        {
            string[] animalStrings = ((string)animalData).Split(',');
            foreach (string animal in animalStrings)
            {
                if (Enum.TryParse(animal, out AnimalType type))
                {
                    animals.Add(type);
                }
            }
        }
        return animals;
    }

    #endregion

    #region Game Actions

    public void SendMove(Vector2Int from, Vector2Int to)
    {
        object[] content = new object[] { from.x, from.y, to.x, to.y };
        RaiseEventOptions options = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent(1, content, options, SendOptions.SendReliable);
    }

    public void SendAttack(int attackerId, int defenderId)
    {
        object[] content = new object[] { attackerId, defenderId };
        RaiseEventOptions options = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent(2, content, options, SendOptions.SendReliable);
    }

    public void SendSkillUse(int animalId, int skillIndex, Vector2Int target)
    {
        object[] content = new object[] { animalId, skillIndex, target.x, target.y };
        RaiseEventOptions options = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent(3, content, options, SendOptions.SendReliable);
    }

    #endregion

    #region Photon Callbacks

    public override void OnConnectedToMaster()
    {
        isConnecting = false;
        Debug.Log("Connected to Master Server");
        OnConnectedToServer?.Invoke();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        isConnecting = false;
        Debug.Log($"Disconnected: {cause}");
        OnDisconnectedFromServer?.Invoke();
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined Room");
        OnJoinedRoom?.Invoke();
    }

    public override void OnLeftRoom()
    {
        Debug.Log("Left Room");
        OnLeftRoom?.Invoke();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log($"Player Joined: {newPlayer.NickName}");
        OnPlayerJoined?.Invoke(newPlayer);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log($"Player Left: {otherPlayer.NickName}");
        OnPlayerLeft?.Invoke(otherPlayer);
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("Failed to join random room. Creating new room...");
        CreateRoom();
    }

    #endregion

    #region Custom Event Handling

    private void OnEventReceived(EventData photonEvent)
    {
        byte eventCode = photonEvent.Code;
        object[] data = (object[])photonEvent.CustomData;

        switch (eventCode)
        {
            case 1: // Move
                HandleMoveEvent(data);
                break;
            case 2: // Attack
                HandleAttackEvent(data);
                break;
            case 3: // Skill Use
                HandleSkillEvent(data);
                break;
        }
    }

    private void HandleMoveEvent(object[] data)
    {
        Vector2Int from = new Vector2Int((int)data[0], (int)data[1]);
        Vector2Int to = new Vector2Int((int)data[2], (int)data[3]);
        GameManager.Instance.OnNetworkMove(from, to);
    }

    private void HandleAttackEvent(object[] data)
    {
        int attackerId = (int)data[0];
        int defenderId = (int)data[1];
        GameManager.Instance.OnNetworkAttack(attackerId, defenderId);
    }

    private void HandleSkillEvent(object[] data)
    {
        int animalId = (int)data[0];
        int skillIndex = (int)data[1];
        Vector2Int target = new Vector2Int((int)data[2], (int)data[3]);
        GameManager.Instance.OnNetworkSkillUse(animalId, skillIndex, target);
    }

    #endregion

    private void OnDestroy()
    {
        if (Instance == this)
        {
            if (PhotonNetwork.IsConnected)
            {
                PhotonNetwork.Disconnect();
            }
            PhotonNetwork.NetworkingClient.EventReceived -= OnEventReceived;
            Instance = null;
        }
    }
}

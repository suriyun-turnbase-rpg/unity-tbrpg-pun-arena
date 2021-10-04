using Photon.Pun;
using Photon.Realtime;
using PunArena.Enums;
using PunArena.Event;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PunPlayer = Photon.Realtime.Player;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using UnityEngine.Events;
using System.Linq;

namespace PunArena
{
    public class PunArenaManager : MonoBehaviourPunCallbacks
    {
        public const string CUSTOM_ROOM_ROOM_NAME = "RN";
        public const string CUSTOM_ROOM_ROOM_PASSWORD = "RP";
        public const string CUSTOM_ROOM_PLAYER_ID = "ID";
        public const string CUSTOM_ROOM_PLAYER_NAME = "PN";
        public const string CUSTOM_ROOM_STATE = "RS";
        public const string CUSTOM_PLAYER_STATE = "PS";
        public const string CUSTOM_PLAYER_EXP = "XP";
        public const string CUSTOM_PLAYER_TEAM = "PT";
        public const string CUSTOM_PLAYER_BP = "XP";
        public delegate void OnUpdateActiveCharacter(string entityId);
        public delegate void OnDoSelectedAction(string entityId, string targetEntityId, int action, int seed);
        public delegate void OnUpdateGameplayState(int winnerActorNumber, int loserActorNumber);
        public static PunArenaManager Instance { get; private set; }
        public string battleScene = "PunBattleScene";
        public string roomName = string.Empty;
        public string roomPassword = string.Empty;
        public string saveSelectedRegionKey = "SAVE_SELECTED_REGION";
        public string gameVersion = "1";
        public string masterAddress = "localhost";
        public int masterPort = 5055;
        public string region;
        public UnityEvent onConnecting = new UnityEvent();
        public UnityEvent onConnect = new UnityEvent();
        public UnityEvent onJoiningRoom = new UnityEvent();
        public UnityEvent onJoinRoom = new UnityEvent();
        public StringEvent onJoinRoomFailed = new StringEvent();
        public UnityEvent onLeaveRoom = new UnityEvent();
        public PunPlayerEvent onPlayerJoin = new PunPlayerEvent();
        public PunPlayerEvent onPlayerLeave = new PunPlayerEvent();
        public RoomStateChangeEvent onRoomStateChange = new RoomStateChangeEvent();
        public UnityEvent onRoomListUpdate = new UnityEvent();
        public PlayerPropertiesUpdateEvent onPlayerPropertiesUpdate = new PlayerPropertiesUpdateEvent();
        public event OnUpdateActiveCharacter onUpdateActiveCharacter = null;
        public event OnDoSelectedAction onDoSelectedAction = null;
        public event OnUpdateGameplayState onUpdateGameplayState = null;
        public readonly Dictionary<string, PunArenaRoom> Rooms = new Dictionary<string, PunArenaRoom>();
        private PhotonView view;
        private bool isConnectingToBestRegion;
        private bool isConnectedToBestRegion;
        private bool isConnectingToSelectedRegion;
        private bool waitForPlayerAction;
        private Coroutine countDownToStartGameCoroutine;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            view = GetComponent<PhotonView>();
            if (view == null)
                view = gameObject.AddComponent<PhotonView>();
            view.ViewID = 999;

            PhotonNetwork.NetworkingClient.AppId = PhotonNetwork.PhotonServerSettings.AppSettings.AppIdRealtime;
        }

        public void LoadBattleScene(bool loadIfNotLoaded = false)
        {
            GameInstance.Singleton.LoadSceneIfNotLoaded(battleScene, loadIfNotLoaded);
        }

        public void CreateRoom()
        {
            var roomOptions = new RoomOptions();
            roomOptions.CustomRoomPropertiesForLobby = new string[]
            {
                CUSTOM_ROOM_ROOM_NAME,
                CUSTOM_ROOM_ROOM_PASSWORD,
                CUSTOM_ROOM_PLAYER_ID,
                CUSTOM_ROOM_PLAYER_NAME,
                CUSTOM_ROOM_STATE,
            };
            roomOptions.MaxPlayers = 2;
            roomOptions.PublishUserId = true;
            PhotonNetwork.CreateRoom(null, roomOptions, null);
        }

        public override void OnCreatedRoom()
        {
            Hashtable customProperties = new Hashtable();
            customProperties[CUSTOM_ROOM_ROOM_NAME] = roomName;
            customProperties[CUSTOM_ROOM_ROOM_PASSWORD] = roomPassword;
            customProperties[CUSTOM_ROOM_PLAYER_ID] = PhotonNetwork.LocalPlayer.UserId;
            customProperties[CUSTOM_ROOM_PLAYER_NAME] = PhotonNetwork.LocalPlayer.NickName;
            customProperties[CUSTOM_ROOM_STATE] = (byte)ERoomState.WaitPlayersToReady;
            PhotonNetwork.CurrentRoom.SetCustomProperties(customProperties);
        }

        public void JoinRoom(string roomName)
        {
            PhotonNetwork.JoinRoom(roomName);
            onJoiningRoom.Invoke();
        }

        public override void OnJoinedRoom()
        {
            PhotonNetwork.LocalPlayer.SetState(EPlayerState.None);
            PhotonNetwork.LocalPlayer.SetExp(Player.CurrentPlayer.exp);
            // TODO: Calculate and set team bp value
            PhotonNetwork.LocalPlayer.SetBp(0);
            onJoinRoom.Invoke();
        }

        public override void OnJoinRoomFailed(short returnCode, string message)
        {
            onJoinRoomFailed.Invoke(message);
        }

        public override void OnLeftRoom()
        {
            onLeaveRoom.Invoke();
            if (countDownToStartGameCoroutine != null)
                StopCoroutine(countDownToStartGameCoroutine);
        }

        public override void OnPlayerEnteredRoom(PunPlayer newPlayer)
        {
            onPlayerJoin.Invoke(newPlayer);
            // Set player team
            var players = PhotonNetwork.CurrentRoom.Players.Values.ToList();
            players.Sort((a, b) =>
            {
                return a.UserId.CompareTo(b.UserId);
            });
            byte team = 0;
            for (int i = 0; i < players.Count; ++i)
            {
                var player = players[i];
                player.SetTeam(team++);
            }
        }

        public override void OnPlayerLeftRoom(PunPlayer otherPlayer)
        {
            onPlayerLeave.Invoke(otherPlayer);
            if (countDownToStartGameCoroutine != null)
                StopCoroutine(countDownToStartGameCoroutine);
        }

        public override void OnRoomListUpdate(List<RoomInfo> roomList)
        {
            foreach (RoomInfo room in roomList)
            {
                // Remove room from cached room list if it got closed, became invisible or was marked as removed
                if (!room.IsOpen || !room.IsVisible || room.RemovedFromList)
                {
                    if (Rooms.ContainsKey(room.Name))
                    {
                        Rooms.Remove(room.Name);
                    }

                    continue;
                }

                PunArenaRoom roomData = new PunArenaRoom()
                {
                    name = room.Name,
                    roomName = (string)room.CustomProperties[CUSTOM_ROOM_ROOM_NAME],
                    roomPassword = (string)room.CustomProperties[CUSTOM_ROOM_ROOM_PASSWORD],
                    playerId = (string)room.CustomProperties[CUSTOM_ROOM_PLAYER_ID],
                    playerName = (string)room.CustomProperties[CUSTOM_ROOM_PLAYER_NAME],
                    state = (ERoomState)room.CustomProperties[CUSTOM_ROOM_STATE],
                    playerCount = room.PlayerCount,
                    maxPlayers = room.MaxPlayers,
                };
                if (Rooms.ContainsKey(room.Name))
                {
                    Rooms[room.Name] = roomData;
                }
                else
                {
                    Rooms.Add(room.Name, roomData);
                }
            }
            onRoomListUpdate.Invoke();
        }

        public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
        {
            if (propertiesThatChanged.ContainsKey(CUSTOM_ROOM_STATE))
            {
                onRoomStateChange.Invoke(PunArenaExtensions.GetRoomState());
                if (PhotonNetwork.IsMasterClient)
                {
                    if (PunArenaExtensions.GetRoomState() == ERoomState.CountDownToStartGame)
                    {
                        countDownToStartGameCoroutine = StartCoroutine(CountDownToStartGame());
                    }
                    else
                    {
                        if (countDownToStartGameCoroutine != null)
                            StopCoroutine(countDownToStartGameCoroutine);
                    }
                }
            }
        }

        public override void OnPlayerPropertiesUpdate(PunPlayer targetPlayer, Hashtable changedProps)
        {
            if (changedProps.ContainsKey(CUSTOM_PLAYER_STATE))
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    if (PhotonNetwork.CurrentRoom.PlayerCount >= 2)
                    {
                        if (PunArenaExtensions.GetRoomState() < ERoomState.WaitPlayersToEnterGame)
                        {
                            bool playersReady = true;
                            foreach (var player in PhotonNetwork.CurrentRoom.Players.Values)
                            {
                                if (player.GetState() < EPlayerState.Ready)
                                {
                                    playersReady = false;
                                }
                            }
                            // Players are ready?, count down to start game
                            if (playersReady)
                            {
                                PunArenaExtensions.SetRoomState(ERoomState.CountDownToStartGame);
                            }
                            else
                            {
                                PunArenaExtensions.SetRoomState(ERoomState.WaitPlayersToReady);
                            }
                        }
                        else
                        {
                            bool playersInGame = true;
                            foreach (var player in PhotonNetwork.CurrentRoom.Players.Values)
                            {
                                if (player.GetState() < EPlayerState.InGame)
                                {
                                    playersInGame = false;
                                }
                            }
                            // Players are in game?, game started
                            if (playersInGame)
                            {
                                PunArenaExtensions.SetRoomState(ERoomState.Battle);
                            }
                            else
                            {
                                PunArenaExtensions.SetRoomState(ERoomState.WaitPlayersToEnterGame);
                            }
                        }
                    }
                }
            }
            onPlayerPropertiesUpdate.Invoke(targetPlayer, changedProps);
        }

        IEnumerator CountDownToStartGame()
        {
            yield return new WaitForSecondsRealtime(PunArenaConsts.ENTER_GAME_COUNT_DOWN);
            if (PhotonNetwork.IsMasterClient)
            {
                PunArenaExtensions.SetRoomState(ERoomState.WaitPlayersToEnterGame);
            }
        }

        #region Cloud server connection
        public void ConnectToBestCloudServer()
        {
            // Delete saved best region, to re-ping all regions, to fix unknow ping problem
            ServerSettings.ResetBestRegionCodeInPreferences();
            PhotonNetwork.NetworkingClient.SerializationProtocol = ExitGames.Client.Photon.SerializationProtocol.GpBinaryV18;
            PhotonNetwork.AutomaticallySyncScene = false;
            PhotonNetwork.OfflineMode = false;
            PhotonNetwork.AuthValues = new AuthenticationValues(Player.CurrentPlayer.Id);
            PhotonNetwork.ConnectToBestCloudServer();
            isConnectingToBestRegion = true;
            onConnecting.Invoke();
        }

        public void ConnectToRegion()
        {
            // Hacking PUN, It seems like PUN won't connect to name server when call `PhotonNetwork.ConnectToRegion()`
            // Have to connect to best cloud server to make it connect to name server to get all regions list
            if (!isConnectedToBestRegion)
            {
                // If not connected to best region once, connect to best region to ping all regions
                ConnectToBestCloudServer();
                isConnectingToSelectedRegion = true;
            }
            else
            {
                // It's ready to connect to selected region because it was connected to best region once and pinged all regions
                PhotonNetwork.NetworkingClient.SerializationProtocol = ExitGames.Client.Photon.SerializationProtocol.GpBinaryV18;
                PhotonNetwork.AutomaticallySyncScene = false;
                PhotonNetwork.OfflineMode = false;
                PhotonNetwork.AuthValues = new AuthenticationValues(Player.CurrentPlayer.Id);
                PhotonNetwork.ConnectToRegion(region);
            }
            onConnecting.Invoke();
        }

        public override void OnConnectedToMaster()
        {
            PlayerPrefs.SetString(saveSelectedRegionKey, PhotonNetwork.CloudRegion.TrimEnd('/', '*'));
            PlayerPrefs.Save();
            if (isConnectingToBestRegion)
            {
                isConnectingToBestRegion = false;
                isConnectedToBestRegion = true;
            }
            if (isConnectingToSelectedRegion)
            {
                isConnectingToSelectedRegion = false;
                PhotonNetwork.Disconnect();
                ConnectToRegion();
            }
            else
            {
                PhotonNetwork.JoinLobby();
            }
            PhotonNetwork.LocalPlayer.NickName = Player.CurrentPlayer.ProfileName;
            onConnect.Invoke();
        }
        #endregion

        #region Networking messages
        public void TogglePlayerReadyState()
        {
            if (PunArenaExtensions.GetRoomState() >= ERoomState.WaitPlayersToEnterGame) return;
            if (PhotonNetwork.LocalPlayer.GetState() < EPlayerState.Ready)
            {
                PhotonNetwork.LocalPlayer.SetState(EPlayerState.Ready);
            }
            else
            {
                PhotonNetwork.LocalPlayer.SetState(EPlayerState.None);
            }
        }

        public void SetPlayerEnterGameState()
        {
            if (PunArenaExtensions.GetRoomState() < ERoomState.WaitPlayersToEnterGame) return;
            if (PhotonNetwork.LocalPlayer.GetState() < EPlayerState.InGame)
                PhotonNetwork.LocalPlayer.SetState(EPlayerState.InGame);
        }

        public void SendUpdateActiveCharacter(string id)
        {
            if (!PhotonNetwork.IsMasterClient) return;
            if (PunArenaExtensions.GetRoomState() != ERoomState.Battle) return;
            waitForPlayerAction = true;
            photonView.RPC(nameof(SendUpdateActiveCharacterFromMaster), RpcTarget.Others, id);
            if (onUpdateActiveCharacter != null)
                onUpdateActiveCharacter.Invoke(id);
        }

        [PunRPC]
        private void SendUpdateActiveCharacterFromMaster(string id)
        {
            if (onUpdateActiveCharacter != null)
                onUpdateActiveCharacter.Invoke(id);
        }

        public void SendDoSelectedAction(string entityId, string targetEntityId, int action, int seed)
        {
            photonView.RPC(nameof(SendDoSelectedActionToMaster), RpcTarget.MasterClient, entityId, targetEntityId, action, seed);
        }

        [PunRPC]
        private void SendDoSelectedActionToMaster(string entityId, string targetEntityId, int action, int seed)
        {
            if (!PhotonNetwork.IsMasterClient) return;
            if (PunArenaExtensions.GetRoomState() != ERoomState.Battle) return;
            if (!waitForPlayerAction) return;
            waitForPlayerAction = false;
            photonView.RPC(nameof(SendDoSelectedActionFromMaster), RpcTarget.Others, entityId, targetEntityId, action, seed);
            if (onDoSelectedAction != null)
                onDoSelectedAction.Invoke(entityId, targetEntityId, action, seed);
        }

        [PunRPC]
        private void SendDoSelectedActionFromMaster(string entityId, string targetEntityId, int action, int seed)
        {
            if (onDoSelectedAction != null)
                onDoSelectedAction.Invoke(entityId, targetEntityId, action, seed);
        }

        public void SendUpdateGameplayState(int winnerActorNumber, int loserActorNumber)
        {
            if (!PhotonNetwork.IsMasterClient) return;
            if (PunArenaExtensions.GetRoomState() != ERoomState.Battle) return;
            // TODO: Store gameplay state in-case player disconnect and reconnect they won't lose data.
            photonView.RPC(nameof(SendUpdateGameplayStateFromMaster), RpcTarget.Others, winnerActorNumber, loserActorNumber);
            if (onUpdateGameplayState != null)
                onUpdateGameplayState.Invoke(winnerActorNumber, loserActorNumber);
        }

        [PunRPC]
        private void SendUpdateGameplayStateFromMaster(int winnerActorNumber, int loserActorNumber)
        {
            if (onUpdateGameplayState != null)
                onUpdateGameplayState.Invoke(winnerActorNumber, loserActorNumber);
        }
        #endregion
    }
}

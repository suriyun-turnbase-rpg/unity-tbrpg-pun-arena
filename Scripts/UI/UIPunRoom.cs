using PunArena.Enums;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Photon.Pun;
using PunPlayer = Photon.Realtime.Player;

namespace PunArena.UI
{
    public class UIPunRoom : UIBase
    {
        public UIPunRoomPlayer playerPrefab;
        public Transform playerContainer;
        public Text textCountDown;

        private Coroutine countDownCoroutine = null;
        private readonly Dictionary<int, UIPunRoomPlayer> uiRoomPlayers = new Dictionary<int, UIPunRoomPlayer>();

        private void OnEnable()
        {
            if (textCountDown)
                textCountDown.text = string.Empty;
            PunArenaManager.Instance.onRoomStateChange.AddListener(OnStateChange);
            PunArenaManager.Instance.onPlayerJoin.AddListener(OnJoin);
            PunArenaManager.Instance.onPlayerLeave.AddListener(OnLeave);
            UpdatePlayers();
        }

        private void OnDisable()
        {
            PunArenaManager.Instance.onRoomStateChange.RemoveListener(OnStateChange);
            PunArenaManager.Instance.onPlayerJoin.RemoveListener(OnJoin);
            PunArenaManager.Instance.onPlayerLeave.RemoveListener(OnLeave);
        }

        private void OnStateChange(ERoomState state)
        {
            if (textCountDown)
                textCountDown.text = string.Empty;
            if (countDownCoroutine != null)
            {
                StopCoroutine(countDownCoroutine);
                countDownCoroutine = null;
            }
            switch (state)
            {
                case ERoomState.CountDownToStartGame:
                    // Count down before enter game
                    countDownCoroutine = StartCoroutine(CountDownRoutine());
                    break;
                case ERoomState.WaitPlayersToEnterGame:
                    // Load battle scene to enter game
                    PunArenaManager.Instance.LoadBattleScene();
                    break;
            }
        }

        private IEnumerator CountDownRoutine()
        {
            int countDown = PunArenaConsts.ENTER_GAME_COUNT_DOWN;
            do
            {
                if (textCountDown)
                    textCountDown.text = countDown.ToString();
                yield return new WaitForSeconds(1);
                countDown--;
            }
            while (countDown > 0);
            if (textCountDown)
                textCountDown.text = string.Empty;
        }

        private void OnJoin(PunPlayer player)
        {
            UpdatePlayers();
        }

        private void OnLeave(PunPlayer player)
        {
            UpdatePlayers();
        }

        private void UpdatePlayers()
        {
            playerContainer.RemoveAllChildren();
            uiRoomPlayers.Clear();
            foreach (var player in PhotonNetwork.CurrentRoom.Players.Values)
            {
                UIPunRoomPlayer newRoomUI = Instantiate(playerPrefab, playerContainer);
                newRoomUI.Player = player;
                newRoomUI.Show();
                uiRoomPlayers[player.ActorNumber] = newRoomUI;
            }
        }

        public void OnClickReady()
        {
            PunArenaManager.Instance.TogglePlayerReadyState();
        }

        public void OnClickLeave()
        {
            PhotonNetwork.LeaveRoom();
        }
    }
}

using PunArena.Enums;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Photon.Pun;

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
            PunArenaManager.Instance.onRoomError.AddListener(OnError);
            PunArenaManager.Instance.onRoomStateChange.AddListener(OnStateChange);
            PunArenaManager.Instance.onLeaveRoom.AddListener(OnLeave);
            UpdateRoomState(PunArenaExtensions.GetRoomState());
        }

        private void OnDisable()
        {
            PunArenaManager.Instance.onRoomError.RemoveListener(OnError);
            PunArenaManager.Instance.onRoomStateChange.RemoveListener(OnStateChange);
            PunArenaManager.Instance.onLeaveRoom.RemoveListener(OnLeave);
        }

        private void OnError(int code, string message)
        {

        }

        private void OnStateChange(ERoomState state)
        {
            UpdateRoomState(state);
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

        private void OnLeave()
        {

        }

        private void UpdateRoomState(ERoomState state)
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

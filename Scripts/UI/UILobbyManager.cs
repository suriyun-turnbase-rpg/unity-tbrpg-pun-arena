using UnityEngine;

namespace PunArena.UI
{
    public class UILobbyManager : MonoBehaviour
    {
        public UIRoomList uiRoomList;
        public UIRoom uiRoom;

        private void Start()
        {
            uiRoom.Hide();
            uiRoomList.Show();
            PunArenaManager.Instance.onJoinRoom.AddListener(OnJoin);
            PunArenaManager.Instance.onLeaveRoom.AddListener(OnLeave);
        }

        private void OnDestroy()
        {
            PunArenaManager.Instance.onJoinRoom.RemoveListener(OnJoin);
            PunArenaManager.Instance.onLeaveRoom.RemoveListener(OnLeave);
        }

        private void OnJoin()
        {
            uiRoomList.Hide();
            uiRoom.Show();
        }

        private void OnLeave()
        {
            uiRoom.Hide();
            uiRoomList.Show();
        }
    }
}

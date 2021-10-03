using UnityEngine;

namespace PunArena.UI
{
    public class UIPunLobbyManager : MonoBehaviour
    {
        public UIPunRoomList uiRoomList;
        public UIPunRoom uiRoom;

        private void Start()
        {
            uiRoom.Hide();
            uiRoomList.Show();
            PunArenaManager.Instance.onJoinRoom.AddListener(OnJoin);
            PunArenaManager.Instance.onLeaveRoom.AddListener(OnLeave);
            PunArenaManager.Instance.ConnectToBestCloudServer();
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

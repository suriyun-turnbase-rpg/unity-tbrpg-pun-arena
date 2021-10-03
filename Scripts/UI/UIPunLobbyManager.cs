using UnityEngine;

namespace PunArena.UI
{
    public class UIPunLobbyManager : MonoBehaviour
    {
        public UIPunRoomList uiRoomList;
        public UIPunRoom uiRoom;
        public GameObject preloader;

        private void Start()
        {
            uiRoom.Hide();
            uiRoomList.Show();
            PunArenaManager.Instance.onConnecting.AddListener(OnConnecting);
            PunArenaManager.Instance.onConnect.AddListener(OnConnect);
            PunArenaManager.Instance.onJoiningRoom.AddListener(OnJoining);
            PunArenaManager.Instance.onJoinRoom.AddListener(OnJoin);
            PunArenaManager.Instance.onLeaveRoom.AddListener(OnLeave);
            PunArenaManager.Instance.ConnectToBestCloudServer();
        }

        private void OnDestroy()
        {
            PunArenaManager.Instance.onJoinRoom.RemoveListener(OnJoin);
            PunArenaManager.Instance.onLeaveRoom.RemoveListener(OnLeave);
        }

        private void OnConnecting()
        {
            if (preloader)
                preloader.SetActive(true);
        }

        private void OnConnect()
        {
            if (preloader)
                preloader.SetActive(false);
        }

        private void OnJoining()
        {
            if (preloader)
                preloader.SetActive(true);
        }

        private void OnJoin()
        {
            if (preloader)
                preloader.SetActive(false);
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

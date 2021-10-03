using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace PunArena.UI
{
    public class UIPunRoomList : UIBase
    {
        public UIPunRoomListEntry entryPrefab;
        public Transform entryContainer;
        public UIPunRoomPassword uiRoomPassword;

        private void OnEnable()
        {
            OnRoomListUpdate();
            PunArenaManager.Instance.onRoomListUpdate.AddListener(OnRoomListUpdate);
        }

        private void OnRoomListUpdate()
        {
            var rooms = PunArenaManager.Instance.Rooms.Values.ToArray();
            entryContainer.RemoveAllChildren();
            for (int i = 0; i < rooms.Length; ++i)
            {
                UIPunRoomListEntry newRoomUI = Instantiate(entryPrefab, entryContainer);
                newRoomUI.uiRoomList = this;
                newRoomUI.RoomData = rooms[i];
                newRoomUI.RoomTitle = rooms[i].roomName;
                newRoomUI.Show();
            }
        }

        public void ShowUIRoomPassword(PunArenaRoom room, string roomTitle)
        {
            uiRoomPassword.RoomData = room;
            uiRoomPassword.Show();
        }
    }
}

using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace PunArena.UI
{
    public class UIRoomList : UIBase
    {
        public UIRoomListEntry entryPrefab;
        public Transform entryContainer;
        public UIRoomPassword uiRoomPassword;

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
                UIRoomListEntry newRoomUI = Instantiate(entryPrefab, entryContainer);
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

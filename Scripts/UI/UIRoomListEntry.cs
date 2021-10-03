using UnityEngine.UI;

namespace PunArena.UI
{
    public class UIRoomListEntry : UIBase
    {
        public UIRoomList uiRoomList;
        public Text textTitle;

        public PunArenaRoom RoomData { get; set; }
        public bool HasPassword { get { return !string.IsNullOrEmpty(RoomData.roomPassword); } }

        private string _roomTitle;
        public string RoomTitle
        {
            get { return _roomTitle; }
            set
            {
                _roomTitle = value;
                if (textTitle)
                    textTitle.text = value;
            }
        }

        public void OnClickJoin()
        {
            // Show room password UI if the room password is required
            if (HasPassword)
                uiRoomList.ShowUIRoomPassword(RoomData, RoomTitle);
            else
                PunArenaManager.Instance.JoinRoom(RoomData.name);
        }
    }
}

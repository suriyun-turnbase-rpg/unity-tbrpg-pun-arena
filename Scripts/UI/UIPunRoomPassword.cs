using RealtimeArena.Room;
using UnityEngine.UI;

namespace PunArena.UI
{
    public class UIPunRoomPassword : UIBase
    {
        public Text textTitle;
        public InputField inputPassword;

        public PunArenaRoom RoomData { get; set; }

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

        protected override void Awake()
        {
            base.Awake();
            if (inputPassword)
            {
                inputPassword.inputType = InputField.InputType.Password;
                inputPassword.contentType = InputField.ContentType.Pin;
                inputPassword.characterLimit = GameRoomConsts.MAX_PASSWORD_LENGTH;
            }
        }

        public void OnClickJoin()
        {
            if (inputPassword && !inputPassword.text.Equals(RoomData.roomPassword))
            {
                // TODO: Wrong password
                return;
            }
            PunArenaManager.Instance.JoinRoom(RoomData.name);
        }
    }
}
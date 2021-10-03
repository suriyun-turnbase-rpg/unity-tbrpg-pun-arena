using System.Collections.Generic;
using UnityEngine.UI;

namespace PunArena.UI
{
    public class UIPunRoomCreate : UIBase
    {
        public InputField inputTitle;
        public InputField inputPassword;

        protected override void Awake()
        {
            base.Awake();
            if (inputTitle)
            {
                inputTitle.inputType = InputField.InputType.Standard;
                inputTitle.contentType = InputField.ContentType.Name;
            }
            if (inputPassword)
            {
                inputPassword.inputType = InputField.InputType.Password;
                inputPassword.contentType = InputField.ContentType.Pin;
                inputPassword.characterLimit = PunArenaConsts.MAX_PASSWORD_LENGTH;
            }
        }

        public void OnClickCreate()
        {
            Dictionary<string, object> options = new Dictionary<string, object>();
            if (inputTitle && !string.IsNullOrEmpty(inputTitle.text))
                PunArenaManager.Instance.roomName = inputTitle.text;
            if (inputPassword && !string.IsNullOrEmpty(inputPassword.text))
                PunArenaManager.Instance.roomPassword = inputPassword.text;
            PunArenaManager.Instance.CreateRoom();
        }
    }
}

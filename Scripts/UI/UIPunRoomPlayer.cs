using PunArena.Enums;
using UnityEngine;
using UnityEngine.UI;
using PunPlayer = Photon.Realtime.Player;

namespace PunArena.UI
{
    public class UIPunRoomPlayer : UIBase
    {
        public Text textPlayerName;
        public Text textPlayerLevel;
        public Text textTeamBP;
        public GameObject[] readyObjects;
        public GameObject[] notReadyObjects;

        private PunPlayer _player;
        public PunPlayer Player
        {
            get { return _player; }
            set
            {
                _player = value;
                var player = new Player();
                player.Exp = _player.GetExp();
                PlayerName = _player.NickName;
                PlayerLevel = player.Level;
                TeamBP = _player.GetBp();
                IsReady = _player.GetState() >= EPlayerState.Ready;
            }
        }

        private string _playerName;
        public string PlayerName
        {
            get { return _playerName; }
            set
            {
                _playerName = value;
                if (textPlayerName)
                    textPlayerName.text = value;
            }
        }

        private int _playerLevel;
        public int PlayerLevel
        {
            get { return _playerLevel; }
            set
            {
                _playerLevel = value;
                if (textPlayerLevel)
                    textPlayerLevel.text = value.ToString("N0");
            }
        }

        private int _teamBP;
        public int TeamBP
        {
            get { return _teamBP; }
            set
            {
                _teamBP = value;
                if (textTeamBP)
                    textTeamBP.text = value.ToString("N0");
            }
        }

        private bool _isReady;
        public bool IsReady
        {
            get { return _isReady; }
            set
            {
                _isReady = value;
                if (readyObjects != null && readyObjects.Length > 0)
                {
                    for (int i = 0; i < readyObjects.Length; ++i)
                    {
                        readyObjects[i].SetActive(value);
                    }
                }
                if (notReadyObjects != null && notReadyObjects.Length > 0)
                {
                    for (int i = 0; i < notReadyObjects.Length; ++i)
                    {
                        notReadyObjects[i].SetActive(!value);
                    }
                }
            }
        }
    }
}

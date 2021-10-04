using PunPlayer = Photon.Realtime.Player;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Photon.Pun;
using PunArena.Enums;

namespace PunArena
{
    public static class PunArenaExtensions
    {
        public static EPlayerState GetState(this PunPlayer player)
        {
            return player.GetRoomPlayerProperty<EPlayerState>(PunArenaManager.CUSTOM_PLAYER_STATE);
        }

        public static int GetExp(this PunPlayer player)
        {
            return player.GetRoomPlayerProperty<int>(PunArenaManager.CUSTOM_PLAYER_EXP);
        }

        public static int GetBp(this PunPlayer player)
        {
            return player.GetRoomPlayerProperty<int>(PunArenaManager.CUSTOM_PLAYER_EXP);
        }

        public static byte GetTeam(this PunPlayer player)
        {
            return player.GetRoomPlayerProperty<byte>(PunArenaManager.CUSTOM_PLAYER_TEAM);
        }

        public static void SetState(this PunPlayer player, EPlayerState state)
        {
            player.SetRoomPlayerProperty(PunArenaManager.CUSTOM_PLAYER_STATE, state);
        }

        public static void SetExp(this PunPlayer player, int exp)
        {
            player.SetRoomPlayerProperty(PunArenaManager.CUSTOM_PLAYER_EXP, exp);
        }

        public static void SetBp(this PunPlayer player, int bp)
        {
            player.SetRoomPlayerProperty(PunArenaManager.CUSTOM_PLAYER_EXP, bp);
        }

        public static void SetTeam(this PunPlayer player, byte team)
        {
            player.SetRoomPlayerProperty(PunArenaManager.CUSTOM_PLAYER_TEAM, team);
        }

        public static ERoomState GetRoomState()
        {
            return GetRoomProperty<ERoomState>(PunArenaManager.CUSTOM_ROOM_STATE);
        }

        public static void SetRoomState(ERoomState state)
        {
            SetRoomProperty(PunArenaManager.CUSTOM_ROOM_STATE, state);
        }

        public static T GetRoomProperty<T>(string key, T defaultValue = default)
        {
            if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(key))
                return (T)PhotonNetwork.CurrentRoom.CustomProperties[key];
            return defaultValue;
        }

        public static void SetRoomProperty<T>(string key, T value)
        {
            if (PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable()
                {
                    { key, value }
                });
            }
        }

        public static T GetRoomPlayerProperty<T>(this PunPlayer player, string key, T defaultValue = default)
        {
            if (player.CustomProperties.ContainsKey(key))
                return (T)player.CustomProperties[key];
            return defaultValue;
        }

        public static void SetRoomPlayerProperty<T>(this PunPlayer player, string key, T value)
        {
            Hashtable properties = player.CustomProperties;
            if (properties.ContainsKey(key))
                properties[key] = value;
            else
                properties.Add(key, value);
            player.SetCustomProperties(properties);
        }
    }
}

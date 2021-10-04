using PunArena.Enums;

namespace PunArena
{
    [System.Serializable]
    public struct PunArenaRoom
    {
        public string name;
        public string roomName;
        public string roomPassword;
        public string playerId;
        public string playerName;
        public ERoomState state;
        public int playerCount;
        public int maxPlayers;
    }
}

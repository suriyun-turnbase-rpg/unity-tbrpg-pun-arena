using System.Collections.Generic;

namespace PunArena.Message
{
    [System.Serializable]
    public struct UpdateGameplayStateMsg
    {
        public int winnerActorNumber;
        public int loserActorNumber;
        public List<UpdateCharacterEntityMsg> characters;
    }
}

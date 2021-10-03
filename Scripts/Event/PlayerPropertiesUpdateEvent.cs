using System;
using UnityEngine.Events;
using PunPlayer = Photon.Realtime.Player;
using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace PunArena.Event
{
    /// <summary>
    /// [PunPlayer targetPlayer, Hashtable changedProps]
    /// </summary>
    [Serializable]
    public class PlayerPropertiesUpdateEvent : UnityEvent<PunPlayer, Hashtable>
    {
    }
}

using System;
using UnityEngine.Events;
using PunPlayer = Photon.Realtime.Player;

namespace PunArena.Event
{
    /// <summary>
    /// [PunPlayer player]
    /// </summary>
    [Serializable]
    public class PunPlayerEvent : UnityEvent<PunPlayer>
    {
    }
}

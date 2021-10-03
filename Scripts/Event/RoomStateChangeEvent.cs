using PunArena.Enums;
using System;
using UnityEngine.Events;

namespace PunArena.Event
{
    /// <summary>
    /// [ERoomState state]
    /// </summary>
    [Serializable]
    public class RoomStateChangeEvent : UnityEvent<ERoomState>
    {
    }
}

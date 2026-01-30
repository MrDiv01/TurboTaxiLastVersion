using System;
using System.Collections.Generic;
using System.Text;

namespace TurboTaxi.Domain.Enums
{
    public enum RideStatus
    {
        Pending = 0,   // request atılıb, sürücü hələ accept etməyib
        Approved = 1,  // sürücü accept edib
        Arrived = 2,   // sürücü pickup nöqtəsinə çatıb
        OnWay = 3,     // ride başladı, user maşındadır
        Canceled = 4,
        Completed = 5
    }

}

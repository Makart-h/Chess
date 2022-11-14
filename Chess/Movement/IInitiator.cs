using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess.Movement;

internal interface IInitiator : IDisposable, IUpdateable
{
    IMovable Target { get; set; }
    EventHandler DestinationReached { get; set; }
    float Velocity { get; set; }
}

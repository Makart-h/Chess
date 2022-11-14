using System;
using System.Collections.Generic;

namespace Chess.Graphics;

internal interface IOverlayFactory
{
    IEnumerable<IMovableDrawable> CreateOverlays(object sender, EventArgs e);
}

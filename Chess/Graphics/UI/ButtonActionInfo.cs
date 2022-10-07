using System;

namespace Chess.Graphics.UI;

internal struct ButtonActionInfo
{
    public Action OnClick { get; set; }
    public Action OnRelease { get; set; }
    public Action OnHoverStarted { get; set; }
    public Action OnHoverEnded { get; set; }
}

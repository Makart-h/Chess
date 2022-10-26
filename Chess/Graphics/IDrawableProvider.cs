using System.Collections.Generic;

namespace Chess.Graphics;

internal interface IDrawableProvider
{
    public IEnumerable<DrawableObject> GetDrawableObjects();
}

using System.Collections.Generic;

namespace Chess.Graphics;

internal interface ITextProvider
{
    public IEnumerable<TextObject> GetTextObjects();
}

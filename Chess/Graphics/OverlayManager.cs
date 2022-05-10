using System;

namespace Chess.Graphics
{
  public class OverlayManager
  {
   private readonly List<SquareOverlay> selections;
   private readonly List<SquareOverlay> moves;

   public void OnPieceSelected(object sender, PieceSelectedEventArgs args)
   {

   }
   public void OnPieceMoved(object sender, PieceMovedEventArgs args)
   {
   }

  }
}

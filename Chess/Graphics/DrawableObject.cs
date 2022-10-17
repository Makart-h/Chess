using Chess.Movement;
using Microsoft.Xna.Framework;

namespace Chess.Graphics;

internal class DrawableObject
{
    public Model Model { get; protected set; }
    public Rectangle DestinationRectangle { get; protected set; }
    public Vector2 Position { get; protected set; }
    public Color Color { get; set; }
    public DrawableObject(Model model, Rectangle destinationRectangle)
    {
        Model = model;
        Position = new Vector2(destinationRectangle.X, destinationRectangle.Y);
        DestinationRectangle = destinationRectangle;
        Color = Color.White;
    }
    public void MoveObject(Vector2 vector)
    { 
        Position += vector;
        DestinationRectangle = new Rectangle((int)Position.X, (int)Position.Y, DestinationRectangle.Width, DestinationRectangle.Height);
    }
    public void RecalculatePosition() 
    { 
        Position = MovementManager.RecalculateVector(Position);
        DestinationRectangle = new Rectangle((int)Position.X, (int)Position.Y, DestinationRectangle.Width, DestinationRectangle.Height);
    }
}

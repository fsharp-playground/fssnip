namespace NewWareOrder

open System
open System.Collections.Generic
open System.Linq
open System.Text
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Audio
open Microsoft.Xna.Framework.Content
open Microsoft.Xna.Framework.GamerServices
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework.Input
open Microsoft.Xna.Framework.Media


type Player(animation : Animation, position : Vector2) =
    /// Position of the Player relative to the upper left side of the screen
    member val Position = position with get, set
    /// State of the player
    member val Active = true with get, set
    /// Amount of hit points that player has
    member val Health = 100 with get, set
    /// Animation representing the player
    member o.Animation = animation
    /// Get the width of the player ship
    member o.Width = Animation.FrameWidth
    /// Get the height of the player ship
    member o.Height = Animation.FrameHeight

    /// Update the player animation
    member o.Update(gameTime) =
        animation.Position = o.Position
        animation.Update(gameTime)

    /// Draw the player
    member o.Draw(priteBatch) =
        animation.Draw(spriteBatch)

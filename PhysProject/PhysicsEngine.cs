using System;
using Microsoft.Xna.Framework;

namespace PhysProject
{
    public static class PhysicsEngine
    {
        public const float Gravity = 1000f;       
        public const float GroundDrag = 0.8f;     
        public const float AirDrag = 0.2f;        
        public const float Epsilon = 0.1f;        

        public static void ApplyGravity(ref Vector2 velocity, bool isGrounded, float deltaTime)
        {
            if (!isGrounded)
            {
                velocity.Y += Gravity * deltaTime;
            }
            else if (Math.Abs(velocity.Y) < Epsilon)
            {
                velocity.Y = 0;
            }
        }

        public static void ApplyDrag(ref Vector2 velocity, bool isGrounded, float deltaTime)
        {
            float drag = isGrounded ? GroundDrag : AirDrag;
            velocity.X *= (1 - drag * deltaTime * 60);
            
            if (Math.Abs(velocity.X) < Epsilon) 
                velocity.X = 0;
        }

        public static void ApplyMovement(ref Vector2 velocity, float acceleration, 
                                       float maxSpeed, float inputDirection, 
                                       float deltaTime)
        {
            velocity.X += acceleration * inputDirection * deltaTime;
            velocity.X = MathHelper.Clamp(velocity.X, -maxSpeed, maxSpeed);
        }

        public static bool CheckGroundCollision(Vector2 position, float groundY, float height)
        {
            return position.Y >= groundY - height - Epsilon;
        }

        public static void HandleGroundClamping(ref Vector2 position, ref Vector2 velocity, 
                                              float groundY, float height)
        {
            if (position.Y > groundY - height)
            {
                position.Y = groundY - height;
                if (velocity.Y > 0) velocity.Y = 0;
            }
        }
    }
}
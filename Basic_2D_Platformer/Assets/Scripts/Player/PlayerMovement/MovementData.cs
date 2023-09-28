using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GMDG.Basic2DPlatformer.PlayerMovement
{
    [CreateAssetMenu(fileName = "MovementData", menuName = "ScriptableObjects/MovementData")]
    public class MovementData : ScriptableObject
    {
        // MaxJumpHeigh 3.5
        // MaxJumpWidth 9

        public float WalkingSpeed;
        public float WalkingBuildUpSpeed;

        public float FallingSpeed;
        public float FallingBuildUpSpeed;
        
        public float Gravity;
        public float GravityMultiplier;

        public float MaxYSpeed;

        public float JumpForce;
        public float JumpBufferTime;
        public float JumpCoyoteTime;

        public float FallDamageYThreshold;
        
        public float CollisionThreashold;
    }
}
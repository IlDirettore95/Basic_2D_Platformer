using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GMDG.Basic_2D_Platformer.PlayerMovement
{
    [CreateAssetMenu(fileName = "MovementData", menuName = "ScriptableObjects/MovementData")]
    public class MovementData : ScriptableObject
    {
        public float WalkingSpeed;
        public float Gravity;
        public float JumpForce;
        public float CollisionThreashold;
    }
}


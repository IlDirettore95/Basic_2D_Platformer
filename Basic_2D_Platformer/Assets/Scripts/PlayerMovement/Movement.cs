using GMDG.NoProduct.Utility;
using UnityEngine;


namespace GMDG.Basic_2D_Platformer.PlayerMovement
{
    public class Movement : MonoBehaviour
    {
        public float WalkingSpeed = 3f;

        private CharacterStatus _characterStatus;

        private void Awake()
        {
            _characterStatus = new CharacterStatus(transform);
        }
        private void Update()
        {
            Vector2 velocity = Vector2.zero;

            if(Input.GetKey(KeyCode.A))
            {
                velocity += Vector2.left;
            }
            if(Input.GetKey(KeyCode.D)) 
            {
                velocity += Vector2.right;
            }

            velocity *= WalkingSpeed;

            _characterStatus.UpdateState();
            _characterStatus.UpdateKinematic(new KinematicSteeringOutput2D(velocity, 0));
        }
    }
}

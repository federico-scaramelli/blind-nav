using UnityEngine;

namespace CMF
{
    public class CharacterMovementInput : MonoBehaviour
    {
        [SerializeField] [Range(0.0f, 1.0f)] private float horizontalSpeedMultiplier;
        [SerializeField] [Range(0.0f, 1.0f)] private float backwardSpeedMultiplier;
        private float horizontal, vertical;
	    
        public float GetHorizontalMovementInput()
        {
            return horizontal;
        }

        public float GetVerticalMovementInput()
        {
            return vertical;
        }

        public void SetMovementInput(Vector2 movement)
        {
            horizontal = movement.x * horizontalSpeedMultiplier;
            vertical = movement.y < 0 ? movement.y * backwardSpeedMultiplier : movement.y;
        }
    }
}

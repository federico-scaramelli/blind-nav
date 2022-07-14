using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CMF
{
    //A very simplified controller script;
	//This script is an example of a very simple walker controller that covers only the basics of character movement;
    public class SimpleWalkerController : Controller
    {
        private Mover mover;
        float currentVerticalSpeed = 0f;
        bool isGrounded;
        public float movementSpeed = 7f;
        public float gravity = 10f;

		Vector3 lastVelocity = Vector3.zero;

		public Transform cameraTransform;
        CharacterMovementInput _characterMovementInput;
        Transform tr;

        // Use this for initialization
        void Start()
        {
            tr = transform;
            mover = GetComponent<Mover>();
            _characterMovementInput = GetComponent<CharacterMovementInput>();
        }

        void FixedUpdate()
        {
            //Run initial mover ground check;
            mover.CheckForGround();

            //If character was not grounded int the last frame and is now grounded, call 'OnGroundContactRegained' function;
            if(isGrounded == false && mover.IsGrounded() == true)
                OnGroundContactRegained(lastVelocity);

            //Check whether the character is grounded and store result;
            isGrounded = mover.IsGrounded();

            Vector3 _velocity = Vector3.zero;

            //Add player movement to velocity;
            _velocity += CalculateMovementDirection() * movementSpeed;
            
            //Handle gravity;
            if (!isGrounded)
            {
                currentVerticalSpeed -= gravity * Time.deltaTime;
            }
            else
            {
                if (currentVerticalSpeed <= 0f)
                    currentVerticalSpeed = 0f;
            }

            //Add vertical velocity;
            _velocity += tr.up * currentVerticalSpeed;

			//Save current velocity for next frame;
			lastVelocity = _velocity;

            mover.SetExtendSensorRange(isGrounded);
            mover.SetVelocity(_velocity);
        }

        private Vector3 CalculateMovementDirection()
        {
            //If no character input script is attached to this object, return no input;
			if(_characterMovementInput == null)
				return Vector3.zero;

			Vector3 _direction = Vector3.zero;

			//If no camera transform has been assigned, use the character's transform axes to calculate the movement direction;
			if(cameraTransform == null)
			{
				_direction += tr.right * _characterMovementInput.GetHorizontalMovementInput();
				_direction += tr.forward * _characterMovementInput.GetVerticalMovementInput();
			}
			else
			{
				//If a camera transform has been assigned, use the assigned transform's axes for movement direction;
				//Project movement direction so movement stays parallel to the ground;
				_direction += Vector3.ProjectOnPlane(cameraTransform.right, tr.up).normalized * _characterMovementInput.GetHorizontalMovementInput();
				_direction += Vector3.ProjectOnPlane(cameraTransform.forward, tr.up).normalized * _characterMovementInput.GetVerticalMovementInput();
			}

			//If necessary, clamp movement vector to magnitude of 1f;
			if(_direction.magnitude > 1f)
				_direction.Normalize();

			return _direction;
        }

        //This function is called when the controller has landed on a surface after being in the air;
		void OnGroundContactRegained(Vector3 _collisionVelocity)
		{
			//Call 'OnLand' delegate function;
			if(OnLand != null)
				OnLand(_collisionVelocity);
		}

		//Return the current velocity of the character;
        public override Vector3 GetVelocity()
        {
            return lastVelocity;
        }

        //Return only the current movement velocity (without any vertical velocity);
        public override Vector3 GetMovementVelocity()
        {
            return lastVelocity;
        }

        //Return whether the character is currently grounded;
        public override bool IsGrounded()
        {
            return isGrounded;
        }

    }

}


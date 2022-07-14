using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CMF
{
	//This script controls the character's animation by passing velocity values and other information ('isGrounded') to an animator component;
	public class AnimationControl : MonoBehaviour {

		Controller controller;
		Animator animator;
		//Setup;
		void Awake () {
			controller = GetComponent<Controller>();
			animator = GetComponentInChildren<Animator>();
		}
		
		//Update;
		void Update ()
		{
			//Get controller velocity;
			Vector3 _velocity = controller.GetVelocity();
			animator.speed = _velocity.magnitude > 2f ? _velocity.magnitude / 5f : 1;
			animator.SetBool("isWalking", _velocity.magnitude > 0);
		}
	}
}

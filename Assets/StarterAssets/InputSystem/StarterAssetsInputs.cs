using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
	public class StarterAssetsInputs : MonoBehaviour
	{
		[Header("Character Input Values")]
		public Vector2 move;
		public Vector2 look;
		public bool jump;
		public bool sprint;
		public bool shoot;
		public bool switchShield;
		public bool interact;
		public bool gunPressed;

		[Header("Movement Settings")]
		public bool analogMovement;

		[Header("Mouse Cursor Settings")]
		public bool cursorLocked = true;
		public bool cursorInputForLook = true;

#if ENABLE_INPUT_SYSTEM
		public void OnMove(InputValue value) {
			MoveInput(value.Get<Vector2>());
		}

		public void OnLook(InputValue value) {
			if(cursorInputForLook) {
				LookInput(value.Get<Vector2>());
			}
		}

		public void OnJump(InputValue value) {
			JumpInput(value.isPressed);
		}

		public void OnSprint(InputValue value) {
			SprintInput(value.isPressed);
		}

		public void OnShoot(InputValue value) {
			ShootInput(value.isPressed);
		}

		public void OnSwitchShield(InputValue value) {
			ShieldInput(value.isPressed);
		}

		public void OnInteract(InputValue value) {
			InteractInput(value.isPressed);
		}

		public void OnSwitchGun(InputValue value) {
			GunInput(value.isPressed);
		}
#endif


		public void MoveInput(Vector2 newMoveDirection) {
			move = newMoveDirection;
		} 

		public void LookInput(Vector2 newLookDirection) {
			look = newLookDirection;
		}

		public void JumpInput(bool newJumpState) {
			jump = newJumpState;
		}

		public void SprintInput(bool newSprintState) {
			sprint = newSprintState;
		}

		public void ShootInput(bool didShoot) {
			shoot = didShoot;
		}

		public void InteractInput(bool didInteract) {
			interact = didInteract;
		}
		
		private void OnApplicationFocus(bool hasFocus) {
			SetCursorState(cursorLocked);
		}

		private void ShieldInput(bool Shield) {
			switchShield = Shield;
		}

		private void GunInput(bool Gun) {
			gunPressed = Gun;
		}

		private void SetCursorState(bool newState) {
			Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
		}
	}
	
}
using Unity.VisualScripting;
using UnityEditor.ShaderGraph.Drawing;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace StarterAssets {
	[RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM
	[RequireComponent(typeof(PlayerInput))]
#endif
	public class FirstPersonController : MonoBehaviour
	{

		//varibales for gun
		[SerializeField]
		GameObject[] guns;
		private int activeGunIndex;
		private Gun gun;

        //interact objects like ammo health doors etc
        [SerializeField]
        LayerMask interactObjectMask;
		private IInteractObject interactObject = null;


        //varibales for shield
        [SerializeField]
		public GameObject[] shields;
		private int activeShieldIndex;
		private Shield shield;

		[Header("Player")]
		[Tooltip("Move speed of the character in m/s")]
		public float MoveSpeed = 4.0f;
		[Tooltip("Sprint speed of the character in m/s")]
		public float SprintSpeed = 6.0f;
		[Tooltip("Rotation speed of the character")]
		public float RotationSpeed = 1.0f;
		[Tooltip("Acceleration and deceleration")]
		public float SpeedChangeRate = 10.0f;

		[Space(10)]
		[Tooltip("The height the player can jump")]
		public float JumpHeight = 1.2f;
		[Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
		public float Gravity = -15.0f;

		[Space(10)]
		[Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
		public float JumpTimeout = 0.1f;
		[Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
		public float FallTimeout = 0.15f;

		[Header("Player Grounded")]
		[Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
		public bool Grounded = true;
		[Tooltip("Useful for rough ground")]
		public float GroundedOffset = -0.14f;
		[Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
		public float GroundedRadius = 0.5f;
		[Tooltip("What layers the character uses as ground")]
		public LayerMask GroundLayers;

		[Header("Cinemachine")]
		[Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
		public GameObject CinemachineCameraTarget;
		[Tooltip("How far in degrees can you move the camera up")]
		public float TopClamp = 90.0f;
		[Tooltip("How far in degrees can you move the camera down")]
		public float BottomClamp = -90.0f;

		// cinemachine
		private float _cinemachineTargetPitch;

		// player
		private float _speed;
		private float _rotationVelocity;
		private float _verticalVelocity;
		private float _terminalVelocity = 53.0f;

		// timeout deltatime
		private float _jumpTimeoutDelta;
		private float _fallTimeoutDelta;


        //switch gun time
        private readonly float switchGunLoadTime = 0.3f;
        private float lastGunSwitch;

        //hitmarker on canvas show timer
        private readonly float hitMarkTimer = 0.05f;
        private float lastHitTime;

		//health
		[SerializeField]
		public HealthBar healthBar;
		private readonly int Max_Health = 100;
		private int currentHealth;

        //singleton for instance
        public static FirstPersonController Instance { get; private set; }


#if ENABLE_INPUT_SYSTEM
        private PlayerInput _playerInput;
#endif
		private CharacterController _controller;
		private StarterAssetsInputs _input;
		private GameObject _mainCamera;

		private const float _threshold = 0.01f;

		private bool IsCurrentDeviceMouse {
			get {
				#if ENABLE_INPUT_SYSTEM
				return _playerInput.currentControlScheme == "KeyboardMouse";
				#else
				return false;
				#endif
			}
		}

		private void Awake() {
			// get a reference to our main camera
			if (_mainCamera == null) {
				_mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
			}

            // If there is an instance, and it's not me, delete myself.
            if (Instance != null && Instance != this) {
                Destroy(this);
            }
            else {
                Instance = this;
            }

			
        }

		private void Start() {
			_controller = GetComponent<CharacterController>();
			_input = GetComponent<StarterAssetsInputs>();
#if ENABLE_INPUT_SYSTEM
			_playerInput = GetComponent<PlayerInput>();
#else
			Debug.LogError( "Starter Assets package is missing dependencies. Please use Tools/Starter Assets/Reinstall Dependencies to fix it");
#endif

			// reset our timeouts on start
			_jumpTimeoutDelta = JumpTimeout;
			_fallTimeoutDelta = FallTimeout;
			activeShieldIndex = 0;
			shield = shields[activeShieldIndex].GetComponent<Shield>();
			currentHealth = Max_Health;
			InitGuns();
        }

		//the initalizer to set all gun data for the player to use
        private void InitGuns() {
			activeGunIndex = 0;
            lastGunSwitch = Time.time;
            for (int i = 0; i < guns.Length; i++) {
                if (i == activeGunIndex) {
                    guns[i].GetComponent<Renderer>().enabled = true;
                    gun = guns[i].GetComponent<Gun>();
                    gun.SetDisplayActive(true);
                    guns[i].GetComponent<Animator>().SetBool("switchGuns", true);
                }
                else {
                    guns[i].GetComponent<Renderer>().enabled = false;
					guns[i].GetComponent<Gun>().SetDisplayActive(false);
                    guns[i].GetComponent<Animator>().SetBool("switchGuns", false);
                }

            }
        }

        private void Update() {
			JumpAndGravity();
			GroundedCheck();
			Move();
			Shoot();
			SwitchShield();
			SwitchGun();
			Interact();
			Debug.Log(_playerInput.currentControlScheme);

        }

		private void LateUpdate() {
			CameraRotation();
		}

		private void GroundedCheck() {
			// set sphere position, with offset
			Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
			Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);  //cannot have default layer selected
		}

		private void CameraRotation() {
			// if there is an input
			if (_input.look.sqrMagnitude >= _threshold) {
				//Don't multiply mouse input by Time.deltaTime
				float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;
				
				_cinemachineTargetPitch += _input.look.y * RotationSpeed * deltaTimeMultiplier;
				_rotationVelocity = _input.look.x * RotationSpeed * deltaTimeMultiplier;

				// clamp our pitch rotation
				_cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

				// Update Cinemachine camera target pitch
				CinemachineCameraTarget.transform.localRotation = Quaternion.Euler(_cinemachineTargetPitch, 0.0f, 0.0f);

				// rotate the player left and right
				transform.Rotate(Vector3.up * _rotationVelocity);
			}
		}

		private void Move() {
			// set target speed based on move speed, sprint speed and if sprint is pressed
			float targetSpeed = _input.sprint ? SprintSpeed : MoveSpeed;

			// a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

			// note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
			// if there is no input, set the target speed to 0
			if (_input.move == Vector2.zero) targetSpeed = 0.0f;

			// a reference to the players current horizontal velocity
			float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

			float speedOffset = 0.1f;
			float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

			// accelerate or decelerate to target speed
			if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset) {
				// creates curved result rather than a linear one giving a more organic speed change
				// note T in Lerp is clamped, so we don't need to clamp our speed
				_speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * SpeedChangeRate);

				// round speed to 3 decimal places
				_speed = Mathf.Round(_speed * 1000f) / 1000f;
			} else {
				_speed = targetSpeed;
			}

			// normalise input direction
			Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

			// note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
			// if there is a move input rotate player when the player is moving
			if (_input.move != Vector2.zero) {
				// move
				inputDirection = transform.right * _input.move.x + transform.forward * _input.move.y;
			}

			// move the player
			_controller.Move(inputDirection.normalized * (_speed * Time.deltaTime) + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
		}

        private void JumpAndGravity() {
            if (Grounded) {
                // reset the fall timeout timer
                _fallTimeoutDelta = FallTimeout;

                // stop our velocity dropping infinitely when grounded
                if (_verticalVelocity < 0.0f) {
                    _verticalVelocity = -2f;
                }

                // Jump
                if (_input.jump && _jumpTimeoutDelta <= 0.0f) {
                    _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);
                }

                // jump timeout
                if (_jumpTimeoutDelta >= 0.0f) {
                    _jumpTimeoutDelta -= Time.deltaTime;
                }
            } else {
                // reset the jump timeout timer
                _jumpTimeoutDelta = JumpTimeout;

                // fall timeout
                if (_fallTimeoutDelta >= 0.0f) {
                    _fallTimeoutDelta -= Time.deltaTime;
                }

                // if we are not grounded, do not jump
                _input.jump = false;
            }

            // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
            if (_verticalVelocity < _terminalVelocity) {
                _verticalVelocity += Gravity * Time.deltaTime;
            }
        }


        private void Shoot() {
			if(_input.shoot) {
				gun.Shoot();
			}
            _input.shoot = false;
        }

		public StarterAssetsInputs GetInputs() {
			return _input;
		}


		private void SwitchShield() {
			if (_input.switchShield) {
				activeShieldIndex += 1;
				if (activeShieldIndex >= shields.Length) activeShieldIndex = 0;
				for (int i = 0; i < shields.Length; i++) {
					if (i == activeShieldIndex) {
						shields[i].SetActive(true);
						shield = shields[i].GetComponent<Shield>();
					}
					else {
						shields[i].SetActive(false);
					}
				}
			}
            _input.switchShield = false;
        }

		// For gun handling and swaping
        private void SwitchGun() {
            if (_input.gunPressed) {
                activeGunIndex += 1;
                if (activeGunIndex >= guns.Length) activeGunIndex = 0;
                for (int i = 0; i < guns.Length; i++) {
					if (i == activeGunIndex) {
						guns[i].GetComponent<Renderer>().enabled = true;
						gun = guns[i].GetComponent<Gun>();
						gun.PlayWeponChangeAudio();
						gun.SetDisplayActive(true);
                        guns[i].GetComponent<Animator>().SetBool("switchGuns", true);
                        SetGunSwitchLastTime();
					}
					else {
						guns[i].GetComponent<Renderer>().enabled = false;
                        guns[i].GetComponent<Gun>().SetDisplayActive(false);
                        guns[i].GetComponent<Animator>().SetBool("switchGuns", false);
                    }
                    
                }
            }
            _input.gunPressed = false;
        }

        public void SetGunSwitchLastTime() {
            lastGunSwitch = Time.time;
        }


		//delay for the run swap before shooting also helps with same time on anim
        public bool GunSwapComplete() {
            if (lastGunSwitch + switchGunLoadTime < Time.time) {
                return true;
            }
            return false;
        }


		//dealing with interact objects
		private void Interact() {
			if(_input.interact && DetectAndSetNearByInteractObjects() && interactObject != null) {
				interactObject.Interact();
                interactObject = null;
            }
			_input.interact = false;
		}

        private bool DetectAndSetNearByInteractObjects() {
            //if the player is by the interact object
            var colliders = Physics.OverlapSphere(transform.position, 3f, interactObjectMask);
			//because we only interact with ammo, health, doors, and levers there will never be a time where they overlap due to map planning
			//so we can just return the first object which is the only object
            if (colliders.Length > 0) {
				interactObject = colliders[0].GetComponent<IInteractObject>();
				return true;
            }
			interactObject = null;
			return false;
        }

        public Gun GetGun() { return gun; }

		public GameObject[] GetGuns() {
			return guns;
		}

		public void GiveAmmoByColor(string color) {
			for (int i = 0; i < guns.Length; i++) {
				var current = guns[i].GetComponent<Gun>();
				if (current.GetColor() == color) {
					current.GiveMaxAmmo();
				}
			}
		}

		public int GetCurrentHealthInt() {
			return currentHealth;
		}

		public string GetCurrentHealthString() {
			return currentHealth.ToString();
		}

		public bool TakeDamage(int damage, string color) {
			if (shield.GetColor() == color) {
				return false;
			}
			currentHealth -= damage;
			healthBar.SetHealth(currentHealth);
			return true;
		}

		public void GiveMaxHealth() {
			currentHealth = Max_Health;
			healthBar.SetMaxHealth(currentHealth);
		}

		public Vector3 GetPosition() {
			return transform.position;
		}

		public Transform GetTransform() {
			return transform;
		}


		private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
		{
			if (lfAngle < -360f) lfAngle += 360f;
			if (lfAngle > 360f) lfAngle -= 360f;
			return Mathf.Clamp(lfAngle, lfMin, lfMax);
		}

		private void OnDrawGizmosSelected()
		{
			Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
			Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

			if (Grounded) Gizmos.color = transparentGreen;
			else Gizmos.color = transparentRed;

			// when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
			Gizmos.DrawSphere(new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z), GroundedRadius);
		}
	}
}
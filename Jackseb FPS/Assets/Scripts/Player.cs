using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using Photon.Pun;
using TMPro;

namespace Com.Jackseb.FPS
{
	public class Player : MonoBehaviourPunCallbacks, IPunObservable
	{
		#region Variables

		public float speed;
		public float sprintModifier;
		public float crouchModifier;
		public float jumpForce;
		public float cameraTransformAmount;
		public int maxHealth;
		public Camera normalCam;
		public Camera weaponCam;
		public GameObject cameraParent;
		public Transform weaponParent;
		public Transform groundDetector;
		public LayerMask ground;

		[HideInInspector] public ProfileData playerProfile;
		public TextMeshPro playerUsernameText;

		public float crouchAmount;
		public GameObject standingCollider;
		public GameObject crouchingCollider;

		private Transform uiHealthBar;
		private Text uiAmmo;
		private Text uiAmmoFrame;
		private Text uiUsername;

		private Rigidbody rig;

		private Vector3 targetWeaponBobPosition;
		private Vector3 weaponParentOrigin;
		private Vector3 weaponParentCurrentPos;

		private float movementCounter;
		private float idleCounter;

		private float baseFOV;
		private float sprintFOVModifier = 1.25f;
		private Vector3 origin;

		private int currentHealth;

		private GameManager r_GameManager;
		private Weapon r_Weapon;

		public bool crouched;

		private bool isAiming;

		private float aimAngle;

		private Vector3 normalCamTarget;
		private Vector3 weaponCamTarget;

		#endregion


		#region Photon Callbacks

		public void OnPhotonSerializeView(PhotonStream p_stream, PhotonMessageInfo p_message)
		{
			if (p_stream.IsWriting)
			{
				p_stream.SendNext((int)(weaponParent.transform.localEulerAngles.x * 100f));
			}
			else
			{
				aimAngle = (int)p_stream.ReceiveNext() / 100f;
			}
		}

		#endregion


		#region MonoBehavior Callbacks

		private void Start()
		{
			r_GameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
			r_Weapon = GetComponent<Weapon>();
			currentHealth = maxHealth;

			cameraParent.SetActive(photonView.IsMine);
			if (!photonView.IsMine)
			{
				gameObject.layer = 11;
				standingCollider.layer = 11;
				crouchingCollider.layer = 11;
			}

			baseFOV = normalCam.fieldOfView;

			origin = normalCam.transform.localPosition;

			if (Camera.main) Camera.main.enabled = false;

			rig = GetComponent<Rigidbody>();
			weaponParentOrigin = weaponParent.localPosition;
			weaponParentCurrentPos = weaponParentOrigin;


			if (photonView.IsMine)
			{
				uiHealthBar = GameObject.Find("HUD/Health/Bar").transform;
				uiAmmo = GameObject.Find("HUD/Ammo/Text").GetComponent<Text>();
				uiAmmoFrame = GameObject.Find("HUD/Ammo/Frame").GetComponent<Text>();
				uiUsername = GameObject.Find("HUD/Health/Username").GetComponent<Text>();

				RefreshHealthBar();
				uiUsername.text = Launcher.myProfile.username;

				photonView.RPC("SyncProfile", RpcTarget.All, Launcher.myProfile.convertToObjArr());
			}
		}

		private void Update()
		{
			if (!photonView.IsMine)
			{
				RefreshMultiplayerState();
				return;
			}
			
			// Axes
			float t_hMove = Input.GetAxisRaw("Horizontal");
			float t_vMove = Input.GetAxisRaw("Vertical");

			// Controls
			bool sprint = Input.GetKey(KeyCode.LeftShift);
			bool jump = Input.GetKeyDown(KeyCode.Space);
			bool crouch = Input.GetKeyDown(KeyCode.LeftControl);
			bool pause = Input.GetKeyDown(KeyCode.Escape);

			// States
			bool isGrounded = Physics.Raycast(groundDetector.position, Vector3.down, 0.1f, ground);
			bool isJumping = jump && isGrounded;
			bool isSprinting = sprint && t_vMove > 0 && !isJumping && isGrounded;
			bool isCrouching = crouch && !isSprinting && !isJumping && isGrounded;

			// Pause
			if (pause)
			{
				GameObject.Find("Pause").GetComponent<Pause>().TogglePause();
			}
			if (Pause.paused)
			{
				t_hMove = 0f;
				t_vMove = 0f;
				sprint = false;
				jump = false;
				pause = false;
				isGrounded = false;
				isJumping = false;
				isSprinting = false;
			}

			// Crouching
			if (isCrouching)
			{
				photonView.RPC("SetCrouch", RpcTarget.AllBuffered, !crouched);
			}

			// Jumping
			if (isJumping)
			{
				if (crouched) photonView.RPC("SetCrouch", RpcTarget.AllBuffered, false);
				rig.AddForce(Vector3.up * jumpForce);
			}

			// if (Input.GetKeyDown(KeyCode.U)) TakeDamage(Random.Range(15, 23)); TEST TAKE DAMAGE COMMAND

			// Headbob
			if (!isGrounded)
			{
				Headbob(idleCounter, 0.025f, 0.025f);
				weaponParent.localPosition = Vector3.Lerp(weaponParent.localPosition, targetWeaponBobPosition, Time.deltaTime * 2f);
			}
			else if (t_hMove == 0 && t_vMove == 0)
			{
				Headbob(idleCounter, 0.025f, 0.025f);
				idleCounter += Time.deltaTime;
				weaponParent.localPosition = Vector3.Lerp(weaponParent.localPosition, targetWeaponBobPosition, Time.deltaTime * 2f);
			}
			else if (!isSprinting && !crouched)
			{
				Headbob(movementCounter, 0.035f, 0.035f);
				movementCounter += Time.deltaTime * 3f;
				weaponParent.localPosition = Vector3.Lerp(weaponParent.localPosition, targetWeaponBobPosition, Time.deltaTime * 6f);
			}
			else if (crouched)
			{
				Headbob(movementCounter, 0.02f, 0.02f);
				movementCounter += Time.deltaTime * 1.75f;
				weaponParent.localPosition = Vector3.Lerp(weaponParent.localPosition, targetWeaponBobPosition, Time.deltaTime * 6f);
			}
			else
			{
				Headbob(movementCounter, 0.15f, 0.075f);
				movementCounter += Time.deltaTime * 5f;
				weaponParent.localPosition = Vector3.Lerp(weaponParent.localPosition, targetWeaponBobPosition, Time.deltaTime * 10f);
			}

			//UI Refreshes
			RefreshHealthBar();
			if (r_Weapon.currentGunData != null)
			{
				uiAmmo.enabled = true;
				uiAmmoFrame.enabled = true;
				r_Weapon.RefreshAmmo(uiAmmo, uiAmmoFrame);
			}
			else
			{
				uiAmmo.enabled = false;
				uiAmmoFrame.enabled = false;
			}
		}

		private void FixedUpdate()
		{
			if (!photonView.IsMine) return;

			// Axes
			float t_hMove = Input.GetAxisRaw("Horizontal");
			float t_vMove = Input.GetAxisRaw("Vertical");

			// Controls
			bool sprint = Input.GetKey(KeyCode.LeftShift);
			bool jump = Input.GetKeyDown(KeyCode.Space);
			bool slide = Input.GetKey(KeyCode.LeftAlt);
			bool aim = Input.GetMouseButton(1);

			// States
			bool isGrounded = Physics.Raycast(groundDetector.position, Vector3.down, 0.1f, ground);
			bool isJumping = jump && isGrounded;
			bool isSprinting = sprint && t_vMove > 0 && !isJumping && isGrounded;
			isAiming = aim && !Input.GetKey(KeyCode.LeftShift);

			// Pause
			if (Pause.paused)
			{
				t_hMove = 0f;
				t_vMove = 0f;
				sprint = false;
				jump = false;
				isGrounded = false;
				isJumping = false;
				isSprinting = false;
				isAiming = false;
			}

			// Movement
			Vector3 t_direction = Vector3.zero;
			float t_adjustedSpeed = speed;

			t_direction = new Vector3(t_hMove, 0, t_vMove);
			t_direction.Normalize();
			t_direction = transform.TransformDirection(t_direction);

			if (isSprinting)
			{
				if (crouched) photonView.RPC("SetCrouch", RpcTarget.AllBuffered, false);
				t_adjustedSpeed *= sprintModifier;
			}
			else if (crouched)
			{
				t_adjustedSpeed *= crouchModifier;
			}

			Vector3 t_targetVelocity = t_direction * t_adjustedSpeed * Time.deltaTime;
			t_targetVelocity.y = rig.velocity.y;
			rig.velocity = t_targetVelocity;

			// Aiming
			isAiming = r_Weapon.Aim(isAiming);

			// Camera stuff
			if (isSprinting)
			{
				normalCam.fieldOfView = Mathf.Lerp(normalCam.fieldOfView, baseFOV * sprintFOVModifier, Time.deltaTime * 8f);
				weaponCam.fieldOfView = Mathf.Lerp(weaponCam.fieldOfView, baseFOV * sprintFOVModifier, Time.deltaTime * 8f);
			}
			else if (isAiming)
			{
				normalCam.fieldOfView = Mathf.Lerp(normalCam.fieldOfView, baseFOV * r_Weapon.currentGunData.mainFOV, Time.deltaTime * 8f);
				weaponCam.fieldOfView = Mathf.Lerp(weaponCam.fieldOfView, baseFOV * r_Weapon.currentGunData.weaponFOV, Time.deltaTime * 8f);
			}
			else
			{
				normalCam.fieldOfView = Mathf.Lerp(normalCam.fieldOfView, baseFOV, Time.deltaTime * 8f);
				weaponCam.fieldOfView = Mathf.Lerp(weaponCam.fieldOfView, baseFOV, Time.deltaTime * 8f);
			}

			if (crouched)
			{
				normalCamTarget = Vector3.Lerp(normalCam.transform.localPosition, origin + Vector3.down * crouchAmount, Time.deltaTime * 6f);
				weaponCamTarget = Vector3.Lerp(weaponCam.transform.localPosition, origin + Vector3.down * crouchAmount, Time.deltaTime * 6f);
			}
			else
			{
				normalCamTarget = Vector3.Lerp(normalCam.transform.localPosition, origin, Time.deltaTime * 6f);
				weaponCamTarget = Vector3.Lerp(weaponCam.transform.localPosition, origin, Time.deltaTime * 6f);
			}
		}

		private void LateUpdate()
		{
			normalCam.transform.localPosition = normalCamTarget;
			weaponCam.transform.localPosition = weaponCamTarget;
		}

		#endregion


		#region Private Methods

		void RefreshMultiplayerState()
		{
			float cacheEulY = weaponParent.localEulerAngles.y;

			Quaternion targetRotation = Quaternion.identity * Quaternion.AngleAxis(aimAngle, Vector3.right);
			weaponParent.rotation = Quaternion.Slerp(weaponParent.rotation, targetRotation, Time.deltaTime * 8f);

			Vector3 finalRotation = weaponParent.localEulerAngles;
			finalRotation.y = cacheEulY;

			weaponParent.localEulerAngles = finalRotation;
		}

		void Headbob(float p_z, float p_xIntensity, float p_yIntensity)
		{
			float t_aimAdjust = 1f;
			if (isAiming) t_aimAdjust = 0.01f;
			targetWeaponBobPosition = weaponParentCurrentPos + new Vector3(Mathf.Cos(p_z) * p_xIntensity * t_aimAdjust, Mathf.Sin(p_z * 2) * p_yIntensity * t_aimAdjust, 0);
		}

		void RefreshHealthBar()
		{
			float t_healthRatio = (float)currentHealth / (float)maxHealth;

			uiHealthBar.localScale = Vector3.Lerp(uiHealthBar.localScale, new Vector3(t_healthRatio, 1, 1), Time.deltaTime * 8f);

			if (currentHealth >= 50 && currentHealth <= 100)
			{
				uiHealthBar.GetComponent<Image>().color = Color.Lerp(uiHealthBar.GetComponent<Image>().color, Color.green, Time.deltaTime * 8f);
			}
			else if (currentHealth > 20 && currentHealth < 50)
			{
				uiHealthBar.GetComponent<Image>().color = Color.Lerp(uiHealthBar.GetComponent<Image>().color, Color.yellow, Time.deltaTime * 8f);
			}
			else if (currentHealth > 0 && currentHealth <= 20)
			{
				uiHealthBar.GetComponent<Image>().color = Color.Lerp(uiHealthBar.GetComponent<Image>().color, Color.red, Time.deltaTime * 8f);
			}
		}

		[PunRPC]
		void SetCrouch (bool p_state)
		{
			if (crouched == p_state) return;

			crouched = p_state;

			if (crouched)
			{
				standingCollider.SetActive(false);
				crouchingCollider.SetActive(true);
				weaponParentCurrentPos += Vector3.down * crouchAmount;
			}
			else
			{
				standingCollider.SetActive(true);
				crouchingCollider.SetActive(false);
				weaponParentCurrentPos -= Vector3.down * crouchAmount;
			}
		}

		[PunRPC]
		private void SyncProfile(object[] arrOfObj)
		{
			playerProfile = new ProfileData(arrOfObj);
			playerUsernameText.text = playerProfile.username;
		}

		#endregion


		#region Public Methods

		public void TakeDamage(int p_damage)
		{
			if (photonView.IsMine)
			{
				currentHealth -= p_damage;
				RefreshHealthBar();

				if (currentHealth <= 0)
				{
					r_GameManager.Spawn();
					PhotonNetwork.Destroy(gameObject);
				}
			}
		}

		#endregion
	}

}
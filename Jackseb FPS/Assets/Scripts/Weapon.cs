using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using Photon.Pun;

namespace Com.Jackseb.FPS
{
	public class Weapon : MonoBehaviourPunCallbacks
	{
		#region Variables

		public List<Gun> loadout;
		[HideInInspector] public Gun currentGunData;
		public Transform weaponParent;
		public GameObject bulletholePrefab;
		public LayerMask canBeShot;
		public AudioSource sfx;
		public AudioClip hitmarkerSound;
		public AudioClip equipSound;
		public bool isAiming = false;

		private float currentCooldown;
		private int currentIndex;
		private GameObject currentWeapon;

		private Image hitmarkerImage;
		private float hitmarkerWait;

		private bool isReloading;
		private Coroutine lastReload = null;

		#endregion


		#region MonoBehavior Callbacks

		private void Start()
		{
			foreach (Gun a in loadout)
			{
				if (a != null) a.Initialize();
			}
			hitmarkerImage = GameObject.Find("HUD/Hitmarker").GetComponent<Image>();
			hitmarkerImage.color = new Color(1, 1, 1, 0);
			if (loadout[0] != null)
			{
				Equip(0);
			}
			else if (loadout[1] != null)
			{
				Equip(1);
			}
		}

		void Update()
		{
			if (Pause.paused && photonView.IsMine) return;

			if (photonView.IsMine && Input.GetKeyDown(KeyCode.Alpha1) && loadout[0] != null)
			{
				photonView.RPC("Equip", RpcTarget.AllBuffered, 0);
			}

			if (photonView.IsMine && Input.GetKeyDown(KeyCode.Alpha2) && loadout[1] != null)
			{
				photonView.RPC("Equip", RpcTarget.AllBuffered, 1);
			}

			if (currentWeapon != null)
			{
				if (photonView.IsMine)
				{
					if (currentGunData.burst != 1)
					{
						if (Input.GetMouseButtonDown(0) && currentCooldown <= 0 && !isReloading)
						{
							if (currentGunData.CanFireBullet()) photonView.RPC("Shoot", RpcTarget.All);
							else if (currentGunData.CanReload())
							{
								lastReload = StartCoroutine(Reload(currentGunData.reloadTime));
							}
						}
					}
					else
					{
						if (Input.GetMouseButton(0) && currentCooldown <= 0 && !isReloading)
						{
							if (currentGunData.CanFireBullet()) photonView.RPC("Shoot", RpcTarget.All);
							else if (currentGunData.CanReload())
							{
								lastReload = StartCoroutine(Reload(currentGunData.reloadTime));
							}
						}
					}

					if (Input.GetKeyDown(KeyCode.R) && currentGunData.CanReload()) lastReload = StartCoroutine(Reload(currentGunData.reloadTime));

					// Cooldown
					if (currentCooldown > 0) currentCooldown -= Time.deltaTime;
				}

				// Weapon position elasticity
				currentWeapon.transform.localPosition = Vector3.Lerp(currentWeapon.transform.localPosition, Vector3.zero, Time.deltaTime * 4f);
			}

			if (photonView.IsMine)
			{
				if (hitmarkerWait > 0)
				{
					hitmarkerWait -= Time.deltaTime;
				}
				else if (hitmarkerImage.color.a > 0)
				{
					hitmarkerImage.color = Color.Lerp(hitmarkerImage.color, new Color(1, 1, 1, 0), Time.deltaTime * 0.5f);
				}
			}
		}

		#endregion


		#region Private Methods

		IEnumerator Reload(float p_wait)
		{
			isReloading = true;
			currentWeapon.SetActive(false);

			// Sound
			sfx.clip = currentGunData.reloadSound;
			sfx.Play();

			yield return new WaitForSeconds(p_wait);

			currentGunData.Reload();
			currentWeapon.SetActive(true);
			isReloading = false;

			// Sound
			sfx.clip = currentGunData.finishReloadSound;
			sfx.Play();
		}

		[PunRPC]
		void Equip(int p_ind)
		{
			if (currentIndex != p_ind)
			{
				if (currentWeapon != null)
				{
					if (isReloading)
					{
						StopCoroutine(lastReload);
						isReloading = false;
					}
					Destroy(currentWeapon);
				}

				currentIndex = p_ind;

				GameObject t_newWeapon = Instantiate(loadout[p_ind].prefab, weaponParent.position, weaponParent.rotation, weaponParent) as GameObject;
				t_newWeapon.transform.localPosition = Vector3.zero;
				t_newWeapon.transform.localEulerAngles = Vector3.zero;
				t_newWeapon.GetComponent<Sway>().isMine = photonView.IsMine;

				if (photonView.IsMine) ChangeLayersRecursively(t_newWeapon, 10);
				else ChangeLayersRecursively(t_newWeapon, 0);

				currentWeapon = t_newWeapon;
				currentGunData = loadout[p_ind];

				// Sound
				sfx.PlayOneShot(equipSound, 0.5f);
			}
		}

		[PunRPC]
		void PickupWeapon(string name)
		{
			// find the weapon from a library
			Gun newWeapon = GunLibrary.FindGun(name);

			loadout[newWeapon.slot] = newWeapon;
			newWeapon.Initialize();
			Equip(0);
			Equip(1);
			Equip(newWeapon.slot);
		}

		void ChangeLayersRecursively (GameObject p_target, int p_layer)
		{
			p_target.layer = p_layer;
			foreach (Transform a in p_target.transform) ChangeLayersRecursively(a.gameObject, p_layer);
		}

		public bool Aim(bool p_isAiming)
		{
			if (!currentWeapon) return false;
			if (isReloading) p_isAiming = false;

			isAiming = p_isAiming;

			Transform t_anchor = currentWeapon.transform.Find("Anchor");
			Transform t_stateHip = currentWeapon.transform.Find("States/Hip");
			Transform t_stateADS = currentWeapon.transform.Find("States/ADS");

			if (p_isAiming)
			{
				// Aim
				t_anchor.position = Vector3.Lerp(t_anchor.position, t_stateADS.position, Time.deltaTime * currentGunData.aimSpeed);
			}
			else
			{
				// Hip
				t_anchor.position = Vector3.Lerp(t_anchor.position, t_stateHip.position, Time.deltaTime * currentGunData.aimSpeed);
			}

			return isAiming;
		}

		[PunRPC]
		void Shoot()
		{
			Transform t_spawn = transform.Find("Cameras/Normal Camera");

			// Cooldown
			currentCooldown = currentGunData.firerate;

			for (int i = 0; i < Mathf.Max(1, currentGunData.pellets); i++)
			{
				// Bloom
				Vector3 t_bloom = t_spawn.position + t_spawn.forward * 1000f;
				t_bloom += Random.Range(-currentGunData.bloom, currentGunData.bloom) * t_spawn.up;
				t_bloom += Random.Range(-currentGunData.bloom, currentGunData.bloom) * t_spawn.right;
				t_bloom -= t_spawn.position;
				t_bloom.Normalize();

				// Raycast
				RaycastHit t_hit = new RaycastHit();
				Debug.DrawRay(t_spawn.position, t_spawn.forward * 1000f, Color.red, 1f);
				Debug.DrawRay(t_spawn.position, t_bloom * 1000f, Color.blue, 1f);
				if (Physics.Raycast(t_spawn.position, t_bloom, out t_hit, 1000f, canBeShot))
				{
					if (t_hit.collider.gameObject.layer != 11)
					{
						GameObject t_newHole = Instantiate(bulletholePrefab, t_hit.point + t_hit.normal * 0.001f, Quaternion.identity) as GameObject;
						t_newHole.transform.LookAt(t_hit.point + t_hit.normal);
						Destroy(t_newHole, 5f);
					}

					if (photonView.IsMine)
					{
						// Shooting other player on network
						if (t_hit.collider.gameObject.layer == 11)
						{
							// Give damage
							t_hit.collider.transform.root.gameObject.GetPhotonView().RPC("TakeDamage", RpcTarget.AllBuffered, loadout[currentIndex].damage);

							// Show hitmarker
							hitmarkerImage.color = Color.white;
							sfx.PlayOneShot(hitmarkerSound);
							hitmarkerWait = 0.5f;
						}
					}
				}
			}

			// Sound
			sfx.clip = currentGunData.gunshotSound;
			sfx.pitch = 1 - currentGunData.pitchRandomization + Random.Range(-currentGunData.pitchRandomization, currentGunData.pitchRandomization);
			sfx.Play();

			// Gun FX
			currentWeapon.transform.Rotate(-currentGunData.recoil, 0, 0);
			currentWeapon.transform.position -= currentWeapon.transform.forward * currentGunData.kickback;
		}

		[PunRPC]
		private void TakeDamage(int p_damage)
		{
			GetComponent<Player>().TakeDamage(p_damage);
		}

		#endregion


		#region Public Methods

		public void RefreshAmmo(Text p_text, Text p_textFrame)
		{
			if (loadout[currentIndex] != null)
			{
				int t_clip = loadout[currentIndex].GetClip();
				int t_stash = loadout[currentIndex].GetStash();

				p_text.text = t_clip.ToString("00") + " / " + t_stash.ToString("00");
				p_textFrame.text = t_clip.ToString("00") + " / " + t_stash.ToString("00");
			}
		}

		#endregion
	}
}
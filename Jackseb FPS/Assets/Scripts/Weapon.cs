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
		public AudioClip emptyClip;
		public bool isAiming = false;

		private float currentCooldown;
		private int currentIndex = -1;
		private GameObject currentWeapon;

		private Image hitmarkerImage;
		private float hitmarkerWait;

		private bool isReloading;
		private bool holdingDownMouse1 = false;
		private Coroutine lastReload = null;

		#endregion


		#region MonoBehavior Callbacks

		private void Start()
		{
			if (photonView.IsMine)
			{
				foreach (Gun a in loadout)
				{
					if (a != null) a.Initialize();
				}
			}
			hitmarkerImage = GameObject.Find("HUD/Hitmarker").GetComponent<Image>();
			hitmarkerImage.color = new Color(1, 1, 1, 0);
			if (loadout[0] != null)
			{
				photonView.RPC("Equip", RpcTarget.AllBuffered, 0);

			}
			else if (loadout[1] != null)
			{
				photonView.RPC("Equip", RpcTarget.AllBuffered, 1);
			}
			else if (loadout[2] != null)
			{
				photonView.RPC("Equip", RpcTarget.AllBuffered, 2);
			}
		}

		void Update()
		{
			if (Pause.paused && photonView.IsMine) return;

			holdingDownMouse1 = Input.GetMouseButtonDown(0);

			if (Input.GetKeyDown(KeyCode.U)) TakeDamage(15, PhotonNetwork.LocalPlayer.ActorNumber, 1);

			if (photonView.IsMine && Input.GetKeyDown(KeyCode.Alpha1) && loadout[0] != null)
			{
				photonView.RPC("Equip", RpcTarget.AllBuffered, 0);
			}

			if (photonView.IsMine && Input.GetKeyDown(KeyCode.Alpha2) && loadout[1] != null)
			{
				photonView.RPC("Equip", RpcTarget.AllBuffered, 1);
			}

			if (photonView.IsMine && Input.GetKeyDown(KeyCode.Alpha3) && loadout[2] != null)
			{
				photonView.RPC("Equip", RpcTarget.AllBuffered, 2);
			}

			if (currentWeapon != null)
			{
				if (photonView.IsMine)
				{
					if (currentGunData.burst != 1)
					{
						if (Input.GetMouseButtonDown(0) && currentCooldown <= 0 && !isReloading)
						{
							if (currentGunData.CanFireBullet(isAiming)) photonView.RPC("Shoot", RpcTarget.All);
							else if (currentGunData.CanReload() && currentGunData.needsReload)
							{
								lastReload = StartCoroutine(Reload(currentGunData.reloadTime, currentGunData.playSoundIndividually));
							}
							else if (currentGunData.GetStash() == 0)
							{
								sfx.PlayOneShot(emptyClip);
							}
						}
					}
					else
					{
						if (Input.GetMouseButton(0) && currentCooldown <= 0 && !isReloading)
						{
							if (currentGunData.CanFireBullet(isAiming)) photonView.RPC("Shoot", RpcTarget.All);
							else if (currentGunData.CanReload() && currentGunData.needsReload)
							{
								lastReload = StartCoroutine(Reload(currentGunData.reloadTime, currentGunData.playSoundIndividually));
							}
							else if (currentGunData.GetStash() == 0 && holdingDownMouse1)
							{
								sfx.PlayOneShot(emptyClip);
							}
						}
					}

					if (Input.GetKeyDown(KeyCode.R) && currentGunData.CanReload() && !isReloading) photonView.RPC("ReloadRPC", RpcTarget.AllBuffered);
					else if(Input.GetKeyDown(KeyCode.R) && !currentGunData.CanReload() && currentGunData.GetStash() == 0) sfx.PlayOneShot(emptyClip);

					// Cooldown
					if (currentCooldown > 0) currentCooldown -= Time.deltaTime;
				}

				// Weapon position elasticity
				currentWeapon.transform.localPosition = Vector3.Lerp(currentWeapon.transform.localPosition, Vector3.zero, Time.deltaTime * 8f);
			}

			// Animation
			if (currentWeapon.GetComponent<Animator>() != null)
			{
				currentWeapon.GetComponent<Animator>().SetFloat("FOV", transform.Find("Cameras/Normal Camera").GetComponent<Camera>().fieldOfView);
				currentWeapon.GetComponent<Animator>().SetBool("HasAmmo", (currentGunData.GetClip() > 0));
				currentWeapon.GetComponent<Animator>().SetBool("IsReloading", isReloading);
			}

			if (currentGunData.projectileBased)
			{
				if (currentWeapon.transform.Find("Anchor/Design/Projectile/ProjectileAnim") != null)
				{
					currentWeapon.transform.Find("Anchor/Design/Projectile/ProjectileAnim").GetComponent<Animator>().SetFloat("FOV", transform.Find("Cameras/Normal Camera").GetComponent<Camera>().fieldOfView);
				}
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

		private void OnTriggerEnter(Collider other)
		{
			if (other.gameObject.tag == "Killzone")
			{
				Debug.Log("lava");
				if (photonView.IsMine)
				{
					photonView.RPC("TakeDamage", RpcTarget.AllBuffered, 999, 0, 1f);
				}
			}

			if (other.gameObject.layer == 12)
			{
				// find the weapon from a library
				Projectile newProjectile = ProjectileLibrary.FindProjectile(other.transform.root.gameObject.name);

				if (photonView.IsMine)
				{
					ProjectileHelper script = other.transform.root.GetComponent<ProjectileHelper>();
					// ^ what was this for...?
					photonView.RPC("TakeDamage", RpcTarget.AllBuffered, newProjectile.GetDamage(), newProjectile.GetActorNumber(), newProjectile.GetMultiplier());
				}

				Destroy(other.gameObject);
			}
		}

		#endregion


		#region Private Methods

		[PunRPC]
		private void ReloadRPC()
		{
			if (photonView.IsMine)
			{
				lastReload = StartCoroutine(Reload(currentGunData.reloadTime, currentGunData.playSoundIndividually));
			}
		}

		IEnumerator Reload(float p_wait, bool individual)
		{
			isReloading = true;

			if (!currentWeapon.GetComponent<Animator>())
			{
				currentWeapon.SetActive(false);
			}

			if (individual)
			{
				StartCoroutine(SingleReload(p_wait, (currentGunData.clipSize - currentGunData.GetClip())));
			}
			else
			{
				// Sound
				sfx.PlayOneShot(currentGunData.reloadSound);

				yield return new WaitForSeconds(p_wait);

				currentGunData.Reload();
				currentWeapon.SetActive(true);
				isReloading = false;

				// Sound
				sfx.PlayOneShot(currentGunData.finishReloadSound);
			}
		}

		IEnumerator SingleReload(float p_wait, int i)
		{
			if (i != 0)
			{
				Debug.Log("Needs a reload");
				sfx.PlayOneShot(currentGunData.reloadSound);

				yield return new WaitForSeconds(p_wait);

				i--;
				StartCoroutine(SingleReload(p_wait, i));
			}
			else
			{
				Debug.Log("done reloading");
				currentGunData.Reload();
				currentWeapon.SetActive(true);
				isReloading = false;

				// Sound
				sfx.PlayOneShot(currentGunData.finishReloadSound);
			}
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
			newWeapon.Initialize();

			loadout[newWeapon.slot] = newWeapon;

			newWeapon.Initialize();

			currentIndex = -1;
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
			if (!currentGunData.canADS) return false;

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
				Vector3 t_bloom = t_spawn.position + t_spawn.forward * currentGunData.range;

				float t_factor = currentGunData.bloom;
				if (GetComponent<Player>().jumped) t_factor = currentGunData.jumpingBloom;
				else if (Mathf.Abs(GetComponent<Player>().t_hMove) > 0.5 || Mathf.Abs(GetComponent<Player>().t_vMove) > 0.5) t_factor = currentGunData.movingBloom;

				if (isAiming) t_factor *= currentGunData.ADSBloomMultipler;

				t_bloom += Random.Range(-t_factor, t_factor) * t_spawn.up;
				t_bloom += Random.Range(-t_factor, t_factor) * t_spawn.right;
				t_bloom -= t_spawn.position;
				t_bloom.Normalize();

				// Raycast
				RaycastHit t_hit = new RaycastHit();

				if (currentGunData.boxCast)
				{
					if (Physics.BoxCast(transform.position, new Vector3(0.5f, 1, 0.5f), transform.Find("Cameras/Normal Camera").forward, out t_hit, transform.Find("Cameras/Normal Camera").localRotation, currentGunData.range, canBeShot))
					{
						if (photonView.IsMine)
						{
							// Shooting other player on network
							if (t_hit.collider.gameObject.layer == 11)
							{
								float t_multiplier = 1;

								if (Vector3.Distance(transform.position, t_hit.collider.transform.root.Find("Misc/Front").position) > Vector3.Distance(transform.position, t_hit.collider.transform.root.Find("Misc/Back").position)) t_multiplier = currentGunData.headshotMultiplier;

								// Give damage
								t_hit.collider.transform.root.gameObject.GetPhotonView().RPC("TakeDamage", RpcTarget.AllBuffered, currentGunData.damage, PhotonNetwork.LocalPlayer.ActorNumber, t_multiplier);
								photonView.RPC("TakeDamage", RpcTarget.AllBuffered, currentGunData.lifeSteal, PhotonNetwork.LocalPlayer.ActorNumber, t_multiplier);

								// Show hitmarker
								hitmarkerImage.color = Color.white;
								sfx.PlayOneShot(hitmarkerSound);
								hitmarkerWait = 0.5f;
							}
						}
					}
				}
				else if (currentGunData.projectileBased)
				{
					if (photonView.IsMine)
					{
						photonView.RPC("ShootProjectile", RpcTarget.All, currentGunData.gunName, t_bloom, PhotonNetwork.LocalPlayer.ActorNumber);
					}
				}
				else
				{
					Debug.DrawRay(t_spawn.position, t_bloom * currentGunData.range, Color.red, 1f);
					if (Physics.Raycast(t_spawn.position, t_bloom, out t_hit, currentGunData.range, canBeShot))
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
								float t_multiplier = 1;

								if (t_hit.collider.gameObject.name == "Head") t_multiplier = currentGunData.headshotMultiplier;

								// Give damage
								t_hit.collider.transform.root.gameObject.GetPhotonView().RPC("TakeDamage", RpcTarget.AllBuffered, currentGunData.damage, PhotonNetwork.LocalPlayer.ActorNumber, t_multiplier);
								photonView.RPC("TakeDamage", RpcTarget.AllBuffered, currentGunData.lifeSteal, PhotonNetwork.LocalPlayer.ActorNumber, t_multiplier);

								// Show hitmarker
								hitmarkerImage.color = Color.white;
								sfx.PlayOneShot(hitmarkerSound);
								hitmarkerWait = 0.5f;
							}
						}
					}
				}
			}

			// Sound
			sfx.pitch = 1 - currentGunData.pitchRandomization + Random.Range(-currentGunData.pitchRandomization, currentGunData.pitchRandomization);
			sfx.PlayOneShot(currentGunData.gunshotSound);

			// Animation
			if (currentWeapon.GetComponent<Animator>() != null)
			{
				currentWeapon.GetComponent<Animator>().Play("Shoot", 0, 0);
			}

			// Gun FX
			currentWeapon.transform.Rotate(-currentGunData.recoil, 0, 0);
			currentWeapon.transform.position -= currentWeapon.transform.forward * currentGunData.kickback;
		}

		[PunRPC]
		private void TakeDamage(int p_damage, int p_actor, float p_multi)
		{
			GetComponent<Player>().TakeDamage(p_damage, p_actor, p_multi);
		}

		[PunRPC]
		private void ShootProjectile(string name, Vector3 direction, int actorNum)
		{
			// find the weapon from a library
			Gun newWeapon = GunLibrary.FindGun(name);

			newWeapon.projectile.SetActorNumber(actorNum);

			GameObject projectile = Instantiate(newWeapon.projectile.prefab) as GameObject;
			projectile.name = newWeapon.projectile.projectileName;
			projectile.transform.position = transform.Find("Cameras/Normal Camera").position + (transform.Find("Cameras/Normal Camera").forward * 2);
			projectile.transform.LookAt(transform.position + direction);
			Rigidbody rb = projectile.GetComponent<Rigidbody>();
			rb.velocity = direction * currentGunData.projectileSpeed;

			projectile.GetComponent<ProjectileHelper>().projParent = newWeapon.projectile;

			Destroy(projectile, newWeapon.destroyTime);
		}

		#endregion


		#region Public Methods

		public void RefreshAmmo(Text p_text, Text p_textFrame)
		{
			if (currentGunData != null)
			{
				int t_clip = currentGunData.GetClip();
				int t_stash = currentGunData.GetStash();

				if (t_clip <= (currentGunData.clipSize * 0.25))
				{
					p_text.color = Color.red;
				}
				else
				{
					p_text.color = Color.white;
				}

				p_text.text = t_clip.ToString("00") + " / " + t_stash.ToString("00");
				p_textFrame.text = t_clip.ToString("00") + " / " + t_stash.ToString("00");
			}
		}

		#endregion
	}
}
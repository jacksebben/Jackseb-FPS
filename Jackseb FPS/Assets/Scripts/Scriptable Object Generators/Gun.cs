using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Com.Jackseb.FPS
{
	[CreateAssetMenu(fileName = "New Gun", menuName = "Gun")]
	public class Gun : ScriptableObject
	{
		[Header("General")]
		[Tooltip("The name of the gun")]
		public string gunName;
		[Tooltip("0 = primary, 1 = seconday, etc...")]
		public int slot;
		[Tooltip("Must the gun be aimed down sight in order to shoot?")]
		public bool mustAimFirst;
		[Tooltip("How fast the player moves when holding this weapon")]
		public int playerSpeed;
		[Tooltip("How high the player jumps when holding this weapon")]
		public int playerJump;


		[Header("Damage")]
		[Tooltip("Base damage of the gun")]
		public int damage;
		[Tooltip("How much damage will be multiplied to get headshot damage")]
		public float headshotMultiplier;
		[Tooltip("How much health you regain after killing with it")]
		public int lifeSteal;
		[Tooltip("How far a gun can be shot")]
		public float range;
		[Tooltip("0 = semi, 1 = automatic, 2 = burst")]
		public int burst;
		[Tooltip("How many pellets the gun will produce on fire")]
		public int pellets;
		[Tooltip("Time in seconds between each bullet")]
		public float firerate;


		[Header("Bloom")]
		[Tooltip("The spread of the weapon")]
		public float bloom;
		[Tooltip("The spread of the weapon when moving")]
		public float movingBloom;
		[Tooltip("The spread of the weapon when jumping")]
		public float jumpingBloom;
		[Tooltip("The multiplier for bloom when scoping")]
		public float ADSBloomMultipler;
		[Tooltip("How much gun rotates on fire")]
		public float recoil;
		[Tooltip("How much gun moves back on fire")]
		public float kickback;


		[Header("Projectiles")]
		[Tooltip("Does this gun fire projectiles instead of raycasting?")]
		public bool projectileBased;
		[Tooltip("The projectile the gun shoots")]
		public Projectile projectile;
		[Tooltip("How fast does the projectile move in the air?")]
		public int projectileSpeed;
		[Tooltip("How many seconds until the projectile despawns")]
		public float destroyTime;
		[Tooltip("How many bounces until the projectile despawns")]
		public int maxBounces;
		[Tooltip("What plays when the projectile hits something")]
		public AudioClip hitSound;


		[Header("ADS")]
		[Tooltip("Can this gun aim down a sight?")]
		public bool canADS;
		[Tooltip("Check for hit detection with a box instead of a ray?")]
		public bool boxCast;
		[Tooltip("Speed to aim down the sights (Time.deltaTime)")]
		public float aimSpeed;


		[Header("Reloading")]
		[Tooltip("Does this gun need to reload?")]
		public bool needsReload;
		[Tooltip("Does the reload sound occur per bullet reloaded?")]
		public bool playSoundIndividually;
		[Tooltip("Amount in a single magazine")]
		public int clipSize;
		[Tooltip("Amount of reserve ammo")]
		public int ammo;
		[Tooltip("Time in seconds to reload")]
		public float reloadTime;


		[Header("Sounds")]
		[Tooltip("Sound when gun is fired")]
		public AudioClip gunshotSound;
		[Tooltip("Sound when gun is reloaded")]
		public AudioClip reloadSound;
		[Tooltip("Sound when gun is done reloading")]
		public AudioClip finishReloadSound;
		[Tooltip("How much the pitch of the gunshot sound differs between shots")]
		public float pitchRandomization;


		[Header("Misc")]
		[Tooltip("Prefab of gun model")]
		public GameObject prefab;
		[Tooltip("Prefab of pickup model")]
		public GameObject displayPrefab;
		[Tooltip("How big the weapon is unscoped")]
		[Range(0, 1)] public float mainFOV;
		[Tooltip("How big the weapon is ADS")]
		[Range(0, 1)] public float weaponFOV;

		private int clip; // Current ammo
		private int stash; // Current reserve
		private int currentBounces; // Current bounces

		public void Initialize()
		{
			stash = ammo;
			clip = clipSize;

			if (projectileBased)
			{
				projectile.Initialize(this);
			}
		}

		public bool CanFireBullet(bool isAiming)
		{
			if (mustAimFirst && !isAiming) return false;

			if (needsReload)
			{
				if (clip > 0)
				{
					clip -= 1;
					return true;
				}
				else return false;
			}
			else
			{
				return true;
			}
		}

		public bool CanReload()
		{
			if (clip == clipSize) return false;
			if (stash == 0) return false;
			else return true;
		}

		public void Reload()
		{
			stash += clip;
			clip = Mathf.Min(clipSize, stash);
			stash -= clip;
		}

		public int GetStash() { return stash; }

		public int GetClip() { return clip; }
	}
}
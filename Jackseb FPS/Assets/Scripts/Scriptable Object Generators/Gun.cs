using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.Jackseb.FPS
{
	[CreateAssetMenu(fileName = "New Gun", menuName = "Gun")]
	public class Gun : ScriptableObject
	{
		[Tooltip("The name of the gun")]
		public string gunName;
		[Tooltip("Base damage of the gun")]
		public int damage;
		[Tooltip("How much damage will be multiplied to get headshot damage")]
		public float headshotMultiplier;
		[Tooltip("How far a gun can be shot")]
		public float range;
		[Tooltip("Amount in a single magazine")]
		public int clipSize;
		[Tooltip("Amount of reserve ammo")]
		public int ammo;
		[Tooltip("0 = semi, 1 = automatic, 2 = burst")]
		public int burst;
		[Tooltip("How many pellets the gun will produce on fire")]
		public int pellets;
		[Tooltip("0 = primary, 1 = seconday, etc...")]
		public int slot;
		[Tooltip("Time in seconds between each bullet")]
		public float firerate;
		[Tooltip("The spread of the weapon")]
		public float bloom;
		[Tooltip("The spread of the weapon when moving")]
		public float movingBloom;
		[Tooltip("The spread of the weapon when jumping")]
		public float jumpingBloom;
		[Tooltip("How much gun rotates on fire")]
		public float recoil;
		[Tooltip("How much gun moves back on fire")]
		public float kickback;
		[Tooltip("Can this gun aim down a sight?")]
		public bool canADS;
		[Tooltip("Check for hit detection with a box instead of a ray?")]
		public bool boxCast;
		[Tooltip("Speed to aim down the sights (Time.deltaTime)")]
		public float aimSpeed;
		[Tooltip("Does this gun need to reload?")]
		public bool canReload;
		[Tooltip("Time in seconds to reload")]
		public float reloadTime;
		[Tooltip("How big the weapon is unscoped")]
		[Range(0, 1)] public float mainFOV;
		[Tooltip("How big the weapon is ADS")]
		[Range(0, 1)] public float weaponFOV;
		[Tooltip("Sound when gun is fired")]
		public AudioClip gunshotSound;
		[Tooltip("Sound when gun is reloaded")]
		public AudioClip reloadSound;
		[Tooltip("Sound when gun is done reloading")]
		public AudioClip finishReloadSound;
		[Tooltip("How much the pitch of the gunshot sound differs between shots")]
		public float pitchRandomization;
		[Tooltip("Prefab of gun model")]
		public GameObject prefab;
		[Tooltip("Prefab of pickup model")]
		public GameObject displayPrefab;

		private int stash; // Current ammo
		private int clip; // Current clip

		public void Initialize()
		{
			stash = ammo;
			clip = clipSize;
		}

		public bool CanFireBullet()
		{
			if (canReload)
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
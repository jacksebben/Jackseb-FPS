using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.Jackseb.FPS
{
	[CreateAssetMenu(fileName = "New Gun", menuName = "Gun")]
	public class Gun : ScriptableObject
	{
		public string gunName;
		public int damage;
		public int ammo;
		public int burst; // 0 semi | 1 automatic | 2 
		public int pellets;
		public int clipSize;
		public int slot; // 0 = alpha1 | 1 = alpha 2 | etc...
		public float firerate;
		public float bloom;
		public float recoil;
		public float kickback;
		public float aimSpeed;
		public float reloadTime;
		[Range(0, 1)] public float mainFOV;
		[Range(0, 1)] public float weaponFOV;
		public AudioClip gunshotSound;
		public AudioClip reloadSound;
		public AudioClip finishReloadSound;
		public float pitchRandomization;
		public GameObject prefab;
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
			if (clip > 0)
			{
				clip -= 1;
				return true;
			}
			else return false;
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
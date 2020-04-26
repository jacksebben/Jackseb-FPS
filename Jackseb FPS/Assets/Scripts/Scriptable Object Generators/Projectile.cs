using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.Jackseb.FPS
{
	[CreateAssetMenu(fileName = "New Projectile", menuName = "Projectile")]
	public class Projectile : ScriptableObject
	{
		[Tooltip("The name of the projectile")]
		public string projectileName;
		[Tooltip("Prefab of projectile model")]
		public GameObject prefab;

		private int actorNumber; // actor number of player who shot projectile
		private int damage; // matches that of gun
		private float headshotMultiplier; // matches that of gun
		private AudioClip hitSound; // matches that of gun

		public void Initialize(Gun gunParent)
		{
			damage = gunParent.damage;
			headshotMultiplier = gunParent.headshotMultiplier;
			hitSound = gunParent.hitSound;
		}

		public void SetActorNumber(int actorNum)
		{
			actorNumber = actorNum;
		}

		public int GetActorNumber() { return actorNumber; }

		public int GetDamage() { return damage; }

		public float GetMultiplier() { return headshotMultiplier; }
	}
}
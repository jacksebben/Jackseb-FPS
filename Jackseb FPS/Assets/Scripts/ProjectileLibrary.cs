using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.Jackseb.FPS
{
	public class ProjectileLibrary : MonoBehaviour
	{
		public Projectile[] allProjectiles;
		public static Projectile[] projectiles;

		private void Awake()
		{
			projectiles = allProjectiles;
		}

		public static Projectile FindProjectile(string name)
		{
			foreach (Projectile a in projectiles)
			{
				if (a.name.Equals(name)) return a;
			}

			return projectiles[0];
		}
	}
}
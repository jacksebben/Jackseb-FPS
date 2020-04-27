using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.Jackseb.FPS
{
	public class ProjectileHelper : MonoBehaviour
	{
		public Projectile projParent;

		public int spinx = 0;
		public int spiny = 0;
		public int spinz = 0;

		public AudioSource audioS;

		private int currentBounces = 0;

		private void Update()
		{
			transform.Find("Design").Rotate(spinx, spiny, spinz);

			if (GetComponent<Rigidbody>().velocity != Vector3.zero)
			{
				transform.LookAt(transform.position + GetComponent<Rigidbody>().velocity);
			}
		}

		private void OnCollisionEnter(Collision collision)
		{
			audioS.PlayOneShot(projParent.GetHitSound());

			currentBounces++;
			if (currentBounces == projParent.gunParent.maxBounces)
			{
				Destroy(gameObject);
			}
		}
	}

}
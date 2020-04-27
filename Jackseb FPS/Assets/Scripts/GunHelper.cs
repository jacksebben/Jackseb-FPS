using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.Jackseb.FPS
{
	public class GunHelper : MonoBehaviour
	{
		public Gun gunParent;

		public GameObject[] disableOnEmpty;

		// Update is called once per frame
		void Update()
		{
			if (!gunParent.CanFireBullet(true))
			{
				foreach (GameObject a in disableOnEmpty)
				{
					a.SetActive(false);
				}
			}
			else
			{
				foreach (GameObject a in disableOnEmpty)
				{
					a.SetActive(true);
				}
			}
		}
	}
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace Com.Jackseb.FPS
{
	public class Pickup : MonoBehaviourPunCallbacks
	{
		public Gun[] weapons;
		public float cooldown;
		public GameObject gunDisplay;
		public List<GameObject> targets;

		public bool randomWeapons;
		public bool isDisabled;

		private float wait;
		private Gun currentWeapon;

		private void Start()
		{
			InstantiateWeapon();
		}

		void InstantiateWeapon()
		{
			foreach (Transform t in gunDisplay.transform) Destroy(t.gameObject);

			if (randomWeapons)
			{
				currentWeapon = weapons[Random.Range(0, weapons.Length)];
			}
			else
			{
				currentWeapon = weapons[0];
			}
			GameObject newDisplay = Instantiate(currentWeapon.displayPrefab, gunDisplay.transform.position, gunDisplay.transform.rotation) as GameObject;
			newDisplay.transform.SetParent(gunDisplay.transform);
		}

		private void Update()
		{
			if (isDisabled)
			{
				if (wait > 0)
				{
					wait -= Time.deltaTime;
				}
				else
				{
					Enable();
				}
			}

			gunDisplay.transform.Rotate(0, 1, 0);
		}

		private void OnTriggerEnter(Collider other)
		{
			if (other.attachedRigidbody == null) return;

			if (other.gameObject.tag.Equals("Player"))
			{
				Weapon weaponController = other.transform.root.gameObject.GetComponent<Weapon>();
				weaponController.photonView.RPC("PickupWeapon", RpcTarget.AllBuffered, currentWeapon.name);
				photonView.RPC("Disable", RpcTarget.AllBuffered);
			}
		}

		[PunRPC]
		public void Disable()
		{
			isDisabled = true;
			wait = cooldown;

			foreach (GameObject a in targets) a.SetActive(false);
		}

		private void Enable()
		{
			isDisabled = false;
			wait = 0;

			foreach (GameObject a in targets) a.SetActive(true);

			InstantiateWeapon();
		}
	}
}
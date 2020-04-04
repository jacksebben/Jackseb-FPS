using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace Com.Jackseb.FPS
{
	public class Pickup : MonoBehaviourPunCallbacks
	{
		public Gun weapon;
		public float cooldown;
		public GameObject gunDisplay;
		public List<GameObject> targets;

		public bool isDisabled;
		private float wait;

		private void Start()
		{
			foreach (Transform t in gunDisplay.transform) Destroy(t.gameObject);

			GameObject newDisplay = Instantiate(weapon.displayPrefab, gunDisplay.transform.position, gunDisplay.transform.rotation) as GameObject;
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
		}

		private void OnTriggerEnter(Collider other)
		{
			if (other.attachedRigidbody == null) return;

			if (other.gameObject.tag.Equals("Player"))
			{
				Weapon weaponController = other.transform.root.gameObject.GetComponent<Weapon>();
				weaponController.photonView.RPC("PickupWeapon", RpcTarget.AllBuffered, weapon.name);
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
		}
	}
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileHelper : MonoBehaviour
{
	public int spinx = 0;
	public int spiny = 0;
	public int spinz = 0;

	public AudioSource audioS;
	public AudioClip hitSound;

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
		audioS.PlayOneShot(hitSound);
	}
}

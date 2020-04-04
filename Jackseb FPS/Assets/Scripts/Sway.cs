using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace Com.Jackseb.FPS
{
	public class Sway : MonoBehaviourPunCallbacks
	{
		#region Variables

		public float intensity;
		public float smooth;
		public bool isMine;

		private Quaternion originRotation;

		#endregion


		#region MonoBehavior Callbacks

		private void Start()
		{
			originRotation = transform.localRotation;
		}

		private void Update()
		{
			if (Pause.paused) return;

			UpdateSway();
		}

		#endregion


		#region Private Methods

		private void UpdateSway()
		{
			// Controls
			float t_xMouse = Input.GetAxis("Mouse X");
			float t_yMouse = Input.GetAxis("Mouse Y");

			if (!isMine)
			{
				t_xMouse = 0;
				t_yMouse = 0;
			}

			// Calculate target rotation
			Quaternion t_xAdj = Quaternion.AngleAxis(intensity * t_xMouse, -Vector3.up);
			Quaternion t_yAdj = Quaternion.AngleAxis(intensity * t_yMouse, Vector3.right);
			Quaternion t_targetRotation = originRotation * t_xAdj * t_yAdj;

			// Rotate towards target
			transform.localRotation = Quaternion.Lerp(transform.localRotation, t_targetRotation, Time.deltaTime * smooth);
		}

		#endregion
	}
}
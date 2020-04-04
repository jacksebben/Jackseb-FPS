using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;

namespace Com.Jackseb.FPS
{
	public class Pause : MonoBehaviour
	{
		public static bool paused = false;
		private bool disconnecting = false;

		public void TogglePause()
		{
			if (disconnecting) return;

			paused = !paused;

			transform.GetChild(0).gameObject.SetActive(paused);
			Cursor.lockState = (paused) ? CursorLockMode.None : CursorLockMode.Locked;
			Cursor.visible = paused;
		}

		public void Quit()
		{
			disconnecting = true;
			PhotonNetwork.LeaveRoom();
			SceneManager.LoadScene(0);
		}
	}
}
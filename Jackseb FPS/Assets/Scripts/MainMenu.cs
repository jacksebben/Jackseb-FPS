using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.Jackseb.FPS
{
	public class MainMenu : MonoBehaviour
	{
		public Launcher r_Launcher;

		private void Start()
		{
			Pause.paused = false;
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		}

		public void JoinMatch()
		{
			r_Launcher.Join();
		}

		public void CreateMatch()
		{
			r_Launcher.Create();
		}

		public void QuitGame()
		{
			Application.Quit();
		}
	}
}
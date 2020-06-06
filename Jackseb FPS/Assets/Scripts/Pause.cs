using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using Photon.Pun;

namespace Com.Jackseb.FPS
{
	public class Pause : MonoBehaviour
	{
		public GameObject mainScreen;
		public GameObject optionsScreen;

		public Slider volumeSlider;
		public Text volumeValue;
		public Slider sensSlider;
		public Text sensValue;
		public AudioMixer mixer;

		public static bool paused = false;
		private bool disconnecting = false;

		private void Awake()
		{
			volumeSlider.value = PlayerPrefs.GetFloat("volumeSlider");
			sensSlider.value = PlayerPrefs.GetFloat("sensSlider");
		}

		private void Update()
		{
			mixer.SetFloat("masterVol", (PlayerPrefs.GetFloat("volumeSlider") * 0.8f) - 80);
		}

		public void TogglePause()
		{
			if (disconnecting) return;

			paused = !paused;

			transform.GetChild(0).gameObject.SetActive(paused);
			Cursor.lockState = (paused) ? CursorLockMode.None : CursorLockMode.Locked;
			Cursor.visible = paused;

			mainScreen.SetActive(paused);
			optionsScreen.SetActive(false);
		}

		public void EnableOptions(bool options)
		{
			mainScreen.SetActive(!options);
			optionsScreen.SetActive(options);
		}

		public void ChangeVolumeSlider(float p_value)
		{
			volumeValue.text = Mathf.RoundToInt(p_value).ToString() + "%";
			mixer.SetFloat("masterVol", (p_value * 0.8f) - 80);
			PlayerPrefs.SetFloat("volumeSlider", volumeSlider.value);
		}

		public void ChangeSensSlider(float p_value)
		{
			sensValue.text = (p_value / 10).ToString("0.0");
			PlayerPrefs.SetFloat("sensSlider", sensSlider.value);
		}

		public void Quit()
		{
			disconnecting = true;
			PhotonNetwork.LeaveRoom();
			SceneManager.LoadScene(0);
		}
	}
}
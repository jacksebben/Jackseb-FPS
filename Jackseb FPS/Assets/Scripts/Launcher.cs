using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using PhotonHash = ExitGames.Client.Photon.Hashtable;

namespace Com.Jackseb.FPS
{
	[System.Serializable]
	public class ProfileData
	{
		public string username;
		public int level;
		public int xp;
		public int elo;

		public ProfileData()
		{
			this.username = "DEFAULT USERNAME";
			this.level = 0;
			this.xp = 0;
			this.elo = 0;
		}

		public ProfileData(string u, int l, int x, int e)
		{
			this.username = u;
			this.level = l;
			this.xp = x;
			this.elo = e;
		}

		public ProfileData(object[] objs)
		{
			this.username = objs[0] as string;
			this.level = (int)objs[1];
			this.xp = (int)objs[2];
			this.elo = (int)objs[3];
		}

		public object[] convertToObjArr ()
		{
			object[] ret = new object[4];

			ret[0] = username;
			ret[1] = level;
			ret[2] = xp;
			ret[3] = elo;

			return ret;
		}
	}

	[System.Serializable]
	public class MapData
	{
		public string name;
		public int scene;
	}

	public class Launcher : MonoBehaviourPunCallbacks
	{
		[Header("Game Version")]
		public string version;

		public GameObject tabMain;
		public GameObject tabRooms;
		public GameObject tabCreate;

		public GameObject buttonRoomPrefab;

		public GameObject connectingText;
		public GameObject versionText;
		public GameObject regionText;
		public GameObject[] hideWhileConnecting;

		public InputField usernameField;
		public InputField roomNameField;
		public Text mapValue;
		public Slider maxPlayerSlider;
		public Text maxPlayerValue;
		public Slider killCountSlider;
		public Text killCountValue;
		public static ProfileData myProfile = new ProfileData();

		public MapData[] maps;
		private int currentMap = 0;

		private List<RoomInfo> roomList;

		public void Awake()
		{
			TabCloseAll();

			connectingText.SetActive(true);
			foreach (GameObject obj in hideWhileConnecting)
			{
				obj.SetActive(false);
			}

			PhotonNetwork.AutomaticallySyncScene = true;

			myProfile = Data.LoadProfile();
			usernameField.text = myProfile.username;

			Connect();
		}

		private void Update()
		{
			versionText.GetComponent<Text>().text = "Version " + version;
			if (PhotonNetwork.CloudRegion == "us")
			{
				regionText.GetComponent<Text>().text = "US-East";
			}
			else if (PhotonNetwork.CloudRegion == "usw")
			{
				regionText.GetComponent<Text>().text = "US-West";
			}
		}

		public override void OnConnectedToMaster()
		{
			connectingText.SetActive(false);
			foreach (GameObject obj in hideWhileConnecting)
			{
				obj.SetActive(true);
			}

			TabOpenMain();

			PhotonNetwork.JoinLobby();

			PhotonNetwork.NetworkingClient.AppVersion = version;

			base.OnConnectedToMaster();
		}

		public override void OnJoinedRoom()
		{
			StartGame();

			base.OnJoinedRoom();
		}

		public override void OnJoinRandomFailed(short returnCode, string message)
		{
			Create();

			base.OnJoinRandomFailed(returnCode, message);
		}

		public void Connect()
		{
			PhotonNetwork.GameVersion = version;
			PhotonNetwork.ConnectUsingSettings();
			PhotonNetwork.GameVersion = version;
		}

		public void Join()
		{
			connectingText.SetActive(true);
			foreach (GameObject obj in hideWhileConnecting)
			{
				obj.SetActive(false);
			}

			TabCloseAll();

			PhotonNetwork.JoinRandomRoom();
		}

		public void Create()
		{
			connectingText.SetActive(true);
			foreach (GameObject obj in hideWhileConnecting)
			{
				obj.SetActive(false);
			}

			TabCloseAll();

			RoomOptions options = new RoomOptions();
			options.MaxPlayers = (byte)maxPlayerSlider.value;

			options.CustomRoomPropertiesForLobby = new string[] { "map", "killCount" };

			PhotonHash properties = new PhotonHash();
			properties.Add("map", currentMap);
			properties.Add("killCount", killCountSlider.value * 5);
			options.CustomRoomProperties = properties;

			PhotonNetwork.CreateRoom(roomNameField.text, options);
		}

		public void ChangeMap()
		{
			currentMap++;
			if (currentMap >= maps.Length) currentMap = 0;
			mapValue.text = "MAP: " + maps[currentMap].name.ToUpper();
		}

		public void ChangeMaxPlayerSlider(float p_value)
		{
			maxPlayerValue.text = Mathf.RoundToInt(p_value).ToString();
		}

		public void ChangeKillCountSlider(float p_value)
		{
			killCountValue.text = Mathf.RoundToInt(p_value * 5).ToString();
		}

		public void TabCloseAll()
		{
			tabMain.SetActive(false);
			tabRooms.SetActive(false);
			tabCreate.SetActive(false);
		}

		public void TabOpenMain()
		{
			TabCloseAll();
			tabMain.SetActive(true);
		}

		public void TabOpenRooms()
		{
			TabCloseAll();
			tabRooms.SetActive(true);
		}

		public void TabOpenCreate()
		{
			TabCloseAll();
			tabCreate.SetActive(true);

			roomNameField.text = "";

			currentMap = 0;
			mapValue.text = "MAP: " + maps[currentMap].name.ToUpper();

			maxPlayerSlider.value = 8;
			maxPlayerValue.text = Mathf.RoundToInt(maxPlayerSlider.value).ToString();

			killCountSlider.value = 5;
			killCountValue.text = Mathf.RoundToInt(killCountSlider.value * 5).ToString();
		}

		private void ClearRoomList()
		{
			Transform content = tabRooms.transform.Find("Scroll View/Viewport/Content");
			foreach (Transform a in content) Destroy(a.gameObject);
		}

		private void VerifyUsername()
		{
			if (string.IsNullOrEmpty(usernameField.text))
			{
				myProfile.username = "RANDOM_USER_" + Random.Range(100, 1000);
			}
			else
			{
				myProfile.username = usernameField.text;
			}
		}

		public override void OnRoomListUpdate(List<RoomInfo> p_list)
		{
			roomList = p_list;
			ClearRoomList();

			Transform content = tabRooms.transform.Find("Scroll View/Viewport/Content");

			foreach (RoomInfo a in roomList)
			{
				GameObject newRoomButton = Instantiate(buttonRoomPrefab, content) as GameObject;

				newRoomButton.transform.Find("Name").GetComponent<Text>().text = a.Name;
				newRoomButton.transform.Find("Players").GetComponent<Text>().text = a.PlayerCount + " / " + a.MaxPlayers;

				if (a.CustomProperties.ContainsKey("map"))
				{
					newRoomButton.transform.Find("Map/Name").GetComponent<Text>().text = maps[(int)a.CustomProperties["map"]].name;
				}
				else
				{
					newRoomButton.transform.Find("Map/Name").GetComponent<Text>().text = "-----";
				}

				newRoomButton.GetComponent<Button>().onClick.AddListener(delegate { JoinRoom(newRoomButton.transform); });
			}

			base.OnRoomListUpdate(roomList);
		}

		public void JoinRoom(Transform p_button)
		{
			string t_roomName = p_button.Find("Name").GetComponent<Text>().text;

			VerifyUsername();

			PhotonNetwork.JoinRoom(t_roomName);
		}

		public void StartGame()
		{
			VerifyUsername();

			if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
			{
				Data.SaveProfile(myProfile);
				PhotonNetwork.LoadLevel(maps[currentMap].scene);
			}
		}
	}
}
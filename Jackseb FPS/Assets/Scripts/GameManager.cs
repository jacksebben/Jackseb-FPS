using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

namespace Com.Jackseb.FPS
{
	public class PlayerInfo
	{
		public ProfileData profile;
		public int actor;
		public short kills;
		public short deaths;

		public PlayerInfo(ProfileData p, int a, short k, short d)
		{
			this.profile = p;
			this.actor = a;
			this.kills = k;
			this.deaths = d;
		}
	}

	public enum GameState
	{
		Waiting = 0,
		Starting = 1,
		Playing = 2,
		Ending = 3
	}

	public class GameManager : MonoBehaviourPunCallbacks, IOnEventCallback
	{
		#region Fields

		public int mainMenu = 0;
		public int killCount = 3;
		public bool perpetual = false;

		public GameObject mapCam;

		public string playerPrefabString;
		public GameObject playerPrefab;
		public Transform[] spawnPoints;

		public List<PlayerInfo> playerInfo = new List<PlayerInfo>();
		public int myInd;

		private Text uiMyKills;
		private Text uiMyDeaths;
		private Transform uiScoreboard;
		private Transform uiEndgame;

		private GameState state = GameState.Waiting;

		#endregion

		#region Codes

		public enum EventCodes : byte
		{
			NewPlayer,
			UpdatePlayers,
			ChangeStat,
			NewMatch
		}

		#endregion

		#region MB Callbacks

		private void Start()
		{
			mapCam.SetActive(false);

			killCount = Mathf.RoundToInt((float)PhotonNetwork.CurrentRoom.CustomProperties["killCount"]);

			ValidateConnection();
			InitializeUI();
			NewPlayer_S(Launcher.myProfile);
			Spawn();
		}

		private void Update()
		{
			if (state == GameState.Ending)
			{
				return;
			}

			if (Input.GetKey(KeyCode.Tab) || Input.GetKey(KeyCode.F1))
			{
				Scoreboard(uiScoreboard);
			}
			else
			{
				uiScoreboard.gameObject.SetActive(false);
			}
		}

		private void OnEnable()
		{
			PhotonNetwork.AddCallbackTarget(this);
		}

		private void OnDisable()
		{
			PhotonNetwork.RemoveCallbackTarget(this);
		}

		#endregion

		#region Photon

		public void OnEvent(EventData photonEvent)
		{
			if (photonEvent.Code >= 200) return;

			EventCodes e = (EventCodes)photonEvent.Code;
			object[] o = (object[])photonEvent.CustomData;

			switch (e)
			{
				case EventCodes.NewPlayer:
					NewPlayer_R(o);
					break;
				case EventCodes.UpdatePlayers:
					UpdatePlayers_R(o);
					break;
				case EventCodes.ChangeStat:
					ChangeStat_R(o);
					break;
				case EventCodes.NewMatch:
					NewMatch_R();
					break;
			}
		}

		public override void OnLeftRoom()
		{
			base.OnLeftRoom();
			SceneManager.LoadScene(mainMenu);
		}

		#endregion

		#region Methods

		public void Spawn()
		{
			Transform t_spawn = spawnPoints[Random.Range(0, spawnPoints.Length)];

			if (PhotonNetwork.IsConnected)
			{
				PhotonNetwork.Instantiate(playerPrefabString, t_spawn.position, t_spawn.rotation);
			}
			else
			{
				GameObject newPlayer = Instantiate(playerPrefab, t_spawn.position, t_spawn.rotation) as GameObject;
			}
		}

		private void InitializeUI()
		{
			uiMyKills = GameObject.Find("HUD/Stats/Kills/Text").GetComponent<Text>();
			uiMyDeaths = GameObject.Find("HUD/Stats/Deaths/Text").GetComponent<Text>();
			uiScoreboard = GameObject.Find("HUD").transform.Find("Scoreboard").transform;
			uiEndgame = GameObject.Find("Canvas").transform.Find("End Game").transform;

			RefreshMyStats();
		}

		private void RefreshMyStats()
		{
			if (playerInfo.Count > myInd)
			{
				uiMyKills.text = $"{playerInfo[myInd].kills} kills";
				uiMyDeaths.text = $"{playerInfo[myInd].deaths} deaths";
			}
			else
			{
				uiMyKills.text = "0 kills";
				uiMyDeaths.text = "0 deaths";
			}
		}

		private void Scoreboard (Transform p_sb)
		{
			// clean up
			for (int i = 2; i < p_sb.childCount; i++)
			{
				Destroy(p_sb.GetChild(i).gameObject);
			}

			// set details
			p_sb.Find("Header/Mode").GetComponent<Text>().text = "FREE FOR ALL";
			p_sb.Find("Header/Map").GetComponent<Text>().text = "Test1";

			// cache prefab
			GameObject playerCard = p_sb.GetChild(1).gameObject;
			playerCard.SetActive(false);

			// sort
			List<PlayerInfo> sorted = SortPlayers(playerInfo);

			// display
			bool t_alternateColors = false;
			foreach (PlayerInfo a in sorted)
			{
				GameObject newCard = Instantiate(playerCard, p_sb) as GameObject;

				if (t_alternateColors) newCard.GetComponent<Image>().color = new Color32(0, 0, 0, 100);
				t_alternateColors = !t_alternateColors;

				newCard.transform.Find("Level").GetComponent<Text>().text = a.profile.level.ToString("00");
				newCard.transform.Find("Username").GetComponent<Text>().text = a.profile.username;
				newCard.transform.Find("Score Value").GetComponent<Text>().text = (a.kills * 100).ToString();
				newCard.transform.Find("Kills Value").GetComponent<Text>().text = a.kills.ToString();
				newCard.transform.Find("Deaths Value").GetComponent<Text>().text = a.deaths.ToString();

				newCard.SetActive(true);
			}

			// activate
			p_sb.gameObject.SetActive(true);
		}

		private List<PlayerInfo> SortPlayers(List<PlayerInfo> p_info)
		{
			List<PlayerInfo> sorted = new List<PlayerInfo>();

			while (sorted.Count < p_info.Count)
			{
				// set defaults
				short highest = -1;
				PlayerInfo selection = p_info[0];

				// grab next highest player
				foreach (PlayerInfo a in p_info)
				{
					if (sorted.Contains(a)) continue;
					if (a.kills > highest)
					{
						selection = a;
						highest = a.kills;
					}
				}

				// add player
				sorted.Add(selection);
			}

			return sorted;
		}

		private void ValidateConnection()
		{
			if (PhotonNetwork.IsConnected) return;
			SceneManager.LoadScene(mainMenu);
		}

		private void StateCheck()
		{
			if (state == GameState.Ending)
			{
				EndGame();
			}
		}

		private void ScoreCheck()
		{
			// define temporary	variables
			bool detectwin = false;

			// check to see if any player has met the win conditions
			foreach (PlayerInfo a in playerInfo)
			{
				// free for all
				if (a.kills >= killCount)
				{
					detectwin = true;
					break;
				}
			}

			// did we find a winner?
			if (detectwin)
			{
				// are we the master client? is the game still going?
				if (PhotonNetwork.IsMasterClient && state != GameState.Ending)
				{
					// if so, tell the other players that a winner has been detected
					UpdatePlayers_S((int)GameState.Ending, playerInfo);
				}
			}
		}

		private void EndGame()
		{
			// set game state to ending
			state = GameState.Ending;

			// disable room
			if (PhotonNetwork.IsMasterClient)
			{
				PhotonNetwork.DestroyAll();

				if (!perpetual)
				{
					PhotonNetwork.CurrentRoom.IsVisible = false;
					PhotonNetwork.CurrentRoom.IsOpen = false;
				}
			}

			// activate map camera
			mapCam.SetActive(true);

			// show end game ui
			uiEndgame.gameObject.SetActive(true);
			Scoreboard(uiEndgame.Find("Scoreboard"));

			// wait x seconds and then return to main menu
			StartCoroutine(End(6f));
		}

		#endregion

		#region Events

		public void NewPlayer_S(ProfileData p)
		{
			object[] package = new object[7];

			package[0] = p.username;
			package[1] = p.level;
			package[2] = p.xp;
			package[3] = p.elo;
			package[4] = PhotonNetwork.LocalPlayer.ActorNumber;
			package[5] = (short)0;
			package[6] = (short)0;

			PhotonNetwork.RaiseEvent(
				(byte)EventCodes.NewPlayer,
				package,
				new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient },
				new SendOptions { Reliability = true }
			);
		}

		public void NewPlayer_R(object[] data)
		{
			PlayerInfo p = new PlayerInfo(
				new ProfileData(
					(string) data[0],
					(int) data[1],
					(int) data[2],
					(int) data[3]
				),
				(int) data[4],
				(short) data[5],
				(short) data[6]
			);

			playerInfo.Add(p);

			UpdatePlayers_S((int)state, playerInfo);
		}

		public void UpdatePlayers_S(int state, List<PlayerInfo> info)
		{
			object[] package = new object[info.Count + 1];

			package[0] = state;
			for (int i = 0; i < info.Count; i++)
			{
				object[] piece = new object[7];

				piece[0] = info[i].profile.username;
				piece[1] = info[i].profile.level;
				piece[2] = info[i].profile.xp;
				piece[3] = info[i].profile.elo;
				piece[4] = info[i].actor;
				piece[5] = info[i].kills;
				piece[6] = info[i].deaths;

				package[i + 1] = piece;
			}

			PhotonNetwork.RaiseEvent(
				(byte)EventCodes.UpdatePlayers,
				package,
				new RaiseEventOptions { Receivers = ReceiverGroup.All },
				new SendOptions { Reliability = true }
			);
		}

		public void UpdatePlayers_R (object[] data)
		{
			state = (GameState)data[0];
			playerInfo = new List<PlayerInfo>();

			for (int i = 1; i < data.Length; i++)
			{
				object[] extract = (object[])data[i];

				PlayerInfo p = new PlayerInfo(
					new ProfileData(
						(string) extract[0],
						(int) extract[1],
						(int) extract[2],
						(int) extract[3]
					),
					(int) extract[4],
					(short) extract[5],
					(short) extract[6]
				);

				playerInfo.Add(p);

				if (PhotonNetwork.LocalPlayer.ActorNumber == p.actor) myInd = i - 1;
			}

			StateCheck();
		}

		public void ChangeStat_S (int actor, byte stat, byte amt)
		{
			object[] package = new object[] { actor, stat, amt };

			PhotonNetwork.RaiseEvent(
				(byte)EventCodes.ChangeStat,
				package,
				new RaiseEventOptions { Receivers = ReceiverGroup.All },
				new SendOptions { Reliability = true }
			);
		}

		public void ChangeStat_R(object[] data)
		{
			int actor = (int)data[0];
			byte stat = (byte)data[1];
			byte amt = (byte)data[2];

			for (int i = 0; i < playerInfo.Count; i++)
			{
				if (playerInfo[i].actor == actor)
				{
					switch (stat)
					{
						case 0: // kills
							playerInfo[i].kills += amt;
							Debug.Log($"Player {playerInfo[i].profile.username} : kills = {playerInfo[i].kills}");
							break;
						case 1: // deaths
							playerInfo[i].deaths += amt;
							Debug.Log($"Player {playerInfo[i].profile.username} : deaths = {playerInfo[i].deaths}");
							break;
					}

					if (i == myInd) RefreshMyStats();
					if (uiScoreboard.gameObject.activeSelf) Scoreboard(uiScoreboard);

					break;
				}
			}

			ScoreCheck();
		}

		public void NewMatch_S()
		{
			PhotonNetwork.RaiseEvent(
				(byte)EventCodes.NewMatch,
				null,
				new RaiseEventOptions { Receivers = ReceiverGroup.All },
				new SendOptions { Reliability = true }
			);
		}

		public void NewMatch_R()
		{
			// set game state to waiting
			state = GameState.Waiting;

			// deactivate map camera
			mapCam.SetActive(false);

			//hide end game ui
			uiEndgame.gameObject.SetActive(false);

			// reset scores
			foreach (PlayerInfo p in playerInfo)
			{
				p.kills = 0;
				p.deaths = 0;
			}

			// reset ui
			RefreshMyStats();

			// spawn
			Spawn();
		}

		#endregion

		#region Coroutines

		private IEnumerator End (float p_wait)
		{
			yield return new WaitForSeconds(p_wait);

			if (perpetual)
			{
				// new match
				if (PhotonNetwork.IsMasterClient)
				{
					NewMatch_S();
				}
			}
			else
			{
				// disconnect
				PhotonNetwork.AutomaticallySyncScene = false;
				PhotonNetwork.LeaveRoom();
			}
		}

		#endregion
	}
}
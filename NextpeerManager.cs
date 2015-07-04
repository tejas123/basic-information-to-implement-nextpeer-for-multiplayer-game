using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NextpeerManager : MonoBehaviour
{
    #region Public Variables

		//	public

		/// <summary>
		/// The android game key.
		/// </summary>
		public string androidGameKey;
		/// <summary>
		/// The ios game key.
		/// </summary>
		public string iosGameKey;

		// The synchronized event name that the clients will register to. Cannot be empty or null.
		public string START_GAME_SYNC_EVENT_NAME = "com.tag.yourgame.syncevent.startgame";

		// The timeout (as a TimeSpan instance) for the synchronized event. Cannot be zero or negative.
		public System.TimeSpan START_GAME_SYNC_EVENT_TIMEOUT = System.TimeSpan.FromSeconds (15);
		public int currentPlayerJoiningRank = 0;
		public int totalOppnents;
		public List<string> opponentsName;
		public List<string> opponentsId;
		public bool isSending = false;
		public List<long> playerIDList = new List<long> ();
		public long currentPlayerID;
    #endregion

    #region Private Variables
    
		public static NextpeerManager instance = null;
		private string thisGameKey, playerId;
		private string[] iOSTournamentId = {
		"NPA4565659***********",
		"NPA4565659***********"
		};
		
		// You have to give tournament id to this.
		private string[] androidTournamentId = {
				"NPA45603*************",
				"NPA45603*************"
		};
		private NPTournamentPlayer[] opponents;
    #endregion

    #region Unity Callbacks
		void Start ()
		{
				opponentsName = new List<string> ();
				opponentsId = new List<string> ();
				Debug.Log ("==NextpeerManager Awake==");
				DontDestroyOnLoad (gameObject);
#if UNITY_ANDROID
        thisGameKey = androidGameKey;
#elif UNITY_IPHONE
				thisGameKey = iosGameKey;
#endif
				//Init nextpeer
				Init ();
		}

		void Awake ()
		{
				instance = this;
		}

		public void setTournamentWithId (int index)
		{
				Debug.Log ("==Set Tournament==");

				for (int currentIndex = 0; currentIndex < iOSTournamentId.Length; currentIndex++) {
						if (currentIndex == index)
								allowTournamentWithId (currentIndex);
						else
								disableTournamentWithId (currentIndex);
				}

				Debug.Log ("-----Before Nextpeer Dashboard Lauched-----");
				LaunchDashbord ();
				Debug.Log ("-----Nextpeer Dashboard Lauched-----");
		}

		private void allowTournamentWithId (int index)
		{
#if UNITY_ANDROID
        Nextpeer.AddWhitelistTournament(androidTournamentId[index]);
#elif UNITY_IPHONE
				Nextpeer.AddWhitelistTournament (iOSTournamentId [index]);
#endif
		}

		private void disableTournamentWithId (int index)
		{
#if UNITY_ANDROID
        Nextpeer.RemoveWhitelistTournament(androidTournamentId[index]);
#elif UNITY_IPHONE
				Nextpeer.RemoveWhitelistTournament (iOSTournamentId [index]);
#endif
		}

		/// <summary>
		/// Raises the enable event.
		/// </summary>
		void OnEnable ()
		{
				// Basic information
				Nextpeer.DashboardWillAppear += this.DashboardWillAppear;
				Nextpeer.DashboardDidAppear += this.DashboardDidAppear;
				Nextpeer.DashboardWillDisappear += this.DashboardWillDisappear;
				Nextpeer.DashboardDidDisappear += this.DashboardDidDisappear;

				Nextpeer.WillTournamentStartWithDetails += this.WillTournamentStartWithDetails;
				Nextpeer.DidTournamentStartWithDetails += this.DidTournamentStartWithDetails;
				Nextpeer.DidReceiveTournamentCustomMessage += this.DidReceiveTournamentCustomMessage;
				Nextpeer.DidReceiveUnreliableTournamentCustomMessage += this.DidReceiveUnreliableTournamentCustomMessage;
				Nextpeer.DidTournamentEnd += this.DidTournamentEnd;
				Nextpeer.DidReceiveTournamentResults += this.DidReceiveTournamentResults;
				Nextpeer.DidReceiveTournamentStatus += this.HandleNextpeerDidReceiveTournamentStatus;

				Nextpeer.DidReceiveSynchronizedEvent += this.DidReceiveSynchronizedEvent;
		}

		/// <summary>
		/// Raises the disable event.
		/// </summary>
		void OnDisable ()
		{
				// Basic information
				Nextpeer.DashboardWillAppear -= this.DashboardWillAppear;
				Nextpeer.DashboardDidAppear -= this.DashboardDidAppear;
				Nextpeer.DashboardWillDisappear -= this.DashboardWillDisappear;
				Nextpeer.DashboardDidDisappear -= this.DashboardDidDisappear;

				Nextpeer.WillTournamentStartWithDetails -= this.WillTournamentStartWithDetails;
				Nextpeer.DidTournamentStartWithDetails -= this.DidTournamentStartWithDetails;
				Nextpeer.DidReceiveTournamentCustomMessage -= this.DidReceiveTournamentCustomMessage;
				Nextpeer.DidTournamentEnd -= this.DidTournamentEnd;
				Nextpeer.DidReceiveTournamentResults -= this.DidReceiveTournamentResults;
				Nextpeer.DidReceiveTournamentStatus -= this.HandleNextpeerDidReceiveTournamentStatus;

				Nextpeer.DidReceiveSynchronizedEvent -= this.DidReceiveSynchronizedEvent;
		}
    #endregion

    #region Public Functions
		/// <summary>
		/// Assign player.
		/// </summary>
		/// <param name="startInfo"></param>
		public void addPlayerToList (NPTournamentStartDataContainer startInfo)
		{
				Random.seed = ((int)startInfo.TournamentRandomSeed);
				playerIDList.Add (long.Parse (startInfo.CurrentPlayer.PlayerId));
				playerIDList.Add (long.Parse (startInfo.Opponents [0].PlayerId));
				playerIDList.Sort ();
		}
		/* 

		IEnumerator assignControlToPlayer (NPTournamentStartDataContainer startInfo)
		{
				Debug.Log ("In coroutine :)");
        
				if (playerIDList [0] == currentPlayerID && GlobalConstant.isRed) {
						GlobalConstant.isTouch = true;
				} else if (playerIDList [1] == currentPlayerID && GlobalConstant.isBlue) {
						GlobalConstant.isTouch = true;
				} else {
						GlobalConstant.isTouch = false;
				}
				yield return 0;
				StartCoroutine (assignControlToPlayer (startInfo));
		} */
    
		//
		public void assignTurnText ()
		{
				if (playerIDList [0] == currentPlayerID && GlobalConstant.isRed) {
						//GameManager.instance.turnName.text = "Your Turn";
                        GameManager.instance.playerTurn.SetActive(true);
                        GameManager.instance.ComputerTurn.SetActive(false);
				} else if (playerIDList [1] == currentPlayerID && GlobalConstant.isBlue) {
						//GameManager.instance.turnName.text = "Your Turn";
                        GameManager.instance.playerTurn.SetActive(false);
                        GameManager.instance.ComputerTurn.SetActive(true);
				} else {
						//GameManager.instance.turnName.text = "Opponent's Turn";
				}
		}

		/// <summary>
		/// Gets the instance.
		/// </summary>
		/// <returns>The instance.</returns>
		public static NextpeerManager GetInstance ()
		{
				if (instance == null) {
						instance = new NextpeerManager ();
				}
				return instance;
		}

		/// <summary>
		/// Launchs the dashbord.
		/// </summary>

		public void LaunchDashbord ()
		{
				Debug.Log ("==Before Dashboard Lauch==");
				Nextpeer.LaunchDashboard ();
				Debug.Log ("==Dashboard Lauched==");
		}

		/// <summary>
		/// Posts the score for controlled game.
		/// </summary>
		/// <param name="finalScore">Final score.</param>
		public void PostScore (uint finalScore)
		{
				// Game is over, our current player is dead. Notify Nextpeer if needed.
				if (Nextpeer.IsCurrentlyInTournament ())
						Nextpeer.ReportControlledTournamentOverWithScore (finalScore);
		}

		/// <summary>
		/// Exits the current tournament.
		/// </summary>
		public void ExitCurrentTournament ()
		{
				// User wishes to exit the current game
				if (Nextpeer.IsCurrentlyInTournament ()) {
						Nextpeer.ReportForfeitForCurrentTournament ();
				}
		}

		/// <summary>
		/// Reports score for current tournament.
		/// </summary>
		public void ReportScore (uint scoreToSubmit)
		{
				if (Nextpeer.IsCurrentlyInTournament ()) {
						Nextpeer.ReportScoreForCurrentTournament (scoreToSubmit);
				}
		}

		public void GeneratedStringToSend (string stringToSend)
		{
				SendReliableData (convertToByte (stringToSend));
		}
		/// <summary>
		/// Sends the reliable data.
		/// </summary>
		/// <param name="data">Data.</param>
		public void SendReliableData (byte[] data)
		{
				if (Nextpeer.IsCurrentlyInTournament ()) {
						
						Nextpeer.PushDataToOtherPlayers (data);
				}
		}

		/// <summary>
		/// Sends the un reliable data.
		/// </summary>
		/// <param name="data">Data.</param>
		public void SendUnReliableData (byte[] data)
		{
				if (Nextpeer.IsCurrentlyInTournament ()) {
						Nextpeer.UnreliablePushDataToOtherPlayers (data);
						//	NetworkPanel.instance.dataSend.text = convertToString (data);
				}
		}
    #endregion

    #region Private Functions
		/// <summary>
		/// Init nextpeer.
		/// </summary>
		public void Init ()
		{
				Debug.Log ("==Before Nextpeer Initialize==");
				NPGameSettings settings = new NPGameSettings ();
				Nextpeer.Init (thisGameKey, settings);
				Debug.Log ("==After Nextpeer Initialized==");
		}

    #endregion

    #region Nextpeer event handlers

		/// <summary>
		/// Called When Dashboards will appear.
		/// </summary>
		private void DashboardWillAppear ()
		{
				//	Debug.Log("Dashboard will appear..");
		}

		/// <summary>
		/// Called When Dashboards did appear.
		/// </summary>
		private void DashboardDidAppear ()
		{
				// It's important this code remain in DashboardDidAppear. Due to push notifications, it may be that Nextpeer
				// will be launched indirectly, and the game won't be able to call LaunchDashboard itself. So it must listen
				// to this event and do scene initialization here.
				//	Debug.Log("Dashboard did appear...");
		}

		/// <summary>
		/// Called When Dashboards will Disappear.
		/// </summary>
		private void DashboardWillDisappear ()
		{
				GlobalConstant.resetStaticVariable ();
				//Debug.Log("Dashboard wil disapear...");
				//Debug.Log("Called from Dashboard Disappear");

		}

		/// <summary>
		/// Called When Dashboards did Disappear.
		/// </summary>
		private void DashboardDidDisappear ()
		{
				//	Debug.Log("Dashboard did disapear...");
				// We must ensure that the player can go back to title if he 
				// goes out of tournament or wants to change controls or 
				// tournament kind
        
				if (!Nextpeer.IsCurrentlyInTournament ()) {
						//You can go back from here
		
						// COMMENTED
						//			UIManager.instance.hideLoadingPanel();
				}
		}

		/// <summary>
		/// Wills the tournament start with details.
		/// </summary>
		/// <param name="startInfo">Start info.</param>
		private void WillTournamentStartWithDetails (NPTournamentStartDataContainer startInfo)
		{
				//	UIManager.instance.selectionPanel.gameObject.SetActive (false);
				//	UIManager.instance.mainPanel.gameObject.SetActive (false);
				Debug.Log ("Will Start Tournamnet");
                
		}

		/// <summary>
		/// Dids the tournament start with details.
		/// </summary>
		/// <param name="startInfo">Start info.</param>
		private void DidTournamentStartWithDetails (NPTournamentStartDataContainer startInfo)
		{
				addPlayerToList (startInfo);
				playerId = startInfo.CurrentPlayer.PlayerId;
				for (int i = 0; i < startInfo.Opponents.Length; i++) {
						opponentsName.Add (startInfo.Opponents [i].PlayerName);
						opponentsId.Add (startInfo.Opponents [i].PlayerId.ToString ());
				}
				totalOppnents = startInfo.Opponents.Length;
				Debug.Log (totalOppnents);
				Debug.Log ("id count " + opponentsId.Count);
				Debug.Log ("name count " + opponentsName.Count);
				Debug.Log ("Player iD" + playerId);
                //Call to game start method
				// GameStart();
		}

		public void splitTheStringAndExecuteIt (string stringMessage)
		{
				string[] splitString = stringMessage.Split (',');
				if (splitString [0] == "blablabla") {
					// Do somthing when string is "blablabla"
				}
				else {
						//Do somthing when string is not "blablabla"
				}  
		}

    
		/// <summary>
		/// Dids the receive tournament custom message.
		/// </summary>
		/// <param name="Mess">Mess.</param>
		/// Main message Iyya AveisNotBreak
		private void DidReceiveTournamentCustomMessage (NPTournamentCustomMessageContainer Mess)
		{
				string receivedMsg = convertToString (Mess.Message);
				splitTheStringAndExecuteIt (receivedMsg);
				Debug.Log ("Receive Msg : = " + receivedMsg);
		}

		/// <summary>
		/// Dids the receive unreliable tournament custom message.
		/// </summary>
		/// <param name="msg">Message.</param>
		private void DidReceiveUnreliableTournamentCustomMessage (NPTournamentUnreliableCustomMessageContainer msg)
		{   
				string receivedMsg = convertToString (msg.Message);

				if (receivedMsg != "win") {
						//	InGamePanel.instance.OnGetData (receivedMsg);
				} else {
						//	PostScore((uint)GameManager.instance.totalScore);
				}
		}

		/// <summary>
		/// Handles the nextpeer did receive tournament status.
		/// </summary>
		/// <param name="status">Status.</param>
		private void HandleNextpeerDidReceiveTournamentStatus (NPTournamentStatusInfo status)
		{
				//	Debug.Log("HandleNextpeerDidReceiveTournamentStatus..." );
		}

		/// <summary>
		/// Dids the receive tournament results.
		/// </summary>
		/// <param name="endInfo">End info.</param>
		private void DidReceiveTournamentResults (NPTournamentEndDataContainer endInfo)
		{

		}

		/// <summary>
		/// Dids the tournament end.
		/// </summary>
		private void DidTournamentEnd ()
		{

		}

		// This method will be called once the event is fired by the server.
		// Make sure it is subscribed to Nextpeer.DidReceiveSynchronizedEvent!
		void DidReceiveSynchronizedEvent (string eventName, NPSynchronizedEventFireReason fireReason)
		{
				//	Debug.Log("Did Receive Sync Event Called..");
				if (START_GAME_SYNC_EVENT_NAME == eventName) {
						// TODO: Start the game!
						//	Debug.Log("**---Did Receive Synchronized Event BEFORE---**");
						//	Debug.Log("**---Did Receive Synchronized Event AFTER---**");
				}
		}

		public void registerSynchronizesEvent ()
		{
				Nextpeer.RegisterToSynchronizedEvent (START_GAME_SYNC_EVENT_NAME, START_GAME_SYNC_EVENT_TIMEOUT);
		}
	
		// This method will convert string to byte.
		public byte[] convertToByte (string stringDataToConvert)
		{
				System.Text.Encoding encoding = System.Text.Encoding.UTF8;  //or some other, but prefer some UTF is Unicode is used
				return encoding.GetBytes (stringDataToConvert);
		}

		// This method will convert byte to string.
		public string convertToString (byte[] bytesDataToConvert)
		{
				System.Text.Encoding encoding = System.Text.Encoding.UTF8;  //or some other, but prefer some UTF is Unicode is used
				return encoding.GetString (bytesDataToConvert);
		}

    #endregion
}
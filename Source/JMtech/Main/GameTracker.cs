using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Assets;
using Assets.Scripts.Addons.JMtech;
using Assets.Scripts.Addons.ShareScript;
using Assets.Scripts.Addons.ShareScript.Events;
using JMtech.JDIS;
using JMtech.Translations;
using ServerControlFramework;
using ServerControlSoftware.Externals.Shared.Packets;
using ServerTransferProgram.LogicControllers;
using ServerTransferProgram.ServerControlLiberary.DataControllers.DefaultEvents;
using UnityEngine;

namespace JMtech.Main
{
	public class GameTracker : MonoBehaviour
	{
		public static void tryStart()
		{
			if (GameTracker.Me == null)
			{
				GameTracker.Me = new GameObject();
				GameTracker.Tracker = GameTracker.Me.AddComponent<GameTracker>();
				GameTracker.Tracker.start();
				GameTracker.Me.SetActive(true);
				GameTracker.Tracker.SetContext();
				UnityEngine.Object.DontDestroyOnLoad(GameTracker.Me);
				UpdateableObject.Initiate();
				LoggerContext.setMainLogger(new LoggerInstance());
				new Checker();
				Translation.loadTranslations<SFST>();
			}
		}

		public void init()
		{
			Debug.Log("Authable JDIS Client has started and is ready for use!");
		}

		private void SetContext()
		{
			if (SyncContext.context == null)
			{
				SyncContext.context = SynchronizationContext.Current;
			}
		}

		private void Update()
		{
			UpdateableObject.InternalTick();
			this.SetContext();
			if (this.AuthClient != null && this.AuthClient.Initialized() && !this.InitatedPosted)
			{
				this.InitatedPosted = true;
				this.init();
			}
			this.TickSpeed = UpdateableObject.CoreTicksPerSecond;
			if (this.client != null && this.client.rec != null)
			{
				this.ClientSpeed = this.client.rec.TicksPerSecond;
			}
		}

		private void start()
		{
			base.enabled = true;
		}

		public void StartConnection(Action after)
		{
			this.doAfterFetch = after;
			MasterInfo masterInfo = MasterInfo.Fetch();
			if (masterInfo == null)
			{
				if (GameTracker.mgcache0 == null)
				{
					GameTracker.mgcache0 = new Action<string>(MsgController.ShowMsg);
				}
				SyncContext.RunOnUI<string>(GameTracker.mgcache0, SFST.T.STP_Request_Timeout);
				return;
			}
			this.client = new Client(masterInfo.REDIRECT, 100);
			this.client.rec.eventSystem.RegisterListener(new EventListener<ClientAuthedEvent>(delegate(ClientAuthedEvent e)
			{
				SyncContext.RunOnUI(after);
			}));
			this.client.rec.eventSystem.RegisterListener(new EventListener<InitialTimeoutEvent>(delegate(InitialTimeoutEvent e)
			{
				if (GameTracker.mgcache1 == null)
				{
					GameTracker.mgcache1 = new Action<string>(MsgController.ShowMsg);
				}
				SyncContext.RunOnUI<string>(GameTracker.mgcache1, SFST.T.STP_Request_Timeout);
			}));
			this.client.rec.eventSystem.RegisterListener(new EventListener<PacketReceivedEvent>(delegate(PacketReceivedEvent e)
			{
				if (e.receivedPacket.Is<ServerErrorPacket>())
				{
					ServerErrorPacket serverErrorPacket = e.receivedPacket.ToTargetPacket<ServerErrorPacket>();
					if (serverErrorPacket.Error == "AUTHENTICATION")
					{
						SyncContext.RunOnUI(delegate
						{
							MsgController.ShowMsg(SFST.T.STP_Old_Credentials);
							Sharing.sharing.downloadMenu.SetActive(false);
							this.client.rec.Delete();
							this.client.CleanCreds();
						});
						this.StartConnection(delegate
						{
							if (GameTracker.mgcache2 == null)
							{
								GameTracker.mgcache2 = new Action<string>(MsgController.ShowMsg);
							}
							SyncContext.RunOnUI<string>(GameTracker.mgcache2, SFST.T.STP_Reconnected);
						});
					}
				}
			}));
		}

		public void StartedConnection()
		{
			Debug.Log("Connected");
			this.ClientInitiated = true;
			SyncContext.RunOnUI(new Action(this.client.FetchCreds));
		}

		public long TickSpeed;

		public long ClientSpeed;

		public static GameObject Me;

		public static GameTracker Tracker;

		public JDISClient AuthClient;

		private bool InitatedPosted;

		public Client client;

		public bool ClientInitiated;

		public static SynchronizationContext Sync;

		private Action doAfterFetch;

		[CompilerGenerated]
		private static Action<string> mgcache0;

		[CompilerGenerated]
		private static Action<string> mgcache1;

		[CompilerGenerated]
		private static Action<string> mgcache2;
	}
}

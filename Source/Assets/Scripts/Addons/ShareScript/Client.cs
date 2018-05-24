using System;
using System.IO;
using Assets.Scripts.Addons.ShareScript.Events;
using JMtech.Main;
using ServerControlSoftware.Externals.Localized.MassData;
using ServerControlSoftware.Externals.Shared;
using ServerControlSoftware.Externals.Shared.Objects;
using ServerControlSoftware.Externals.Shared.Packets;
using ServerTransferProgram.LogicControllers;
using ServerTransferProgram.ServerControlLiberary;
using ServerTransferProgram.ServerControlLiberary.DataControllers.PacketSystem;
using UnityEngine;

namespace Assets.Scripts.Addons.ShareScript
{
	public class Client
	{
		public Client(string host, int port)
		{
			this.rec = new SCReceiver(host, port);
			this.rec.eventSystem.RegisterListener(new EventListener<ClientInitiatedEvent>(new Action<ClientInitiatedEvent>(this.ClientReady), null));
		}

		public void ClientReady(ClientInitiatedEvent e)
		{
			GameTracker.Tracker.StartedConnection();
		}

		public void AuthedRequest(AuthedPacket toSend, Action<DefaultPacket> response)
		{
			toSend.Credentials = this.myCredentials;
			this.rec.Request(toSend, response);
		}

		public void CleanCreds()
		{
			string path = Path.Combine(Application.persistentDataPath, "myCredentials");
			if (File.Exists(path))
			{
				File.Delete(path);
			}
		}

		public void FetchCreds()
		{
			string path = Path.Combine(Application.persistentDataPath, "myCredentials");
			if (!File.Exists(path))
			{
				this.rec.Request(new GenerateAuthPacket(), delegate(DefaultPacket r)
				{
					if (r.Success)
					{
						this.myCredentials = r.ToTargetPacket<AuthedPacket>().Credentials;
						new DefaultIOContext().StoreToFile(path, this.myCredentials);
						this.rec.eventSystem.Invoke<ClientAuthedEvent>(new ClientAuthedEvent());
					}
				});
			}
			else
			{
				new DefaultIOContext().FetchFromFile<AuthID>(path, out this.myCredentials);
				this.rec.eventSystem.Invoke<ClientAuthedEvent>(new ClientAuthedEvent());
			}
		}

		public SCReceiver rec;

		public AuthID myCredentials;
	}
}

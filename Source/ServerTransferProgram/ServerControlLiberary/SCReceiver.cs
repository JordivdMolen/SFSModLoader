using System;
using System.Net.Sockets;
using System.Threading;
using Assets.Scripts.Addons.ShareScript.Events;
using ServerControlFramework;
using ServerControlFramework.ServerControlLiberary.DataControllers.DefaultEvents;
using ServerControlFramework.ServerControlLiberary.DataControllers.PacketSystem.Packets;
using ServerTransferProgram.LogicControllers;
using ServerTransferProgram.ServerControlLiberary.DataControllers.DefaultEvents;
using ServerTransferProgram.ServerControlLiberary.DataControllers.PacketSystem;
using ServerTransferProgram.ServerControlLiberary.DataControllers.PacketSystem.Packets;
using UnityEngine;

namespace ServerTransferProgram.ServerControlLiberary
{
	[DelayedStart]
	public class SCReceiver : UpdateableObject
	{
		public SCReceiver(string host, int port)
		{
			this.eventSystem.RegisterListener(new EventListener<PacketReceivedEvent>(new Action<PacketReceivedEvent>(this.PacketReceived), null));
			this.eventSystem.RegisterListener(new EventListener<ConnectionReadyEvent>(new Action<ConnectionReadyEvent>(this.ConnectionReady), null));
			UpdateableObject uo = this;
			ThreadPool.QueueUserWorkItem(delegate(object s)
			{
				IAsyncResult asyncResult = this.client.BeginConnect(host, port, null, null);
				bool flag = asyncResult.AsyncWaitHandle.WaitOne(1000);
				if (flag)
				{
					this.controller = new PacketController(this);
					uo.SetActive(true);
					Debug.Log(string.Concat(new object[]
					{
						"Connected to ",
						host,
						":",
						port,
						", ",
						this.Active
					}));
					this.Connected = true;
				}
				else
				{
					this.eventSystem.Invoke<InitialTimeoutEvent>(new InitialTimeoutEvent());
				}
			});
		}

		public TcpClient MyConnection()
		{
			return this.client;
		}

		public bool isConnected()
		{
			return !this.Ready || this.controller.Connected();
		}

		private void PacketReceived(PacketReceivedEvent e)
		{
			if (e.receivedPacket.Is<PingPacket>())
			{
				this.controller.SendPacket(new PingPacket());
			}
		}

		private void Update()
		{
			if (this.controller != null && this.controller.IsReady() && !this.Ready)
			{
				this.Ready = true;
				this.eventSystem.Invoke<ConnectionReadyEvent>(new ConnectionReadyEvent());
			}
			else if (this.controller == null)
			{
				LoggerContext.getMainLogger().Generic("CONTROLLER", false);
			}
			foreach (DefaultPacket pack in this.controller.GetPackets())
			{
				this.eventSystem.Invoke<PacketReceivedEvent>(new PacketReceivedEvent(pack));
			}
			this.controller.SendPackets();
		}

		private void ConnectionReady(ConnectionReadyEvent e)
		{
			ThreadPool.QueueUserWorkItem(delegate(object s)
			{
				Debug.Log("connection ready");
				this.controller.SendPacket(new ClientReadyPacket());
				this.eventSystem.Invoke<ClientInitiatedEvent>(new ClientInitiatedEvent());
			});
		}

		public void Request(DefaultPacket toSend, Action<DefaultPacket> onResponse)
		{
			EventListener<PacketReceivedEvent> PRE = null;
			PRE = new EventListener<PacketReceivedEvent>(delegate(PacketReceivedEvent e)
			{
				if (e.receivedPacket.ResponseTo == toSend.PacketID)
				{
					onResponse(e.receivedPacket);
					this.eventSystem.RemoveListener(PRE);
				}
			});
			this.eventSystem.RegisterListener(PRE);
			this.controller.SendPacket(toSend);
		}

		protected void OnDelete()
		{
		}

		private TcpClient client = new TcpClient();

		private bool authed;

		private bool Ready;

		public bool Connected;

		public EventManager eventSystem = new EventManager();

		public PacketController controller;
	}
}

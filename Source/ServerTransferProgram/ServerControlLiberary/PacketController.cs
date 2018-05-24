using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using ServerControlFramework;
using ServerTransferProgram.ServerControlLiberary.DataControllers.PacketSystem;
using UnityEngine;

namespace ServerTransferProgram.ServerControlLiberary
{
	public class PacketController
	{
		private PacketController(NetworkStream stream)
		{
			this.baseStream = stream;
			this.DataWriter = new StreamWriter(this.baseStream);
			this.DataWriter.AutoFlush = false;
			this.DataReader = new StreamReader(this.baseStream);
			this.baseInStream = stream;
		}

		public PacketController(SCReceiver client) : this(client.MyConnection().GetStream())
		{
			this.target = client.MyConnection();
		}

		public void SendPacket(DefaultPacket packet)
		{
			this.toSend.Add(packet);
		}

		public void SendPackets()
		{
			if (this.target == null)
			{
				Debug.Log("Target == null");
				return;
			}
			if (this.DataWriter == null || !this.baseStream.CanWrite)
			{
				Debug.Log("(DataWriter == null || !baseStream.CanWrite)");
				return;
			}
			if (this.toSend.Count == 0)
			{
				return;
			}
			DefaultPacket[] array = this.toSend.ToArray();
			this.toSend.Clear();
			List<string> list = new List<string>();
			foreach (DefaultPacket defaultPacket in array)
			{
				string text = defaultPacket.ToSendable();
				text = text.Replace("<JSON_SPLIT_INDICATOR>", "#CAST_NOT_SUPPORTED");
				list.Add(text);
			}
			string value = string.Join("<JSON_SPLIT_INDICATOR>", list.ToArray());
			this.DataWriter.WriteLine(value);
			this.DataWriter.Flush();
		}

		public bool Connected()
		{
			return this.target != null && this.DataWriter != null && this.baseStream.CanWrite;
		}

		public bool IsReady()
		{
			return this.DataWriter != null && this.baseStream.CanWrite && this.DataReader != null && this.baseStream.CanRead;
		}

		public List<DefaultPacket> GetPackets()
		{
			List<DefaultPacket> list = new List<DefaultPacket>();
			if (this.DataReader == null || !this.DataReader.BaseStream.CanRead)
			{
				return list;
			}
			if (!this.baseStream.DataAvailable)
			{
				return list;
			}
			bool flag = false;
			string text = this.DataReader.ReadLine();
			while (text != null && !flag)
			{
				foreach (string text2 in Regex.Split(text, "<JSON_SPLIT_INDICATOR>"))
				{
					if (!(text2 == string.Empty))
					{
						DefaultPacket item = null;
						try
						{
							item = (DefaultPacket.FromSendable(text2) as DefaultPacket);
						}
						catch (Exception ex)
						{
							LoggerContext.getMainLogger().Generic("Could not parse: " + text2, false);
							goto IL_B7;
						}
						list.Add(item);
					}
					IL_B7:;
				}
				if (!this.baseStream.DataAvailable)
				{
					flag = true;
					break;
				}
				text = this.DataReader.ReadLine();
			}
			return list;
		}

		public TcpClient target;

		private StreamWriter DataWriter;

		private StreamReader DataReader;

		private Stream baseInStream;

		private NetworkStream baseStream;

		private List<DefaultPacket> toSend = new List<DefaultPacket>();
	}
}

using System;
using Newtonsoft.Json;

namespace ServerTransferProgram.ServerControlLiberary.DataControllers.PacketSystem
{
	public class DefaultPacket
	{
		public void SetError(string msg)
		{
			this.Success = false;
			this.Error = msg;
		}

		public string ToJSON()
		{
			return JsonConvert.SerializeObject(this);
		}

		public string ToSendable()
		{
			this.ContainedType = base.GetType().FullName;
			return this.ToJSON();
		}

		public static T FromSendable<T>(string json)
		{
			return JsonConvert.DeserializeObject<T>(json);
		}

		public static object FromSendable(string json)
		{
			json = json;
			DefaultPacket defaultPacket = JsonConvert.DeserializeObject<DefaultPacket>(json);
			Type type = Type.GetType(defaultPacket.ContainedType);
			if (type == null)
			{
				type = typeof(DefaultPacket);
			}
			return JsonConvert.DeserializeObject(json, type);
		}

		public Type GetContainedType()
		{
			return Type.GetType(this.ContainedType);
		}

		public object ToTargetPacket()
		{
			return Convert.ChangeType(this, this.GetContainedType());
		}

		public T ToTargetPacket<T>()
		{
			return (T)((object)this.ToTargetPacket());
		}

		public T GetEncapsulatedData<T>(byte[] data)
		{
			return Util.FromByteArray<T>(data);
		}

		public byte[] EncapsulateData(object data)
		{
			return Util.ToByteArray<object>(data);
		}

		public bool ContainsType<T>()
		{
			return this.GetContainedType() != null && this.GetContainedType() == typeof(T);
		}

		public bool ContainsType(Type t)
		{
			return this.GetContainedType() != null && this.GetContainedType() == t;
		}

		public bool Is<T>()
		{
			return this.Is(typeof(T));
		}

		public bool Is(Type t)
		{
			return this.IsSameOrSubclass(t, this.GetContainedType());
		}

		public bool IsSameOrSubclass(Type potentialBase, Type potentialDescendant)
		{
			return potentialDescendant.IsSubclassOf(potentialBase) || potentialDescendant == potentialBase;
		}

		[NonSerialized]
		public static Random packetRandomizer = new Random();

		[NonSerialized]
		public Action onSended;

		public string ContainedType;

		public int PacketID = DefaultPacket.packetRandomizer.Next(0, 9999999);

		public int ResponseTo = -1;

		public bool Success = true;

		public string Error = string.Empty;
	}
}

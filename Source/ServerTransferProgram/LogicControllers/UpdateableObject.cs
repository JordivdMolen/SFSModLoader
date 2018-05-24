using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;

namespace ServerTransferProgram.LogicControllers
{
	public class UpdateableObject
	{
		public UpdateableObject()
		{
			UpdateableObject.toUpdate.Add(this);
			this.watchDog.Start();
			this.loopHolder.Start();
			this.ID = UpdateableObject.LastID;
			UpdateableObject.LastID += 1L;
			MethodInfo method = base.GetType().GetMethod("Update", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (method != null)
			{
				this.aUpdate = method;
				Delayer[] array = (Delayer[])method.GetCustomAttributes(typeof(Delayer), true);
				if (array.Length != 0)
				{
					this.hold = array[0].holdMS;
				}
			}
			MethodInfo method2 = base.GetType().GetMethod("OnDelete", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (method2 != null)
			{
				this.aDelete = method2;
			}
			object[] customAttributes = base.GetType().GetCustomAttributes(typeof(DelayedStart), true);
			if (customAttributes.Length == 0)
			{
				this.SetActive(true);
			}
		}

		public static void StartUpdater()
		{
			if (UpdateableObject.mgcache0 == null)
			{
				UpdateableObject.mgcache0 = new WaitCallback(UpdateableObject.StartUpdaterAsync);
			}
			ThreadPool.QueueUserWorkItem(UpdateableObject.mgcache0);
		}

		private static void StartUpdaterAsync(object state)
		{
			UpdateableObject.Initiate();
			while (UpdateableObject.Running)
			{
				UpdateableObject.InternalTick();
			}
			UnityEngine.Debug.Log("Loop has ended");
		}

		public static void Initiate()
		{
			UpdateableObject.Running = true;
			UpdateableObject.tpsMeasurer.Reset();
			UpdateableObject.tpsMeasurer.Start();
		}

		public static void InternalTick()
		{
			try
			{
				UpdateableObject.timer.Reset();
				UpdateableObject.timer.Start();
				UpdateableObject[] array = UpdateableObject.toUpdate.ToArray();
				foreach (UpdateableObject updateableObject in array)
				{
					if (updateableObject == null)
					{
						UnityEngine.Debug.Log("How the fuck?");
					}
					try
					{
						updateableObject.InteralUpdate();
					}
					catch (Exception ex)
					{
						UnityEngine.Debug.Log(string.Concat(new object[]
						{
							"InternalUpdateException: ",
							ex.Message,
							ex.StackTrace,
							ex.InnerException
						}));
					}
				}
				UpdateableObject.timer.Stop();
				UpdateableObject.LastCylceDuration = UpdateableObject.timer.ElapsedMilliseconds;
				UpdateableObject.Ticks += 1L;
				if (UpdateableObject.tpsMeasurer.ElapsedMilliseconds >= 1000L)
				{
					UpdateableObject.CoreTicksPerSecond = UpdateableObject.Ticks;
					UpdateableObject.Ticks = 0L;
					UpdateableObject.tpsMeasurer.Reset();
					UpdateableObject.tpsMeasurer.Start();
				}
			}
			catch (Exception ex2)
			{
				UnityEngine.Debug.Log(string.Concat(new object[]
				{
					ex2.GetType().Name,
					"> ",
					ex2.Message,
					ex2.StackTrace,
					ex2.InnerException
				}));
			}
		}

		private void InteralUpdate()
		{
			try
			{
				if (this.Active && this.aUpdate != null && this.loopHolder.ElapsedMilliseconds >= (long)this.hold)
				{
					this.ticks += 1L;
					this.loopHolder.Reset();
					this.loopHolder.Start();
					this.aUpdate.Invoke(this, null);
				}
				if (this.watchDog.ElapsedMilliseconds >= 1000L)
				{
					this.TicksPerSecond = this.ticks;
					this.ticks = 0L;
					this.watchDog.Reset();
				}
			}
			catch (Exception ex)
			{
				UnityEngine.Debug.Log("WHOOPS, " + ex.Message + ex.StackTrace);
				UnityEngine.Debug.Log(ex.InnerException);
			}
		}

		public void SetActive(bool state)
		{
			this.Active = state;
		}

		public void Delete()
		{
			UpdateableObject.toUpdate.Remove(this);
			this.watchDog.Stop();
			if (this.aDelete != null)
			{
				this.aDelete.Invoke(this, null);
			}
		}

		private static List<UpdateableObject> toUpdate = new List<UpdateableObject>();

		public static bool Running = false;

		public static long LastCylceDuration = 0L;

		public static long CoreTicksPerSecond = 0L;

		private static long Ticks = 0L;

		private static Stopwatch tpsMeasurer = new Stopwatch();

		private static Stopwatch timer = new Stopwatch();

		private long ticks;

		private Stopwatch watchDog = new Stopwatch();

		public long TicksPerSecond;

		protected bool Active;

		protected long ID;

		private static long LastID = 0L;

		private MethodInfo aUpdate;

		private MethodInfo aDelete;

		private int hold;

		private Stopwatch loopHolder = new Stopwatch();

		[CompilerGenerated]
		private static WaitCallback mgcache0;
	}
}

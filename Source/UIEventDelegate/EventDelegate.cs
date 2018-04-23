using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace UIEventDelegate
{
	[Serializable]
	public class EventDelegate
	{
		public EventDelegate()
		{
		}

		public EventDelegate(Delegate call)
		{
			this.Set(call);
		}

		public EventDelegate(UnityEngine.Object target, string methodName)
		{
			this.Set(target, methodName);
		}

		public UnityEngine.Object target
		{
			get
			{
				return this.mTarget;
			}
			set
			{
				this.mTarget = value;
				this.mCachedCallback = null;
				this.mCachedFieldInfo = null;
				this.mCachedPropertyInfo = null;
				this.mRawDelegate = false;
				this.mCached = false;
				this.mMethod = null;
				this.mParameterInfos = null;
				this.mParameters = null;
			}
		}

		public string methodName
		{
			get
			{
				return this.mMethodName;
			}
			set
			{
				this.mMethodName = value;
				this.mCachedCallback = null;
				this.mCachedFieldInfo = null;
				this.mCachedPropertyInfo = null;
				this.mRawDelegate = false;
				this.mCached = false;
				this.mMethod = null;
				this.mParameterInfos = null;
				this.mParameters = null;
			}
		}

		public EventDelegate.Parameter[] parameters
		{
			get
			{
				bool flag = !this.mCached;
				if (flag)
				{
					this.Cache(true);
				}
				return this.mParameters;
			}
		}

		public bool isValid
		{
			get
			{
				bool flag = !this.mCached;
				if (flag)
				{
					this.Cache(true);
				}
				return (this.mRawDelegate && this.mCachedCallback != null) || this.mCachedFieldInfo != null || this.mCachedPropertyInfo != null || this.ExistMethod() || this.ExistField();
			}
		}

		public bool isEnabled
		{
			get
			{
				bool flag = !this.mCached;
				if (flag)
				{
					this.Cache(true);
				}
				bool flag2 = this.mRawDelegate && this.mCachedCallback != null;
				bool result;
				if (flag2)
				{
					result = true;
				}
				else
				{
					bool flag3 = this.mTarget == null;
					if (flag3)
					{
						result = false;
					}
					else
					{
						Behaviour behaviour = this.mTarget as Behaviour;
						result = (!behaviour || behaviour.enabled);
					}
				}
				return result;
			}
		}

		private static string GetMethodName(Delegate callback)
		{
			return callback.Method.Name;
		}

		private static bool IsValid(Delegate callback)
		{
			return callback != null && callback.Method != null;
		}

		public override bool Equals(object obj)
		{
			bool flag = obj == null;
			bool result;
			if (flag)
			{
				result = !this.isValid;
			}
			else
			{
				bool flag2 = obj is EventDelegate.Callback;
				if (flag2)
				{
					EventDelegate.Callback callback = obj as EventDelegate.Callback;
					bool flag3 = callback.Equals(this.mCachedCallback);
					if (flag3)
					{
						result = true;
					}
					else
					{
						UnityEngine.Object y = callback.Target as UnityEngine.Object;
						result = (this.mTarget == y && string.Equals(this.mMethodName, EventDelegate.GetMethodName(callback)));
					}
				}
				else
				{
					bool flag4 = obj is Delegate;
					if (flag4)
					{
						Delegate @delegate = obj as Delegate;
						bool flag5 = @delegate.Equals(this.mCachedCallback);
						if (flag5)
						{
							result = true;
						}
						else
						{
							UnityEngine.Object y2 = @delegate.Target as UnityEngine.Object;
							result = (this.mTarget == y2 && string.Equals(this.mMethodName, EventDelegate.GetMethodName(@delegate)));
						}
					}
					else
					{
						bool flag6 = obj is EventDelegate;
						if (flag6)
						{
							EventDelegate eventDelegate = obj as EventDelegate;
							result = (this.mTarget == eventDelegate.mTarget && string.Equals(this.mMethodName, eventDelegate.mMethodName));
						}
						else
						{
							result = false;
						}
					}
				}
			}
			return result;
		}

		public override int GetHashCode()
		{
			return EventDelegate.s_Hash;
		}

		private void Set(Delegate call)
		{
			this.Clear();
			bool flag = call != null && EventDelegate.IsValid(call);
			if (flag)
			{
				this.mTarget = (call.Target as UnityEngine.Object);
				bool flag2 = this.mTarget == null;
				if (flag2)
				{
					this.mRawDelegate = true;
					this.mCachedCallback = call;
					this.mMethodName = null;
				}
				else
				{
					this.mMethodName = EventDelegate.GetMethodName(call);
					this.mRawDelegate = false;
				}
			}
		}

		public void Set(UnityEngine.Object target, string methodName)
		{
			this.Clear();
			this.mTarget = target;
			this.mMethodName = methodName;
		}

		public bool ExistMethod()
		{
			bool flag = this.mTarget != null && !string.IsNullOrEmpty(this.mMethodName);
			bool result;
			if (flag)
			{
				Type type = this.mTarget.GetType();
				MethodInfo methodInfo = null;
				try
				{
					while (type != null)
					{
						methodInfo = type.GetMethod(this.mMethodName, EventDelegate.MethodFlags);
						bool flag2 = methodInfo != null;
						if (flag2)
						{
							break;
						}
						type = type.BaseType;
					}
				}
				catch (Exception)
				{
				}
				result = (methodInfo != null);
			}
			else
			{
				result = false;
			}
			return result;
		}

		public bool ExistField()
		{
			bool flag = this.mTarget != null && !string.IsNullOrEmpty(this.mMethodName);
			bool result;
			if (flag)
			{
				Type type = this.mTarget.GetType();
				FieldInfo fieldInfo = null;
				PropertyInfo propertyInfo = null;
				try
				{
					while (type != null)
					{
						fieldInfo = type.GetField(this.mMethodName, EventDelegate.FieldFlags);
						bool flag2 = fieldInfo != null;
						if (flag2)
						{
							break;
						}
						propertyInfo = type.GetProperty(this.mMethodName, EventDelegate.FieldFlags);
						bool flag3 = propertyInfo != null;
						if (flag3)
						{
							break;
						}
						type = type.BaseType;
					}
				}
				catch (Exception)
				{
				}
				result = (fieldInfo != null || propertyInfo != null);
			}
			else
			{
				result = false;
			}
			return result;
		}

		private void Cache(bool showError = true)
		{
			this.mCached = true;
			bool flag = this.mRawDelegate;
			if (!flag)
			{
				bool flag2 = (this.mCachedCallback == null || this.mCachedCallback.Target as UnityEngine.Object != this.mTarget || EventDelegate.GetMethodName(this.mCachedCallback) != this.mMethodName) && this.mTarget != null && !string.IsNullOrEmpty(this.mMethodName);
				if (flag2)
				{
					Type type = this.mTarget.GetType();
					this.mMethod = null;
					while (type != null)
					{
						try
						{
							this.mMethod = type.GetMethod(this.mMethodName, EventDelegate.MethodFlags);
							bool flag3 = this.mMethod != null;
							if (flag3)
							{
								break;
							}
						}
						catch (Exception ex)
						{
							Debug.LogError(ex.Message + " Inner exception: " + ex.InnerException);
						}
						type = type.BaseType;
					}
					bool flag4 = this.mMethod == null;
					if (flag4)
					{
						this.mArgs = null;
						this.mParameterInfos = null;
						this.mCachedCallback = null;
						FieldInfo fieldInfo = null;
						PropertyInfo propertyInfo = null;
						type = this.mTarget.GetType();
						try
						{
							while (type != null)
							{
								fieldInfo = type.GetField(this.mMethodName, EventDelegate.FieldFlags);
								bool flag5 = fieldInfo != null;
								if (flag5)
								{
									break;
								}
								propertyInfo = type.GetProperty(this.mMethodName, EventDelegate.FieldFlags);
								bool flag6 = propertyInfo != null;
								if (flag6)
								{
									break;
								}
								type = type.BaseType;
							}
						}
						catch (Exception)
						{
						}
						bool flag7 = fieldInfo != null;
						if (flag7)
						{
							this.mCachedFieldInfo = fieldInfo;
							bool flag8 = this.mParameters == null || this.mParameters.Length != 1;
							if (flag8)
							{
								this.mParameters = new EventDelegate.Parameter[1];
								this.mParameters[0] = new EventDelegate.Parameter();
							}
							this.mParameters[0].expectedType = fieldInfo.FieldType;
							this.mParameters[0].name = this.mMethodName;
						}
						else
						{
							bool flag9 = propertyInfo != null;
							if (flag9)
							{
								this.mCachedPropertyInfo = propertyInfo;
								bool flag10 = this.mParameters == null || this.mParameters.Length != 1;
								if (flag10)
								{
									this.mParameters = new EventDelegate.Parameter[1];
									this.mParameters[0] = new EventDelegate.Parameter();
								}
								this.mParameters[0].expectedType = propertyInfo.PropertyType;
								this.mParameters[0].name = this.mMethodName;
							}
							else if (showError)
							{
								Debug.LogError(string.Concat(new object[]
								{
									"Could not find method or field '",
									this.mMethodName,
									"' on ",
									this.mTarget.GetType()
								}), this.mTarget);
							}
						}
					}
					else
					{
						this.mParameterInfos = this.mMethod.GetParameters();
						bool flag11 = this.mParameterInfos.Length == 0;
						if (flag11)
						{
							bool flag12 = this.mMethod.ReturnType == typeof(bool);
							if (flag12)
							{
								this.mCachedCallback = Delegate.CreateDelegate(typeof(EventDelegate.BoolCallback), this.mTarget, this.mMethodName);
							}
							else
							{
								bool flag13 = this.mMethod.ReturnType.IsSubclassOf(typeof(Behaviour));
								if (flag13)
								{
									this.mCachedCallback = Delegate.CreateDelegate(typeof(EventDelegate.BehaviourCallback), this.mTarget, this.mMethodName);
								}
								else
								{
									bool flag14 = this.mMethod.ReturnType.IsSubclassOf(typeof(UnityEngine.Object));
									if (flag14)
									{
										this.mCachedCallback = Delegate.CreateDelegate(typeof(EventDelegate.UnityObjectCallback), this.mTarget, this.mMethodName);
									}
									else
									{
										bool flag15 = this.mMethod.ReturnType != typeof(void) && this.mMethod.ReturnType.IsSubclassOf(typeof(object));
										if (flag15)
										{
											this.mCachedCallback = Delegate.CreateDelegate(typeof(EventDelegate.SystemObjectCallback), this.mTarget, this.mMethodName);
										}
										else
										{
											bool flag16 = this.mMethod.ReturnType == typeof(void);
											if (flag16)
											{
												this.mCachedCallback = Delegate.CreateDelegate(typeof(EventDelegate.Callback), this.mTarget, this.mMethodName);
											}
											else
											{
												this.mCachedCallback = Delegate.CreateDelegate(this.mMethod.ReturnType, this.mTarget, this.mMethodName);
											}
										}
									}
								}
							}
							this.mArgs = null;
							this.mParameters = null;
						}
						else
						{
							this.mCachedCallback = null;
							bool flag17 = this.mParameters == null || this.mParameters.Length != this.mParameterInfos.Length;
							if (flag17)
							{
								this.mParameters = new EventDelegate.Parameter[this.mParameterInfos.Length];
								int i = 0;
								int num = this.mParameters.Length;
								while (i < num)
								{
									this.mParameters[i] = new EventDelegate.Parameter();
									i++;
								}
							}
							int j = 0;
							int num2 = this.mParameters.Length;
							while (j < num2)
							{
								this.mParameters[j].expectedType = this.mParameterInfos[j].ParameterType;
								this.mParameters[j].name = this.mParameterInfos[j].Name;
								j++;
							}
						}
					}
				}
			}
		}

		public bool Execute()
		{
			bool flag = !this.mCached || (this.mCachedFieldInfo == null && this.mCachedPropertyInfo == null && this.mMethod == null);
			if (flag)
			{
				this.Cache(true);
			}
			bool flag2 = this.mCachedFieldInfo != null;
			if (flag2)
			{
				bool flag3 = this.mParameters != null && this.mParameters[0] != null;
				if (flag3)
				{
					this.mParameters[0].value = null;
					this.mCachedFieldInfo.SetValue(this.mTarget, this.mParameters[0].value);
					return true;
				}
			}
			else
			{
				bool flag4 = this.mCachedPropertyInfo != null && this.mParameters != null && this.mParameters[0] != null;
				if (flag4)
				{
					this.mParameters[0].value = null;
					this.mCachedPropertyInfo.SetValue(this.mTarget, this.mParameters[0].value, null);
					return true;
				}
			}
			bool flag5 = this.mCachedCallback != null;
			bool result;
			if (flag5)
			{
				this.mCachedCallback.DynamicInvoke(null);
				result = true;
			}
			else
			{
				bool flag6 = this.mMethod != null;
				if (flag6)
				{
					bool flag7 = this.mParameters == null || this.mParameters.Length == 0;
					if (flag7)
					{
						try
						{
							this.mMethod.Invoke(this.mTarget, null);
						}
						catch (ArgumentException ex)
						{
							this.LogInvokeError(ex);
						}
					}
					else
					{
						bool flag8 = this.mArgs == null || this.mArgs.Length != this.mParameters.Length;
						if (flag8)
						{
							this.mArgs = new object[this.mParameters.Length];
						}
						int i = 0;
						int num = this.mParameters.Length;
						while (i < num)
						{
							EventDelegate.Parameter parameter = this.mParameters[i];
							bool flag9 = parameter != null;
							if (flag9)
							{
								bool flag10 = parameter.paramRefType == ParameterType.Reference || Application.isEditor;
								if (flag10)
								{
									parameter.value = null;
								}
								this.mArgs[i] = parameter.value;
							}
							i++;
						}
						try
						{
							this.mMethod.Invoke(this.mTarget, this.mArgs);
						}
						catch (ArgumentException ex2)
						{
							this.LogInvokeError(ex2);
						}
						int j = 0;
						int num2 = this.mArgs.Length;
						while (j < num2)
						{
							bool flag11 = this.mParameterInfos[j].IsIn || this.mParameterInfos[j].IsOut;
							if (flag11)
							{
								this.mParameters[j].value = this.mArgs[j];
							}
							this.mArgs[j] = null;
							j++;
						}
					}
					result = true;
				}
				else
				{
					result = false;
				}
			}
			return result;
		}

		private void LogInvokeError(ArgumentException ex)
		{
			string text = "Error calling ";
			bool flag = this.mTarget == null;
			if (flag)
			{
				text += this.mMethod.Name;
			}
			else
			{
				string text2 = text;
				text = string.Concat(new object[]
				{
					text2,
					this.mTarget.GetType(),
					".",
					this.mMethod.Name
				});
			}
			text = text + ": " + ex.Message;
			text += "\n  Expected: ";
			bool flag2 = this.mParameterInfos.Length == 0;
			if (flag2)
			{
				text += "no arguments";
			}
			else
			{
				text += this.mParameterInfos[0];
				for (int i = 1; i < this.mParameterInfos.Length; i++)
				{
					text = text + ", " + this.mParameterInfos[i].ParameterType;
				}
			}
			text += "\n  Received: ";
			bool flag3 = this.mParameters.Length == 0;
			if (flag3)
			{
				text += "no arguments";
			}
			else
			{
				text += this.mParameters[0].type;
				for (int j = 1; j < this.mParameters.Length; j++)
				{
					text = text + ", " + this.mParameters[j].type;
				}
			}
			text += "\n";
			Debug.LogError(text);
		}

		public void Clear()
		{
			this.mTarget = null;
			this.mMethodName = null;
			this.mRawDelegate = false;
			this.mCachedCallback = null;
			this.mCachedFieldInfo = null;
			this.mCachedPropertyInfo = null;
			this.mParameters = null;
			this.mCached = false;
			this.mMethod = null;
			this.mParameterInfos = null;
			this.mArgs = null;
		}

		public override string ToString()
		{
			bool flag = !(this.mTarget != null);
			string result;
			if (flag)
			{
				result = ((!this.mRawDelegate) ? null : "[delegate]");
			}
			else
			{
				string text = this.mTarget.GetType().ToString();
				int num = text.LastIndexOf('/');
				bool flag2 = num > 0;
				if (flag2)
				{
					text = text.Substring(num + 1);
				}
				bool flag3 = !string.IsNullOrEmpty(this.methodName);
				if (flag3)
				{
					result = text + "/" + this.methodName;
				}
				else
				{
					result = text + "/[delegate]";
				}
			}
			return result;
		}

		public static void Execute(List<EventDelegate> list)
		{
			bool flag = list != null;
			if (flag)
			{
				int count = list.Count;
				int i = 0;
				while (i < count)
				{
					EventDelegate eventDelegate = list[i];
					bool flag2 = eventDelegate != null;
					if (flag2)
					{
						try
						{
							eventDelegate.Execute();
						}
						catch (Exception ex)
						{
							bool flag3 = ex.InnerException != null;
							if (flag3)
							{
								Debug.LogError(ex.InnerException.Message);
							}
							else
							{
								Debug.LogError(ex.Message);
							}
						}
						bool flag4 = i >= list.Count;
						if (flag4)
						{
							break;
						}
						bool flag5 = list[i] != eventDelegate;
						if (!flag5)
						{
							bool flag6 = eventDelegate.oneShot;
							if (flag6)
							{
								list.RemoveAt(i);
							}
						}
					}
					IL_BC:
					i++;
					continue;
					goto IL_BC;
				}
			}
		}

		public static bool IsValid(List<EventDelegate> list)
		{
			bool flag = list != null;
			if (flag)
			{
				int i = 0;
				int count = list.Count;
				while (i < count)
				{
					EventDelegate eventDelegate = list[i];
					bool flag2 = eventDelegate != null && eventDelegate.isValid;
					if (flag2)
					{
						return true;
					}
					i++;
				}
			}
			return false;
		}

		public static EventDelegate Set(List<EventDelegate> list, EventDelegate.Callback callback)
		{
			bool flag = list != null;
			EventDelegate result;
			if (flag)
			{
				EventDelegate eventDelegate = new EventDelegate(callback);
				list.Clear();
				list.Add(eventDelegate);
				result = eventDelegate;
			}
			else
			{
				result = null;
			}
			return result;
		}

		public static void Set(List<EventDelegate> list, EventDelegate del)
		{
			bool flag = list != null;
			if (flag)
			{
				list.Clear();
				list.Add(del);
			}
		}

		public static EventDelegate Add(List<EventDelegate> list, Delegate callback)
		{
			return EventDelegate.Add(list, callback, false);
		}

		public static EventDelegate Add(List<EventDelegate> list, Delegate callback, bool oneShot)
		{
			bool flag = list != null;
			EventDelegate result;
			if (flag)
			{
				int i = 0;
				int count = list.Count;
				while (i < count)
				{
					EventDelegate eventDelegate = list[i];
					bool flag2 = eventDelegate != null && eventDelegate.Equals(callback);
					if (flag2)
					{
						return eventDelegate;
					}
					i++;
				}
				EventDelegate eventDelegate2 = new EventDelegate(callback);
				eventDelegate2.oneShot = oneShot;
				list.Add(eventDelegate2);
				result = eventDelegate2;
			}
			else
			{
				Debug.LogWarning("Attempting to add a callback to a list that's null");
				result = null;
			}
			return result;
		}

		public static void Add(List<EventDelegate> list, EventDelegate ev)
		{
			EventDelegate.Add(list, ev, ev.oneShot);
		}

		public static void Add(List<EventDelegate> list, EventDelegate ev, bool oneShot)
		{
			bool flag = ev.mRawDelegate || ev.target == null || string.IsNullOrEmpty(ev.methodName);
			if (flag)
			{
				EventDelegate.Add(list, ev.mCachedCallback, oneShot);
			}
			else
			{
				bool flag2 = list != null;
				if (flag2)
				{
					int i = 0;
					int count = list.Count;
					while (i < count)
					{
						EventDelegate eventDelegate = list[i];
						bool flag3 = eventDelegate != null && eventDelegate.Equals(ev);
						if (flag3)
						{
							return;
						}
						i++;
					}
					EventDelegate eventDelegate2 = new EventDelegate(ev.target, ev.methodName);
					eventDelegate2.oneShot = oneShot;
					bool flag4 = ev.mParameters != null && ev.mParameters.Length != 0;
					if (flag4)
					{
						eventDelegate2.mParameters = new EventDelegate.Parameter[ev.mParameters.Length];
						for (int j = 0; j < ev.mParameters.Length; j++)
						{
							eventDelegate2.mParameters[j] = ev.mParameters[j];
						}
					}
					list.Add(eventDelegate2);
				}
				else
				{
					Debug.LogWarning("Attempting to add a callback to a list that's null");
				}
			}
		}

		public static bool Remove(List<EventDelegate> list, EventDelegate.Callback callback)
		{
			bool flag = list != null;
			if (flag)
			{
				int i = 0;
				int count = list.Count;
				while (i < count)
				{
					EventDelegate eventDelegate = list[i];
					bool flag2 = eventDelegate != null && eventDelegate.Equals(callback);
					if (flag2)
					{
						list.RemoveAt(i);
						return true;
					}
					i++;
				}
			}
			return false;
		}

		public static bool Remove(List<EventDelegate> list, EventDelegate ev)
		{
			bool flag = list != null;
			if (flag)
			{
				int i = 0;
				int count = list.Count;
				while (i < count)
				{
					EventDelegate eventDelegate = list[i];
					bool flag2 = eventDelegate != null && eventDelegate.Equals(ev);
					if (flag2)
					{
						list.RemoveAt(i);
						return true;
					}
					i++;
				}
			}
			return false;
		}

		public static string GetFuncName(object obj, string method)
		{
			bool flag = obj == null;
			string result;
			if (flag)
			{
				result = "<null>";
			}
			else
			{
				string text = obj.GetType().ToString();
				int num = text.LastIndexOf('/');
				bool flag2 = num > 0;
				if (flag2)
				{
					text = text.Substring(num + 1);
				}
				result = ((!string.IsNullOrEmpty(method)) ? (text + "/" + method) : text);
			}
			return result;
		}

		static EventDelegate()
		{
			// Note: this type is marked as 'beforefieldinit'.
		}

		[SerializeField]
		public string mEventName = "Event";

		[HideInInspector]
		public bool mShowGroup = true;

		[HideInInspector]
		public bool mUpdateEntryList = true;

		[HideInInspector]
		public Entry[] mEntryList;

		[HideInInspector]
		public static BindingFlags MethodFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.OptionalParamBinding;

		[HideInInspector]
		public static BindingFlags FieldFlags = BindingFlags.Instance | BindingFlags.Public;

		[SerializeField]
		private UnityEngine.Object mTarget;

		[SerializeField]
		private string mMethodName;

		[SerializeField]
		private EventDelegate.Parameter[] mParameters;

		[SerializeField]
		private bool mCached;

		public bool oneShot;

		[NonSerialized]
		private Delegate mCachedCallback;

		[NonSerialized]
		private FieldInfo mCachedFieldInfo;

		[NonSerialized]
		private PropertyInfo mCachedPropertyInfo;

		[NonSerialized]
		private bool mRawDelegate;

		[NonSerialized]
		private MethodInfo mMethod;

		[NonSerialized]
		private ParameterInfo[] mParameterInfos;

		[NonSerialized]
		private object[] mArgs;

		private static int s_Hash = "EventDelegate".GetHashCode();

		[Serializable]
		public class Parameter
		{
			public Parameter()
			{
			}

			public Parameter(UnityEngine.Object obj, string field)
			{
				this.obj = obj;
				this.field = field;
			}

			public Parameter(object val)
			{
				this.mValue = val;
			}

			public object value
			{
				get
				{
					bool flag = this.mValue != null;
					object result;
					if (flag)
					{
						result = this.mValue;
					}
					else
					{
						bool flag2 = !this.cached;
						if (flag2)
						{
							this.cached = true;
							this.fieldInfo = null;
							this.propInfo = null;
							bool flag3 = this.paramRefType == ParameterType.Value;
							if (flag3)
							{
								bool flag4 = this.expectedType == typeof(string);
								if (flag4)
								{
									this.mValue = this.argStringValue;
									return this.argStringValue;
								}
								bool flag5 = this.expectedType == typeof(int);
								if (flag5)
								{
									this.mValue = this.argIntValue;
									return this.argIntValue;
								}
								bool flag6 = this.expectedType == typeof(float);
								if (flag6)
								{
									this.mValue = this.argFloatValue;
									return this.argFloatValue;
								}
								bool flag7 = this.expectedType == typeof(double);
								if (flag7)
								{
									this.mValue = this.argDoubleValue;
									return this.argDoubleValue;
								}
								bool flag8 = this.expectedType == typeof(bool);
								if (flag8)
								{
									this.mValue = this.argBoolValue;
									return this.argBoolValue;
								}
								bool flag9 = this.expectedType == typeof(Color);
								if (flag9)
								{
									this.mValue = this.argColor;
									return this.argColor;
								}
								bool flag10 = this.expectedType == typeof(Vector2);
								if (flag10)
								{
									this.mValue = this.argVector2;
									return this.argVector2;
								}
								bool flag11 = this.expectedType == typeof(Vector3);
								if (flag11)
								{
									this.mValue = this.argVector3;
									return this.argVector3;
								}
								bool flag12 = this.expectedType == typeof(Vector4);
								if (flag12)
								{
									this.mValue = this.argVector4;
									return this.argVector4;
								}
								bool isEnum = this.expectedType.IsEnum;
								if (isEnum)
								{
									this.mValue = (Enum)Enum.ToObject(this.expectedType, this.argIntValue);
									return this.mValue;
								}
							}
							bool flag13 = this.obj != null && !string.IsNullOrEmpty(this.field);
							if (flag13)
							{
								Type type = this.obj.GetType();
								this.propInfo = type.GetProperty(this.field);
								bool flag14 = this.propInfo == null;
								if (flag14)
								{
									this.fieldInfo = type.GetField(this.field);
								}
							}
						}
						bool flag15 = this.propInfo != null;
						if (flag15)
						{
							result = this.propInfo.GetValue(this.obj, null);
						}
						else
						{
							bool flag16 = this.fieldInfo != null;
							if (flag16)
							{
								result = this.fieldInfo.GetValue(this.obj);
							}
							else
							{
								bool flag17 = this.obj != null;
								if (flag17)
								{
									result = this.obj;
								}
								else
								{
									bool flag18 = this.expectedType != null && this.expectedType.IsValueType;
									if (flag18)
									{
										result = null;
									}
									else
									{
										result = Convert.ChangeType(null, this.expectedType);
									}
								}
							}
						}
					}
					return result;
				}
				set
				{
					this.mValue = value;
					bool flag = this.mValue == null;
					if (flag)
					{
						this.cached = false;
					}
				}
			}

			public Type type
			{
				get
				{
					bool flag = this.mValue != null;
					Type result;
					if (flag)
					{
						result = this.mValue.GetType();
					}
					else
					{
						bool flag2 = this.obj == null;
						if (flag2)
						{
							result = typeof(void);
						}
						else
						{
							result = this.obj.GetType();
						}
					}
					return result;
				}
			}

			public UnityEngine.Object obj;

			public string field;

			public ParameterType paramRefType;

			public string argStringValue;

			public int argIntValue;

			public float argFloatValue;

			public double argDoubleValue;

			public bool argBoolValue;

			public Color argColor;

			public Vector2 argVector2;

			public Vector3 argVector3;

			public Vector4 argVector4;

			private object mValue;

			[NonSerialized]
			public Type expectedType = typeof(void);

			[NonSerialized]
			public string name;

			[NonSerialized]
			public bool cached;

			[NonSerialized]
			public PropertyInfo propInfo;

			[NonSerialized]
			public FieldInfo fieldInfo;
		}

		public delegate bool BoolCallback();

		public delegate Behaviour BehaviourCallback();

		public delegate UnityEngine.Object UnityObjectCallback();

		public delegate object SystemObjectCallback();

		public delegate void Callback();
	}
}

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace UIEventDelegate
{
	[Serializable]
	public class EventDelegate
	{
		[Serializable]
		public class Parameter
		{
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

			public object value
			{
				get
				{
					if (this.mValue != null)
					{
						return this.mValue;
					}
					if (!this.cached)
					{
						this.cached = true;
						this.fieldInfo = null;
						this.propInfo = null;
						if (this.paramRefType == ParameterType.Value)
						{
							if (this.expectedType == typeof(string))
							{
								this.mValue = this.argStringValue;
								return this.argStringValue;
							}
							if (this.expectedType == typeof(int))
							{
								this.mValue = this.argIntValue;
								return this.argIntValue;
							}
							if (this.expectedType == typeof(float))
							{
								this.mValue = this.argFloatValue;
								return this.argFloatValue;
							}
							if (this.expectedType == typeof(double))
							{
								this.mValue = this.argDoubleValue;
								return this.argDoubleValue;
							}
							if (this.expectedType == typeof(bool))
							{
								this.mValue = this.argBoolValue;
								return this.argBoolValue;
							}
							if (this.expectedType == typeof(Color))
							{
								this.mValue = this.argColor;
								return this.argColor;
							}
							if (this.expectedType == typeof(Vector2))
							{
								this.mValue = this.argVector2;
								return this.argVector2;
							}
							if (this.expectedType == typeof(Vector3))
							{
								this.mValue = this.argVector3;
								return this.argVector3;
							}
							if (this.expectedType == typeof(Vector4))
							{
								this.mValue = this.argVector4;
								return this.argVector4;
							}
							if (this.expectedType.IsEnum)
							{
								this.mValue = (Enum)Enum.ToObject(this.expectedType, this.argIntValue);
								return this.mValue;
							}
						}
						if (this.obj != null && !string.IsNullOrEmpty(this.field))
						{
							Type type = this.obj.GetType();
							this.propInfo = type.GetProperty(this.field);
							if (this.propInfo == null)
							{
								this.fieldInfo = type.GetField(this.field);
							}
						}
					}
					if (this.propInfo != null)
					{
						return this.propInfo.GetValue(this.obj, null);
					}
					if (this.fieldInfo != null)
					{
						return this.fieldInfo.GetValue(this.obj);
					}
					if (this.obj != null)
					{
						return this.obj;
					}
					if (this.expectedType != null && this.expectedType.IsValueType)
					{
						return null;
					}
					return Convert.ChangeType(null, this.expectedType);
				}
				set
				{
					this.mValue = value;
					if (this.mValue == null)
					{
						this.cached = false;
					}
				}
			}

			public Type type
			{
				get
				{
					if (this.mValue != null)
					{
						return this.mValue.GetType();
					}
					if (this.obj == null)
					{
						return typeof(void);
					}
					return this.obj.GetType();
				}
			}

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
		}

		public delegate bool BoolCallback();

		public delegate Behaviour BehaviourCallback();

		public delegate UnityEngine.Object UnityObjectCallback();

		public delegate object SystemObjectCallback();

		public delegate void Callback();

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
				if (!this.mCached)
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
				if (!this.mCached)
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
				if (!this.mCached)
				{
					this.Cache(true);
				}
				if (this.mRawDelegate && this.mCachedCallback != null)
				{
					return true;
				}
				if (this.mTarget == null)
				{
					return false;
				}
				Behaviour behaviour = this.mTarget as Behaviour;
				return !behaviour || behaviour.enabled;
			}
		}

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
			if (obj == null)
			{
				return !this.isValid;
			}
			if (obj is EventDelegate.Callback)
			{
				EventDelegate.Callback callback = obj as EventDelegate.Callback;
				if (callback.Equals(this.mCachedCallback))
				{
					return true;
				}
				UnityEngine.Object y = callback.Target as UnityEngine.Object;
				return this.mTarget == y && string.Equals(this.mMethodName, EventDelegate.GetMethodName(callback));
			}
			else if (obj is Delegate)
			{
				Delegate @delegate = obj as Delegate;
				if (@delegate.Equals(this.mCachedCallback))
				{
					return true;
				}
				UnityEngine.Object y2 = @delegate.Target as UnityEngine.Object;
				return this.mTarget == y2 && string.Equals(this.mMethodName, EventDelegate.GetMethodName(@delegate));
			}
			else
			{
				if (obj is EventDelegate)
				{
					EventDelegate eventDelegate = obj as EventDelegate;
					return this.mTarget == eventDelegate.mTarget && string.Equals(this.mMethodName, eventDelegate.mMethodName);
				}
				return false;
			}
		}

		public override int GetHashCode()
		{
			return EventDelegate.s_Hash;
		}

		private void Set(Delegate call)
		{
			this.Clear();
			if (call != null && EventDelegate.IsValid(call))
			{
				this.mTarget = (call.Target as UnityEngine.Object);
				if (this.mTarget == null)
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
			if (this.mTarget != null && !string.IsNullOrEmpty(this.mMethodName))
			{
				Type type = this.mTarget.GetType();
				MethodInfo methodInfo = null;
				try
				{
					while (type != null)
					{
						methodInfo = type.GetMethod(this.mMethodName, EventDelegate.MethodFlags);
						if (methodInfo != null)
						{
							break;
						}
						type = type.BaseType;
					}
				}
				catch (Exception)
				{
				}
				return methodInfo != null;
			}
			return false;
		}

		public bool ExistField()
		{
			if (this.mTarget != null && !string.IsNullOrEmpty(this.mMethodName))
			{
				Type type = this.mTarget.GetType();
				FieldInfo fieldInfo = null;
				PropertyInfo propertyInfo = null;
				try
				{
					while (type != null)
					{
						fieldInfo = type.GetField(this.mMethodName, EventDelegate.FieldFlags);
						if (fieldInfo != null)
						{
							break;
						}
						propertyInfo = type.GetProperty(this.mMethodName, EventDelegate.FieldFlags);
						if (propertyInfo != null)
						{
							break;
						}
						type = type.BaseType;
					}
				}
				catch (Exception)
				{
				}
				return fieldInfo != null || propertyInfo != null;
			}
			return false;
		}

		private void Cache(bool showError = true)
		{
			this.mCached = true;
			if (this.mRawDelegate)
			{
				return;
			}
			if ((this.mCachedCallback == null || this.mCachedCallback.Target as UnityEngine.Object != this.mTarget || EventDelegate.GetMethodName(this.mCachedCallback) != this.mMethodName) && this.mTarget != null && !string.IsNullOrEmpty(this.mMethodName))
			{
				Type type = this.mTarget.GetType();
				this.mMethod = null;
				while (type != null)
				{
					try
					{
						this.mMethod = type.GetMethod(this.mMethodName, EventDelegate.MethodFlags);
						if (this.mMethod != null)
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
				if (this.mMethod == null)
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
							if (fieldInfo != null)
							{
								break;
							}
							propertyInfo = type.GetProperty(this.mMethodName, EventDelegate.FieldFlags);
							if (propertyInfo != null)
							{
								break;
							}
							type = type.BaseType;
						}
					}
					catch (Exception)
					{
					}
					if (fieldInfo != null)
					{
						this.mCachedFieldInfo = fieldInfo;
						if (this.mParameters == null || this.mParameters.Length != 1)
						{
							this.mParameters = new EventDelegate.Parameter[1];
							this.mParameters[0] = new EventDelegate.Parameter();
						}
						this.mParameters[0].expectedType = fieldInfo.FieldType;
						this.mParameters[0].name = this.mMethodName;
						return;
					}
					if (propertyInfo != null)
					{
						this.mCachedPropertyInfo = propertyInfo;
						if (this.mParameters == null || this.mParameters.Length != 1)
						{
							this.mParameters = new EventDelegate.Parameter[1];
							this.mParameters[0] = new EventDelegate.Parameter();
						}
						this.mParameters[0].expectedType = propertyInfo.PropertyType;
						this.mParameters[0].name = this.mMethodName;
						return;
					}
					if (showError)
					{
						Debug.LogError(string.Concat(new object[]
						{
							"Could not find method or field '",
							this.mMethodName,
							"' on ",
							this.mTarget.GetType()
						}), this.mTarget);
					}
					return;
				}
				else
				{
					this.mParameterInfos = this.mMethod.GetParameters();
					if (this.mParameterInfos.Length == 0)
					{
						if (this.mMethod.ReturnType == typeof(bool))
						{
							this.mCachedCallback = Delegate.CreateDelegate(typeof(EventDelegate.BoolCallback), this.mTarget, this.mMethodName);
						}
						else if (this.mMethod.ReturnType.IsSubclassOf(typeof(Behaviour)))
						{
							this.mCachedCallback = Delegate.CreateDelegate(typeof(EventDelegate.BehaviourCallback), this.mTarget, this.mMethodName);
						}
						else if (this.mMethod.ReturnType.IsSubclassOf(typeof(UnityEngine.Object)))
						{
							this.mCachedCallback = Delegate.CreateDelegate(typeof(EventDelegate.UnityObjectCallback), this.mTarget, this.mMethodName);
						}
						else if (this.mMethod.ReturnType != typeof(void) && this.mMethod.ReturnType.IsSubclassOf(typeof(object)))
						{
							this.mCachedCallback = Delegate.CreateDelegate(typeof(EventDelegate.SystemObjectCallback), this.mTarget, this.mMethodName);
						}
						else if (this.mMethod.ReturnType == typeof(void))
						{
							this.mCachedCallback = Delegate.CreateDelegate(typeof(EventDelegate.Callback), this.mTarget, this.mMethodName);
						}
						else
						{
							this.mCachedCallback = Delegate.CreateDelegate(this.mMethod.ReturnType, this.mTarget, this.mMethodName);
						}
						this.mArgs = null;
						this.mParameters = null;
						return;
					}
					this.mCachedCallback = null;
					if (this.mParameters == null || this.mParameters.Length != this.mParameterInfos.Length)
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

		public bool Execute()
		{
			if (!this.mCached || (this.mCachedFieldInfo == null && this.mCachedPropertyInfo == null && this.mMethod == null))
			{
				this.Cache(true);
			}
			if (this.mCachedFieldInfo != null)
			{
				if (this.mParameters != null && this.mParameters[0] != null)
				{
					this.mParameters[0].value = null;
					this.mCachedFieldInfo.SetValue(this.mTarget, this.mParameters[0].value);
					return true;
				}
			}
			else if (this.mCachedPropertyInfo != null && this.mParameters != null && this.mParameters[0] != null)
			{
				this.mParameters[0].value = null;
				this.mCachedPropertyInfo.SetValue(this.mTarget, this.mParameters[0].value, null);
				return true;
			}
			if (this.mCachedCallback != null)
			{
				this.mCachedCallback.DynamicInvoke(null);
				return true;
			}
			if (this.mMethod != null)
			{
				if (this.mParameters == null || this.mParameters.Length == 0)
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
					if (this.mArgs == null || this.mArgs.Length != this.mParameters.Length)
					{
						this.mArgs = new object[this.mParameters.Length];
					}
					int i = 0;
					int num = this.mParameters.Length;
					while (i < num)
					{
						EventDelegate.Parameter parameter = this.mParameters[i];
						if (parameter != null)
						{
							if (parameter.paramRefType == ParameterType.Reference || Application.isEditor)
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
						if (this.mParameterInfos[j].IsIn || this.mParameterInfos[j].IsOut)
						{
							this.mParameters[j].value = this.mArgs[j];
						}
						this.mArgs[j] = null;
						j++;
					}
				}
				return true;
			}
			return false;
		}

		private void LogInvokeError(ArgumentException ex)
		{
			string text = "Error calling ";
			if (this.mTarget == null)
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
			if (this.mParameterInfos.Length == 0)
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
			if (this.mParameters.Length == 0)
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
			if (!(this.mTarget != null))
			{
				return (!this.mRawDelegate) ? null : "[delegate]";
			}
			string text = this.mTarget.GetType().ToString();
			int num = text.LastIndexOf('/');
			if (num > 0)
			{
				text = text.Substring(num + 1);
			}
			if (!string.IsNullOrEmpty(this.methodName))
			{
				return text + "/" + this.methodName;
			}
			return text + "/[delegate]";
		}

		public static void Execute(List<EventDelegate> list)
		{
			if (list != null)
			{
				int count = list.Count;
				for (int i = 0; i < count; i++)
				{
					EventDelegate eventDelegate = list[i];
					if (eventDelegate != null)
					{
						try
						{
							eventDelegate.Execute();
						}
						catch (Exception ex)
						{
							if (ex.InnerException != null)
							{
								Debug.LogError(ex.InnerException.Message);
							}
							else
							{
								Debug.LogError(ex.Message);
							}
						}
						if (i >= list.Count)
						{
							break;
						}
						if (list[i] != eventDelegate)
						{
							continue;
						}
						if (eventDelegate.oneShot)
						{
							list.RemoveAt(i);
							continue;
						}
					}
				}
			}
		}

		public static bool IsValid(List<EventDelegate> list)
		{
			if (list != null)
			{
				int i = 0;
				int count = list.Count;
				while (i < count)
				{
					EventDelegate eventDelegate = list[i];
					if (eventDelegate != null && eventDelegate.isValid)
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
			if (list != null)
			{
				EventDelegate eventDelegate = new EventDelegate(callback);
				list.Clear();
				list.Add(eventDelegate);
				return eventDelegate;
			}
			return null;
		}

		public static void Set(List<EventDelegate> list, EventDelegate del)
		{
			if (list != null)
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
			if (list != null)
			{
				int i = 0;
				int count = list.Count;
				while (i < count)
				{
					EventDelegate eventDelegate = list[i];
					if (eventDelegate != null && eventDelegate.Equals(callback))
					{
						return eventDelegate;
					}
					i++;
				}
				EventDelegate eventDelegate2 = new EventDelegate(callback);
				eventDelegate2.oneShot = oneShot;
				list.Add(eventDelegate2);
				return eventDelegate2;
			}
			Debug.LogWarning("Attempting to add a callback to a list that's null");
			return null;
		}

		public static void Add(List<EventDelegate> list, EventDelegate ev)
		{
			EventDelegate.Add(list, ev, ev.oneShot);
		}

		public static void Add(List<EventDelegate> list, EventDelegate ev, bool oneShot)
		{
			if (ev.mRawDelegate || ev.target == null || string.IsNullOrEmpty(ev.methodName))
			{
				EventDelegate.Add(list, ev.mCachedCallback, oneShot);
			}
			else if (list != null)
			{
				int i = 0;
				int count = list.Count;
				while (i < count)
				{
					EventDelegate eventDelegate = list[i];
					if (eventDelegate != null && eventDelegate.Equals(ev))
					{
						return;
					}
					i++;
				}
				EventDelegate eventDelegate2 = new EventDelegate(ev.target, ev.methodName);
				eventDelegate2.oneShot = oneShot;
				if (ev.mParameters != null && ev.mParameters.Length > 0)
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

		public static bool Remove(List<EventDelegate> list, EventDelegate.Callback callback)
		{
			if (list != null)
			{
				int i = 0;
				int count = list.Count;
				while (i < count)
				{
					EventDelegate eventDelegate = list[i];
					if (eventDelegate != null && eventDelegate.Equals(callback))
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
			if (list != null)
			{
				int i = 0;
				int count = list.Count;
				while (i < count)
				{
					EventDelegate eventDelegate = list[i];
					if (eventDelegate != null && eventDelegate.Equals(ev))
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
			if (obj == null)
			{
				return "<null>";
			}
			string text = obj.GetType().ToString();
			int num = text.LastIndexOf('/');
			if (num > 0)
			{
				text = text.Substring(num + 1);
			}
			return (!string.IsNullOrEmpty(method)) ? (text + "/" + method) : text;
		}
	}
}

using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace NewBuildSystem
{
    [Serializable]
    public class Orientation
    {
        public Orientation(int flipedX, int flipedY, int rotation)
        {
            this.x = ((flipedX != 0) ? flipedX : 1);
            this.y = ((flipedY != 0) ? flipedY : 1);
            this.z = rotation;
        }

        public Orientation DeepCopy()
        {
            return new Orientation(this.x, this.y, this.z);
        }

        public void Rotate90()
        {
            this.z = (this.z - 90) % 360;
        }

        public void FlipX()
        {
            if (this.z % 180 == 0)
            {
                this.x = ((this.x != 1) ? 1 : -1);
            }
            else
            {
                this.y = ((this.y != 1) ? 1 : -1);
            }
        }

        public void FlipY()
        {
            if (this.z % 180 == 0)
            {
                this.y = ((this.y != 1) ? 1 : -1);
            }
            else
            {
                this.x = ((this.x != 1) ? 1 : -1);
            }
        }

        public bool InversedAxis()
        {
            return this.z % 180 != 0;
        }

        public static float operator *(float radianAngle, Orientation b)
        {
            if (b.y == -1)
            {
                radianAngle = -radianAngle;
            }
            if (b.x == -1)
            {
                radianAngle = -(radianAngle - 1.57079637f) + 1.57079637f;
            }
            return radianAngle + (float)b.z * 0.0174532924f;
        }

        public static Vector2 operator *(Vector2 a, Orientation b)
        {
            return Quaternion.Euler(0f, 0f, (float)b.z) * new Vector2(a.x * (float)b.x, a.y * (float)b.y);
        }

        public static Vector3 operator *(Vector3 a, Orientation b)
        {
            return Quaternion.Euler(0f, 0f, (float)b.z) * new Vector3(a.x * (float)b.x, a.y * (float)b.y, 0f);
        }

        public static Orientation operator -(Orientation a, Orientation b)
        {
            return new Orientation((a.x != b.x) ? -1 : 1, (a.y != b.y) ? -1 : 1, a.z - b.z);
        }

        public static Orientation operator +(Orientation a, Orientation b)
        {
            return new Orientation((a.x != 1) ? (-b.x) : b.x, (a.y != 1) ? (-b.y) : b.y, a.z + b.z);
        }

        public static void ApplyOrientation(Transform a, Orientation b)
        {
            a.localScale = new Vector3((float)b.x, (float)b.y, 1f);
            a.localEulerAngles = new Vector3(0f, 0f, (float)b.z);
        }

        [HorizontalGroup(0f, 0, 0, 0)]
        [HideLabel]
        public int x = 1;

        [HorizontalGroup(0f, 0, 0, 0)]
        [HideLabel]
        public int y = 1;

        [HorizontalGroup(0f, 0, 0, 0)]
        [HideLabel]
        public int z;
    }
}

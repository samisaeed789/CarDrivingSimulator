using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace ITS.Utils
{
    public static class Random
    {
        private static System.Random Randomly => _random ?? (_random = new System.Random());

        private static System.Random _random;
        public static float Range(float from, float to)
        {
            var newRandom = (float)Randomly.NextDouble();
            return Mathf.Lerp(from, to, newRandom);
        }
        
        public static int Range(int from, int to)
        {
            return Randomly.Next(from, to);
        }
    }
    
    public static class QuaternionExt
    {
        public static Quaternion GetNormalized(this Quaternion q)
        {
            float f = 1f / Mathf.Sqrt(q.x * q.x + q.y * q.y + q.z * q.z + q.w * q.w);
            return new Quaternion(q.x*f, q.y*f, q.z*f, q.w*f);
        }
    }
    
    public static class WaypointsUtils
    {
        public static int GetNearestWayppoint(TSPoints[] waypoint, Vector3 point)
        {
            return GetNearestWayppoint(waypoint, point, float.MaxValue);
        }

        public static int GetNearestWayppoint(TSPoints[] waypoint, Vector3 point, float minsidedist)
        {
            int o = 0;
            int i = 0;
            bool entered = false;
            foreach (TSPoints waypointEval in waypoint)
            {
                float fdist =
                    Vector3.Distance(waypointEval.point,
                        point); // waypointEval.InverseTransformPoint(myTransform.position).magnitude;
                if (fdist < minsidedist)
                {
                    entered = true;
                    minsidedist = fdist;
                    o = i;
                }

                i++;
            }

            if (o == 0 && !entered) o = -1;
            return o;
        }
    }

    public static class TSUtils
    {
        private static int CurrentID = 0;
        public static int GetUniqueID()
        {
            ++CurrentID;
            return CurrentID;
        }
        
        public static Type[] GetTypes<T>(bool ignoreBaseClass = false)
        {
            if (ignoreBaseClass)
            {
                return (from domainAssembly in AppDomain.CurrentDomain.GetAssemblies()
                    from assemblyType in domainAssembly.GetTypes()
                    where (typeof (T).IsAssignableFrom(assemblyType) || assemblyType.IsSubclassOf(typeof(T))) && assemblyType!= typeof(T)
                    select assemblyType).ToArray();
            }

            return (from domainAssembly in AppDomain.CurrentDomain.GetAssemblies()
                from assemblyType in domainAssembly.GetTypes()
                where typeof (T).IsAssignableFrom(assemblyType) || assemblyType.IsSubclassOf(typeof(T)) || assemblyType == typeof(T)
                select assemblyType).ToArray();
        }

        public static float MPSToKph(this float metersPerSecond)
        {
            return metersPerSecond * MPSToKHP;
        }
        
        public static float KphToMPS(this float Kph)
        {
            return Kph * KPHToMPS;
        }
        /// <summary>
        /// Meters per second to Kilometers per hour (multiply mps by this to obtain KPH)
        /// </summary>
        public const float MPSToKHP = 3.6f;
        public const float KPHToMPS = 0.277777778f;
        public static float Sq(this float f)
        {
            return f * f;
        }
        
        public static float Sqrt(this float f)
        {
            return Mathf.Sqrt(f);
        }

        public static float CalculateDriftAngle(Vector3 velocity)
        {
            return ((Mathf.Atan2(velocity.x, velocity.z) * Mathf.Rad2Deg));
        }

        public static Bounds GetBoundsFromColliders(this Transform myTransform)
        {
            var tempRotation = myTransform.rotation;
            var tempPosition = myTransform.position;
            myTransform.rotation = Quaternion.Euler(Vector3.zero);
            myTransform.position = Vector3.zero;
            var bounds = new Bounds(myTransform.position, Vector3.zero);
            var renderers = myTransform.gameObject.GetComponentsInChildren<Collider>();
            foreach (var renderer in renderers)
            {
                if (!renderer.isTrigger)
                    bounds.Encapsulate(renderer.bounds);
            }

            myTransform.rotation = tempRotation;
            myTransform.position = tempPosition;
            return bounds;
        }

        public static Bounds GetBoundsFromTransforms(Transform[] transforms)
        {
            if (transforms.Length == 0)
            {
                return new Bounds();
            }

            var bounds = new Bounds(transforms[0].position, Vector3.zero);
            foreach (var wheel in transforms)
            {
                bounds.Encapsulate(wheel.position);
            }

            return bounds;
        }

        public static void CreatePoints<T>(float numSegments, Vector3[] pts, ref T[] points, ref float totalDistance) where T : TSPoints, new()
        {
            int countPT = 0;
            totalDistance = 0;
            AddPoint(ref points, ref totalDistance, pts[0]);
            for (int i = 0; i < pts.Length - 3; i++)
            {
                float tcurr = 0;

                float t= 0.01f;
                
                if (i == 0)
                {
                    t = (numSegments / Vector3.Distance(pts[i], pts[i + 3]));
                }
                else if (i == pts.Length - 4)
                {
                    t = (numSegments / Vector3.Distance(pts[i + 1], pts[i + 2]));
                }
                else
                {
                    t = (numSegments / Vector3.Distance(pts[i + 1], pts[i + 2]));
                }
                
                while (tcurr <= 1)
                {
                    if (1 - tcurr < t / 2f) break;
                    
                    var point = InterpolateEvenly(tcurr, pts, i);
                    
                    AddPoint(ref points,ref totalDistance, point);
                    tcurr += t;
                    countPT++;
                }
            }

            AddPoint(ref points, ref totalDistance, pts[pts.Length - 1]);
            var distance = totalDistance / points.Length;
            
        }

        private static void AddPoint<T>(ref T[] points, ref float totalDistance, Vector3 point)where T : TSPoints, new()
        {
            T pointLast = new T {point = point};
            RaycastHit hit1;
            if (Physics.Raycast(pointLast.point + Vector3.up * 5, -Vector3.up, out hit1))
            {
                pointLast.point = hit1.point;
            }
            
            if (points.Length - 1 >= 0)
            {
                pointLast.distanceToNextPoint = (points[points.Length - 1].point - pointLast.point).magnitude;
            }

            totalDistance += pointLast.distanceToNextPoint;
            points = points.Add(pointLast);
        }

        public static Vector3 InterpolateEvenly(float t, Vector3[] pts, int currPt)
        {
            var u = t;
            var a = pts[currPt];
            var b = pts[currPt + 1];
            var c = pts[currPt + 2];
            var d = pts[currPt + 3];
            return .5f * ((-a + 3f * b - 3f * c + d) * (u * u * u) + (2f * a - 5f * b + 4f * c - d) * (u * u) +
                          (-a + c) * u + 2f * b);
        }

        public static Vector3 Interpolate(float t, Vector3[] pts)
        {
            var numSections = pts.Length - 3;
            var currPt = Mathf.Min(Mathf.FloorToInt(t * (float) numSections), numSections - 1);
            var u = t * (float) numSections - (float) currPt;
            var a = pts[currPt];
            var b = pts[currPt + 1];
            var c = pts[currPt + 2];
            var d = pts[currPt + 3];
            return .5f * ((-a + 3f * b - 3f * c + d) * (u * u * u) + (2f * a - 5f * b + 4f * c - d) * (u * u) +
                          (-a + c) * u + 2f * b);
        }

        public static Vector3 Cubic_Interpolate(Vector3 y0, Vector3 y1, Vector3 y2, Vector3 y3, float mu)
        {
            Vector3 a0, a1, a2, a3;
            float mu2;
            mu2 = mu * mu;
            a0 = y3 - y2 - y0 + y1; //p
            a1 = y0 - y1 - a0;
            a2 = y2 - y0;
            a3 = y1;
            return (a0 * mu * mu2 + a1 * mu2 + a2 * mu + a3);
        }

        public static bool TypeExists(string typeName)
        {
            var type = (from assembly in AppDomain.CurrentDomain.GetAssemblies()
                from type1 in assembly.GetTypes()
                where type1.Name == typeName
                select type1).FirstOrDefault();
            return type != null;
        }
        
        public static bool MethodExists(string typeName, string methodName)
        {
            var type = (from assembly in AppDomain.CurrentDomain.GetAssemblies()
                from type1 in assembly.GetTypes()
                where type1.Name == typeName
                select type1).FirstOrDefault();
            if (type == null)
            { return false;}

            var methodInfo = type.GetMethod(methodName);
            return methodInfo != null;
        }
    }

    public static class Extensions
    {
        public static string RemoveSpecialCharactersAndPunctuation(this string str)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in str)
            {
                if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || c == '_')
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }
        //=========================================================================
        // Removes all instances of [itemToRemove] from array [original]
        // Returns the new array, without modifying [original] directly
        // .Net2.0-compatible
        public static T[] Remove<T>(this T[] original, T itemToRemove)
        {
            int numIdx = System.Array.IndexOf(original, itemToRemove);
            if (numIdx == -1) return original;
            List<T> tmp = new List<T>(original);
            tmp.RemoveAt(numIdx);
            return tmp.ToArray();
        }

        public static T[] Add<T>(this T[] original, T itemToAdd)
        {
            if (original == null) original = new T[0];
            System.Array.Resize(ref original, original.Length + 1);
            original[original.Length - 1] = itemToAdd;
            return original;
        }

        public static int FindIndex<T>(this T[] original, T itemToFind)
        {
            if (original == null) original = new T[0];
            for (int i = 0; i < original.Length; i++)
            {
                if (original[i].Equals(itemToFind))
                {
                    return i;
                }
            }

            return -1;
        }

        public static TSLaneInfo FindNearestLastPoint(this TSLaneInfo[] original, Vector3 point)
        {
            var distance = float.MaxValue;
            var index = 0;
            for (int i = 0; i < original.Length; i++)
            {
                var compareDistance = Vector3.Distance(original[i].points.Last().point, point);
                if (compareDistance < distance)
                {
                    distance = compareDistance;
                    index = i;
                }
            }

            return original[index];
        }

        public static TSLaneInfo FindNearestFirstPoint(this TSLaneInfo[] original, Vector3 point)
        {
            var distance = float.MaxValue;
            var index = 0;
            for (int i = 0; i < original.Length; i++)
            {
                var compareDistance = Vector3.Distance(original[i].points.First().point, point);
                if (compareDistance < distance)
                {
                    distance = compareDistance;
                    index = i;
                }
            }

            return original[index];
        }
        
        public static List<TSLaneInfo> FindAllNearestFirstPoint(this TSLaneInfo[] original, Vector3 point, float maxDistance)
        {
            var results = new List<TSLaneInfo>();
            var distance = maxDistance;
            for (int i = 0; i < original.Length; i++)
            {
                var compareDistance = Vector3.Distance(original[i].points.First().point, point);
                if (compareDistance <= distance)
                {
                    results.Add(original[i]);
                }
            }

            return results;
        }
        
        public static List<int> FindAllIndexNearestFirstPoint(this TSLaneInfo[] original, Vector3 point, float maxDistance)
        {
            var results = new List<int>();
            var distance = maxDistance;
            for (int i = 0; i < original.Length; i++)
            {
                var compareDistance = Vector3.Distance(original[i].points.First().point, point);
                if (compareDistance <= distance)
                {
                    results.Add(i);
                }
            }

            return results;
        }

        public static T Find<T>(this T[] original, Predicate<T> match)
        {
            if (match == null)
                return default(T);
            for (var index = 0; index < original.Length; ++index)
            {
                if (match(original[index]))
                    return original[index];
            }

            return default(T);
        }
        
        public static bool Exist<T>(this T[] original, Predicate<T> match)
        {
            if (match == null)
                return false;
            for (var index = 0; index < original.Length; ++index)
            {
                if (match(original[index]))
                    return true;
            }

            return false;
        }

        public static void Move<T>(this List<T> list, int oldIndex, int newIndex)
        {
            T aux = list[newIndex];
            list[newIndex] = list[oldIndex];
            list[oldIndex] = aux;
        }

        #region EnumExtensions

        public static bool Has<T>(this System.Enum type, T value)
        {
            try
            {
                return (((((int) (object) type) & (1 << (int) (object) value))) > 0); //(int)(object)value);
            }
            catch
            {
                return false;
            }
        }

        public static bool Is<T>(this System.Enum type, T value)
        {
            try
            {
                return (int) (object) type == (int) (object) value;
            }
            catch
            {
                return false;
            }
        }


        public static T Add<T>(this System.Enum type, T value)
        {
            try
            {
                return (T) (object) ((((int) (object) type) | (1 << (int) (object) value)));
            }
            catch (System.Exception ex)
            {
                throw new System.ArgumentException(
                    string.Format(
                        "Could not append value from enumerated type '{0}'.",
                        typeof(T).Name
                    ), ex);
            }
        }


        public static T Remove<T>(this System.Enum type, T value)
        {
            try
            {
                return (T) (object) ((((int) (object) type) & (~(1 << (int) (object) value))));
            }
            catch (System.Exception ex)
            {
                throw new System.ArgumentException(
                    string.Format(
                        "Could not remove value from enumerated type '{0}'.",
                        typeof(T).Name
                    ), ex);
            }
        }

        #endregion
        
    }
}
using System;
using UnityEngine;
 
[AttributeUsage(System.AttributeTargets.Method)]
public class EditorButtonAttribute : PropertyAttribute { }
[AttributeUsage(System.AttributeTargets.Field)]
public class ProgressbarAttribute : PropertyAttribute { }
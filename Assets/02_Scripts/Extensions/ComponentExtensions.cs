using System;
using System.Collections.Generic;

namespace UnityEngine
{
	public static class ComponentExtensions
	{
		#region Management
		public static T AddComponent<T>(this Component component) where T : Component
		{
			return component.gameObject.AddComponent<T>();
		}
		
		public static T GetComponent<T>(this Component component, bool isAdded = false) where T : Component
		{
			if (component.TryGetComponent(out T t))
				return t;

			return isAdded ? component.gameObject.AddComponent<T>() : null;
		}

		public static T GetComponent<T>(this Component component, string path) where T : Component
		{
			var isNull = string.IsNullOrEmpty(path);
			if (isNull)
				return component.GetComponent<T>();

			var transform = component.transform.Find(path);
			if (transform != null)
				return transform.GetComponent<T>();

			return null;
		}

		public static T[] GetComponents<T>(this Component component, string path) where T : Component
		{
			var isNull = string.IsNullOrEmpty(path);
			if (isNull)
				return component.GetComponents<T>();

			var transform = component.transform.Find(path);
			if (transform != null)
				return transform.GetComponents<T>();

			return null;
		}

		public static T GetComponentInChildren<T>(this Component component, string path, bool includeInactive = true) where T : Component
		{
			var isNull = string.IsNullOrEmpty(path);
			if (isNull)
				return component.GetComponentInChildren<T>(includeInactive);

			var transform = component.transform.Find(path);
			if (transform != null)
				return transform.GetComponentInChildren<T>(includeInactive);

			return null;
		}

		public static T[] GetComponentsInChildren<T>(this Component component, string path, bool includeInactive = true) where T : Component
		{
			var isNull = string.IsNullOrEmpty(path);
			if (isNull)
				return component.GetComponentsInChildren<T>(includeInactive);

			var transform = component.transform.Find(path);
			if (transform != null)
				return transform.GetComponentsInChildren<T>(includeInactive);

			return null;
		}

		public static void RemoveComponent<T>(this Component component) where T : Component
		{
			var target = component.GetComponent<T>();
			if (target != null)
				Object.Destroy(target);
		}
		
		public static void Clear(this Component component)
		{
			component.transform.Clear();
		}
		#endregion

		#region Create Object
		public static T CreateInChild<T>(this Component component, string name) where T : Component
		{
			return component.CreateInChild<T>(name, Vector3.zero, Vector3.one, Quaternion.identity);
		}

		public static T CreateInChild<T>(this Component component, string name, Vector3 position, Vector3 scale, Quaternion quaternion) where T : Component
		{
			var go = new GameObject(name);
			
			go.transform.SetParent(component.transform);
			go.transform.localPosition = position;
			go.transform.localRotation = quaternion;
			go.transform.localScale = scale;
			
			return go.AddComponent<T>();
		}
		#endregion
		
		#region Active
		public static bool IsActive(this Component component)
		{
			return component.gameObject.activeSelf;
		}

		public static void SetActive(this Component component, bool value)
		{
			component.gameObject.SetActive(value);
		}
		
		public static void SetActives(this ICollection<Component> collection, bool value)
		{
			foreach (var component in collection)
			{
				if (component != null)
					component.SetActive(value);
			}
		}
		#endregion

		#region Position
		public static void SetPosition(this Component component, Component target)
		{
			component.transform.position = target.transform.position;
		}
		
		public static void SetPosition(this Component component, Vector3 position)
		{
			component.transform.position = position;
		}
		
		public static Vector3 GetPositionX(this Component component, float x = 0)
		{
			return component.GetPosition(new Vector3(x, 0, 0));
		}
        
		public static Vector3 GetPositionY(this Component component, float y = 0)
		{
			return component.GetPosition(new Vector3(0, y, 0));
		}
        
		public static Vector3 GetPositionZ(this Component component, float z = 0)
		{
			return component.GetPosition(new Vector3(0, 0, z));
		}
        
		public static Vector3 GetPosition(this Component component, Vector3 vector)
		{
			var position = component.transform.position;

			position.x += vector.x;
			position.y += vector.y;
			position.z += vector.z;

			return position;
		}
		
		
		#endregion

		#region Scale
		public static void SetScale(this Component component, float value)
		{
			var scale = component.transform.localScale;
			scale.x = value;
			scale.y = value;
			component.transform.localScale = scale;
		}

		public static void SetScale(this Component component, float x, float y)
		{
			var scale = component.transform.localScale;
			scale.x = x;
			scale.y = y;
			component.transform.localScale = scale;
		}

		public static float GetScaleX(this Component component)
		{
			return component.transform.localScale.x;
		}

		public static void SetScaleX(this Component component, float x)
		{
			var scale = component.transform.localScale;
			scale.x = x;
			component.transform.localScale = scale;
		}

		public static float GetScaleY(this Component component)
		{
			return component.transform.localScale.y;
		}

		public static void SetScaleY(this Component component, float y)
		{
			var scale = component.transform.localScale;
			scale.y = y;
			component.transform.localScale = scale;
		}
		#endregion
		
		#region Local Position

		public static void SetLocalPosition(this Component component, Component target)
		{
			component.transform.localPosition = target.transform.localPosition;
		}
		
		public static void SetLocalPosition(this Component component, Vector3 position)
		{
			component.transform.localPosition = position;
		}
		
		public static void SetLocalPosition(this Component component, float x, float y)
		{
			var vector = component.transform.localPosition;
			vector.x = x;
			vector.y = y;
			component.transform.localPosition = vector;
		}
		
		public static void SetLocalPositionX(this Component component, float x)
		{
			var vector = component.transform.localPosition;
			vector.x = x;
			component.transform.localPosition = vector;
		}
		
		public static void SetLocalPositionY(this Component component, float y)
		{
			var vector = component.transform.localPosition;
			vector.y = y;
			component.transform.localPosition = vector;
		}

		public static void SetLocalPositionZ(this Component component, float z)
		{
			var vector = component.transform.localPosition;
			vector.z = z;
			component.transform.localPosition = vector;
		}
		#endregion

		#region Size
		public static float GetWidth(this Component component)
		{
			var rectTransform = component.GetComponent<RectTransform>();
			if (rectTransform == null)
				throw new Exception();

			return rectTransform.rect.width;
		}
		
		public static float GetHeight(this Component component)
		{
			var rectTransform = component.GetComponent<RectTransform>();
			if (rectTransform == null)
				throw new Exception();

			return rectTransform.rect.height;
		}
		#endregion
	}
}
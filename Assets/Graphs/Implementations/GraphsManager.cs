using System;
using System.Collections.Generic;
using Graphs.Abstract;
using UnityEngine;

namespace Graphs.Implementations
{
	public class GraphsManager : MonoBehaviour, IGraphsManager
	{
		public static IGraphsManager instance { get; private set; }
		
		private const string DefaultGroupName = "Debug";
		private readonly Dictionary<string, GraphGroup> m_groups = new Dictionary<string, GraphGroup>();
		
		[SerializeField]
		private GraphGroup m_groupPrefab;
		
		private GraphGroup m_defaultGroup;

		public GraphGroup defaultGroup
		{
			get
			{
				if (m_defaultGroup == null)
				{
					m_defaultGroup = GetOrCreateGroup(DefaultGroupName);
				}

				return m_defaultGroup;
			}
		}

		private void Awake()
		{
			if (instance == null)
			{
				instance = this;
			}
		}

		private void OnDestroy()
		{
#pragma warning disable 0252
			if (instance == this)
#pragma warning restore 0252
			{
				instance = null;
			}
		}

		public GraphGroup GetOrCreateGroup(string groupName, Transform parent = null)
		{
			if (m_groups.TryGetValue(groupName, out var group))
			{
				return group;
			}

			if (parent == null)
			{
				parent = transform;
			}

			group = SpawnGroup(groupName, parent);
			m_groups.Add(groupName, group);

			return group;
		}

		public GraphGroup GetGroup(string groupName)
		{
			return m_groups.TryGetValue(groupName, out var group) ? group : null;
		}

		public IDisposable RegisterGroup(GraphGroup group)
		{
			if (m_groups.ContainsKey(group.title))
			{
				Debug.LogError($"Group with name: '{group.title}' already registered, make sure you give unique name for each group");
				return null;
			}
			
			m_groups.Add(group.title, group);
			return new DisposableAction(() => UnregisterGroup(group));
		}

		private void UnregisterGroup(GraphGroup group)
		{
			if (!m_groups.ContainsKey(group.title))
			{
				return;
			}

			m_groups.Remove(group.title);
		}

		private GraphGroup SpawnGroup(string groupName, Transform parent)
		{
			var group = Instantiate(m_groupPrefab, parent);
			group.SetTitle(groupName);

			return group;
		}
	}
}
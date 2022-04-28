using System;
using Graphs.Abstract;
using UnityEngine;

namespace Graphs.Implementations
{
	[RequireComponent(typeof(IGraphGroup))]
	public class RegisterGraphGroupComponent : MonoBehaviour
	{
		[SerializeField]
		private string m_groupName;

		[SerializeField]
		private bool m_enableSyncMode;
		
		private IGraphsManager m_graphsManager;
		private GraphGroup m_group;

		private IDisposable m_groupSubscription;
		
		private void Start()
		{
			m_graphsManager = GraphsManager.instance;
			m_group = GetComponent<GraphGroup>();
			
			m_group.SetTitle(m_groupName);
			m_group.SetSyncMode(m_enableSyncMode);
			
			RegisterGroup();
		}

		private void OnDestroy()
		{
			m_groupSubscription?.Dispose();
			Destroy(m_group.gameObject);
		}

		private void RegisterGroup()
		{
			m_groupSubscription = m_graphsManager.RegisterGroup(m_group);
		}
	}
}
using System;
using UnityEngine;

namespace Exit
{
	public class ExitDetection : MonoBehaviour
	{
		#region Variables

		public static ExitDetection Instance { get; private set; }

		public Action GameWon = delegate { };

		#endregion

		#region Unity

		private void Awake()
		{
			if (Instance != null)
				Destroy(this);
			else
				Instance = this;
		}

		private void OnTriggerEnter(Collider other)
		{
			if (other.CompareTag("Player"))
				GameWon.Invoke();
		}

		#endregion
	}
}

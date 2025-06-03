using Graphs;
using UnityEngine;

namespace Creature
{
	public class WeightedNode
	{
		#region Construction

		public WeightedNode(Node node, float nodeWeightNeutralizingSpeed)
		{
			Node = node;
			Weight = 1;

			m_weightNeutralizingSpeed = nodeWeightNeutralizingSpeed;
		}

		#endregion

		#region Variables

		public Node Node;

		public float Weight
		{
			get => m_weight;
			set
			{
				m_changeTime = Time.time;
				m_startingDifference = value - 1;

				m_weight = value;
			}
		}

		private float m_weight;

		private float m_changeTime;
		private float m_startingDifference;

		private readonly float m_weightNeutralizingSpeed;

		#endregion

		#region Implementation

		// Uses cubic ease out curve to reduce the node weight back to 1
		public float NeutralizeWeight(float time, float deltaTime)
		{
			if (Weight == 1)
				return 0;

			float currentDifference = Weight - 1;
			float absCurrentDifference = Mathf.Abs(currentDifference);

			if (absCurrentDifference < 0.01f)
			{
				m_weight -= currentDifference;
				return -currentDifference;
			}

			float timeSinceChange = time - m_changeTime;

			float weightChange = m_startingDifference * m_weightNeutralizingSpeed * Mathf.Pow(timeSinceChange, 3) * deltaTime;
			weightChange = Mathf.Clamp(weightChange, -absCurrentDifference, absCurrentDifference);

			m_weight -= weightChange;
			return -weightChange;
		}

		#endregion
	}
}

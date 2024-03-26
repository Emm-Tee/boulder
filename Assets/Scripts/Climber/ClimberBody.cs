using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClimberBody : MonoBehaviour
{
	#region Properties
	public Vector3 Position => transform.position;

	public float LimbLength => _limbLength;
	#endregion

	#region Fields
	[SerializeField] private ClimberHand[] _hands;
	[SerializeField] private float _limbLength;

	[Space]

	[SerializeField] private GameObject _centre;

	private bool _init = false;
	#endregion

	#region Unity Methods
	private void Update()
	{
		if(!_init)
		{
			return;
		}

		foreach(ClimberHand hand in _hands)
		{
			//hand.UpdatePosition();
		}

		CalculateBodyPosition();

	}

	private void Awake()
	{
		foreach(ClimberHand hand in _hands)
		{
			hand.Init(this);
		}
		_init = true;
	}
	#endregion

	#region Public Methods
	#endregion

	#region Private Methods
	private void CalculateBodyPosition()
	{
		foreach(ClimberHand hand in _hands)
		{
			Debug.DrawLine(hand.transform.position, transform.position);

			Vector3 direction = transform.position - hand.transform.position;

			direction = Vector3.Normalize(direction);

			direction *= LimbLength;

			hand.DebugCube.transform.position = hand.transform.position + direction;
		}


	}
	#endregion
}

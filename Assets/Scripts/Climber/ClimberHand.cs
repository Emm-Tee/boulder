using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClimberHand : MonoBehaviour
{

	#region Properties
	public GameObject DebugCube;
	public Vector2 Position => new Vector2(transform.position.x, transform.position.y);
	#endregion

	#region Fields
	private RouteHold _targetHold;
	private bool _lockedOnHold;

	private ClimberBody _anchor;
	#endregion


	#region Unity Methods
	private void Update()
	{
		if(_lockedOnHold) return;
	}
	#endregion


	#region Public Methods
	public void Init(ClimberBody body)
	{
		_anchor = body;
		_lockedOnHold = false;
	}

	public void ReleaseHold()
	{
		_lockedOnHold = false;
		_targetHold = null;
	}

	public void TargetHold(RouteHold target)
	{
		_targetHold = target;
	}

	public void UpdatePosition()
	{
		if(_lockedOnHold) return;

		//if within reach, grab it
		Vector3 direction = _targetHold.Position - _anchor.Position;
		if(direction.sqrMagnitude >= _anchor.LimbLength * _anchor.LimbLength)
		{
			TakeHold();
			return;
		}

		//Get direction normalised
		direction = Vector3.Normalize(_targetHold.Position - _anchor.Position);

		//Reach towards it as far as possible
		Vector3 reach = direction * _anchor.LimbLength;

		transform.position = reach;
	}
	#endregion

	#region Private Methods
	private void TakeHold()
	{
		_lockedOnHold = true;
		transform.position = _targetHold.Position;
	}
	#endregion
}

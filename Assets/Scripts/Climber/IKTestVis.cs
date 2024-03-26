using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IKTestVis : MonoBehaviour
{
	public Vector2 Position => new Vector2(transform.position.x, transform.position.y);

	public void SetPosition(Vector2 position)
	{
		transform.position = new Vector3(position.x, position.y);
	}

	public void SetGoalPosition()
	{

	}
}

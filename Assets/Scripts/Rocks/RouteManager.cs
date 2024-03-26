using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RouteManager : MonoBehaviour
{
	#region Constants
	#endregion


	#region Properties	
	private float RandomAngleOffset => Random.Range(-_angleMaxStepVariance, _angleMaxStepVariance);
	#endregion

	#region Fields
	[SerializeField] private RouteHold _holdPrefab;
	[SerializeField] private RouteMove _movePrefab;

	[Space]
	[Header("Route")]
	[SerializeField] private int _numMoves;
	[SerializeField] private int _numHoldsPerMove;
	[Header("Angle")]
	[SerializeField] private float _maxAngle = 180;
	[SerializeField] private float _angleDifference = 45;
	[SerializeField] private float _angleMaxStepVariance = 5f;
	[Header("Position")]
	[SerializeField] private float _positionStepDistance = 10;
	[SerializeField] private float _positionMaxVariance = 2;
	[SerializeField] private float _positionMinVariance = 0;

	[SerializeField] private GameObject _cube;

	private List<RouteMove> _moves = new();
	private List<RouteHold> _holds = new();
	#endregion

	private void Start()
	{
		SpawnRocks();
	}

	[ContextMenu("Reload")]
	public void SpawnRocks()
	{
		for (int i = 0; i < _holds.Count; i++)
		{
			_holds[i].gameObject.SetActive(false);
		}
		_holds.Clear();

		float currentAngle = Random.Range(0, _maxAngle);
		Vector2 position = Vector2.zero;

		for (int i = 0; i < _numMoves; i++)
		{
			RouteMove move = Instantiate(_movePrefab, transform);
			move.transform.position = position;
			_moves.Add(move);

			for (int j = 0; j < _numHoldsPerMove; j++)
			{
				//Instantiate 
				RouteHold rock = Instantiate(_holdPrefab, transform);
				rock.transform.position = position + RandomPositionOffset(j);
				_holds.Add(rock);
			}

			//get new position;
			position += GetMoveCentre(currentAngle + RandomAngleOffset);

			//Get new angle
			currentAngle = GetRouteAngle(currentAngle);
		}
	}

	//Each few steps of the route go out in the same angle, the individual ones get a little random applied to them too
	private float GetRouteAngle(float previousAngle)
	{
		//find the range of the anglewe're aiming for, clamping within the normal range
		float min = Mathf.Min(_maxAngle, previousAngle + _angleDifference);
		float max = Mathf.Max(0, previousAngle - _angleDifference);

		return Random.Range(max, min);
	}

	// Returns the new position the next rock will be based on before its variance is factored in
	private Vector2 GetMoveCentre(float angle)
	{
		float radians = Mathf.Deg2Rad * angle;

		float newX = _positionStepDistance * Mathf.Cos(radians);
		float newY = _positionStepDistance * Mathf.Sin(radians);

		return new Vector2(newX, newY);
	}

	private Vector2 RandomPositionOffset(int quad)
	{
		float x = Random.Range(_positionMinVariance, _positionMaxVariance);
		float y = Random.Range(_positionMinVariance, _positionMaxVariance);

		int xPolarity = -1 + 2 *(quad % 2);
		int yPolarity = 1 - 2 * Mathf.FloorToInt(quad/ 2);

		x *= xPolarity;
		y *= yPolarity;

		return new Vector2(x, y);
	}

}

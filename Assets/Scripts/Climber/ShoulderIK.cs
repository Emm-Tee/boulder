using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using UnityEngine;
using UnityEngine.AdaptivePerformance;
using UnityEngine.UIElements;
using UnityEngine.XR;

public class ShoulderIK : MonoBehaviour
{
	#region Constants
	#endregion

	#region Properties
	private Vector2 Position => transform.position;
	#endregion

	#region Fields
	private float _armLength = 6;
	private float _shoulderWidth = 2;

	[SerializeField] private float _backLength = 8;
	[SerializeField] private float _hipWidth = 1.5f;

	[SerializeField] private float _thighLength = 8;
	[SerializeField] private float _shinLength = 8;


	private Vector2 _testPreviousPosition = Vector2.zero;
	private Vector2 _goalPosition = Vector2.zero;

	//BodyParts
	[Header("Upper")]
	[SerializeField] private IKTestVis _backAnchor;
	[SerializeField] private IKTestVis _leftShoulder;
	[SerializeField] private IKTestVis _rightShoulder;
	[SerializeField] private IKTestVis _leftHand;
	[SerializeField] private IKTestVis _rightHand;
	[SerializeField] private IKTestVis _testTarget;

	[Header("Lower")]
	[SerializeField] private IKTestVis _hipAnchor;
	[SerializeField] private IKTestVis _leftHip;
	[SerializeField] private IKTestVis _rightHip;
	[SerializeField] private IKTestVis _leftKnee;
	[SerializeField] private IKTestVis _rightKnee;
	[SerializeField] private IKTestVis _leftFoot;
	[SerializeField] private IKTestVis _rightFoot;

	[Space]
	[SerializeField] private int _steps = 3;
	[SerializeField] private float _armLengthMargin = 0.5f;
	[SerializeField] private float _moveSpeed = 0.5f;

	[Space]
	[SerializeField] private Color[] _stepsColors;
	#endregion

	#region Unity Methods
	private void Awake()
	{
		StartClimb();
	}

	private void Update()
	{
		TestMoveLegs();
	}
	#endregion

	#region Private Methods
	[ContextMenu("Set starting position")]
	private void StartClimb()
	{
		//add shoulder
		_leftShoulder.SetPosition(_backAnchor.Position + Vector2.left.normalized * _shoulderWidth);
		_rightShoulder.SetPosition(_backAnchor.Position + Vector2.right.normalized * _shoulderWidth);

		_leftHand.SetPosition(_leftShoulder.Position + new Vector2(-1,1).normalized * _armLength);
		_rightHand.SetPosition(_rightShoulder.Position + Vector2.one.normalized * _armLength);

		_goalPosition = _testTarget.Position;

		//Legs 
		_hipAnchor.SetPosition(_backAnchor.Position + ((Vector2.down).normalized) * _backLength);

		_leftHip.SetPosition(_hipAnchor.Position + ((Vector2.left).normalized) * _hipWidth);
		_rightHip.SetPosition(_hipAnchor.Position + ((Vector2.right).normalized) * _hipWidth);

		_leftKnee.SetPosition(_leftHip.Position + (new Vector2(-1, 0.6f).normalized * _thighLength));
		_rightKnee.SetPosition(_rightHip.Position + (new Vector2(1, 0.6f).normalized * _thighLength));

		_leftFoot.SetPosition(_leftKnee.Position + (new Vector2( -1, -0.9f).normalized * _shinLength));
		_rightFoot.SetPosition(_rightKnee.Position + (new Vector2(1, -0.9f).normalized * _shinLength));

		Debug.Log($" DIst? {Vector2.Distance(_rightKnee.Position, _rightFoot.Position)} ->> {_shinLength}");
	}

	[ContextMenu("MOVE")]
	private void AdjustIK()
	{
		if(_testPreviousPosition == _testTarget.Position)
		{
			return;
		}

		Vector2 direction;
		int sideIndex = 0;

		//Get furthest hand
		float leftDist = Mathf.Abs( (_leftHand.Position - _leftShoulder.Position).sqrMagnitude);
		float rightDist = Mathf.Abs((_rightHand.Position - _rightShoulder.Position).sqrMagnitude);

		//Add left only to change correct index to right afterwards
		IKTestVis[] hands = new IKTestVis[] { _leftHand,_leftHand};
		IKTestVis[] shoulders = new IKTestVis[] { _leftShoulder, _leftShoulder};

		//insert right to the correct position
		int rightIndex = leftDist > rightDist ? 1 : 0;
		hands[rightIndex] = _rightHand;
		shoulders[rightIndex] = _rightShoulder;

		bool[] finished = new bool[] { false, false };

		//First set shoulders on target position
		//original anchor to shoulder position
		direction = shoulders[sideIndex].Position - _backAnchor.Position;
		direction = direction.normalized * _shoulderWidth;
		_backAnchor.SetPosition(_testTarget.Position);
		shoulders[sideIndex].SetPosition(_backAnchor.Position + direction);

		//Set previous value for next frame
		_testPreviousPosition= _testTarget.Position;

		for (int i = 0; i < _steps; i++)
		{
			//test to see if we've reached an acceptable ik location

			float distance = (hands[sideIndex].Position - shoulders[sideIndex].Position).magnitude;
			float delta = Mathf.Abs(distance) - _armLength;

			Debug.Log($"{distance}: delta {delta} < ALM: {_armLengthMargin} |||| {Mathf.Abs(distance)} - {_armLength} = {delta}");
			finished[sideIndex] = Mathf.Abs(delta) < _armLengthMargin;

			bool acceptable = true;
			foreach(bool comp in finished)
			{
				if (!comp)
				{
					acceptable = false;
				}
			}
			if (acceptable)
			{
				Debug.Log($"Acceptable");
				break;
			}

			// Then ik the hand to new shoulder position
			direction = shoulders[sideIndex].Position - hands[sideIndex].Position;
			direction = direction.normalized * _armLength;
			shoulders[sideIndex].SetPosition(hands[sideIndex].Position + direction);

			//Adjust the middle point and new shoulder
			//shoulder to centre 
			direction = _backAnchor.Position - shoulders[sideIndex].Position;
			direction = direction.normalized * _shoulderWidth;
			_backAnchor.SetPosition(shoulders[sideIndex].Position + direction);
			//toggle sides
			sideIndex++;
			sideIndex %= 2;
			//extend new shoulder out 
			shoulders[sideIndex].SetPosition(_backAnchor.Position + direction);
		}

		float dist0 = Vector2.Distance(hands[0].Position, shoulders[0].Position);
		float dist1 = Vector2.Distance(hands[1].Position, shoulders[1].Position);

		Debug.Log($"FINISHED :: {dist0} -- {dist1}");
	}

	private Vector2 MoveAnchorWithSpeed()
	{
		Vector2 direction = _testTarget.Position - _backAnchor.Position;
		Vector2 move = direction.normalized * _moveSpeed;
		Debug.Log($"Move length : {move.magnitude} || speed {_moveSpeed} || move {move}");
		return (_backAnchor.Position + move);
	}

	[ContextMenu("Print distance")]
	private void Print()
	{
		float dist0 = (_leftHand.Position - _leftShoulder.Position).magnitude;
		float dist1 = (_rightHand.Position - _rightShoulder.Position).magnitude;

		Vector3 right = _rightShoulder.Position - _rightHand.Position;
		Vector3 left = _leftShoulder.Position - _leftHand.Position;

		right = right.normalized * _armLength;
		left = left.normalized * _armLength;

		Debug.DrawRay(_leftHand.Position, left, Color.black, 5);
		Debug.DrawRay(_rightHand.Position, right, Color.black, 5);

		Debug.Log($"Distance left : {dist0} || right {dist1}");
	}

	[ContextMenu("Print Move")]
	private void PrintMOve()
	{
		Debug.DrawRay(_backAnchor.Position, MoveAnchorWithSpeed(), Color.black, 5);
		_testTarget.SetPosition(MoveAnchorWithSpeed());
	}

	[ContextMenu("Move legs test")]
	private void TestMoveLegs()
	{
		//shoulder to hips
		Vector2 direction = _hipAnchor.Position - _backAnchor.Position;
		direction = direction.normalized * _backLength;

		AdjustIK();

		MoveLegs(_backAnchor.Position + direction);
	}

	private void MoveLegs(Vector2 anchor)
	{
		Debug.Log("MOving legs");
		//set up variables to be used 
		Vector2 direction;
		int sideIndex = 0;

		IKTestVis[] hips = new IKTestVis[] { _leftHip, _rightHip };
		IKTestVis[] knees = new IKTestVis[] { _leftKnee, _rightKnee };
		IKTestVis[] feet = new IKTestVis[] {_leftFoot, _rightFoot };

		//get the offset from the hip centre to the first hip before we update the centre position
		direction = hips[sideIndex].Position - _hipAnchor.Position;
		direction = direction.normalized * _hipWidth;
		_hipAnchor.SetPosition(anchor);
		hips[sideIndex].SetPosition(_hipAnchor.Position + direction);

		//Now cycle through the legs
		for(int i =0; i < _steps; i++)
		{
			//hip to knee
			direction = knees[sideIndex].Position - hips[sideIndex].Position;
			direction = direction.normalized * _thighLength;
			knees[sideIndex].SetPosition(hips[sideIndex].Position + direction);

			//foot to new knee
			direction = knees[sideIndex].Position - feet[sideIndex].Position;
			direction = direction.normalized * _shinLength;
			knees[sideIndex].SetPosition(feet[sideIndex].Position + direction);

			//knee to hip
			direction = hips[sideIndex].Position - knees[sideIndex].Position;
			direction = direction.normalized * _thighLength;
			hips[sideIndex].SetPosition(knees[sideIndex].Position + direction);

			//hip to centre
			direction = _hipAnchor.Position - hips[sideIndex].Position;
			direction = direction.normalized * _hipWidth;
			_hipAnchor.SetPosition(hips[sideIndex].Position + direction);

			//swapping sides and doing second hip
			sideIndex++;
			sideIndex %= 2;

			hips[sideIndex].SetPosition(_hipAnchor.Position + direction);
			//repeat
		}
	}

	[ContextMenu("LAtest")]
	private void TestShoulders()
	{
		AdjustLimbSet(true);

		//Pass anchor goal position
	}

	private Vector2 GetGoalAnchorPosition(bool arms)
	{
		if(arms)
		{
			return _testTarget.Position;
		}

		Vector2 direction = _hipAnchor.Position - _backAnchor.Position;
		direction = direction.normalized * _backLength;

		return _backAnchor.Position + direction;
	}

	private void GetLimbData(bool arms, out IKTestVis[][] joints, out float[] lengths, out IKTestVis anchor, out Vector2 goalAnchorPosition)
	{
		//make joints list
		IKTestVis[] left;
		IKTestVis[] right;

		if(arms)
		{
			left = new IKTestVis[] { _leftShoulder, _leftHand };
			right = new IKTestVis[] { _rightShoulder, _rightHand };

			lengths = new float[] { _shoulderWidth, _armLength };

			//set anchor
			anchor = _backAnchor;
		}
		else
		{
			left = new IKTestVis[] { _leftHip, _leftKnee, _leftHand };
			right = new IKTestVis[] { _rightHip, _rightKnee, _rightFoot };

			lengths = new float[] { _hipWidth, _thighLength, _shinLength };

			//set anchor
			anchor = _hipAnchor;
		}

		joints = new IKTestVis[][] { left, right };

		goalAnchorPosition = GetGoalAnchorPosition(arms);
	}

	private void AdjustLimbSet(bool arms)
	{
		int sideIndex = 0;
		int jointIndex = 0;
		int dir = 1;
		Vector2 direction;

		GetLimbData(arms, out IKTestVis[][] joints, out float[] lengths, out IKTestVis anchor, out Vector2 goalAnchorPosition);

		//Get direction of first joint from anchor
		direction = CurrenJoint().Position - anchor.Position;
		SetDirectionLength();

		//move anchor to desired position
		anchor.SetPosition(goalAnchorPosition);
		//move first joint
		CurrenJoint().SetPosition(anchor.Position + direction);


		int loopCount = (JointCount() * 2 - 2) * _steps + 1;
		Debug.Log($"loop count = {loopCount} == {JointCount()} * 2 - 2 = {JointCount() * 2 - 2} || * {_steps} + 1");
		for (int i = 0; i < loopCount; i++)
		{
			//If at 0 and going back to centre
			if (jointIndex == 0 && dir < 0)
			{
				//direction from first joint to anchor
				direction = anchor.Position - CurrenJoint().Position;
				SetDirectionLength();
				//move anchor
				anchor.SetPosition(direction);

				//flip sides
				sideIndex = (sideIndex + 1) % 2;

				//Set other shoulder
				CurrenJoint().SetPosition(anchor.Position + direction);

				FlipDirection();

			}
			//If at second last joint and going out to end
			else if (jointIndex == JointCount() - 2 && dir > 0)
			{
				//Move end limb to the place it's meant to be
				joints[sideIndex][LastJointIndex()].SetGoalPosition();
				UpdateJointIndex();
				//don't do anything else for this joint
				continue;
			}
			//if at last joint
			else if (jointIndex == LastJointIndex())
			{
				FlipDirection();
			}

			Debug.Log($"adjusting joints at the end :: \n" +
				$"side index {sideIndex}\n" +
				$"joint index {jointIndex}\n" +
				$"direction {dir}");
			//now adjust joints
			direction = CurrenJoint().Position - NextJoint().Position;
			SetDirectionLength();

			NextJoint().SetPosition(CurrenJoint().Position + direction);

			UpdateJointIndex();
		}

		//Helper functions
		int JointCount()
		{
			return joints[sideIndex].Length;
		}

		int LastJointIndex()
		{
			return JointCount() - 1;
		}

		void FlipDirection()
		{
			dir = -dir;
		}

		void SetDirectionLength()
		{
			//dir adjustment gives 0 or 1 based on direction, with 0 being the direction behind current joint and 1 being in front
			direction = direction.normalized * lengths[jointIndex + ((dir + 1)/2)];
		}

		void UpdateJointIndex()
		{
			jointIndex += dir;
		}

		IKTestVis CurrenJoint()
		{
			return joints[sideIndex][jointIndex];
		}

		IKTestVis NextJoint()
		{
			Debug.Log($"trying for next joint : {joints[sideIndex].Length}[ {jointIndex + dir} ] || made up of {jointIndex} + {dir}");
			return joints[sideIndex][jointIndex + dir];
		}
	}
	#endregion
}

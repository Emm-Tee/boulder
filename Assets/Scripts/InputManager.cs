using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{

    #region Fields
	#endregion


	// Start is called before the first frame update
	void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        if (IsTouchPressed(out Vector2 touchPosition))
		{
			Debug.Log("TAP");
			if(DidTapRock(touchPosition, out RouteHold rock))
			{
				Debug.Log("ROCK", rock.gameObject);
			}
		}

    }

    private bool IsTouchPressed (out Vector2 position)
    {
		position = Vector2.zero;

		if (Input.GetMouseButtonDown(0))
		{
			position = Input.mousePosition;
			return true;
		}

		if (Input.touchCount > 0)
		{
			position = Input.touches[0].position;
			return true;
		}


		return false;
	}

	private bool DidTapRock(Vector2 tapPosition, out RouteHold rock)
	{
		rock = null;

		Ray ray = Camera.main.ScreenPointToRay(tapPosition);
		RaycastHit hit;

		if (Physics.Raycast(ray, out hit))
		{
			Debug.Log("HIT", hit.transform.gameObject);
			// Check if the ray hit a GameObject
			GameObject clickedObject = hit.transform.gameObject;

			if((rock = clickedObject.GetComponentInParent<RouteHold>()) != null)
			{
				return true;
			}
		}

		return false;
	}
    
}

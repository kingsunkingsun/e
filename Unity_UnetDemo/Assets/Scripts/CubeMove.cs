using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

struct CubeState
{
	public int moveNum;
	public int x;
	public int y;
}

public class CubeMove : NetworkBehaviour {

	Queue pendingMoves;

	[SyncVar(hook = "OnServerStateChanged")] 
	CubeState serverState;

	CubeState predictedState;

	// Use this for initialization
	void Start () {
		InitState ();
		pendingMoves = new Queue ();
		predictedState = new CubeState 
		{
			moveNum = 0,
			x = 0,
			y = 0
		};
	}

	[Server]void InitState()
	{
		if (isLocalPlayer) 
		{
			serverState = new CubeState {
				x = 0,
				y = 0
			};
		}
	}
	// Update is called once per frame
	void Update () {
		if (isLocalPlayer) 
		{
			KeyCode[] arrowKeys = { KeyCode.UpArrow, KeyCode.DownArrow, KeyCode.RightArrow, KeyCode.LeftArrow };
			foreach (KeyCode arrowKey in arrowKeys) 
			{
				if (!Input.GetKeyDown (arrowKey))
					continue;

				pendingMoves.Enqueue (arrowKey);

				predictedState = Move (predictedState, arrowKey);

				CmdMoveOnServer(predictedState,arrowKey);
			}
		}

		SyncState ();
	}

	void OnServerStateChanged(CubeState newState)
	{
		serverState = newState;
		if (!isLocalPlayer)
			return;
		if (pendingMoves != null) 
		{
			while (pendingMoves.Count > (predictedState.moveNum - serverState.moveNum))
			{
				pendingMoves.Dequeue ();
			}
			UpdatePredictedState ();
		}

	}

	void UpdatePredictedState()
	{
		predictedState = serverState;
		foreach(KeyCode arrowKey in pendingMoves)
		{
			predictedState = Move (predictedState, arrowKey);
		}
	}

	CubeState Move(CubeState previous, KeyCode arrowKey)
	{
		int dx = 0;
		int dy = 0;
		switch (arrowKey)
		{
		case KeyCode.UpArrow:
			dy = 1;
			break;
		case KeyCode.DownArrow:
			dy = -1;
			break;
		case KeyCode.RightArrow:
			dx = 1;
			break;
		case KeyCode.LeftArrow:
			dx = -1;
			break;
		}


		return new CubeState
		{
			moveNum = 1 + previous.moveNum,
			x = dx + previous.x,
			y = dy + previous.y
		};
	}

	void SyncState()
	{
		CubeState stateToRender = isLocalPlayer ? predictedState : serverState;
		transform.position = new Vector2 (stateToRender.x, stateToRender.y);
	}

	[Command] void CmdMoveOnServer(CubeState state, KeyCode arrowKey)
	{
		int dx = 0;
		int dy = 0;
		switch (arrowKey)
		{
		case KeyCode.UpArrow:
			dy = 1;
			break;
		case KeyCode.DownArrow:
			dy = -1;
			break;
		case KeyCode.RightArrow:
			dx = 1;
			break;
		case KeyCode.LeftArrow:
			dx = -1;
			break;
		}

		serverState = new CubeState { moveNum = this.serverState.moveNum + 1, x = this.serverState.x + dx, y = this.serverState.y + dy  };
	}

}

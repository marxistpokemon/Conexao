using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ConnectionLines : MonoBehaviour {
	public static ConnectionLines g;
	public float completionDelay;
	private LineRenderer _lineRenderer;
	private List<Piece> sequence;

	void Awake() {
		g = this;
	}

	// Use this for initialization
	void Start () {
		_lineRenderer = GetComponent<LineRenderer>();
	}
	
	// Update is called once per frame
	void Update () {
		sequence = PieceManager.g.sequence;
		if(sequence.Count > 0){
			_lineRenderer.enabled = true;
			_lineRenderer.SetVertexCount(sequence.Count+1);
			Vector2[] points = new Vector2[sequence.Count];
			for (int i = 0; i < sequence.Count; i++) {
				_lineRenderer.SetPosition(i, sequence[i].transform.position);
			}
			_lineRenderer.SetPosition(sequence.Count, sequence[sequence.Count-1].transform.position);
		}
		else {
			_lineRenderer.enabled = false;
		}
		// follow mouse cursor
		if(!GameManager.g.moveLock && sequence.Count > 0){
			_lineRenderer.SetPosition(
				sequence.Count,
				new Vector3(Camera.main.ScreenToWorldPoint(Input.mousePosition).x,
			            Camera.main.ScreenToWorldPoint(Input.mousePosition).y,
			            0));
		}
	}
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour {

	public static GameManager g;

	public LineRenderer connectionLine;
	public List<Transform> pieceSequence;
	public List<Transform> allPieces;

	public float completionDelay = 1f;
	public bool moveLock;

	public PolygonCollider2D polCollider;
	public Triangulator triagulator;

	public float meshOffsetZ = 1;

	void Start () {
		g = this;
		pieceSequence = new List<Transform>();
		connectionLine = GetComponent<LineRenderer>();
		polCollider = GetComponent<PolygonCollider2D>();

		allPieces = new List<Transform>();
		GameObject[] allGO = GameObject.FindGameObjectsWithTag("Piece");
		foreach (var go in allGO) {
			allPieces.Add(go.transform);
		}
		polCollider.enabled = false;
	}

	void Update(){
		polCollider.enabled = (pieceSequence.Count > 2);
		if(pieceSequence.Count > 0){
			connectionLine.enabled = true;
			UpdateConnectionLine();
		}
		else {
			connectionLine.enabled = false;
		}
		// follow mouse cursor
		if(!moveLock){
			connectionLine.SetPosition(
				pieceSequence.Count, 
				Camera.main.ScreenToWorldPoint(Input.mousePosition));
		}
	}

	public void CreateMesh(Vector2[] vertices2D){
		// Use the triangulator to get indices for creating triangles
		Triangulator tr = new Triangulator(vertices2D);
		int[] indices = tr.Triangulate();
		
		// Create the Vector3 vertices
		Vector3[] vertices = new Vector3[vertices2D.Length];
		for (int i=0; i<vertices.Length; i++) {
			vertices[i] = new Vector3(vertices2D[i].x, vertices2D[i].y, meshOffsetZ);
		}
		
		// Create the mesh
		Mesh msh = new Mesh();
		msh.vertices = vertices;
		msh.triangles = indices;
		msh.RecalculateNormals();
		msh.RecalculateBounds();
		msh.uv = vertices2D;
		GetComponent<MeshFilter>().mesh = msh;
	}

	public int SequenceCount(){
		return pieceSequence.Count;
	}

	public bool AddPiece(Transform piece){
		int index = pieceSequence.FindIndex(p => p == piece);
		if (index < 0) {
			pieceSequence.Add(piece);
			UpdateClicked();
			return true;
		}
		return false;
	}

	public void RemovePiece(Transform piece){
		int indexToRemove = pieceSequence.FindIndex(p => p == piece);
		Debug.Log (indexToRemove);
		pieceSequence.RemoveRange(
			indexToRemove,
			pieceSequence.Count - indexToRemove);
		UpdateClicked();
	}

	public void UpdateClicked ()
	{
		allPieces.ForEach(p => p.GetComponent<Piece>().isClicked = false);
		foreach (var piece in pieceSequence) {
			piece.GetComponent<Piece>().isClicked = true;
		};
	}

	public void UpdateConnectionLine ()
	{
		connectionLine.SetVertexCount(pieceSequence.Count+1);
		Vector2[] points = new Vector2[pieceSequence.Count];
		for (int i = 0; i < pieceSequence.Count; i++) {
			connectionLine.SetPosition(i, pieceSequence[i].position);
			points[i] = new Vector2(
				pieceSequence[i].position.x,
				pieceSequence[i].position.y);
		}
		polCollider.SetPath(0, points);
	}

	public IEnumerator CompleteSequence(Transform firstPiece){
		moveLock = true;
		CreateMesh(polCollider.points);
		yield return new WaitForSeconds(completionDelay);
		int sequenceValue = 0;
		CreateMesh(new Vector2[3]);
		foreach (var pieceCapture in allPieces.FindAll(
			p => p.GetComponent<Piece>().onCaptureArea)) {
			Debug.Log("Pontos peca: " + pieceCapture.GetComponent<Piece>().value);
			sequenceValue += pieceCapture.GetComponent<Piece>().value;
			allPieces.Remove(pieceCapture);
			Destroy(pieceCapture.gameObject);
		}
		pieceSequence.Clear();
		UpdateConnectionLine();
		Debug.Log ("Total: " + sequenceValue);
		moveLock = false;
	}

	public bool isInSequence(Transform piece){
		return (pieceSequence.FindIndex(p => p == piece) > 0);
	}
}

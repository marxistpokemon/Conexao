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

	public int level;
	public int points;

	public Transform piecePrefab;
	public Transform txtLevel;
	public Transform txtPoints;
	private GUIText msgLevel;
	private GUIText msgPoints;

	void Start () {
		g = this;
		pieceSequence = new List<Transform>();
		connectionLine = GetComponent<LineRenderer>();
		polCollider = GetComponent<PolygonCollider2D>();
		UpdateAllPieces();
		polCollider.enabled = false;
		msgPoints = txtPoints.GetComponent<GUIText>();
		msgLevel = txtLevel.GetComponent<GUIText>();
	}

	public Vector3 CreateMesh(Vector2[] vertices2D){
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
		// returns the center so we can use it to place the combined piece
		return msh.bounds.center;
	}

	public bool AddPieceToSeq(Transform piece){
		int index = pieceSequence.FindIndex(p => p == piece);
		if (index < 0) {
			pieceSequence.Add(piece);
			UpdateClicked();
			return true;
		}
		return false;
	}

	public void RemovePieceFromSeq(Transform piece){
		Debug.Log ("remove piece");
		int indexToRemove = pieceSequence.FindIndex(p => p == piece);
		pieceSequence.RemoveRange(indexToRemove, SeqCount()-indexToRemove);
		UpdateClicked();
	}

	#region Updates and helper functions
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
		// GUI update
		msgLevel.text = "Level: " + level;
		msgPoints.text = "Points: " + points;
	}

	void UpdateAllPieces(){
		allPieces = new List<Transform>();
		Object[] allGO = GameObject.FindObjectsOfType(typeof(Piece));
		foreach (var go in allGO) {
			allPieces.Add((go as Piece).transform);
		}
	}

	void UpdateClicked ()
	{
		allPieces.ForEach(p => {
			p.GetComponent<Piece>().isClicked = isInSeq(p);
		});
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

	public int SeqCount(){
		return pieceSequence.Count;
	}

	public bool isInSeq(Transform piece){
		return (pieceSequence.FindIndex(p => p == piece) >= 0);
	}
	#endregion

	public IEnumerator CompleteSeq(Transform firstPiece){
		moveLock = true;
		Vector3 spawnPos = CreateMesh(polCollider.points);
		yield return new WaitForSeconds(completionDelay);
		// clear area mesh
		CreateMesh(new Vector2[3]);
		// handle captured pieces
		int capturedValue = 0;
		int seqValue = 0;
		allPieces.ForEach(p => {
			Piece piece = p.GetComponent<Piece>();
			capturedValue += (piece.onCaptureArea)? piece.value : 0;
			seqValue += (piece.isClicked)? piece.value : 0;
			if(piece.isClicked || piece.onCaptureArea){
				DestroyObject(p.gameObject);
			}
			piece.isClicked = false;
		});
		points += seqValue;
		if(capturedValue > 0){
			Transform combinedPiece = 
				Instantiate (piecePrefab, spawnPos, Quaternion.identity) 
					as Transform;
			combinedPiece.GetComponent<Piece>().value = capturedValue;
		}
		Debug.Log("Combined: " + capturedValue + " | Points: " + seqValue);
		pieceSequence.Clear();
		allPieces = null;
		yield return new WaitForSeconds(0);
		UpdateAllPieces();
		UpdateConnectionLine();
		moveLock = false;
	}
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using URandom;

public class GameManager : MonoBehaviour {

	public static GameManager g;

	public int level;
	public int points;

	private LineRenderer connectionLine;
	public int cursorValue = 0;
	public List<Piece> sequence;
	public List<Piece> allPieces;

	public float completionDelay = 1f;
	[HideInInspector] public bool moveLock;

	[HideInInspector] public PolygonCollider2D polCollider;
	[HideInInspector] public Triangulator triagulator;

	public float meshOffsetZ = 1;

	public Transform piecePrefab;
	public Transform txtLevel;
	public Transform txtPoints;
	private GUIText msgLevel;
	private GUIText msgPoints;

	private UnityRandom urand;

	void Awake(){
		g = this;
	}

	void Start () {
		urand = new UnityRandom();
		sequence = new List<Piece>();
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

	public bool AddPieceToSeq(Piece piece){
		int index = sequence.FindIndex(p => p == piece);
		if (index < 0) {
			sequence.Add(piece);
			UpdateClicked();
			return true;
		}
		return false;
	}

	public void RemovePieceFromSeq(Piece piece){
		Debug.Log ("remove piece");
		int indexToRemove = sequence.FindIndex(p => p == piece);
		sequence.RemoveRange(indexToRemove, SeqCount()-indexToRemove);
		UpdateClicked();
	}

	#region Updates and helper functions
	void Update(){
		polCollider.enabled = (sequence.Count > 2);
		if(sequence.Count > 0){
			connectionLine.enabled = true;
			UpdateConnectionLine();
		}
		else {
			connectionLine.enabled = false;
		}
		// follow mouse cursor
		if(!moveLock && sequence.Count > 0){
			connectionLine.SetPosition(
				sequence.Count, 
				Camera.main.ScreenToWorldPoint(Input.mousePosition));
		}
		// GUI update
		msgLevel.text = "Level: " + level;
		msgPoints.text = "Points: " + points;
		if(sequence.Count == 0) cursorValue = 0;
	}

	public void UpdateAllPieces(){
		allPieces = new List<Piece>();
		Object[] allGO = GameObject.FindObjectsOfType(typeof(Piece));
		foreach (var go in allGO) {
			allPieces.Add(go as Piece);
			(go as Piece).onCaptureArea = false;
		}
	}

	void UpdateClicked ()
	{
		allPieces.ForEach(p => {
			p.isClicked = isInSeq(p);
		});
	}

	public void UpdateConnectionLine ()
	{
		connectionLine.SetVertexCount(sequence.Count+1);
		Vector2[] points = new Vector2[sequence.Count];
		for (int i = 0; i < sequence.Count; i++) {
			connectionLine.SetPosition(i, sequence[i].transform.position);
			points[i] = new Vector2(
				sequence[i].transform.position.x,
				sequence[i].transform.position.y);
		}
		connectionLine.SetPosition(sequence.Count, sequence[0].transform.position);
		polCollider.SetPath(0, points);
	}

	public int SeqCount(){
		return sequence.Count;
	}

	public bool isInSeq(Piece piece){
		return (sequence.FindIndex(p => p == piece) >= 0);
	}
	#endregion

	public IEnumerator CompleteSeq(Transform firstPiece){
		moveLock = true;
		//Vector3 spawnPos = CreateMesh(polCollider.points);
		Slot spawnSlot = sequence[0].slot;
		Vector3 spawnPos = spawnSlot.position;
		yield return new WaitForSeconds(completionDelay);
		// clear area mesh
		//CreateMesh(new Vector2[3]);
		// handle captured pieces
		int capturedValue = 0;
		int seqValue = 1;
		allPieces.ForEach(p => {
			Piece piece = p.GetComponent<Piece>();
			capturedValue += (piece.onCaptureArea)? piece.value : 0;
			seqValue *= (piece.isClicked)? piece.value : 1;
			if(piece.isClicked || piece.onCaptureArea){
				p.slot.full = false;
				DestroyObject(p.gameObject);
				Destroy (p);
			}
			piece.isClicked = false;
		});
		points += seqValue;
		if(capturedValue > 0){
			Transform combinedPiece = 
				Instantiate (piecePrefab) as Transform;
			spawnSlot.piece = combinedPiece.GetComponent<Piece>();
			spawnSlot.piece.transform.position = spawnSlot.position;
			spawnSlot.full = true;
			spawnSlot.piece.slot = spawnSlot;
			spawnSlot.piece.value = capturedValue;
		}
		Debug.Log("Combined: " + capturedValue + " | Points: " + seqValue);
		sequence.Clear();
		allPieces = null;
		yield return new WaitForSeconds(0);
		int newWave = Grid.g.FindEmptySlots().Length;
		for (int i = 0; i < newWave; i++) {
			Grid.g.SpawnPiece(Random.Range(1, 5));
		}
		UpdateAllPieces();
		moveLock = false;
	}
}

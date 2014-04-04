using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using URandom;

public class GameManager : MonoBehaviour {

#region Vars
	public static GameManager g;

	public int level;
	public int points;
	public int turns;

	private LineRenderer connectionLine;
	public int cursorValue = 0;
	public List<Piece> sequence;
	public List<Piece> allPieces;

	public float completionDelay = 1f;
	[HideInInspector] public bool moveLock;

	[HideInInspector] public Triangulator triagulator;

	public float meshOffsetZ = 1;

	public Transform piecePrefab;
	public Transform txtLevel;
	public Transform txtPoints;
	private GUIText msgLevel;
	private GUIText msgPoints;

	public UnityRandom urand;

	public int winterFrequency;

#endregion

	void Awake(){
		g = this;
	}

	void Start () {
		urand = new UnityRandom();
		sequence = new List<Piece>();
		connectionLine = GetComponent<LineRenderer>();
		UpdateAllPieces();
		msgPoints = txtPoints.GetComponent<GUIText>();
		msgLevel = txtLevel.GetComponent<GUIText>();
		turns = 0;
	}

#region Updates
	void Update(){
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
		// FIXME cancel sequences
		if(SeqCount() > 0 && Input.GetMouseButtonUp(1)){
			RemovePieceFromSeq(sequence[0]);
			UpdateAllPieces();
			ResetAllPieces();
		}
		// GUI update
		msgPoints.text = "Points: " + points;
		if(sequence.Count == 0) cursorValue = 0;
		//if(LosingConditions()) moveLock = true;
	}

	public void UpdateAllPieces(){
		allPieces = new List<Piece>();
		Object[] allGO = GameObject.FindObjectsOfType(typeof(Piece));
		foreach (var go in allGO) {
			allPieces.Add(go as Piece);
			//(go as Piece).onCaptureArea = false;
		}
	}

	public void ResetAllPieces(){
		foreach (var item in allPieces) {
			item.isClicked = false;
			item.onCaptureArea = false;
		}
	}

	void UpdateClicked ()
	{
		allPieces.ForEach(p => {
			p.onCaptureArea = false;
			p.isClicked = isInSeq(p);
		});
	}

	public void UpdateConnectionLine ()
	{
		connectionLine.SetVertexCount(sequence.Count+1);
		Vector2[] points = new Vector2[sequence.Count];
		for (int i = 0; i < sequence.Count; i++) {
			connectionLine.SetPosition(i, sequence[i].transform.position);
			/*
			points[i] = new Vector2(
				sequence[i].transform.position.x,
				sequence[i].transform.position.y);
			*/
		}
		connectionLine.SetPosition(sequence.Count, sequence[sequence.Count-1].transform.position);

	}

#endregion

#region Sequence handling
	public int SeqCount(){
		return sequence.Count;
	}

	public bool isInSeq(Piece piece){
		return (sequence.FindIndex(p => p == piece) >= 0);
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
		if(indexToRemove >= 0) sequence.RemoveRange(indexToRemove, SeqCount()-indexToRemove);
		UpdateClicked();
	}

	public IEnumerator CombineSeq(int value){
		moveLock = true;
		int newValue = SeqCount() + 1;
		Debug.Log(newValue);
		Slot spawnSlot = sequence[0].slot;
		Vector3 spawnPos = spawnSlot.position;
		yield return new WaitForSeconds(completionDelay);
		UpdateAllPieces();
		allPieces.ForEach(p => {
			Piece piece = p.GetComponent<Piece>();
			if(piece.isClicked){
				p.slot.full = false;
				DestroyObject(p.gameObject);
				Destroy (p);
			}
			piece.isClicked = false;
		});
		Transform combinedPiece = 
			Instantiate (piecePrefab) as Transform;
		spawnSlot.piece = combinedPiece.GetComponent<Piece>();
		spawnSlot.piece.transform.position = spawnSlot.position;
		spawnSlot.full = true;
		spawnSlot.piece.slot = spawnSlot;
		spawnSlot.piece.value = newValue;
		sequence.Clear();
		allPieces = null;
		yield return new WaitForSeconds(0);
		UpdateAllPieces();
		ResetAllPieces();
		moveLock = false;
		AdvanceTurn();
	}

	public IEnumerator ReproSeq(int value, int number){
		int oldPoints = points;
		int cursor = cursorValue;
		moveLock = true;
		Slot spawnSlot = sequence[0].slot;
		Vector3 spawnPos = spawnSlot.position;
		yield return new WaitForSeconds(completionDelay);
		int newValue;
		if(value == 0) {
			newValue = sequence[0].GetComponent<Piece>().value + 1;
		}
		else {
			newValue = value;
		}
		for (int i = 0; i < number; i++) {
			Transform combinedPiece = 
				Instantiate (piecePrefab) as Transform;
			spawnSlot.piece = combinedPiece.GetComponent<Piece>();
			spawnSlot.piece.transform.position = spawnSlot.position;
			spawnSlot.full = true;
			spawnSlot.piece.slot = spawnSlot;
			spawnSlot.piece.value = newValue;
		}
		sequence.Clear();
		allPieces = null;
		yield return new WaitForSeconds(0);
		UpdateAllPieces();
		ResetAllPieces();
		moveLock = false;
		AdvanceTurn();
	}
#endregion

#region Game flow

	public void AdvanceTurn(){
		turns++;
		msgLevel.text = "Turn count: " + turns;
		if(turns % winterFrequency == 0){
			StartWinter();
			msgLevel.text += " WINTER!!!!";
		}
	}

	public void StartWinter(){
		foreach (var p in allPieces) {
			if(p.value == 1){
				p.slot.full = false;
				DestroyObject(p.gameObject);
				Destroy (p);
			}
		}
		UpdateAllPieces();
	}

	public bool LosingConditions(){
		bool lose = true;
		UpdateAllPieces();
		List<int> pieceValues = new List<int>();
		foreach (var item in allPieces) {
			pieceValues.Add(item.value);
		}
		int maxValue = Mathf.Max(pieceValues.ToArray());
		Debug.Log("Max: " + maxValue);
		if(Mathf.Max(pieceValues.ToArray()) >= 3){
			lose = false;
			if(maxValue == 3 && pieceValues.FindAll(v => v == maxValue).Count <= 2){
				lose = true;
			}
		}
		if(allPieces.Count == 1){
			lose = true;
			msgLevel.text = "You win!";
		}
		else if (lose){
			msgLevel.text = "Game over!";
		}
		return lose;
	}

#endregion
}

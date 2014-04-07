using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PieceManager : MonoBehaviour {

	public static PieceManager g;
	public int startingNumber;
	public List<Piece> sequence;
	public List<Piece> all;
	public Transform piecePrefab;
	public int cursorValue;


	private LineRenderer connectionLine;

	void Awake() 
	{
		g = this;
	}
	
	void Start () 
	{
		sequence = new List<Piece>();
		all = new List<Piece>();
		connectionLine = GetComponent<LineRenderer>();
		PopulateScene();
	}

	public void PopulateScene() {
		for (int i = 0; i < startingNumber; i++) {
			Transform newPiece = Instantiate(piecePrefab) 
				as Transform;
			newPiece.position = GameManager.g.urand.PointInACube() * 2;
			newPiece.GetComponent<Piece>().value = 2 - i%2;
		}
	}

	public void ReloadPiecesFromScene() 
	{
		all = null;
		all = new List<Piece>();
		Object[] allGO = GameObject.FindObjectsOfType(typeof(Piece));
		foreach (var go in allGO) {
			all.Add(go as Piece);
		}
	}

	public void ResetAllClicked() 
	{
		if(sequence.Count == 0) cursorValue = 0;
		foreach (var item in all) {
			item.isClicked = false;
		}
	}
	
	public bool isInSequence(Piece piece){
		return (sequence.FindIndex(p => p == piece) >= 0);
	}
	
	public bool AddPieceToSequence(Piece piece){
		int index = sequence.FindIndex(p => p == piece);
		if (index < 0) {
			sequence.Add(piece);
			return true;
		}
		return false;
	}
	
	public void RemovePieceFromSequence(Piece piece){
		Debug.Log ("remove piece");
		int indexToRemove = sequence.FindIndex(p => p == piece);
		if(indexToRemove >= 0) sequence.RemoveRange(indexToRemove, sequence.Count-indexToRemove);
		ResetAllClicked();
	}

	public IEnumerator CombineSequence(int value){
		GameManager.g.moveLock = true;
		int newValue = sequence.Count + 1;
		Slot spawnSlot = sequence[0].slot;
		Vector3 spawnPos = spawnSlot.position;
		yield return new WaitForSeconds(ConnectionLines.g.completionDelay);
		all.ForEach(p => {
			if(p.isClicked){
				p.slot.full = false;
				DestroyObject(p.gameObject);
				Destroy (p);
			}
			p.isClicked = false;
		});
		// FIXME trocar quando tivermos prefabs diferentes
		Transform combinedPiece = 
			Instantiate (piecePrefab) as Transform; 
		spawnSlot.piece = combinedPiece.GetComponent<Piece>();
		spawnSlot.piece.transform.position = spawnSlot.position;
		spawnSlot.full = true;
		spawnSlot.piece.slot = spawnSlot;
		spawnSlot.piece.value = newValue;
		sequence.Clear();
		yield return new WaitForSeconds(0);
		all = null;
		ReloadPiecesFromScene();
		ResetAllClicked();
		GameManager.g.moveLock = false;
		GameManager.g.AdvanceTurn();
	}
	
	public IEnumerator ReproSequence(int value, int number){

		GameManager.g.moveLock = true;
		Slot spawnSlot = sequence[0].slot;
		Vector3 spawnPos = spawnSlot.position;
		yield return new WaitForSeconds(ConnectionLines.g.completionDelay);
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
		yield return new WaitForSeconds(0);
		all = null;
		ReloadPiecesFromScene();
		ResetAllClicked();
		GameManager.g.moveLock = false;
		GameManager.g.AdvanceTurn();
	}
}

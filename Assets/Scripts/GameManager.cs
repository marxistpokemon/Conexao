using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using URandom;

public enum Seasons {
	SPRING = 0,
	SUMMER = 1,
	FALL = 2,
	WINTER = 3
}

public class GameManager : MonoBehaviour {

#region Vars
	public static GameManager g;

	public int level;
	public int points;
	public int turns;
	public Seasons season;

	public int cursorValue = 0;
	public List<Piece> sequence;
	public List<Piece> pieces;

	public float completionDelay = 1f;
	[HideInInspector] public bool moveLock;

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
		msgPoints = txtPoints.GetComponent<GUIText>();
		msgLevel = txtLevel.GetComponent<GUIText>();
		turns = 0;
	}

#region Updates
	void Update(){
		// FIXME cancel sequences
		if(PieceManager.g.sequence.Count > 0 && Input.GetMouseButtonUp(1)){
			PieceManager.g.RemovePieceFromSequence(PieceManager.g.sequence[0]);
			// reset
		}
		// GUI update
		msgPoints.text = "Points: " + points;

		//if(LosingConditions()) moveLock = true;
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
		foreach (var p in PieceManager.g.all) {
			if(p.value == 1){
				p.slot.full = false;
				DestroyObject(p.gameObject);
				Destroy (p);
			}
		}
		PieceManager.g.ReloadPiecesFromScene();
	}

	public bool LosingConditions(){
		bool lose = true;
		List<int> pieceValues = new List<int>();
		foreach (var item in pieces) {
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
		if(pieces.Count == 1){
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

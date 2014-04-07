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

	public bool moveLock;

	public int level; // max animal
	public int points;// total de vida
	public int turns; // cada acao realizada

	public Seasons season;
	public int winterFrequency;

	// Gui
	public Transform txtLevel;
	public Transform txtPoints;
	private GUIText msgLevel;
	private GUIText msgPoints;

	public UnityRandom urand;

#endregion

	void Awake(){
		g = this;
		urand = new UnityRandom();
	}

	void Start () {
		msgPoints = txtPoints.GetComponent<GUIText>();
		msgLevel = txtLevel.GetComponent<GUIText>();
		turns = 1;
	}

	void Update(){
		// FIXME cancelar sequences tem que ir pro PM?
		if(PieceManager.g.sequence.Count > 0 && Input.GetMouseButtonUp(1)){
			PieceManager.g.RemovePieceFromSequence(PieceManager.g.sequence[0]);
			// reset
		}
		// GUI update
		msgPoints.text = "Points: " + points;

		if(CheckGameOver() && turns > 1) moveLock = true;
	}

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

	public bool CheckGameOver(){
		bool lose = false;
		points = 0;
		List<int> pieceValues = new List<int>();
		foreach (var item in PieceManager.g.all) {
			pieceValues.Add(item.value);
			points += item.value;
		}
		level = Mathf.Max(pieceValues.ToArray());
		if(pieceValues.FindAll(v => v == 2).Count < 2){
			lose = true;
		}
		if(PieceManager.g.all.Count == 1 || lose){
			msgLevel.text = "Game over!";
		}
		return lose;
	}

}

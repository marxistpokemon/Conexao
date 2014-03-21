using UnityEngine;
using System.Collections;

public class Slot {

	public Vector2 position;
	public bool full;
	public Piece piece;

	public Slot(){
		position = new Vector2();
		full = false;
		piece = null;
	}
}

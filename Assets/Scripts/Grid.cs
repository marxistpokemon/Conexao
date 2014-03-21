using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Grid : MonoBehaviour {

	public static Grid g;

	public int initialQuantity = 0;
	public int width;
	public int height;
	public float pieceWidth;
	public float pieceHeight;
	[HideInInspector] public Slot[,] slots;
	[HideInInspector] public List<Slot> list;
	float minX;
	float minY;

	void Awake(){
		g = this;
	}

	// Use this for initialization
	void Start () {
		minX = - (width * pieceWidth) / 2 + pieceWidth/2; 
		minY = - (height * pieceHeight) /2 + pieceHeight/2;
		for (int i = 0; i < initialQuantity; i++) {
			Transform newPiece = Instantiate(Resources.Load<Transform>("Piece")) 
				as Transform;
		}
		GameManager.g.UpdateAllPieces();
		slots = new Slot[width,height]; // hardcoded numbers
		for (int col = 0; col < slots.GetLength(0); col++) {
			for (int row = 0; row < slots.GetLength(1); row++) {
				slots[col, row] = new Slot();
				slots[col, row].full = false;
				slots[col, row].position = new Vector2(
					minX + col*pieceWidth,
					minY + row*pieceHeight);
				list.Add(slots[col, row]);
			}
		}
		foreach(var piece in GameManager.g.allPieces){
			bool done = false;
			Slot newSlot;
			do {
				newSlot = slots[Random.Range(0, slots.GetLength(0)),
				                Random.Range(0, slots.GetLength(1))];
				if(!newSlot.full){
					newSlot.piece = piece.GetComponent<Piece>();
					newSlot.full = true;
					newSlot.piece.transform.position = new Vector3(
						newSlot.position.x,
						newSlot.position.y,
						0);
					newSlot.piece.slot = newSlot;
					newSlot.piece.value = 3;
					done = true;
				}
			} while (!done);
		}
	}

	public bool SpawnPiece(int pValue){
		Slot[] availableSlots = FindEmptySlots();
		if(availableSlots.Length > 0){
			Transform newPiece = Instantiate(Resources.Load<Transform>("Piece")) 
				as Transform;
			newPiece.GetComponent<Piece>().value = pValue;
			int rand = Random.Range(0, availableSlots.Length-1);
			availableSlots[rand].piece = newPiece.GetComponent<Piece>();
			availableSlots[rand].piece.transform.position = 
				availableSlots[rand].position;
			availableSlots[rand].full = true;
			availableSlots[rand].piece.slot = availableSlots[rand];
			GameManager.g.UpdateAllPieces();
			return true;
		}
		else {
			return false;
		}
	}

	public Slot[] FindEmptySlots(){
		return list.FindAll(s => s.full == false).ToArray();
	}

	public Slot[] FindFullSlots(){
		return list.FindAll(s => s.full == true).ToArray();
	}
}

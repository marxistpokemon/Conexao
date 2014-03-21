using UnityEngine;
using System.Collections;

public class PieceSpawner : MonoBehaviour {

	public int initialQuantity = 0;
	public float baseWidth;
	public float baseHeight;
	private UnityRandom urand;
	public Slot[,] slots;

	public float camWidth;
	public float camHeight;
	public float minX;
	float maxX;
	public float minY;
	float maxY;

	// Use this for initialization
	void Start () {
		camHeight = Camera.main.orthographicSize;
		camWidth = camHeight * Screen.width/Screen.height;
		minX = -camWidth/2; 
		minY = -camHeight/2;

		for (int i = 0; i < initialQuantity; i++) {
			Transform newPiece = Instantiate(Resources.Load<Transform>("Piece")) 
				as Transform;
		}
		GameManager.g.UpdateAllPieces();
		slots = new Slot[5,4]; // hardcoded numbers
		for (int col = 0; col < slots.GetLength(0); col++) {
			for (int row = 0; row < slots.GetLength(1); row++) {
				slots[col, row] = new Slot();
				slots[col, row].full = false;
				slots[col, row].position = new Vector2(
					minX + col*baseWidth,
					minY + row*baseHeight);
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
					done = true;
				}
			} while (!done);
		}
	}
}

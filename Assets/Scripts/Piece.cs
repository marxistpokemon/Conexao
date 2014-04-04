using UnityEngine;
using System.Collections;


public class Piece : MonoBehaviour {

	public int value = 0;
	public Sprite[] pieceGraphics;
	public bool isClicked = false;
	public bool onCaptureArea = false;
	public Slot slot = null;
	private SpriteRenderer spriteRenderer;
	public GUIText txt;

	void Start(){
		if(value == 0 ) value = Random.Range(1, 3);
		isClicked = false;
		spriteRenderer = GetComponent<SpriteRenderer>();
	}

	void Update(){
		//spriteRenderer.sprite = pieceGraphics[value];
		txt.text = value + "";
		txt.transform.position = Camera.main.WorldToViewportPoint(transform.position);
		renderer.material.color = (value == 1)? Color.green : Color.white;
		renderer.material.color = (isClicked)? Color.yellow : renderer.material.color;
	}

	void FixedUpdate(){
		if(value > 1){
			rigidbody2D.mass = value;
			rigidbody2D.AddForce(new Vector2(Random.Range(-2f, 2f),
			                                 Random.Range(-2f, 2f))*value);
		}
	}

	void OnMouseDown(){
		Debug.Log("Click : " + gameObject.name);
		if(!GameManager.g.moveLock && value > 0){
			if(!isClicked){
				// se primeiro da sequencia
				if(GameManager.g.cursorValue == 0){
					GameManager.g.cursorValue = value;
					GameManager.g.AddPieceToSeq(this);
					isClicked = true;
				}
				// se captura / comer
				else if(GameManager.g.cursorValue  == value + 1){
					GameManager.g.AddPieceToSeq(this);
					GameManager.g.cursorValue = value;
					if(GameManager.g.cursorValue == 1){
						GameManager.g.StartCoroutine(GameManager.g.CombineSeq(0));
					}
					else {
						isClicked = true;
					}
				}
				// se reproducao
				else if(GameManager.g.cursorValue  == value){
					GameManager.g.AddPieceToSeq(this);
					GameManager.g.StartCoroutine(GameManager.g.ReproSeq(value, 1));
				}
			}
			else {
				if(this == GameManager.g.sequence[0] && value > 1){
					Split ();
				}
				else {
					GameManager.g.RemovePieceFromSeq(this);
				}

			}
		}
	}

	void OnTriggerEnter2D(Collider2D info){
		if(info.gameObject.GetComponent<PolygonCollider2D>() != null){
			if(!isClicked) onCaptureArea = true;
		}
	}

	void OnTriggerExit2D(Collider2D info){
		if(info.gameObject.GetComponent<PolygonCollider2D>() != null){
			if(!isClicked) onCaptureArea = false;
		}
	}

	void OnDestroy(){
		Destroy(gameObject);
	}

	void Split(){
		GameManager.g.RemovePieceFromSeq(this);
		GameManager.g.allPieces.Remove(this);
		Destroy(this.gameObject);
		for (int i = 0; i < value; i++) {
			Transform newPiece1 = Instantiate(Resources.Load<Transform>("Piece")) 
				as Transform;
			newPiece1.transform.position = transform.position + (Vector3)GameManager.g.urand.PointInADisk()*0.5f;
			int v1 = 1;
			newPiece1.GetComponent<Piece>().value = v1;
		}
		GameManager.g.UpdateAllPieces();
		GameManager.g.ResetAllPieces();
		GameManager.g.AdvanceTurn();
	}
}

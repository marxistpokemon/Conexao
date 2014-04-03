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
		renderer.material.color = (isClicked)? Color.yellow : Color.white;
		renderer.material.color = (onCaptureArea)? Color.red : renderer.material.color;
	}

	void FixedUpdate(){
		rigidbody2D.mass = value;
		rigidbody2D.AddForce(new Vector2(Random.Range(-2f, 2f),
		                                 Random.Range(-2f, 2f))*value);
	}

	void OnMouseDown(){
		Debug.Log("Click : " + gameObject.name);
		if(!GameManager.g.moveLock && value > 0){
			if(!isClicked){
				// se primeiro da sequencia
				if(GameManager.g.cursorValue == 0 && value > 2){
					GameManager.g.cursorValue = value;
					GameManager.g.AddPieceToSeq(this);
					isClicked = true;
				}
				else if(GameManager.g.cursorValue == 0 && value <= 2){
					Split ();
				}
				else if(GameManager.g.cursorValue == value){
					GameManager.g.AddPieceToSeq(this);
					isClicked = true;
				}

				if(GameManager.g.sequence.Count == GameManager.g.cursorValue && value > 2){
					GameManager.g.StartCoroutine("CompleteSeq", transform);
				}

			}
			else {

				if(this == GameManager.g.sequence[0] &&
				   GameManager.g.SeqCount() > 2){
					GameManager.g.StartCoroutine("CompleteSeq", transform);
				}
				else if(this == GameManager.g.sequence[0]){
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
		GameManager.g.points += value;
		if(value >= 4){
			Transform newPiece1 = Instantiate(Resources.Load<Transform>("Piece")) 
				as Transform;
			Transform newPiece2 = Instantiate(Resources.Load<Transform>("Piece")) 
				as Transform;
			newPiece1.transform.position = transform.position + (Vector3)GameManager.g.urand.PointInADisk()*0.5f;
			newPiece2.transform.position = transform.position + (Vector3)GameManager.g.urand.PointInADisk()*0.5f;
			int v1 = Random.Range(3, value);
			int v2 = value - v1;
			newPiece1.GetComponent<Piece>().value = v1;
			newPiece2.GetComponent<Piece>().value = v2;
		}
		GameManager.g.UpdateAllPieces();
		GameManager.g.ResetAllPieces();
	}
}

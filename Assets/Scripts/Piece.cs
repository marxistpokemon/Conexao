using UnityEngine;
using System.Collections;


public class Piece : MonoBehaviour {

	public int value = 0;
	public Sprite[] pieceGraphics;
	public bool isClicked = false;
	public bool onCaptureArea = false;
	public Slot slot = null;
	private SpriteRenderer spriteRenderer;

	void Start(){
		if(value == 0 ) value = Random.Range(1, 3);
		isClicked = false;
		spriteRenderer = GetComponent<SpriteRenderer>();
	}

	void Update(){
		spriteRenderer.sprite = pieceGraphics[value];
		renderer.material.color = (isClicked)? Color.yellow : Color.white;
		renderer.material.color = (onCaptureArea)? Color.red : renderer.material.color;
	}

	void OnMouseDown(){
		Debug.Log("Click : " + gameObject.name);
		if(!GameManager.g.moveLock && value > 2){
			if(!isClicked){
				// se primeiro da sequencia
				if(GameManager.g.cursorValue == 0){
					GameManager.g.cursorValue = value;
					GameManager.g.AddPieceToSeq(this);
					isClicked = true;
				}
				else if(GameManager.g.cursorValue == value){
					GameManager.g.AddPieceToSeq(this);
					isClicked = true;
				}
				if(GameManager.g.sequence.Count == GameManager.g.cursorValue){
					GameManager.g.StartCoroutine("CompleteSeq", transform);
				}
			}
			else {
				if(this == GameManager.g.sequence[0] &&
				   GameManager.g.SeqCount() > 2){
					GameManager.g.StartCoroutine("CompleteSeq", transform);
				}
				else {
					GameManager.g.RemovePieceFromSeq(this);
				}
			}
		}
	}

	void OnTriggerEnter2D(Collider2D info){
		if(info.gameObject.GetComponent<PolygonCollider2D>() != null){
			onCaptureArea = !isClicked;
		}
	}

	void OnDestroy(){
		Destroy(gameObject);
	}
}

using UnityEngine;
using System.Collections;

public class Piece : MonoBehaviour {

	public int value = 0;
	public Sprite[] pieceGraphics;
	public bool isClicked = false;
	public bool onCaptureArea = false;

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
		if(!GameManager.g.moveLock){
			if(!isClicked){
				GameManager.g.AddPieceToSeq(transform);
				isClicked = true;
			}
			else {
				if(transform == GameManager.g.pieceSequence[0] &&
				   GameManager.g.SeqCount() > 2){
					GameManager.g.StartCoroutine("CompleteSeq", transform);
				}
				else {
					GameManager.g.RemovePieceFromSeq(transform);
				}
			}
		}
	}

	void OnTriggerEnter2D(Collider2D info){
		if(info.gameObject.GetComponent<PolygonCollider2D>() != null){
			onCaptureArea = !isClicked;
		}
	}
}

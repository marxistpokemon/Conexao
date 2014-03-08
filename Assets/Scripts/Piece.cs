using UnityEngine;
using System.Collections;

public class Piece : MonoBehaviour {

	public int value = 0;
	public bool isClicked = false;
	public bool onCaptureArea = false;

	void Start(){
		isClicked = false;
	}

	void Update(){

		renderer.material.color = (isClicked)? Color.yellow : Color.white;
		renderer.material.color = (onCaptureArea)? Color.red : renderer.material.color;
	}

	void OnMouseDown(){
		Debug.Log("Click : " + gameObject.name);
		if(!GameManager.g.moveLock){
			if(!isClicked){
				GameManager.g.AddPiece(transform);
			}
			else {
				if(GameManager.g.SequenceCount() > 0){
					if(transform == GameManager.g.pieceSequence[0] &&
					   GameManager.g.SequenceCount() > 2){
						GameManager.g.StartCoroutine("CompleteSequence", transform);
					}
					else {
						GameManager.g.RemovePiece(transform);
						isClicked = false;
					}
				}
			}
		}
	}

	void OnTriggerEnter2D(Collider2D info){
		Debug.Log("Colisao");
		if(info.gameObject.GetComponent<PolygonCollider2D>() != null){
			Debug.Log("Colisao PolCollider");
			onCaptureArea = true;
		}
	}
}

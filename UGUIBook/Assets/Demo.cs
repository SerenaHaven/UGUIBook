using UnityEngine;

public class Demo : MonoBehaviour {
	void Start () {
		UGUIBook book = GameObject.FindObjectOfType<UGUIBook> ();
		Texture2D texture1 = Resources.Load<Texture2D> ("Page1");
		Texture2D texture2 = Resources.Load<Texture2D> ("Page2");
		book.SetCurrentTexture (texture1);
		book.SetNextTexture (texture2);
	}
}
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine;
using System.Collections;
using UnityEngine.Events;

[RequireComponent(typeof(EventTrigger))]
public class UGUIBook : RawImage, IBeginDragHandler, IEndDragHandler, IDragHandler
{
	public UnityAction TurnToNextPageFinishEvent;
	public UnityAction TurnToPreviousPageFinishEvent;

	private UGUIBookNextPage nextPage = null;
	private Vector2 offset;
	private Vector2 position;

	public enum State {Idle, ManualTurning, AutoTurning}
	private State state = State.Idle;

	public void SetInteractable(bool interactable)
	{
		raycastTarget = interactable;
	}

	public void SetCurrentTexture(Texture2D texture2D)
	{
		texture = texture2D;
	}

	public void SetNextTexture(Texture2D texture2D)
	{
		nextPage.texture = texture2D;
	}

	public void TurnToNextPage()
	{
	}

	public void TurnToPreviousPage()
	{
	}

	protected override void OnEnable()
	{
		base.OnEnable ();
		nextPage = this.GetComponentInChildren<UGUIBookNextPage> ();
		if (nextPage == null) {
			GameObject gameObjectNextPage = new GameObject ("NextPage");
			gameObjectNextPage.transform.SetParent (transform);
			nextPage = gameObjectNextPage.AddComponent<UGUIBookNextPage> ();
		}
		RectTransform rectTransformNextPage = nextPage.GetComponent<RectTransform> () ?? nextPage.gameObject.AddComponent<RectTransform> ();
		rectTransformNextPage.pivot = Vector2.one * 0.5f;
		rectTransformNextPage.anchorMin = Vector2.zero;
		rectTransformNextPage.anchorMax = Vector2.one;
		rectTransformNextPage.offsetMin = Vector2.zero;
		rectTransformNextPage.offsetMax = Vector2.zero;
		rectTransformNextPage.localScale = Vector3.one;
		rectTransformNextPage.localRotation = Quaternion.identity;
		rectTransformNextPage.SetAsLastSibling ();
	}

	protected override void OnRectTransformDimensionsChange ()
	{
		base.OnRectTransformDimensionsChange ();
		offset = new Vector2 (rectTransform.rect.width * (rectTransform.pivot.x - 0.5f), rectTransform.rect.height * (rectTransform.pivot.y - 0.5f));
	}

	public void OnBeginDrag (PointerEventData eventData)
	{
		if (state != State.Idle) {
			return;
		}
		RectTransformUtility.ScreenPointToLocalPointInRectangle (this.rectTransform, eventData.position, canvas.worldCamera, out position);
		nextPage.SetTouchPoint (position + offset);
		state = State.ManualTurning;
	}

	public void OnDrag (PointerEventData eventData)
	{
		if (state != State.ManualTurning) {
			return;
		}
		RectTransformUtility.ScreenPointToLocalPointInRectangle (this.rectTransform, eventData.position, canvas.worldCamera, out position);
		nextPage.SetTargetPoint (position + offset);
	}

	public void OnEndDrag (PointerEventData eventData)
	{
		state = State.AutoTurning;
		nextPage.Finish (
			()=>{
				state = State.Idle;
			});
	}
}
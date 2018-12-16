using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System;
using System.Collections;

public class UGUIBookNextPage : RawImage
{
	[Range(1.0f, 100.0f)]
	[SerializeField]private float damping = 10.0f;

	private Vector2 touchPoint = Vector2.zero;
	private Vector2 targetPoint = Vector2.zero;
	private Vector2 mirrorPoint;

	private Vector3 offset;
	private float halfWidth;
	private float halfHeight;

	private Vector2 topRight;
	private Vector2 topLeft;
	private Vector2 bottomLeft;
	private Vector2 bottomRight;

	private Vector2[] edgePoints = new Vector2[4];
	private Vector2?[] intersections = new Vector2?[4];
	private Vector2[] clampedIntersections = new Vector2[3];
	private Vector2 centerPoint;
	private Vector2 direction;
	private Vector2 normalPoint;

	private Vector3[] vertices = new Vector3[8];
	protected UGUIBookNextPage ()
	{
		useLegacyMeshGeneration = false;
	}

	public void SetTouchPoint(Vector2 touchPoint)
	{
		StopAllCoroutines ();
		this.touchPoint = touchPoint;
		OnValidate ();
	}

	public void SetTargetPoint(Vector2 targetPoint)
	{
		StopAllCoroutines ();
		this.targetPoint = targetPoint;
		OnValidate ();
	}

	public void Finish(Action onFinish)
	{
		StopAllCoroutines ();
		StartCoroutine (FinishEnumerator (onFinish));
	}

	private IEnumerator FinishEnumerator(Action onFinish){
		touchPoint = vertices [2];
		targetPoint = vertices [6];
		while (Vector2.Distance (targetPoint, touchPoint) > 0.1f) {
			targetPoint = Vector2.Lerp (targetPoint, touchPoint, Time.deltaTime * damping);
			OnValidate ();
			yield return 0;
		}
		targetPoint = touchPoint;
		OnValidate ();
		yield return 0;
		if (onFinish != null) {
			onFinish ();
		}
	}

	protected override void OnRectTransformDimensionsChange ()
	{
		base.OnRectTransformDimensionsChange ();
		offset = new Vector3 (rectTransform.rect.width * (rectTransform.pivot.x - 0.5f), rectTransform.rect.height * (rectTransform.pivot.y - 0.5f), 0.0f);
		halfWidth = rectTransform.rect.width * 0.5f;
		halfHeight = rectTransform.rect.height * 0.5f;

		topRight = new Vector2 (halfWidth, halfHeight);
		topLeft = new Vector2 (-halfWidth, halfHeight);
		bottomLeft = new Vector2 (-halfWidth, -halfHeight);
		bottomRight = new Vector2 (halfWidth, -halfHeight);
	}

	protected override void OnPopulateMesh (VertexHelper vh)
	{
		vh.Clear ();
		if (rectTransform.rect.width == 0.0f || rectTransform.rect.height == 0.0f
			|| touchPoint.x == 0.0f
			|| touchPoint.x == targetPoint.x
		    || (touchPoint.x > 0 && (targetPoint.x > touchPoint.x))
		    || (touchPoint.x < 0 && (targetPoint.x < touchPoint.x))) {
			return;
		}	    

		float xMin = 0.0f;
		float xMax = 0.0f;
		if (touchPoint.x > 0) {
			edgePoints [0] = topRight;
			edgePoints [1] = topLeft;
			edgePoints [2] = bottomLeft;
			edgePoints [3] = bottomRight;
			xMax = halfWidth;
		} else {
			edgePoints [0] = topLeft;
			edgePoints [1] = topRight;
			edgePoints [2] = bottomRight;
			edgePoints [3] = bottomLeft;
			xMin = -halfWidth;
		}

		if (CheckIntersection (touchPoint, targetPoint, edgePoints [0], edgePoints [3], out intersections [3]) == true) {
			touchPoint.Set (intersections [3].Value.x, Mathf.Clamp (intersections [3].Value.y, -halfHeight, halfHeight));
		}
		centerPoint = Vector2.Lerp (touchPoint, targetPoint, 0.5f);
		direction = touchPoint - targetPoint;
		direction.Set (direction.y, -direction.x);
		normalPoint = centerPoint + direction;

		if (CheckIntersection (centerPoint, normalPoint, edgePoints [0], edgePoints [1], out intersections [0]) == true) {
			clampedIntersections [0] = new Vector2 (Mathf.Clamp (intersections [0].Value.x, xMin, xMax), intersections [0].Value.y);
		}		
		if (CheckIntersection (centerPoint, normalPoint, edgePoints [2], edgePoints [3], out intersections [1]) == true) {
			clampedIntersections [1] = new Vector2 (Mathf.Clamp (intersections [1].Value.x, xMin, xMax), intersections [1].Value.y);
		}		
		if (CheckIntersection (centerPoint, normalPoint, edgePoints [0], edgePoints [3], out intersections [2]) == true) {
			clampedIntersections [2] = new Vector2 (intersections [2].Value.x, Mathf.Clamp (intersections [2].Value.y, -halfHeight, halfHeight));
		}

		if (intersections [2] != null) {
			if ((intersections [0].Value.x - intersections [2].Value.x) * (intersections [1].Value.x - intersections [2].Value.x) >= 0.0f) {
				if (Mathf.Abs (clampedIntersections [0].x) <= Mathf.Abs (clampedIntersections [1].x)) {
					vertices [0] = vertices [4] = clampedIntersections [0];
					vertices [1] = vertices [5] = clampedIntersections [1];
					vertices [2] = edgePoints [0];
					vertices [3] = edgePoints [3];
				} else {
					vertices [0] = vertices [4] = clampedIntersections [1];
					vertices [1] = vertices [5] = clampedIntersections [0];
					vertices [2] = edgePoints [3];
					vertices [3] = edgePoints [0];
				}
			} else {//竖边交点有效
				vertices [1] = vertices [3] = vertices [5] = vertices [7] = clampedIntersections [2];
				if ((intersections [2].Value.x - intersections [0].Value.x) * intersections [2].Value.x >= 0) {
					vertices [0] = vertices [4] = clampedIntersections [0];
					vertices [2] = edgePoints [0];
				} else {
					vertices [0] = vertices [4] = clampedIntersections [1];
					vertices [2] = edgePoints [3];
				}
			}
		} else {
			vertices [0] = vertices [4] = clampedIntersections [0];
			vertices [1] = vertices [5] = clampedIntersections [1];
			vertices [2] = edgePoints [0];
			vertices [3] = edgePoints [3];
		}

		direction = vertices [0] - vertices [1];
		direction.Set (direction.y, -direction.x);
		if (CheckIntersection (vertices [0], vertices [1], vertices [2], new Vector2 (vertices [2].x, vertices [2].y) + direction, out intersections [3]) == true) {
			vertices [6] = Vector2.LerpUnclamped (vertices [2], intersections [3].Value, 2.0f);
		}
		if (CheckIntersection (vertices [0], vertices [1], vertices [3], new Vector2 (vertices [3].x, vertices [3].y) + direction, out intersections [3]) == true) {
			vertices [7] = Vector2.LerpUnclamped (vertices [3], intersections [3].Value, 2.0f);
		}
		if (CheckIntersection (vertices [0], vertices [1], touchPoint, touchPoint + direction, out intersections [3]) == true) {
			mirrorPoint = Vector2.LerpUnclamped (touchPoint, intersections [3].Value, 2.0f);
		}

		for (int i = 0; i < 4; i++) {
			vertices [i] -= offset;
			vh.AddVert (vertices [i], new Color (0.9f, 0.9f, 0.9f, 1), 
				new Vector2 (vertices [i].x / rectTransform.rect.width + rectTransform.pivot.x, vertices [i].y / rectTransform.rect.height + rectTransform.pivot.y));
		}
		for (int i = 4; i < 8; i++) {
			vertices [i] -= offset;
			vh.AddVert (vertices [i], color, 
				new Vector2 (1.0f - vertices [i - 4].x / rectTransform.rect.width - rectTransform.pivot.x, vertices [i - 4].y / rectTransform.rect.height + rectTransform.pivot.y));
		}
		if ((vertices[2].x == topRight.x && vertices[2].y == topRight.y) || (vertices[2].x == bottomLeft.x) && (vertices[2].y == bottomLeft.y)) {
			vh.AddTriangle (0, 2, 1);
			vh.AddTriangle (1, 2, 3);
			vh.AddTriangle (4, 5, 6);
			vh.AddTriangle (6, 5, 7);
		} else {
			vh.AddTriangle (0, 1, 2);
			vh.AddTriangle (1, 3, 2);
			vh.AddTriangle (4, 6, 5);
			vh.AddTriangle (6, 7, 5);
		}
	}

	public bool CheckIntersection (Vector2 a, Vector2 b, Vector2 c, Vector2 d, out Vector2? intersection)
	{
		float deno = (b.y - a.y) * (d.x - c.x) - (a.x - b.x) * (c.y - d.y);
		if (deno == 0) {
			intersection = null;
			return false;
		}
		Vector2 result = new Vector2 ();
		result.x = ((b.x - a.x) * (d.x - c.x) * (c.y - a.y) + (b.y - a.y) * (d.x - c.x) * a.x - (d.y - c.y) * (b.x - a.x) * c.x) / deno;
		result.y = -((b.y - a.y) * (d.y - c.y) * (c.x - a.x) + (b.x - a.x) * (d.y - c.y) * a.y - (d.x - c.x) * (b.y - a.y) * c.y) / deno;
		intersection = new Vector2? (result);
		return true;
	}

	#if UNITY_EDITOR
	void OnDrawGizmos ()
	{
		DrawLine (topRight, topLeft);
		DrawLine (topRight, bottomRight);
		DrawLine (bottomLeft, topLeft);
		DrawLine (bottomLeft, bottomRight);

		DrawPoint (touchPoint, Color.red);
		DrawPoint (targetPoint, Color.red);
		DrawPoint (mirrorPoint, Color.red);
		DrawPoint (centerPoint, Color.red);

		DrawLine (touchPoint, targetPoint);

		for (int i = 0; i < intersections.Length; i++) {
			if (intersections [i] != null) {
				DrawPoint (intersections [i].Value, Color.green);
			}
		}
		for (int i = 0; i < clampedIntersections.Length; i++) {
			DrawPoint (clampedIntersections [i], Color.blue);
		}
	}

	void DrawPoint(Vector2 point, Color color, float radius = 20.0f)
	{
		Color temp = Gizmos.color;
		Gizmos.color = color;
		Gizmos.DrawWireSphere (transform.TransformPoint (point), radius);
		Gizmos.color = temp;
	}

	void DrawLine (Vector2 start, Vector2 end)
	{
		Gizmos.DrawLine (transform.TransformPoint (start), transform.TransformPoint (end));
	}

	void DrawLine (Vector2 start, Vector2 end, Color color)
	{
		Color temp = Gizmos.color;
		Gizmos.color = color;
		Gizmos.DrawLine (transform.TransformPoint (start), transform.TransformPoint (end));
		Gizmos.color = temp;
	}
	#endif
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class RaycastController : MonoBehaviour
{
	public LayerMask collisionMask;

	public const float skinWidth = .015f;
	const float dstBetweenRays = .25f;

	protected int horizontalRayCount;
	protected int verticalRayCount;
	protected float horizontalRaySpacing;
	protected float verticalRaySpacing;

	[HideInInspector]
	public BoxCollider2D coll;
	protected RaycastOrigins raycastOrigins;

	public virtual void Awake()
	{
		coll = GetComponent<BoxCollider2D>();
	}

	public virtual void Start()
	{
		CalculateRaySpacing();
	}

	public void UpdateRaycastOrigins()
	{
		Bounds bounds = coll.bounds;
		bounds.Expand(skinWidth * -2);

		raycastOrigins.topLeft = new Vector2(bounds.min.x, bounds.max.y);
		raycastOrigins.topRight = new Vector2(bounds.max.x, bounds.max.y);
		raycastOrigins.bottomLeft = new Vector2(bounds.min.x, bounds.min.y);
		raycastOrigins.bottomRight = new Vector2(bounds.max.x, bounds.min.y);
	}

	public void CalculateRaySpacing()
	{
		Bounds bounds = coll.bounds;
		bounds.Expand(skinWidth * -2);

		float boundsWidth = bounds.size.x;
		float boundsHeight = bounds.size.y;

		horizontalRayCount = Mathf.RoundToInt(boundsHeight / dstBetweenRays);
		verticalRayCount = Mathf.RoundToInt(boundsWidth / dstBetweenRays);

		horizontalRaySpacing = bounds.size.y / (horizontalRayCount - 1);
		verticalRaySpacing = bounds.size.x / (verticalRayCount - 1);
	}

	public struct RaycastOrigins
	{
		public Vector2 topLeft;
		public Vector2 topRight;
		public Vector2 bottomLeft;
		public Vector2 bottomRight;
	}
}

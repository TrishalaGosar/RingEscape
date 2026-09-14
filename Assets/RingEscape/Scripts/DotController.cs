using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D))]
public class DotController : MonoBehaviour
{
    public RingEscapeGame game;
    public float dashDistance = 2.1f;
    public float dashSpeed = 8f;
    public float minimumSwipePixels = 25f;

    private Rigidbody2D body;
    private Vector2 dashStart;
    private Vector2 pointerStart;
    private bool trackingPointer;
    private bool dashing;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        body.gravityScale = 0f;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;
        body.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    private void Update()
    {
        if (game != null && game.IsGameOver())
            return;

        if (dashing)
            return;

        if (Input.touchCount > 0)
        {
            HandleTouch(Input.GetTouch(0));
            return;
        }

        HandleMouse();
    }

    private void FixedUpdate()
    {
        if (!dashing)
            return;

        Vector2 travelled = body.position - dashStart;
        if (travelled.sqrMagnitude >= dashDistance * dashDistance)
            StopDash();
    }

    private void HandleTouch(Touch touch)
    {
        if (touch.phase == TouchPhase.Began)
        {
            pointerStart = touch.position;
            trackingPointer = true;
        }
        else if (trackingPointer && (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled))
        {
            TryDash(touch.position - pointerStart);
            trackingPointer = false;
        }
    }

    private void HandleMouse()
    {
        if (Input.GetMouseButtonDown(0))
        {
            pointerStart = Input.mousePosition;
            trackingPointer = true;
        }
        else if (trackingPointer && Input.GetMouseButtonUp(0))
        {
            TryDash((Vector2)Input.mousePosition - pointerStart);
            trackingPointer = false;
        }
    }

    private void TryDash(Vector2 swipe)
    {
        if (dashing || swipe.sqrMagnitude < minimumSwipePixels * minimumSwipePixels)
            return;

        dashStart = body.position;
        dashing = true;
        body.velocity = swipe.normalized * dashSpeed;
    }

    public void StopDash()
    {
        dashing = false;
        if (body != null)
            body.velocity = Vector2.zero;
    }

    public void ResetDot()
    {
        StopDash();
        body.position = Vector2.zero;
        transform.position = Vector3.zero;
        trackingPointer = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.GetComponentInParent<RotatingRing>() != null)
            game.Fail();
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.collider.GetComponentInParent<RotatingRing>() != null)
            game.Fail();
    }
}

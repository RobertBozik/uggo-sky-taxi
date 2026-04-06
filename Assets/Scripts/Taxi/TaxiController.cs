using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class TaxiController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float thrustForce = 12f;
    [SerializeField] private float steerForce = 8f;
    [SerializeField] private float maxVelocity = 6f;
    [SerializeField] private float tiltAmount = 15f;
    [SerializeField] private float tiltSpeed = 5f;

    [Header("Fuel")]
    [SerializeField] private float maxFuel = 100f;
    [SerializeField] private float fuelConsumption = 5f;
    [SerializeField] private float fuelPickupAmount = 20f;

    [Header("Landing")]
    [SerializeField] private float maxLandingSpeed = 4f;
    [SerializeField] private float landedDrag = 5f;
    [SerializeField] private float flyingDrag = 0.5f;

    // State
    private Rigidbody2D rb;
    private float currentFuel;
    private bool isThrusting;
    private float steerInput;
    private bool isLanded;
    private bool hasPassenger;
    private int passengerTargetPlatformId = -1;
    private bool gameStarted;

    // Properties for other scripts to read
    public float CurrentFuel => currentFuel;
    public float MaxFuel => maxFuel;
    public float FuelPercent => currentFuel / maxFuel;
    public bool IsThrusting => isThrusting;
    public bool IsLanded => isLanded;
    public bool HasPassenger => hasPassenger;
    public int PassengerTargetPlatformId => passengerTargetPlatformId;
    public bool IsAlive { get; private set; } = true;

    // Events
    public System.Action OnCrash;
    public System.Action<int> OnLanded; // platform id
    public System.Action OnTakeoff;
    public System.Action OnPassengerPickup;
    public System.Action OnPassengerDropoff;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        currentFuel = maxFuel;
        rb.bodyType = RigidbodyType2D.Kinematic;
    }

    private void Update()
    {
        if (!gameStarted || !IsAlive) return;

        // Visual tilt based on horizontal input
        float targetAngle = -steerInput * tiltAmount;
        float currentAngle = transform.eulerAngles.z;
        if (currentAngle > 180f) currentAngle -= 360f;
        float newAngle = Mathf.Lerp(currentAngle, targetAngle, Time.deltaTime * tiltSpeed);
        transform.rotation = Quaternion.Euler(0f, 0f, newAngle);
    }

    private void FixedUpdate()
    {
        if (!gameStarted) return;
        if (!IsAlive) return;

        // Thrust
        if (isThrusting && currentFuel > 0f)
        {
            rb.AddForce(Vector2.up * thrustForce);
            currentFuel -= fuelConsumption * Time.fixedDeltaTime;
            currentFuel = Mathf.Max(0f, currentFuel);

            if (isLanded)
            {
                isLanded = false;
                rb.linearDamping = flyingDrag;
                OnTakeoff?.Invoke();
            }
        }

        // Steering
        if (Mathf.Abs(steerInput) > 0.01f)
        {
            rb.AddForce(Vector2.right * steerInput * steerForce);
        }

        // Clamp velocity
        if (rb.linearVelocity.magnitude > maxVelocity)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxVelocity;
        }
    }

    // Called by Input System via Player Input "Send Messages"
    public void OnMove(InputValue value)
    {
        if (!gameStarted)
        {
            gameStarted = true;
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 1.5f;
            rb.linearDamping = flyingDrag;
        }

        Vector2 input = value.Get<Vector2>();
        steerInput = input.x;
        isThrusting = input.y > 0.5f;
    }

    // Default "Attack" action = Space/left click
    public void OnAttack(InputValue value)
    {
        if (!isLanded) return;
        if (hasPassenger)
            TryDropoff();
        else
            TryPickup();
    }

    private void TryPickup()
    {
        var colliders = Physics2D.OverlapCircleAll(transform.position, 2f);
        foreach (var col in colliders)
        {
            var passenger = col.GetComponent<Passenger>();
            if (passenger != null && passenger.IsWaiting)
            {
                hasPassenger = true;
                passengerTargetPlatformId = passenger.TargetPlatformId;
                passenger.PickUp();
                AddFuel(fuelPickupAmount);
                OnPassengerPickup?.Invoke();
                return;
            }
        }
    }

    private void TryDropoff()
    {
        var colliders = Physics2D.OverlapCircleAll(transform.position, 2f);
        foreach (var col in colliders)
        {
            var platform = col.GetComponent<Platform>();
            if (platform != null && platform.PlatformId == passengerTargetPlatformId)
            {
                hasPassenger = false;
                passengerTargetPlatformId = -1;
                AddFuel(fuelPickupAmount);
                OnPassengerDropoff?.Invoke();
                return;
            }
        }
    }

    public void AddFuel(float amount)
    {
        currentFuel = Mathf.Min(maxFuel, currentFuel + amount);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!IsAlive) return;

        float impactSpeed = collision.relativeVelocity.magnitude;

        var platform = collision.gameObject.GetComponent<Platform>();
        if (platform != null)
        {
            if (impactSpeed > maxLandingSpeed)
            {
                Crash();
                return;
            }

            foreach (var contact in collision.contacts)
            {
                if (contact.normal.y > 0.5f)
                {
                    Land(platform.PlatformId);
                    return;
                }
            }

            if (impactSpeed > maxLandingSpeed * 0.8f)
            {
                Crash();
            }
        }
        else
        {
            if (impactSpeed > maxLandingSpeed * 0.5f)
            {
                Crash();
            }
        }
    }

    private void Land(int platformId)
    {
        isLanded = true;
        rb.linearDamping = landedDrag;
        OnLanded?.Invoke(platformId);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (isLanded && collision.gameObject.GetComponent<Platform>() != null)
        {
            isLanded = false;
            rb.linearDamping = flyingDrag;
        }
    }

    public void Crash()
    {
        if (!IsAlive) return;
        IsAlive = false;
        rb.linearDamping = 0.2f;
        OnCrash?.Invoke();
    }

    public void Respawn(Vector3 position)
    {
        transform.position = position;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        transform.rotation = Quaternion.identity;
        currentFuel = maxFuel;
        hasPassenger = false;
        passengerTargetPlatformId = -1;
        isLanded = false;
        IsAlive = true;
        gameStarted = false;
        rb.bodyType = RigidbodyType2D.Kinematic;
    }
}
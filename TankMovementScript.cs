using UnityEngine;

public class TankMovementScript : MonoBehaviour
{
    [Header("Main parts of panzer")]
    Rigidbody rb;
    public Transform centerOfMass;
    float headTurningSpeed, headTurningInput, barrelTurningInput, barrelTurningSpeed;
    Quaternion headTurning, barrelTurning;
    public GameObject head, barrel, barrelRotationPart1;
    private float m_horizontalInput, m_verticalInput, m_steeringAngle;
    [Header("New Panzer Movement")]
    public WheelCollider[] wheelColliders;
    public Transform[] wheelTransforms;
    public float maxSteerAngle = 45f;
    public float motorForce = 50f;
    bool isBreaking;
    [Header("Panzer Brakes")]
    public float brakingForce;
    BrakeLight brakeLight;
    public GameObject backLightGO;
    [Header("User Input")]
    public Joystick joystickForBarrel;
    public Joystick joystick;
    [Header("Other panzer vars")]
    int angleConstraint = 100;
    public GameObject audioManagerGO;
    AudioManager _audioManager;
    float _pitch = 0.1f;
    public GameObject deadPanzerCollider;
    Quaternion _quat;
    Vector3 _pos;

    private void Awake()
    {
        rb = gameObject.GetComponent<Rigidbody>();
        rb.centerOfMass = centerOfMass.localPosition;
        brakeLight = backLightGO.GetComponent<BrakeLight>();
        _audioManager = audioManagerGO.GetComponent<AudioManager>();
        FindObjectOfType<AudioManager>().Play("IdleEngine");
        deadPanzerCollider.SetActive(false);
        headTurningSpeed = 30f;
        barrelTurningSpeed = 15f;
        isBreaking = false;
    }

    void FixedUpdate()
    {
        Steer();
        Accelerate();
        UpdateWheelPoses();
        TurnHead();
        TurnBarrel();
        Brake();
    }

    private void Steer()
    {
        m_horizontalInput = joystick.Horizontal;
        m_steeringAngle = maxSteerAngle * m_horizontalInput;
        foreach (int num in new int[] { 0, 2 })
        {
            wheelColliders[num].steerAngle = m_steeringAngle;
            if (wheelColliders[num].rpm >= 350f)
            {
                maxSteerAngle = 25f;
            }
            else
            {
                maxSteerAngle = 45f;
            }
        }
    }

    private void Accelerate()
    {
        m_verticalInput = joystick.Vertical;
        if  (m_verticalInput > 0f || m_verticalInput < 0f)
        {
            _pitch += 0.001f;
            if (_pitch >= 3f)
            {
                _pitch = 3f;
            }
            _audioManager.sounds[6].source.pitch = _pitch;
        }
        if (m_verticalInput == 0f)
        {
            _pitch -= 0.02f;
            if (_pitch <= 0.4f)
            {
                _pitch = 0.4f;
            }
            _audioManager.sounds[6].source.pitch = _pitch;
        }
        foreach (WheelCollider collider in wheelColliders)
        {
            collider.motorTorque = m_verticalInput * motorForce;
        }
    }

    private void UpdateWheelPoses()
    {
        foreach (int num in new int[] { 0, 1, 2, 3 })
        {
            UpdateWheelPose(wheelColliders[num], wheelTransforms[num]);
        }
    }

    private void UpdateWheelPose(WheelCollider _collider, Transform _transform)
    {
        _pos = _transform.position;
        _quat = _transform.rotation;
        _collider.GetWorldPose(out _pos, out _quat);
        _transform.position = _pos;
        _transform.rotation = _quat * Quaternion.Euler(0f, 90f, 0f);
        if(_transform.tag == "LeftWheelTransform")//left wheels shold show their wheel rimps
        {
            _transform.rotation = _quat * Quaternion.Euler(180f, 90f, 0f);
        }
    }

    void TurnHead()
    {
        headTurningInput = joystickForBarrel.Horizontal;
        headTurning = 
            Quaternion.Euler(0f, headTurningInput * headTurningSpeed * Time.deltaTime, 0f);
        head.transform.Rotate(headTurning.eulerAngles, Space.Self);
    }

    void TurnBarrel()
    {
        barrel.transform.position = barrelRotationPart1.transform.position;
        barrelTurningInput = joystickForBarrel.Vertical * -1f;
        if (barrelTurningInput > 0f)//constraints barrelTurningInput values between 1f, -1f, 0f
        {
            barrelTurningInput = 1f;
        }
        if (barrelTurningInput < 0f)
        {
            barrelTurningInput = -1f;
        }
        if (barrelTurningInput == -1f)// 113 - 112 and 5 - 6 it is range constraints to barrel movement
        {
            if (angleConstraint > 5)
            {
                angleConstraint -= 1;
            }
        }
        if (barrelTurningInput == 1f)
        {
            if (angleConstraint < 113)
            {
                angleConstraint += 1;
            }
        }
        if (barrelTurningInput == 0f)
        {
            angleConstraint += 0;
        }
        if (angleConstraint > 112 && barrelTurningInput == 1f)
        {
            angleConstraint = 112;
            barrelTurningInput = 0f;
        }
        if (angleConstraint < 6 && barrelTurningInput == -1f)
        {
            angleConstraint = 6;
            barrelTurningInput = 0f;
        }
        barrelTurning = 
        Quaternion.Euler(barrelTurningInput * barrelTurningSpeed * Time.deltaTime, 0f, 0f);
        barrel.transform.Rotate(barrelTurning.eulerAngles, Space.Self);
    }
    void Brake()
    {
        if (isBreaking)
        {
            foreach (WheelCollider colider in wheelColliders)
            {
                colider.brakeTorque = brakingForce;
                brakeLight.Brake(isBreaking);
            }
        }
        if (!isBreaking)
        {
            foreach (WheelCollider colider in wheelColliders)
            {
                colider.brakeTorque = 0f;
                brakeLight.Brake(isBreaking);
            }
        }
    }
    public void BrakeFromTouchButtonDown()
    {
        isBreaking = true;
    }
    public void BrakeFromTouchButtonUp()
    {
        isBreaking = false;
    }
}

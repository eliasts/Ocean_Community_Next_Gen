using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BoatController : MonoBehaviour{

    [SerializeField] private List<GameObject> m_motors;

	[SerializeField] private bool m_enableAudio = true;
	[SerializeField] private AudioSource m_boatAudioSource;
	[SerializeField] private float m_boatAudioMinPitch = 0.4F;
	[SerializeField] private float m_boatAudioMaxPitch = 1.2F;

	[SerializeField] private float m_accelerationFactor = 2.0F;
	[SerializeField] private float m_turningFactor = 2.0F;
    [SerializeField] private float m_accelerationTorqueFactor = 35F;
	[SerializeField] private float m_turningTorqueFactor = 35F;

	private float m_verticalInput = 0F;
	private float m_horizontalInput = 0F;
    private Rigidbody m_rigidbody;
	private Vector2 m_androidInputInit;


     void Start()   {
       // base.Start();

        m_rigidbody = GetComponent<Rigidbody>();
       // m_rigidbody.drag = 1;
      //  m_rigidbody.angularDrag = 1;

		initPosition ();
	}

	public void initPosition()
	{
		#if UNITY_ANDROID && !UNITY_EDITOR
		m_androidInputInit.x = Input.acceleration.y;
		m_androidInputInit.y = Input.acceleration.x;
		#endif
	}

	void Update()
	{
		#if UNITY_ANDROID && !UNITY_EDITOR
		Vector2 touchInput = Vector2.zero;
		touchInput.x =  -(Input.acceleration.y - m_androidInputInit.y);
		touchInput.y =  Input.acceleration.x - m_androidInputInit.x;

		if (touchInput.sqrMagnitude > 1)
			touchInput.Normalize();

		setInputs (touchInput.x, touchInput.y);
		#else
		setInputs (Input.GetAxisRaw("Vertical"), Input.GetAxisRaw("Horizontal"));
		#endif
	}

	public void setInputs(float iVerticalInput, float iHorizontalInput)
	{
		m_verticalInput = iVerticalInput;
		m_horizontalInput = iHorizontalInput;
	}

	 void FixedUpdate()
	{
		//base.FixedUpdate();
		
		m_rigidbody.AddRelativeForce(Vector3.forward * m_verticalInput * m_accelerationFactor);

        m_rigidbody.AddRelativeTorque(
			m_verticalInput * -m_accelerationTorqueFactor,
			m_horizontalInput * m_turningFactor,
			m_horizontalInput * -m_turningTorqueFactor
        );

        if(m_motors.Count > 0) {

            float motorRotationAngle = 0F;
			float motorMaxRotationAngle = 70;

			motorRotationAngle = - m_horizontalInput * motorMaxRotationAngle;

			for(int i=0; i<m_motors.Count; i++) {
				float currentAngleY = m_motors[i].transform.localEulerAngles.y;
				if (currentAngleY > 180.0f)
					currentAngleY -= 360.0f;

				float localEulerAngleY = Mathf.Lerp(currentAngleY, motorRotationAngle, Time.deltaTime * 10);
				m_motors[i].transform.localEulerAngles = new Vector3(
					m_motors[i].transform.localEulerAngles.x,
					localEulerAngleY,
					m_motors[i].transform.localEulerAngles.z
				);
            }
        }

		if (m_enableAudio && m_boatAudioSource != null) 
		{
			float pitchLevel = m_verticalInput * m_boatAudioMaxPitch;
			if (pitchLevel < m_boatAudioMinPitch)
				pitchLevel = m_boatAudioMinPitch;
			float smoothPitchLevel = Mathf.Lerp(m_boatAudioSource.pitch, pitchLevel, Time.deltaTime);

			m_boatAudioSource.pitch = smoothPitchLevel;
		}
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour
{
    public float maxSpeed=30f;
    public Rigidbody rb;

    public float forwardAccel=8f, reverseAccel=4f;
    private float speedInput;  //storing what player is pressing

    private float turnInput; //storing turn input
    public float turnStrengh = 180f;

   public bool grounded;

    public Transform groundRayPoint, groundRayPoint2;
    public LayerMask whatIsGround;
    public float groundRayLength = 0.75f;

    private float dragOnGround;
    public float gravityMod=10f;

    public Transform leftFrontWheel, rightFrontWheel;
    public float maxWheelTurn=25f;

    public ParticleSystem[] dustTrail;
    public float maxEmission = 25f, emissionFadeSpeed = 20f;
    private float emissionRate;

    // Start is called before the first frame update
    void Start()
    {
        rb.transform.parent = null;  //making sphere not child of car

        dragOnGround = rb.drag;
       
    }

    // Update is called once per frame
    void Update()
    {
        speedInput = 0f;
        if (Input.GetAxis("Vertical")>0) 
        {
            speedInput = Input.GetAxis("Vertical") * forwardAccel;
        }
        else if(Input.GetAxis("Vertical") < 0)
        {
            speedInput = Input.GetAxis("Vertical") * reverseAccel;
        }

        turnInput = Input.GetAxis("Horizontal");
        
        if( Input.GetAxis("Vertical") != 0 &&grounded)
        {
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles + new Vector3(0f,turnInput*turnStrengh*Time.deltaTime * Mathf.Sign(speedInput) * (rb.velocity.magnitude/maxSpeed), 0f));
        }




        //turning the wheels 
        leftFrontWheel.localRotation = Quaternion.Euler(leftFrontWheel.rotation.eulerAngles.x, (turnInput * maxWheelTurn), leftFrontWheel.localRotation.eulerAngles.z );
        rightFrontWheel.localRotation= Quaternion.Euler(rightFrontWheel.rotation.eulerAngles.x, (turnInput * maxWheelTurn), rightFrontWheel.localRotation.eulerAngles.z);

        transform.position = rb.position;  //assigning car to sphere pos

        //control particle emissions
        emissionRate = Mathf.MoveTowards(emissionRate, 0f, emissionFadeSpeed * Time.deltaTime);

        if (grounded && (Mathf.Abs (turnInput)>0.5f || (rb.velocity.magnitude<maxSpeed*0.5f && rb.velocity.magnitude !=0)))
        {
            emissionRate = maxEmission;
        }

        if (rb.velocity.magnitude <= 0.5f)
        {
            emissionRate = 0;
        }

        for(int i = 0; i < dustTrail.Length; i++)
        {
            var emissionModule = dustTrail[i].emission;

            emissionModule.rateOverTime = emissionRate;
        }
    }

    private void FixedUpdate()
    {

       grounded = false;

        RaycastHit hit;
        Vector3 normalTarget = Vector3.zero;

        if (Physics.Raycast(groundRayPoint.position, -transform.up, out hit, groundRayLength, whatIsGround))
        {
            grounded = true;
            normalTarget = hit.normal;
        }

        if(Physics.Raycast(groundRayPoint2.position, -transform.up, out hit, groundRayLength, whatIsGround))
        {
            grounded = true;

            normalTarget = (normalTarget + hit.normal) / 2f;
        }
        //when on ground rotate to match the normal
        if (grounded)
        {
            transform.rotation = Quaternion.FromToRotation(transform.up, normalTarget) * transform.rotation;
        }
        //accelerate the car
        if (grounded)
        {
            rb.drag = dragOnGround;
            rb.AddForce(transform.forward * speedInput * 1000f);
        }
        else
        {
            rb.drag = 0.1f;

            rb.AddForce(-Vector3.up * gravityMod * 100f);
        }

        if (rb.velocity.magnitude > maxSpeed)
        {
            rb.velocity = rb.velocity.normalized * maxSpeed; 
        }
        Debug.Log(rb.velocity.magnitude);

        
    }
}

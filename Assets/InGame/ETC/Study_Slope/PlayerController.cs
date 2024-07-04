using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    Rigidbody2D rig;
    float inputX;
    public int moveSpeed;
    public LayerMask groundMask;

    public Transform chkPos;
    public bool isGround;
    public float checkRadius;

    public float jumpPower = 1;
    bool isJump;

    public float distance;
    public float angle;
    public Vector2 perp;
    public bool isSlope;

    public Transform frontchk;


    void Start()
    {
        rig = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        inputX = Input.GetAxis("Horizontal");
        GroundChk();
        Flip();

        if (inputX == 0)
            rig.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
        else
            rig.constraints = RigidbodyConstraints2D.FreezeRotation;

        RaycastHit2D hit = Physics2D.Raycast(chkPos.position, Vector2.down, distance, groundMask);
        RaycastHit2D fronthit = Physics2D.Raycast(frontchk.position, transform.right, 0.1f, groundMask);

        if (hit || fronthit) {
            if (fronthit)
                SlopeChk(fronthit);
            else if (hit)
                SlopeChk(hit);

            Debug.DrawLine(hit.point, hit.point + hit.normal, Color.blue);
            Debug.DrawLine(hit.point, hit.point + perp, Color.red);
        }

        if (inputX != 0) {
            if (isSlope && isGround && !isJump && angle < maxangle) {
                rig.velocity = Vector2.zero;
                if (inputX > 0)
                    transform.Translate(new Vector2(-perp.x * moveSpeed * inputX * Time.deltaTime,
                        -perp.y * moveSpeed * inputX * Time.deltaTime));
                else if (inputX < 0)
                    transform.Translate(new Vector2(perp.x * moveSpeed * inputX * Time.deltaTime,
                        -perp.y * moveSpeed * inputX * Time.deltaTime));
            }
            else if (!isSlope && isGround && !isJump)
                transform.Translate(Vector2.right * moveSpeed * Time.deltaTime * Mathf.Abs(inputX));
            else if (!isGround)
                transform.Translate(Vector2.right * moveSpeed * Time.deltaTime * Mathf.Abs(inputX));
        }
    }

    public float maxangle;
    private void FixedUpdate()
    {
        /*
        if (isSlope && isGround && !isJump && angle < maxangle)
            rig.velocity = perp * moveSpeed * inputX * -1f;
        else if (!isSlope && isGround && !isJump)
            rig.velocity = new Vector2(inputX * moveSpeed, 0);
        else if (!isGround)
            rig.velocity = new Vector2(inputX * moveSpeed, rig.velocity.y);
        */

        Jump();
    }

    void GroundChk()
    {
        isGround = Physics2D.OverlapCircle(chkPos.position, checkRadius, groundMask);
    }

    void Flip()
    {
        if (inputX > 0)
        {
            transform.eulerAngles = new Vector3(0, 0, 0);
        }
        else if (inputX < 0)
        {
            transform.eulerAngles = new Vector3(0, 180, 0);
        }
    }

    void SlopeChk(RaycastHit2D hit)
    {
        perp = Vector2.Perpendicular(hit.normal).normalized;
        angle = Vector2.Angle(hit.normal, Vector2.up);

        if (angle != 0) isSlope = true;
        else isSlope = false;
    }

    private void Jump()
    {
        if (rig.velocity.y <= 0)
            isJump = false;

        if (isGround)
        {
            if (Input.GetAxis("Jump") != 0)
            {
                isJump = true;
                rig.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);
            }
        }
    }
}

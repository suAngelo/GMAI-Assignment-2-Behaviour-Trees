using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Player : MonoBehaviour
{
    public float moveSpeed = 5.0f;

    private CharacterController cc;
    private Vector3 moveDir;

    // Start is called before the first frame update
    void Awake()
    {
        cc = GetComponent<CharacterController>();   
    }

    // Update is called once per frame
    void Update()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticallInput = Input.GetAxis("Vertical");

        moveDir = new Vector3(horizontalInput, 0.0f, verticallInput);
        //moveDir = transform.TransformDirection(moveDir);
        moveDir *= moveSpeed;

        cc.Move(moveDir * Time.deltaTime);
    }
}

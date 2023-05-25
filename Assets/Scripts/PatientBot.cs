using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatientBot : MonoBehaviour
{
  public void ConveyorTo(string tag)
    {
        GameObject target = GameObject.FindGameObjectWithTag(tag);
        Vector3 targetPos = target.transform.position;
        transform.position = Vector3.MoveTowards(transform.position, targetPos, 1);
    }

    private void FixedUpdate()
    {
        ConveyorTo("ConveyorSoft1");
    }
}

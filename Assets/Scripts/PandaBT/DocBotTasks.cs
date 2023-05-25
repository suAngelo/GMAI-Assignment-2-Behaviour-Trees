using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.AI;
using Panda;
using Unity.VisualScripting;
using BehaviorDesigner.Runtime.Tasks.Unity.UnityGameObject;
using BehaviorDesigner.Runtime.Tasks.Unity.UnityInput;

public class DocBotTasks : MonoBehaviour
{
    NavMeshAgent navAgent;
    Transform target;

    private GameObject player;

    private float stopDistance = 5.0f;
    private float interactionDistance = 20.0f;

    private int universalErrors = 0;
    private int maxUniversalErrors = 3;

    private int localErrors = 0;
    private int maxLocalErrors = 3;

    // Start is called before the first frame update
    void Start()
    {
        navAgent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player");
    }

    [Task]
    void MoveTo(string tag)
        /*
         * This function takes a string of a GameObject tag and moves the agent to a target GameObject position.
         * It is considered successful once the agent reaches a minimum distance of 5.0f away from the target.
         */
    {
        try
        {
            target = GameObject.FindGameObjectWithTag(tag).transform;
            navAgent.SetDestination(target.position);
            if (Vector3.Distance(
                target.position, transform.position) < stopDistance)
            {
                Debug.Log(target.transform.tag);
                navAgent.ResetPath();
                target = null; // reset the target object
                Task.current.Succeed();
            }
            //else
            //{
            //    // Debug.Log(Vector3.Distance(target.position, transform.position));
            //    // Issue: stopDistance was too small as the distance between
            //    // these two positions were always above 1.5f(the previous stopDistance value)
            //}
        }
        catch (Exception e) 
        { 
            Debug.LogError(e);
            Task.current.Fail();
        }
    }

    [Task]
    void Idle()
    {
        if (CheckCustomer())
        {
            Task.current.Succeed();
        }
        else
        {
            Task.current.Fail();
        }
        Debug.Log(Task.current.status);
    }

    [Task]
    void Serving()
    {
        if (Input.GetKeyDown(KeyCode.Y))
        {
            Task.current.Succeed();
        }
        else if (Input.GetKeyDown(KeyCode.N))
        {
            Task.current.Fail();
        }
        else
        {
            Debug.Log("Press either Y or N to confirm your decision.");
        }
    }

    [Task]
    bool CheckCustomer()
    {
        if (Vector3.Distance(player.transform.position, transform.position) <  interactionDistance)
        {
            Debug.Log("Customer in range");
            return true;
        }
        Debug.Log(Vector3.Distance(player.transform.position, transform.position));
        return false;
    }

    [Task]
    bool CheckIfFunctional()
    {
        if(UnityEngine.Random.Range(0, 10) > 1)
        {
            return true;
        }
        return false;
    }

    [Task]
    bool CheckIfRepairable()
    {
        if(UnityEngine.Random.Range(0, 10) > 1)
        {
            return true;
        }
        return false;
    }
    [Task]
    bool AttemptRepair()
    {
        while (LocalErrorsNotMaxed())
        {
            if (UnityEngine.Random.Range(0, 10) > 1)
            {
                Debug.Log("Local Errors Before Success: " + localErrors);
                localErrors = 0; // reset local errors
                return true;
            }
            else
            {
                localErrors++;
                Debug.Log("Local Errors: " + localErrors);
            }
        }
        return false;
    }

    [Task]
    void DischargeCustomer()
    {
        if (Input.anyKeyDown)
        {
            Task.current.Succeed();
        }
        else
        {
            Debug.Log("Press any key to accept the repair and be discharged.");
        }
    }


    [Task]
    bool LocalErrorsNotMaxed()
    {
        if (localErrors <= maxLocalErrors)
        {
            return true;
        }
        return false;
    }
    [Task]
    bool UniversalErrorsNotMaxed()
    {
        if (universalErrors <= maxUniversalErrors)
        {
            return true;
        }
        return false;
    }
    [Task]
    void IncrementUniversalErrors()
    {
        universalErrors++;
        Task.current.Succeed();
    }

    [Task]
    void IndicateGreen(string tag)
    {
        GameObject indicator = GameObject.FindGameObjectWithTag(tag);
        Renderer renderer = indicator.GetComponent<Renderer>();
        renderer.enabled = true;
        renderer.material.color = Color.green;
        Task.current.Succeed(); // Indicate success to ensure behaviour tree does
                                // not get stuck on "running"
    }
    [Task]
    void IndicateYellow(string tag)
    {
        GameObject indicator = GameObject.FindGameObjectWithTag(tag);
        Renderer renderer = indicator.GetComponent<Renderer>();
        renderer.enabled = true;
        renderer.material.color = Color.yellow;
        Task.current.Succeed();
    }
    [Task]
    void IndicateRed(string tag)
    {
        GameObject indicator = GameObject.FindGameObjectWithTag(tag);
        Renderer renderer = indicator.GetComponent<Renderer>();
        renderer.enabled = true;
        renderer.material.color = Color.red;
        Task.current.Succeed();
    }

    [Task]
    void PauseRunTimeDebugLog(string txt)
    {
        Debug.Log(txt);
    }
}

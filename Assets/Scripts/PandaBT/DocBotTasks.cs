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
    public GameObject patientBot;
    public GameObject conveyorManager;
    public float spawnOffset = 5.0f; // spawnOffset is used to ensure that the instantiated
                                     // patientBot appears to touch the conveyor

    GameObject patientInstance;
    NavMeshAgent navAgent;
    Transform target;
    Transform conveyorTarget;
    GameObject debris;

    private GameObject player;

    private float stopDistance = 5.0f;
    private float interactionDistance = 20.0f;

    private int universalErrors = 0;
    private int maxUniversalErrors = 3;

    private int localErrors = 0;
    private int maxLocalErrors = 3;

    private Vector3 spawnPos;
    private Vector3 softRepairPos1;
    private Vector3 softRepairPos2;
    private Vector3 hardRepairPos1;
    private Vector3 hardRepairPos2;


    // Start is called before the first frame update
    void Start()
    {
        navAgent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player");
        debris = GameObject.FindGameObjectWithTag("Debris");
        patientInstance = Instantiate(patientBot);

        spawnPos = conveyorManager.transform.Find("PatientSpawn").transform.position;
        softRepairPos1 = conveyorManager.transform.Find("SoftRepair1").transform.position;
        softRepairPos2 = conveyorManager.transform.Find("SoftRepair2").transform.position;
        hardRepairPos1 = conveyorManager.transform.Find("HardRepair1").transform.position;
        hardRepairPos2 = conveyorManager.transform.Find("HardRepair2").transform.position;

        debris.gameObject.SetActive(false);
        patientInstance.SetActive(true);
        patientInstance.transform.position =
            new Vector3(spawnPos.x, spawnPos.y - spawnOffset, spawnPos.z);
    }

    private void Update()
    {
        
    }



    // ================================================================================
    // === NORMAL FLOW OR GENERAL FUNCTIONS ===========================================
    // ================================================================================
    /* 
     * These functions relate to the behaviours within normal flow, which is as follows:
     * 
     * 1. Idle State
     * 2. Serving State
     * 3. Diagnosis State
     * 4. Software Repair State
     * 5. Hardware Repair State
     * 6. Discharge State
     * 7. Cleanup State
     * 
     * Any states not listed above is under alternate or abnormal flow, the functions of which
     * are below, after the functions in normal flow
     * (note that while the behaviours in normal flow are called "states", a behaviour tree
     * model is used to change agent behaviour.)
     */
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
    void ConveyorMoveTo(string objectName)
    {
            patientInstance.SetActive(true);
            conveyorTarget = conveyorManager.transform.Find(objectName).transform;
            patientInstance.transform.position =
                new Vector3(conveyorTarget.position.x,
                conveyorTarget.position.y - spawnOffset,
                conveyorTarget.position.z);
            Task.current.Succeed();
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
    void CreateMess()
    {
        debris.gameObject.SetActive(true);
        Task.current.Succeed();
    }
    [Task]
    void CleanMess()
    {
        debris.gameObject.SetActive(false);
        Task.current.Succeed();
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
    // End of Normal Flow Functions


    // ================================================================================
    // === ABNORMAL FLOW FUNCTIONS ====================================================
    // ================================================================================
    /* 
     * These functions relate to the behaviours outside of normal flow, which is as
     * follows:
     * 
     * 8. Update State
     * 9. Restocking State
     * 10. Failure State
     * 11. Call Repairman State
     * 
     * In a perpetual best case scenario, these states and their respective functions below
     * will never be called. 
     */
}

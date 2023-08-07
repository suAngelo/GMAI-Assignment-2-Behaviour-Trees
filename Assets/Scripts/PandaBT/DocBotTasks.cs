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

    private GameObject partShelf;
    private Transform[] partsInShelf; // A list of each part in the hardware repair shelf
                                      // to randomly remove a part each time a hardware repair is attemptied.


    // Start is called before the first frame update
    void Start()
    {
        navAgent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player");
        debris = GameObject.FindGameObjectWithTag("Debris");
        spawnPos = conveyorManager.transform.Find("PatientSpawn").transform.position;
        partShelf = GameObject.FindGameObjectWithTag("PartShelf");
        partsInShelf = partShelf.GetComponentsInChildren<Transform>();

        debris.gameObject.SetActive(false);

        patientInstance = Instantiate(patientBot);
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
     * (note that while the behaviours in normal flow are called "states", a behaviour tree
     * model is used to manipulate agent behaviour.)
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
        /*
         * This function takes a string of a GameObject name and moves the patientBot to the target gameobject.
         * It simulates the movement of a conveyor by directly changing the transform and is considered a success
         * once the patientBot has reached its location.
         * 
         * Note that this function should only used with dedicated conveyor points to maintain the simulation
         * of conveyor movement.
         */
    {
            patientInstance.SetActive(true);
            conveyorTarget = conveyorManager.transform.Find(objectName).transform;
            patientInstance.transform.position =
                new Vector3(conveyorTarget.position.x,
                conveyorTarget.position.y - spawnOffset, // spawnOffset used to maintain patientBot y position,
                                                         // aligined with the height of the conveyor belt
                                                         
                conveyorTarget.position.z);
            Task.current.Succeed();
    }
        

    [Task]
    void Idle()
        /*
         * This is a simple function that allows the agent to check for a nearby player.
         * It uses the already defined "CheckCustomer()" function and advances the tree if the player is nearby.
         * Otherwise, the tree will fail and will repeat.
         */
    {
        if (CheckCustomer())
        {
            Task.current.Succeed();
        }
        else
        {
            Task.current.Fail();
        }
        //Debug.Log(Task.current.status);
    }

    [Task]
    void Serving()
        /*
         * This function prompts the player for a yes or no input.
         * During the serving action, the player will be prompted if they want to confirm a repair:
         * If YES(Y): The current task succeeds and the tree advances
         * If NO(N): The current task fails and the tree will repeat from the start of root
         */
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
            Debug.Log("Press either Y or N to confirm a repair.");
        }
    }

    [Task]
    bool CheckCustomer()
        /*
         * This function checks whether or not the player is within a certain distance to the agent.
         * By using an already defined interactionDistance, the function checks if the distance
         * between the agent's position and the player's position reaches a certain threshold
         * for an interaction to begin.
         * 
         * Primarily used to 
         */
    {
        if (Vector3.Distance(player.transform.position, transform.position) <  interactionDistance)
        {
            //Debug.Log("Customer in range");
            return true;
        }
        //Debug.Log(Vector3.Distance(player.transform.position, transform.position));
        return false;
    }

    [Task]
    bool CheckIfFunctional()
        /*
         * This function checks whether or not a patientBot is functional based on a certain chance,
         * similar to the function used in FSM implementation.
         * 
         * As this is a boolean function, Task success is defined by whether or not the function
         * returns true.
         */
    {
        if(UnityEngine.Random.Range(0, 10) > 1) // Set to > 1 for highest chance of success
                                                // Set to > 10 for lowest chance of success
        {
            return true;
        }
        return false;
    }

    [Task]
    bool CheckIfRepairable()
        /*
         * This function acts similarly to CheckIfFunctional(), only that if this returns false,
         * the agent will perform behaviours under the failure state.
         */
    {
        if(UnityEngine.Random.Range(0, 10) > 1)
        {
            return true;
        }
        return false;
    }

    [Task]
    bool AttemptRepair()
        /*
         * This function will return true if the amount of local errors (errors encountered for current task)
         * has not reached the maximum threshold AND the robot is successfully repaired.
         * Otherwise, reaching the failure threshold will fail the task and increment the number of 
         * universal errors, later used during the failure and discharge behaviours. 
         */
    {
        while (LocalErrorsNotMaxed())
            /*
             * WHILE the local error threshold is not met,
             * IF a random number check passes,
             * local errors will be reset to 0 to allow repair attempts in other behaviours (e.g. software repair and hardware repair)
             * ELSE
             * local errors will increment until the threshold is met
             * 
             * Universal errors can be incremented by using IncrementUniversalErrors() in the Panda script.
             */
        {
            if (UnityEngine.Random.Range(0, 10) > 1)
            {
                Debug.Log("Local Errors Before Success: " + localErrors);
                localErrors = 0; // reset local errors, as local errors will need to be used by both
                                 // hardware repair and software repair. 
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
        /*
         * During hardware repair, a mess will be created by the robot. 
         * This function sets the debris gameobject active and makes it visible. 
         * Additionally, a random part on the hardware repair counter will be set to inactive
         * to give the effect of a part being used.
         * 
         */
    {
        debris.gameObject.SetActive(true);
        partsInShelf[UnityEngine.Random.Range(0, partsInShelf.Length)].gameObject.SetActive(false);
        Task.current.Succeed();
    }
    [Task]
    void CleanMess()
        /*
         * This function removes any debris from the hardware repair counter by first checking
         * if there is any debris, and then removing it. Otherwise, the node will return success
         * with no issue or action.
         */
    {
        if (debris.gameObject.activeSelf == false) // CleanMess will be used regardless of whether or not there is a mess, since the action will be used               
                                                   // for failure behaviour. Adding an IF statement ensures there is no runtime error.
        {
            Task.current.Succeed();
        }
        else
        {
            debris.gameObject.SetActive(false);
            Task.current.Succeed();
        }
    }

    [Task]
    void DischargeCustomer()
        /*
         * This function takes any keyboard input from the player/customer and succeeds the task.
         * Primarily used to ensure that there is a distinction between discharge and confirming a robot for repair.
         * Typically, this function will be executed before a transition into cleanup behaviours.
         * 
         * If the customer is not near the counter, the function will repeat until they do so.
         * 
         */
    {
        if (CheckCustomer())
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
        else
        {
            Debug.Log("Please approach the counter for robot discharge.");
        }
        
    }


    [Task]
    bool LocalErrorsNotMaxed()
    {
        /*
         * This function checks if the local error threshold has been met and returns true or false.
         * The reason for this being a function is so that it can be accessible by Panda scripts. 
         * 
         * You may change the local error threshold using the variables at the top of this script.
         */
        if (localErrors <= maxLocalErrors)
        {
            return true;
        }
        return false;
    }

    [Task]
    bool UniversalErrorsNotMaxed()
    {
        /*
         * This function checks if the universal error threshold has been met and returns true or false.
         * The reason for this being a function is so that it can be accessible by Panda scripts. 
         * 
         * You may change the universal error threshold using the variables at the top of this script.
         */
        if (universalErrors <= maxUniversalErrors)
        {
            return true;
        }
        return false;
    }

    [Task]
    void IncrementUniversalErrors()
        /*
         * This function increments the number of universal errors in this script. 
         * Used by the panda script to increment the number of errors in an appropriate tree.
         */
    {
        universalErrors++;
        Task.current.Succeed();
    }

     // Indicator Change Functions
     /*
      * Each machine in the Doc-bot workshop has an interface that changes colour
      * depending on the status of a task. 
      * 
      * Each colour can represent a different meaning:
      * 
      * Green - A task or action has been completed successfully, which usually implies a change in agent behaviour
      * Yellow - A task or action is either currently running or not performing optimally. This is not necessarily an indicator for imminent failure, just that the task is currently running.
      * Red - A task or action has failed, which implies a return to the discharge state. 
      * 
      * Usage:
      * (1) Tag a GameObject with a meshrenderer in the Unity Inspector
      * (2) In the Panda script, write Indicate<SelectedColour>(<string tag of gameobject>) in an appropriate location in the hierarchy
      * 
      * Optimally, there should be a function to accept 2 parameters: a gameobject tag/gameobject and a colour
      * However, PandaBT does not support multiple parameters or a gameobject being passed as a parameter. 
      */
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
}

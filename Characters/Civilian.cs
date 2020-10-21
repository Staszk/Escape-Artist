using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System;

public class Civilian : StateMachine
{
    public float moveSpeed;
    public int moveRate;
    private State currState;
    private bool inLockDown;
    private float moveTime;
    private float maxMoveTime = 1;
    private Rigidbody rb;
    public Sprite civ1, civ2, civ3;

    private void OnEnable()
    {
        AlertSystem.ActionLockdownMode += LockDownMode;
    }

    private void OnDisable()
    {
        AlertSystem.ActionLockdownMode -= LockDownMode;
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        inLockDown = false;
        currState = new ChillState();
        currState.OnEnter(this);

        ChooseSprite();
    }

    private void FixedUpdate()
    {
        //for (int i = 0; i < currState.transitions.Count; ++i)
        //{
        //    if (currState.transitions[i].CheckCondition())
        //    {
        //        State temp = currState;
        //        currState = currState.transitions[i].nextState;
        //        temp.OnExit();
        //        currState.OnEnter(this);
        //    }
        //}

        currState.OnUpdate();
    }

    private void ChooseSprite()
    {
        int num = UnityEngine.Random.Range(0, 3);
        SpriteRenderer sp = GetComponent<SpriteRenderer>();

        switch(num)
        {
            case 0:
                sp.sprite = civ1;
                break;
            case 1:
                sp.sprite = civ2;
                break;
            case 2:
                sp.sprite = civ3;
                break;
        }
    }

    private void LockDownMode()
    {
        inLockDown = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            rb.AddForce(other.transform.forward * moveSpeed);
            moveTime = Time.time;
        }
        else if (other.CompareTag("Civilian") || other.CompareTag("Wall"))
        {
            rb.velocity = Vector3.zero;
            rb.AddForce(-transform.forward * moveSpeed);
            transform.Rotate(0, 180, 0);
            moveTime = Time.time;
        }
    }

    //STATES
    private class ChillState : State
    {
        public override void OnEnter(Civilian theCivilian)
        {
            civilian = theCivilian;

            transitions = new List<Transition>
            {
                //new LockDownTransition(civilian)
            };
        }

        public override void OnUpdate()
        {
            MoveRandomly();
            CheckTime();
        }

        public override void OnExit()
        {
            transitions.Clear();
        }

        private void MoveRandomly()
        {
            if (UnityEngine.Random.Range(0, civilian.moveRate) == 0)
            {
                switch (UnityEngine.Random.Range(0, 4))
                {
                    case 0:
                        civilian.rb.AddForce(civilian.transform.forward * civilian.moveSpeed);
                        civilian.moveTime = Time.time;
                        break;
                    case 1:
                        civilian.rb.AddForce(-civilian.transform.forward * civilian.moveSpeed);
                        // civilian.transform.Rotate(0, 180, 0);
                        civilian.moveTime = Time.time;
                        break;
                    case 2:
                        civilian.rb.AddForce(civilian.transform.right * civilian.moveSpeed);
                        // civilian.transform.Rotate(0, 90, 0);
                        civilian.moveTime = Time.time;
                        break;
                    case 3:
                        civilian.rb.AddForce(-civilian.transform.right * civilian.moveSpeed);
                        // civilian.transform.Rotate(0, -90, 0);
                        civilian.moveTime = Time.time;
                        break;
                }
            }
        }

        private void CheckTime()
        {
            if (Time.time - civilian.moveTime > civilian.maxMoveTime)
            {
                civilian.GetComponent<Rigidbody>().velocity = Vector3.zero;
            }
        }
    }

    private class LockDownState : State
    {
        float moveSpeed;
        int moveRate;

        public override void OnEnter(Civilian theCivilian)
        {
            transitions = new List<Transition>();

            civilian = theCivilian;
            moveSpeed = civilian.moveSpeed * 2;
            moveRate = civilian.moveRate / 2;
        }

        public override void OnUpdate()
        {
            MoveRandomly();
        }

        public override void OnExit()
        {
            transitions.Clear();
        }

        private void MoveRandomly()
        {
            if (UnityEngine.Random.Range(0, moveRate) == 0)
            {

                switch (UnityEngine.Random.Range(0, 4))
                {
                    case 0:
                        civilian.rb.AddForce(civilian.transform.forward * moveSpeed);
                        civilian.moveTime = Time.time;
                        break;
                    case 1:
                        civilian.rb.AddForce(-civilian.transform.forward * moveSpeed);
                        //civilian.transform.Rotate(0, 180, 0);
                        civilian.moveTime = Time.time;
                        break;
                    case 2:
                        civilian.rb.AddForce(civilian.transform.right * moveSpeed);
                        //civilian.transform.Rotate(0, 90, 0);
                        civilian.moveTime = Time.time;
                        break;
                    case 3:
                        civilian.rb.AddForce(-civilian.transform.right * moveSpeed);
                        //civilian.transform.Rotate(0, -90, 0);
                        civilian.moveTime = Time.time;
                        break;
                }
            }
        }
    }

    //TRANSITIONS
    private class LockDownTransition : Transition
    {
        private Civilian civilian;

        public LockDownTransition(Civilian civ)
        {
            nextState = new LockDownState();
            civilian = civ;
        }

        public override bool CheckCondition()
        {
            if (civilian.inLockDown)
            {
                return true;
            }
            return false;
        }
    }
}

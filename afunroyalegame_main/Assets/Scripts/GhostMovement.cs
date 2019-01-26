﻿using UnityEngine;
using Mirror;
using System.Collections;
using System.Collections.Generic;

public class GhostMovement : NetworkBehaviour {
    GameObject MouseFollower;
    public Rigidbody rb;
    public GameObject proj;

    [SyncVar]
    public GameObject parent;
    public float force = 200f;
    public int shots = 3;
    public int abilityIndex = 0;

    [SyncVar]
    public bool spaceDepressed;

    public bool spaceDepressedLocal = true;

    public bool shootable = true;

    public GameObject cameraGO;

    // Use this for initialization
    public override void OnStartAuthority () {
        MouseFollower = parent.transform.Find("AIAim").gameObject;
        gameObject.tag = "ghostLocal";
        Destroy(parent.GetComponent<CamControl>().cameraGO);
        GameObject cam = Instantiate(cameraGO, transform.position, Quaternion.identity);
        cam.GetComponent<CamFollowAI>().parent = transform;
    }

	// Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("space") && hasAuthority)
        {
            spaceDepressed = true;
            CmdSetSpace();
        }
        if (spaceDepressed && shots > 0 && shootable)
        {
            spaceDepressed = false;
            shots--;
            shootable = false;
            GameObject projInst = Instantiate(proj, transform.position, Quaternion.identity);
            for (int i = 0; i < projInst.transform.childCount; i++)
            {
                projInst.transform.GetChild(i).gameObject.GetComponent<AbilityAffector>().abilityIndex = abilityIndex;
                projInst.transform.GetChild(i).gameObject.GetComponent<AbilityAffector>().onServer = hasAuthority;
            }
            projInst.transform.parent = transform;
            StartCoroutine("DelayFalse");
        }
    }

    [Command]
    public void CmdSetSpace()
    {
        spaceDepressed = true;
    }

    IEnumerator DelayFalse()
    {
        yield return new WaitForSeconds(2f);
        shootable = true;
    }

	void FixedUpdate () {
        if (rb.velocity.magnitude < 60f && MouseFollower)
        {
            if (MouseFollower.transform.position.x > transform.position.x)
            {
                rb.AddForce(new Vector3(force * Time.deltaTime * (MouseFollower.transform.position.x - transform.position.x), 0, 0));
            }
            else if (MouseFollower.transform.position.x < transform.position.x)
            {
                rb.AddForce(new Vector3(-force * Time.deltaTime * (transform.position.x - MouseFollower.transform.position.x), 0, 0));
            }

            if (MouseFollower.transform.position.y > transform.position.y)
            {
                rb.AddForce(new Vector3(0, force * Time.deltaTime * (MouseFollower.transform.position.y - transform.position.y), 0));
            }
            else if (MouseFollower.transform.position.y < transform.position.y)
            {
                rb.AddForce(new Vector3(0, -force * Time.deltaTime * (transform.position.y - MouseFollower.transform.position.y), 0));
            }
        }
    }
}

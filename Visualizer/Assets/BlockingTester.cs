using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockingTester : MonoBehaviour
{
    public float moveSpeed = 5f; // Speed at which the GameObject moves

    // Update is called once per frame
    void Update()
    {
        // Get input from arrow keys
        float moveX = Input.GetAxis("Horizontal");
        float moveY = Input.GetAxis("Vertical");

        // Create a movement vector
        Vector3 move = new Vector3(moveX, moveY, 0);

        // Apply movement to the GameObject
        transform.position += move * moveSpeed * Time.deltaTime;
    }
}
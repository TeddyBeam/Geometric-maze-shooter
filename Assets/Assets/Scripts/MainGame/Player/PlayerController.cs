using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MainGame
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField]
        private Rigidbody myRigidbody;

        private Vector3 velocity;

        private bool aa;

        public void Move(Vector3 velo)
        {
            velocity = velo;
        }

        public void Rotate(Vector3 direction)
        {
            Vector3 heightCorrectedPoint = new Vector3(direction.x, transform.position.y, direction.z);
            transform.LookAt(heightCorrectedPoint);
        }

        public void FixedUpdate()
        {
            myRigidbody.MovePosition(myRigidbody.position + velocity * Time.deltaTime);
        }
    }
}

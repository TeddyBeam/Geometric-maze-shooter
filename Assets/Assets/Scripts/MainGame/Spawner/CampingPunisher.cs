using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BaseSystems.Observer;

namespace MainGame
{
    /// <summary>
    /// Detect if player is  camping in one place.
    /// </summary>
    public class CampingPunisher : MonoBehaviour
    {
        [SerializeField]
        private LivingObject player;

        [SerializeField, Range(1f, 5f)]
        private float campingCheckRate = 2f, campThresholdDistance = 5f;

        private Transform playerTransform;
        private Vector3 playerOldPosition = Vector3.zero;

        protected virtual void Start()
        {
            if (player != null)
            {
                playerTransform = player.transform;
                playerOldPosition = playerTransform.position;
                StartCoroutine(CheckCamping());
            }
            else
            {
                Debug.LogError("Empty player");
            }
        }

        private IEnumerator CheckCamping()
        {
            yield return new WaitForSeconds(campingCheckRate); // Wait sometime when the game start.
            while (playerTransform != null)
            {
                if (Vector3.Distance(playerTransform.position, playerOldPosition) < campThresholdDistance)
                {
                    this.PostEvent(ObserverEventID.OnPlayerCampingDetected, playerTransform.position);
                }
                playerOldPosition = playerTransform.position;
                yield return new WaitForSeconds(campingCheckRate);
            }
        }
    }
}

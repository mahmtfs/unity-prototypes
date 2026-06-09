using System.Collections.Generic;
using UnityEngine;

namespace IK_Movement
{
    public class FootstepController : MonoBehaviour
    {
        [SerializeField] private List<AudioClip> _footsteps;
        [SerializeField] private IKComponent _IKComponent;
        [SerializeField] private AudioSource _audioSource;
        private bool wasRightFootGrounded = true;
        private bool wasLeftFootGrouned = true;
        void Update()
        {
            bool isRightFootGrounded = _IKComponent.IsRightFootGrounded();
            bool isLeftFootGrounded = _IKComponent.IsLeftFootGrounded();
            if (isRightFootGrounded && !wasRightFootGrounded)
            {
                PlayFootstep();
            }
            if (isLeftFootGrounded && !wasLeftFootGrouned)
            {
                PlayFootstep();
            }
            wasRightFootGrounded = isRightFootGrounded;
            wasLeftFootGrouned = isLeftFootGrounded;
        }

        void PlayFootstep()
        {
            _audioSource.PlayOneShot(_footsteps[Random.Range(0, _footsteps.Count)]);
        }

    }
}
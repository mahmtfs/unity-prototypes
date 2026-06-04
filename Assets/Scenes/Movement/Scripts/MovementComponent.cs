using UnityEngine;
using UnityEngine.InputSystem;

namespace Movement
{
    public class MovementComponent : MonoBehaviour
    {
        [SerializeField] private float _moveSpeed = 5f;
        [SerializeField] private float _rotationSpeed = 100f;
        [SerializeField] private float _animationSmoothing = 10f;
        [SerializeField] private Animator _playerIKCopyAnimator;
        [SerializeField] private Rigidbody _rigidbody;
        [SerializeField] private float _idleTransitionTime = 0.5f;
        private Animator _animator;
        private int _playerIKCopyLayerCount;
        private int _hashX;
        private int _hashZ;
        private int _hashGrounded;
        private bool _isIdling = true;
        private bool _isRotating = false;
        private bool _idleTimeSet = false;
        private float _lastIdleTime = 0f;

        void Start()
        {
            _animator = GetComponent<Animator>();
            _playerIKCopyLayerCount = _playerIKCopyAnimator.layerCount;
            _hashX = Animator.StringToHash("X");
            _hashZ = Animator.StringToHash("Z");
            _hashGrounded = Animator.StringToHash("Grounded");
            
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        void Update()
        {
            HandleInput();
            if (_animator != null && _animator.GetBool(_hashGrounded))
            {
                HandleAnimation();
            }
        }

        void HandleInput()
        {
            // Get input from WASD keys
            float moveZ = Keyboard.current.wKey.isPressed ? 1 : (Keyboard.current.sKey.isPressed ? -1 : 0);
            float moveX = Keyboard.current.aKey.isPressed ? -1 : (Keyboard.current.dKey.isPressed ? 1 : 0);
            
            Vector3 moveDirection = new Vector3(moveX, 0, moveZ).normalized;

            transform.Translate(moveDirection * _moveSpeed * Time.deltaTime, Space.Self);
            
            // Get mouse movement for rotation
            float mouseX = Mouse.current.delta.x.ReadValue();
            transform.Rotate(Vector3.up, mouseX * _rotationSpeed * Time.deltaTime);
            if (mouseX != 0)
            {
                _isRotating = true;
            }
            else
            {
                _isRotating = false;
            }

            // Handle animation parameters
            if (_playerIKCopyAnimator != null){
                float z_interp = Mathf.Lerp(_playerIKCopyAnimator.GetFloat(_hashZ), moveZ, Time.deltaTime * _animationSmoothing);
                _playerIKCopyAnimator.SetFloat(_hashZ, z_interp);
                float multiplier = moveZ == 0 ? 1 : moveZ;
                float x_interp = Mathf.Lerp(_playerIKCopyAnimator.GetFloat(_hashX), moveX * multiplier, Time.deltaTime * _animationSmoothing);
                _playerIKCopyAnimator.SetFloat(_hashX, x_interp);
            }

            if (moveZ == 0 && moveX == 0)
            {
                if (!_isIdling && !_idleTimeSet)
                {
                    _lastIdleTime = Time.time + _idleTransitionTime;
                    _idleTimeSet = true;
                }
                if (_idleTimeSet && Time.time >= _lastIdleTime)
                {
                    _rigidbody.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
                    _isIdling = true;
                }
            }
            else
            {
                _rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
                _isIdling = false;
                _idleTimeSet = false;
            }
        }

        void HandleAnimation()
        {
            // Copy playerIKCopyAnimator state to current animator
            for (int layer = 0; layer < _playerIKCopyLayerCount; layer++)
            {
                AnimatorStateInfo stateInfo = _playerIKCopyAnimator.GetCurrentAnimatorStateInfo(layer);
                AnimatorStateInfo nextStateInfo = _playerIKCopyAnimator.GetNextAnimatorStateInfo(layer);

                _animator.Play(stateInfo.fullPathHash, layer, stateInfo.normalizedTime);

                if (_playerIKCopyAnimator.IsInTransition(layer))
                {
                    AnimatorTransitionInfo transInfo = _playerIKCopyAnimator.GetAnimatorTransitionInfo(layer);
                    _animator.CrossFadeInFixedTime(
                        nextStateInfo.fullPathHash,
                        transInfo.duration,
                        layer,
                        0f,
                        nextStateInfo.normalizedTime
                    );
                }
            }

            _animator.SetFloat(_hashX, _playerIKCopyAnimator.GetFloat(_hashX));
            _animator.SetFloat(_hashZ, _playerIKCopyAnimator.GetFloat(_hashZ));
        }

        public bool IsIdling()
        {
            return _isIdling;
        }

        public bool IsRotating()
        {
            return _isRotating;
        }
    }
}

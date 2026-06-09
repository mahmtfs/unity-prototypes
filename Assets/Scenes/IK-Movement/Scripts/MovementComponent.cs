using UnityEngine;
using UnityEngine.InputSystem;

namespace IK_Movement
{
    public class MovementComponent : MonoBehaviour
    {
        [SerializeField] private float _moveSpeed = 5f;
        [SerializeField] private float _rotationSpeed = 100f;
        [SerializeField] private float _animationSmoothing = 10f;
        [SerializeField] private Animator _playerIKCopyAnimator;
        [SerializeField] private Rigidbody _rigidbody;
        [SerializeField] private float _idleTransitionTime = 0.5f;
        [SerializeField] private float _groundRaycastDistance = 0.15f;
        public LayerMask _groundLayers;
        private Animator _animator;
        private int _playerIKCopyLayerCount;
        private int _hashX;
        private int _hashZ;
        private int _hashGrounded;
        private bool _isIdling = true;
        private bool _isRotating = false;
        private bool _idleTimeSet = false;
        private float _lastIdleTime = 0f;
        private Vector3 _moveDirection = Vector3.zero;

        void Start()
        {
            _animator = GetComponent<Animator>();
            _playerIKCopyLayerCount = _playerIKCopyAnimator.layerCount;
            _hashX = Animator.StringToHash("X");
            _hashZ = Animator.StringToHash("Z");
            _hashGrounded = Animator.StringToHash("Grounded");
        }

        void Update()
        {
            HandleInput();
            if (_animator != null && _animator.GetBool(_hashGrounded))
            {
                HandleAnimation();
            }
        }

        void FixedUpdate()
        {
            bool isGrounded = IsGrounded(out RaycastHit hit);
            if (isGrounded)
            {
                Vector3 projectedMoveDirection = Vector3.ProjectOnPlane(_moveDirection, hit.normal).normalized;

                if (_moveDirection.magnitude > 0)
                {
                    _rigidbody.linearVelocity = projectedMoveDirection * _moveSpeed;
                }
                else
                {
                    _rigidbody.linearVelocity = Vector3.zero;
                }
            }
            else
            {
                _rigidbody.linearVelocity = new Vector3(_moveDirection.x * _moveSpeed, _rigidbody.linearVelocity.y, _moveDirection.z * _moveSpeed);
            }
        }

        void HandleInput()
        {
            float moveZ = Keyboard.current.wKey.isPressed ? 1 : (Keyboard.current.sKey.isPressed ? -1 : 0);
            float moveX = Keyboard.current.aKey.isPressed ? -1 : (Keyboard.current.dKey.isPressed ? 1 : 0);
            
            _moveDirection = (transform.forward * moveZ + transform.right * moveX).normalized;
            
            float mouseX = Mouse.current.delta.x.ReadValue();
            if (Time.timeScale != 0f)
            {
                transform.Rotate(Vector3.up, mouseX * _rotationSpeed * Time.deltaTime / Time.timeScale);
            }
    
            if (mouseX != 0)
            {
                _isRotating = true;
            }
            else
            {
                _isRotating = false;
            }

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

            bool speedUp = Keyboard.current.leftShiftKey.isPressed;
            if (speedUp)
            {
                Time.timeScale = 2f;
            }
            else
            {
                Time.timeScale = 1f;
            }
        }

        void HandleAnimation()
        {
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

        public void SetMouseSensitivity(float sensitivity)
        {
            _rotationSpeed = sensitivity * 50f;
        }

        public bool IsGrounded(out RaycastHit hitInfo)
        {
            return Physics.Raycast(transform.position + Vector3.up * 0.2f, Vector3.down, out hitInfo, _groundRaycastDistance + 0.2f, _groundLayers);
        }
    }
}

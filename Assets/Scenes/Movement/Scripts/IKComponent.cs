using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace Movement
{
    public class IKComponent : MonoBehaviour
    {
        [SerializeField] private TwoBoneIKConstraint _leftFootRig;
        [SerializeField] private TwoBoneIKConstraint _rightFootRig;
        [SerializeField] private float _IKInterpolationSpeed = 5f;
        [SerializeField] private float _footRotationSmoothing = 15f;
        [SerializeField] private float _raycastOriginOffset = 0.5f;
        [SerializeField] private float _raycastDistance = 1.2f;
        [SerializeField] private float _footOffset = 0.1f;
        [SerializeField] private Transform _leftFootTarget;
        [SerializeField] private Transform _rightFootTarget;
        [SerializeField] private Transform _leftFootBone;
        [SerializeField] private Transform _rightFootBone;
        [SerializeField] private Transform _leftFootBoneRef;
        [SerializeField] private Transform _rightFootBoneRef;
        [SerializeField] private LayerMask _groundLayers;
        [SerializeField] private Transform _playerIKCopy;
        [SerializeField] private CapsuleCollider _collider;
        [SerializeField] private MovementComponent _movementComponent;

        private Animator _animator;
        private bool _leftFootGrounded = true;
        private bool _rightFootGrounded = true;
        private int _hashGrounded;
        private Vector3 _leftFootUp = Vector3.up;
        private Vector3 _rightFootUp = Vector3.up;
        private float _originalColliderYCenter;
        private float _originalColliderZCenter;
        private float _originalColliderHeight;
        private float _leftFootDistanceToGround = 0f;
        private float _rightFootDistanceToGround = 0f;
        private float _feetHeightDifference;
        private bool _isIdling = true;
        private bool _isRotating = false;
        private bool _wasIdling = true;

        void Start()
        {
            _animator = GetComponent<Animator>();
            _hashGrounded = Animator.StringToHash("Grounded");
            _originalColliderYCenter = _collider.center.y;
            _originalColliderZCenter = _collider.center.z;
            _originalColliderHeight = _collider.height;
        }

        void Update()
        {
            if (_playerIKCopy != null)        
            {
                _playerIKCopy.position = transform.position;//new Vector3(transform.position.x, transform.position.y - _colliderYDifference, transform.position.z);
                _playerIKCopy.rotation = transform.rotation;
            }
            _isIdling = _movementComponent.IsIdling();
            _isRotating = _movementComponent.IsRotating();
            _feetHeightDifference = Mathf.Abs(_leftFootBone.position.y - _rightFootBone.position.y);
        }

        void FixedUpdate()
        {
            if (Physics.Raycast(_leftFootBoneRef.position + Vector3.up * _raycastOriginOffset, Vector3.down, out RaycastHit leftHit, _raycastDistance, _groundLayers))
            {
                _leftFootUp = leftHit.normal;
                _leftFootTarget.position = leftHit.point + leftHit.normal * _footOffset;
                
                Vector3 leftFootForward = Vector3.Cross(_leftFootUp, _leftFootBoneRef.right).normalized;
                Quaternion leftFootRotation = Quaternion.LookRotation(-leftFootForward, _leftFootUp);
                _leftFootTarget.rotation = Quaternion.Slerp(_leftFootTarget.rotation, leftFootRotation, Time.deltaTime * _footRotationSmoothing);

                _leftFootRig.weight = 1f;
                if ((_isIdling && _isRotating) || (_isIdling && !_wasIdling)){
                    _leftFootDistanceToGround = Vector3.Distance(_leftFootBone.position, leftHit.point);
                    _wasIdling = true;
                }
            }
            else
            {
                _leftFootRig.weight = 0f;
                if (Physics.Raycast(_leftFootBoneRef.position, Vector3.down, out leftHit, 100f, _groundLayers))
                {
                    if ((_isIdling && _isRotating) || (_isIdling && !_wasIdling)){
                        _leftFootDistanceToGround = Vector3.Distance(_leftFootBone.position, leftHit.point);
                        _wasIdling = true;
                    }
                }
                else
                {
                    _leftFootDistanceToGround = 0f;
                }
            }

            if (Physics.Raycast(_rightFootBoneRef.position + Vector3.up * _raycastOriginOffset, Vector3.down, out RaycastHit rightHit, _raycastDistance, _groundLayers))
            {
                _rightFootUp = rightHit.normal;
                _rightFootTarget.position = rightHit.point + rightHit.normal * _footOffset;

                Vector3 rightFootForward = Vector3.Cross(_rightFootUp, _rightFootBoneRef.right).normalized;
                Quaternion rightFootRotation = Quaternion.LookRotation(-rightFootForward, _rightFootUp);
                _rightFootTarget.rotation = Quaternion.Slerp(_rightFootTarget.rotation, rightFootRotation, Time.deltaTime * _footRotationSmoothing);
            
                _rightFootRig.weight = 1f;
                if ((_isIdling && _isRotating) || (_isIdling && !_wasIdling))
                {
                    _rightFootDistanceToGround = Vector3.Distance(_rightFootBone.position, rightHit.point);
                    _wasIdling = true;
                } 
            }
            else
            { 
                _rightFootRig.weight = 0f;
                if (Physics.Raycast(_rightFootBoneRef.position, Vector3.down, out rightHit, 100f, _groundLayers))
                {
                    if ((_isIdling && _isRotating) || (_isIdling && !_wasIdling))
                    {
                        _rightFootDistanceToGround = Vector3.Distance(_rightFootBone.position, rightHit.point);
                        _wasIdling = true;
                    } 
                }
                else
                {
                    _rightFootDistanceToGround = 0f;
                }
            }

            
            if (_isIdling)
            {
                float _greaterFootDistance = Mathf.Max(_leftFootDistanceToGround, _rightFootDistanceToGround);
                float _newYCenter = _originalColliderYCenter + Mathf.Max(_greaterFootDistance - _footOffset, 0f);
                _collider.center = new Vector3(0f, _newYCenter, _originalColliderZCenter);
                _collider.height = _originalColliderHeight - _feetHeightDifference * 1.5f;
            }
            else
            {
                _collider.center = new Vector3(0f, _originalColliderYCenter, _originalColliderZCenter);
                _collider.height = _originalColliderHeight;
                _wasIdling = false;
            }
        }
    }
}

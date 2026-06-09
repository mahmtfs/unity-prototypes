using UnityEngine;

namespace IK_Movement
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField] private Transform _cameraHolderTransform;
        [SerializeField] private float _cameraWallOffset = 0.1f;
        private Vector3 _offset;
        private float _baseFOV = 60f;
        private Camera _camera;
        private float _cameraDistance;
        void Start()
        {
            _camera = GetComponent<Camera>();
            _offset = transform.position - _cameraHolderTransform.position;
            _cameraDistance = _offset.magnitude;
        }

        void FixedUpdate()
        {
            float adjustedWallOffset = _cameraWallOffset * (_camera.fieldOfView / _baseFOV);
            Vector3 targetCameraPos = _cameraHolderTransform.position + (_cameraHolderTransform.rotation * _offset.normalized * _cameraDistance);
            Vector3 spherecastDirection = targetCameraPos - _cameraHolderTransform.position;

            if (Physics.SphereCast(_cameraHolderTransform.position, adjustedWallOffset, spherecastDirection, out RaycastHit hit, _cameraDistance + adjustedWallOffset))
            {
                transform.position = hit.point + hit.normal * adjustedWallOffset;
            }
            else
            {
                transform.position = targetCameraPos;
            }
        }
    }
}

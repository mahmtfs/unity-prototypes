using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace IK_Movement
{
    public class ControlsComponent : MonoBehaviour
    {
        [SerializeField] private Canvas _controlsCanvas;
        [SerializeField] private MovementComponent _movementComponent;
        [SerializeField] private Slider _mouseSensitivitySlider;
        [SerializeField] private Slider _fovSlider;
        [SerializeField] private Slider _soundsSlider;
        [SerializeField] private TMP_InputField _mouseSensitivityInput;
        [SerializeField] private TMP_InputField _fovInput;
        [SerializeField] private TMP_InputField _soundsInput;
        [SerializeField] private Animator _playerAnimator;
        [SerializeField] private Camera _playerCamera;
        [SerializeField] private AudioSource _audioSource;

        private bool _controlsEnabled = false;
        
        void Start()
        {
            _controlsCanvas.enabled = false;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        void Update()
        {
            if (Keyboard.current.tKey.wasPressedThisFrame && _controlsEnabled)
            {
                _controlsEnabled = false;
                _controlsCanvas.enabled = false;
                Time.timeScale = 1f;
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                _playerAnimator.speed = 1f;
                _movementComponent.enabled = true;
            }
            else if (Keyboard.current.tKey.wasPressedThisFrame && !_controlsEnabled)
            {
                _controlsEnabled = true;
                _controlsCanvas.enabled = true;
                Time.timeScale = 0f;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                _playerAnimator.speed = 0f;
                _movementComponent.enabled = false;
            }
        }
        
        public void OnMouseSensitivitySlider()
        {
            float inputValue = PrepareFloatInput(_mouseSensitivitySlider.value, _mouseSensitivitySlider.minValue, _mouseSensitivitySlider.maxValue);
            _mouseSensitivityInput.text = inputValue.ToString("0.00");
            _movementComponent.SetMouseSensitivity(inputValue);
        }

        public void OnFOVSlider()
        {
            float inputValue = PrepareFloatInput(_fovSlider.value, _fovSlider.minValue, _fovSlider.maxValue);
            _fovInput.text = inputValue.ToString("0.00");
            _playerCamera.fieldOfView = inputValue;
        }

        public void OnSoundsSlider()
        {
            float inputValue = PrepareFloatInput(_soundsSlider.value, _soundsSlider.minValue, _soundsSlider.maxValue);
            _soundsInput.text = inputValue.ToString("0.00");
            _audioSource.volume = inputValue;
        }

        float PrepareFloatInput(float rawInput, float minValue, float maxValue)
        {
            float clampedValue = Mathf.Clamp(rawInput, minValue, maxValue);
            return Mathf.Round(clampedValue * 100f) / 100f;
        }
    }
}
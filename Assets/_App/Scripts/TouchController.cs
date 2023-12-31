using UnityEngine;

namespace _App.Scripts
{
    public class TouchController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private WheelController _wheelController;
        [SerializeField] private Transform _spinningCircle;

        [Header("Parameters")]
        [SerializeField] private float _rotationSpeed;

        private Touch _touch;
        private Vector2 _touchPosition;
        private Quaternion _rotation;

        private void Update()
        {
            if (Input.touchCount <= 0 || _wheelController.IsSpinning) return;

            _touch = Input.GetTouch(0);
            if (_touch.phase != TouchPhase.Moved) return;

            _rotation = Quaternion.Euler(0f, 0f, _touch.deltaPosition.y * _rotationSpeed);
            _spinningCircle.transform.rotation *= _rotation;
        }
    }
}
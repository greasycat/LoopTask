using UnityEngine;

namespace Landmarks.Scripts
{
    public class LM_ObserverCameraController : MonoBehaviour
    {
        private Camera _camera;

        public float zoomSpeed = 10f;
        public float zoomTime = 0.1f;

        public float maxHeight = 12;
        public float minHeight = 7f;
    
        private float zoomVelocity;
        private float targetHeight; 
    
        // Start is called before the first frame update
        private void Start()
        {
            _camera = GetComponent<Camera>();
        }

        // Update is called once per frame
        void Update()
        {
            targetHeight += Input.GetAxis("Mouse ScrollWheel") * zoomSpeed * -1f;
            targetHeight = Mathf.Clamp(targetHeight, minHeight, maxHeight);
            _camera.orthographicSize = Mathf.SmoothDamp(_camera.orthographicSize, targetHeight, ref zoomVelocity, zoomTime);

        }
    }
}

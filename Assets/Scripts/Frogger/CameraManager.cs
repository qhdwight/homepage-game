using UnityEngine;
using UnityEngine.U2D;

namespace Frogger
{
    public static class CameraManager
    {
        private static PixelPerfectCamera _camera;

        [RuntimeInitializeOnLoadMethod]
        private static void Initialize() => _camera = Object.FindObjectOfType<PixelPerfectCamera>();

        public static Bounds Bounds
        {
            get
            {
                var cameraSize = new Vector3(_camera.refResolutionX / (float)_camera.assetsPPU, _camera.refResolutionY / (float)_camera.assetsPPU);
                var cameraBounds = new Bounds(Vector3.zero, cameraSize);
                return cameraBounds;
            }
        }
    }
}
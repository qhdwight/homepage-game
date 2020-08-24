using UnityEngine;
using UnityEngine.U2D;

namespace Frogger
{
    [RequireComponent(typeof(BoxCollider2D)), DefaultExecutionOrder(10)]
    public class MovingBehavior : MonoBehaviour
    {
        [SerializeField] private Vector3 m_Speed = default;
        [SerializeField] private Color[] m_Colors = default;
        [SerializeField] private bool m_IsKilling = default, m_IsCarrying = default;
        
        protected FroggerBehavior m_Frogger;
        private bool m_LastIntersecting;
        private PixelPerfectCamera m_Camera;

        public bool IsKilling => m_IsKilling;
        public bool IsCarrying => m_IsCarrying;
        public BoxCollider2D Box { get; private set; }

        private void Start()
        {
            Box = GetComponent<BoxCollider2D>();
            m_Frogger = FindObjectOfType<FroggerBehavior>();
            m_Camera = FindObjectOfType<PixelPerfectCamera>();
            if (transform.Find("Container") is Transform container && container.TryGetComponent(out SpriteRenderer sprite))
                sprite.color = m_Colors[Random.Range(0, m_Colors.Length)];
        }

        private void Update()
        {
            transform.Translate(m_Speed * Time.deltaTime, Space.World);

            var cameraSize = new Vector3(m_Camera.refResolutionX / (float) m_Camera.assetsPPU, m_Camera.refResolutionY / (float) m_Camera.assetsPPU);
            var cameraBounds = new Bounds(Vector3.zero, cameraSize);
            if (Box.bounds.min.x > cameraBounds.max.x)
                transform.Translate(new Vector3 {x = -cameraSize.x - Box.size.x}, Space.World);
            if (Box.bounds.max.x < cameraBounds.min.x)
                transform.Translate(new Vector3 {x = cameraSize.x + Box.size.x}, Space.World);
        }

        public bool HandleFrogger(bool isIntersecting)
        {
            bool isDifferent = isIntersecting != m_LastIntersecting;
            m_LastIntersecting = isIntersecting;
            return isDifferent;
        }
    }
}
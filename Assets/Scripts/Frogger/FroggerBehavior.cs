using System.Collections;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

namespace Frogger
{
    public class FroggerBehavior : MonoBehaviour
    {
        [SerializeField] private int m_StartingHealth = 3;
        [SerializeField] private Sprite m_WaterSprite = default;
        [SerializeField] private float m_DieSeconds = 1.0f;

        private Animator m_Animator;
        private Tilemap m_Tilemap;
        private Vector3 m_Spawn;
        private GameObject[] m_Hearts;
        private int m_Health;
        private bool m_IsDead;
        private Transform m_Carrier;
        private MovingBehavior[] m_Behaviors;
        private Vector3 m_Input;
        private BoxCollider2D m_Box;
        private BoxCollider2D[] m_CaptureFrogs;

        private Transform Carrier
        {
            get => m_Carrier;
            set
            {
                m_Carrier = value;
                transform.SetParent(m_Carrier);
            }
        }

        private void Start()
        {
            m_Box = GetComponent<BoxCollider2D>();
            m_Animator = GetComponent<Animator>();
            m_Tilemap = FindObjectOfType<Tilemap>();
            m_Behaviors = FindObjectsOfType<MovingBehavior>();
            m_Hearts = GameObject.FindGameObjectsWithTag("Heart").ToArray();
            m_Spawn = transform.position;
            m_CaptureFrogs = FindObjectsOfType<CaptureFrogBehavior>().Select(behavior => behavior.GetComponent<BoxCollider2D>()).ToArray();
            SetHearts(m_StartingHealth);
        }

        private void Update()
        {
            if (m_IsDead) return;

            Sprite sprite = null;
            Vector3Int? nextCellPosition = null;
            Transform t = transform;

            if (m_Input.sqrMagnitude > Mathf.Epsilon)
            {
                t.rotation = Quaternion.AngleAxis(Mathf.Rad2Deg * Mathf.Atan2(-m_Input.x, m_Input.y), Vector3.forward);
                m_Animator.Play("Hop");

                nextCellPosition = m_Tilemap.layoutGrid.WorldToCell(t.position + m_Input);
                sprite = m_Tilemap.GetSprite(nextCellPosition.Value);
                if (sprite) t.position += m_Input;
                else nextCellPosition = null;
            }

            Bounds cameraBounds = CameraManager.Bounds;
            if (m_Box.bounds.min.x > cameraBounds.max.x) Kill();
            if (m_Box.bounds.max.x < cameraBounds.min.x) Kill();

            foreach (MovingBehavior behavior in m_Behaviors)
            {
                bool isIntersecting = m_Box.bounds.Intersects(behavior.Box.bounds);
                if (behavior.HandleFrogger(isIntersecting))
                {
                    if (behavior.IsKilling && isIntersecting) Kill();
                    else if (behavior.IsCarrying)
                    {
                        if (Carrier == behavior.transform) Carrier = null;
                        else if (isIntersecting) Carrier = behavior.transform;
                    }
                }
            }

            foreach (BoxCollider2D captureCollider in m_CaptureFrogs)
            {
                if (captureCollider.isActiveAndEnabled && m_Box.bounds.Intersects(captureCollider.bounds))
                {
                    captureCollider.gameObject.SetActive(false);
                    static IEnumerator CaptureEnumerator()
                    {
                        yield return InterfaceBehavior.SetText("You won!");
                        yield return SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);
                    }
                    StartCoroutine(CaptureEnumerator());
                }
            }

            if (!Carrier && sprite == m_WaterSprite)
                Kill();

            if (!Carrier && nextCellPosition is { } cellPosition)
                t.position = m_Tilemap.CellToWorld(cellPosition) + new Vector3(0.5f, 0.5f);
        }

        private void LateUpdate() => m_Input = Vector3.zero;

        [UsedImplicitly]
        public void OnMove(InputValue input) => m_Input = input.Get<Vector2>();

        [UsedImplicitly]
        public void OnTouch(InputValue input)
        {
            Vector3 position = transform.position,
                    mousePosition = Camera.allCameras.First().ScreenToWorldPoint(input.Get<Vector2>());
            mousePosition.z = position.z;
            Vector3 direction = (mousePosition - position).normalized;
            if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
                m_Input.x = Mathf.Sign(direction.x);
            else
                m_Input.y = Mathf.Sign(direction.y);
        }

        private void DecrementHearts()
        {
            int health = m_Health - 1;
            IEnumerator DecrementEnumerator()
            {
                m_IsDead = true;
                m_Animator.Play("Die");
                yield return new WaitForSeconds(m_DieSeconds);
                if (health == 0)
                {
                    yield return InterfaceBehavior.SetText("Game Over!");
                    yield return SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);
                }
                else
                {
                    m_Animator.Play("Idle");
                    m_IsDead = false;
                    transform.SetPositionAndRotation(m_Spawn, Quaternion.identity);
                }
            }
            StartCoroutine(DecrementEnumerator());
            SetHearts(health);
        }

        private void SetHearts(int health)
        {
            m_Health = health;
            for (var i = 0; i < m_StartingHealth; i++)
                m_Hearts[i].SetActive(i < health);
        }

        private void Kill()
        {
            if (m_IsDead) return;

            DecrementHearts();
        }
    }
}
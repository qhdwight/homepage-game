using UnityEngine;
using UnityEngine.U2D;

[RequireComponent(typeof(BoxCollider2D))]
public class ObstacleBehavior : MonoBehaviour
{
    private BoxCollider2D m_Box;
    private FroggerBehavior m_Frogger;
    private PixelPerfectCamera m_Camera;

    [SerializeField] private Vector3 m_Speed = default;
    [SerializeField] private Color[] m_Colors = default;

    private void Start()
    {
        m_Box = GetComponent<BoxCollider2D>();
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
        if (m_Box.bounds.min.x > cameraBounds.max.x)
            transform.Translate(new Vector3 {x = -cameraSize.x - m_Box.size.x}, Space.World);
        if (m_Box.bounds.max.x < cameraBounds.min.x)
            transform.Translate(new Vector3 {x = cameraSize.x + m_Box.size.x}, Space.World);

        if (!m_Box.bounds.Intersects(m_Frogger.Box.bounds)) return;

        m_Frogger.OnHit();
    }
}
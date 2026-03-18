using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Balloon : MonoBehaviour
{
    public Transform balloonMesh;
    public GameObject balloonFragmentPrefab;
    public Renderer balloonRenderer;
    public Color startColor = Color.green;
    public Color endColor = Color.red;

    public float inflateSpeed = 0.5f;
    public float maxScale = 140f;
    public float durationSeconds = 10f;
    public float minDurationSeconds = 5f;
    public int fragmentCount = 50;
    public float fragmentSpreadForce = 2f;
    private Vector3 baseBalloonPosition;
    private AudioSource audioSource;
    public AudioClip explodeSound;
    public AudioClip[] inflateSounds;

    private Vector3 startScale;
    private Vector3 startPosition;
    private Vector3 originalBalloonLocalPos;
    private List<GameObject> fragments = new List<GameObject>();

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        startScale = balloonMesh.localScale;
        startPosition = balloonMesh.localPosition;
        originalBalloonLocalPos = balloonMesh.localPosition;
        GameManager.Instance.StartTimer(durationSeconds, minDurationSeconds);
        GameManager.Instance.OnTimerEnded += HandleTimeout;
        balloonRenderer.material.color = startColor;
    }

    void HandleTimeout()
    {
        GameManager.Instance.NotifyFail();
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnTimerEnded -= HandleTimeout;
        }
    }

    void Update()
    {
        if (Input.GetButtonDown("P1_B1") || Input.GetButtonDown("P2_B1"))
        {
            Inflate();
        }

        balloonMesh.localPosition = baseBalloonPosition;
    }

    void Inflate()
    {
        balloonMesh.localScale += new Vector3(5f, 6f, 5f) * inflateSpeed;
        PlayInflateSound();
        UpdateBalloonColor();
        AdjustBalloonPosition();

        if (balloonMesh.localScale.x >= maxScale)
        {
            Explode();
        }
    }

    void AdjustBalloonPosition()
    {
        float scaleProgress = Mathf.InverseLerp(startScale.x, maxScale, balloonMesh.localScale.x);

        float liftAmount = scaleProgress * 0.6f;

        baseBalloonPosition = startPosition + new Vector3(0, liftAmount, 0);
    }

    void PlayInflateSound()
    {
        if (inflateSounds.Length == 0) return;

        int randomIndex = Random.Range(0, inflateSounds.Length);

        audioSource.pitch = Random.Range(0.95f, 1.05f);
        audioSource.PlayOneShot(inflateSounds[randomIndex]);
    }

    void UpdateBalloonColor()
    {
        float currentScale = balloonMesh.localScale.x;
        float progress = Mathf.InverseLerp(startScale.x, maxScale, currentScale);
        Color newColor = Color.Lerp(startColor, endColor, progress);

        balloonRenderer.material.color = newColor;
    }

    void DropTheRest()
    {
        Rigidbody rb = gameObject.AddComponent<Rigidbody>();
        rb.mass = 0.3f;
        rb.useGravity = true;

        StartCoroutine(DropTheRestCoroutine());
    }

    IEnumerator DropTheRestCoroutine()
    {
        yield return new WaitForSeconds(1f);

        foreach (GameObject fragment in fragments)
        {
            if (fragment != null)
                Destroy(fragment);
        }
        fragments.Clear();

        GameManager.Instance.NotifyWin();
        Destroy(gameObject);
    }

    void SpawnFragments()
    {
        Vector3 center = balloonMesh.position;

        for (int i = 0; i < fragmentCount; i++)
        {
            Vector3 randomOffset = Random.insideUnitSphere * 0.5f;
            GameObject fragment = Instantiate(balloonFragmentPrefab, center + randomOffset, Random.rotation);

            Rigidbody fragRb = fragment.GetComponent<Rigidbody>();
            if (fragRb == null)
                fragRb = fragment.AddComponent<Rigidbody>();

            fragRb.mass = 0.05f;
            fragRb.AddExplosionForce(fragmentSpreadForce, center, 2f, 0.5f, ForceMode.Impulse);

            fragments.Add(fragment);
        }
    }

    void Explode()
    {
        audioSource.PlayOneShot(explodeSound);
        Destroy(balloonMesh.gameObject);
        SpawnFragments();
        DropTheRest();
    }
}
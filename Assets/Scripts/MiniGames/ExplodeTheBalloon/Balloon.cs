using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Balloon : MonoBehaviour
{
    public Transform balloonMesh;
    public GameObject balloonFragmentPrefab;

    public float inflateSpeed = 0.5f;
    public float maxScale = 140f;
    public float durationSeconds = 15f;
    public float minDurationSeconds = 8f;
    public int fragmentCount = 20;
    public float fragmentSpreadForce = 5f;

    private Vector3 startScale;
    private List<GameObject> fragments = new List<GameObject>();

    void Start()
    {
        startScale = balloonMesh.localScale;
        GameManager.Instance.StartTimer(durationSeconds, minDurationSeconds);
        GameManager.Instance.OnTimerEnded += HandleTimeout;
        GameManager.Instance.OnMinigameWon += AfterWin;
    }

    void HandleTimeout()
    {
        GameManager.Instance.NotifyFail();
    }
    void AfterWin()
    {
        GameManager.Instance.AddRound();
    }

    void Update()
    {
        if (Input.GetButtonDown("P1_B1") || Input.GetButtonDown("P2_B1"))
        {
            Inflate();
        }
    }

    void Inflate()
    {
        balloonMesh.localScale += new Vector3(5f, 6f, 5f) * inflateSpeed;

        if (balloonMesh.localScale.x >= maxScale)
        {
            Explode();
        }
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
        yield return new WaitForSeconds(2f);

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
        Debug.Log("BOOM");
        Destroy(balloonMesh.gameObject);
        SpawnFragments();
        DropTheRest();
    }
}
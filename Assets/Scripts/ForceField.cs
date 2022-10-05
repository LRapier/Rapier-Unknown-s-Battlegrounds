using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForceField : MonoBehaviour
{
    public float shrinkWaitTime;
    public float shrinkAmount;
    public float shrinkDuration;
    public float minShrinkAmount;

    public int ffDamage;

    private float lastShrinkEndTime;
    private bool isShrinking;
    private float targetDiameter;
    private float lastPlayerCheckTime;

    void Start()
    {
        lastShrinkEndTime = Time.time;
        targetDiameter = transform.localScale.x;
    }

    void Update()
    {
        if(isShrinking)
        {
            transform.localScale = Vector3.MoveTowards(transform.localScale, Vector3.one * targetDiameter, (shrinkAmount / shrinkDuration) * Time.deltaTime);

            if(transform.localScale.x == targetDiameter)
                isShrinking = false;
        }
        else
        {
            if (Time.time - lastShrinkEndTime >= shrinkWaitTime && transform.localScale.x > minShrinkAmount)
                Shrink();
        }

        CheckPlayers();
    }

    void Shrink()
    {
        isShrinking = true;

        if (transform.localScale.x - shrinkAmount > minShrinkAmount)
            targetDiameter -= shrinkAmount;
        else
            targetDiameter = minShrinkAmount;

        lastShrinkEndTime = Time.time + shrinkDuration;
    }

    void CheckPlayers()
    {
        if(Time.time - lastPlayerCheckTime > 1.0f)
        {
            lastPlayerCheckTime = Time.time;

            foreach(PlayerController player in GameManager.instance.players)
            {
                if (player.dead || !player)
                    continue;

                if(Vector3.Distance(Vector3.zero, player.transform.position) >= transform.localScale.x)
                {
                    player.photonView.RPC("TakeDamage", player.photonPlayer, 0, ffDamage);
                }
            }
        }
    }
}

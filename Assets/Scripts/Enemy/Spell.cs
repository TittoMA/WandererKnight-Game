using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spell : MonoBehaviour
{

    [SerializeField] private float lifeDuration = 3.5f;
    [SerializeField] private float spellDamage;
    [SerializeField] private GameObject hitVFX;

    AudioSource explosionSfx;

    private void Awake()
    {
        GameObject hitSound = GameObject.FindWithTag("ExplosionSfx");
        explosionSfx = hitSound.GetComponent<AudioSource>();
        Destroy(gameObject, lifeDuration);
    }

    private void OnCollisionEnter(Collision collision)
    {
        ContactPoint contact = collision.contacts[0];
        Quaternion rot = Quaternion.FromToRotation(Vector3.up, contact.normal);
        Vector3 pos = contact.point;
        Destroy(Instantiate(hitVFX, pos, rot), 1.5f);

        if (collision.gameObject.layer == 6)
        {
            // Debug.Log("player Hit by spell");
            collision.gameObject.GetComponent<PlayerStat>().TakeDamage(spellDamage);
            collision.gameObject.GetComponent<PlayerStat>().HitVFX(contact.point);
        }

        explosionSfx.Play();
        Destroy(gameObject);
    }
}

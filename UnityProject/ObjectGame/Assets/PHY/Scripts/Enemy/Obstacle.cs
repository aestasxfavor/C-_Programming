using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : MonoBehaviour
{
    [SerializeField] private float speed = 3f;
    private Vector2 dir;
    // Update is called once per frame
    private void Update()
    {
        transform.Translate(dir * speed * Time.deltaTime);

        if (Mathf.Abs(transform.position.x) > 13f || Mathf.Abs(transform.position.y) > 13f)
        {
            gameObject.SetActive(false);
        }
    }

    public void SetDirection(Vector2 direction)
    {
        dir = direction.normalized;
    }

    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
    }
}

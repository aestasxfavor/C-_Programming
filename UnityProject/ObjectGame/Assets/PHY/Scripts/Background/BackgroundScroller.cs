using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundScroller : MonoBehaviour
{
    [SerializeField] private float scrollSpeed = 2f;
    [SerializeField] private float resetY= -10f;
    [SerializeField] private float startY= 10f;

   private void Update()
    {
        transform.Translate(Vector3.down * scrollSpeed * Time.deltaTime);

        if(transform.position.y <= resetY)
        {
            Vector3 newPosition = new Vector3(transform.position.x, startY, transform.position.z);
            transform.position = newPosition;
        }
    }
}

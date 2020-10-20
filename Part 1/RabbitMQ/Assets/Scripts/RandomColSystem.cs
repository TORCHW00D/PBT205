using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RandomColSystem : MonoBehaviour
{

    private Image Sprite;
    private int spriteIndex = 0;
    public Sprite[] Alt;

    void Start()
    {
        Sprite = gameObject.GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        spriteIndex++;
        if (spriteIndex == Alt.Length)
            spriteIndex = 0;
        
        Sprite.color = new Vector4(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), 1.0f);
        Sprite.sprite = Alt[spriteIndex];
    }
}

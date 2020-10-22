using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.Tilemaps;


struct Entity
{
    public int Xpos, Ypos;
    public string Identifier;
}

public class TilingSystem : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject tileableObject;
    
    public int Width, Height;
    public int EntityCount;

    private bool hasUpdated = false;

    private List<GameObject> tiles;
    private List<Entity> Entities;
    void Start()
    {
        tiles = new List<GameObject>();
        Entities = new List<Entity>();
        for(int i = 0; i < Width; i++)
        {
            for(int j = 0; j < Height; j++)
            {
                GameObject temp = Instantiate(tileableObject, gameObject.transform);
                temp.transform.position = new Vector3(i * 0.3f, j * 0.3f, 0);
                
                tiles.Add(temp);
            }
        }
        Debug.Log("tiles count: " + tiles.Count);
        for(int i = 0; i < EntityCount; i++)
        {
            Entity temp;
            temp.Xpos= Random.Range(0,Width);
            temp.Ypos= Random.Range(0,Width);
            temp.Identifier = i.ToString();
            Entities.Add(temp);

            //          takes x pos times witth  + y pos                                                              an make it preddy
            tiles[temp.Xpos * Width + temp.Ypos].gameObject.GetComponent<SpriteRenderer>().color = Color.cyan;
        }       
    }

    // Update is called once per frame
    void Update()
    {   
        if(Input.GetKeyDown(KeyCode.DownArrow))
        {
            //Entities.ElementAt<Entity>(0).Xpos += 1;
            if (Entities[0].Ypos > 0)
            {
                Entities[0] = new Entity { Xpos = Entities[0].Xpos, Ypos = Entities[0].Ypos - 1, Identifier = Entities[0].Identifier };
                hasUpdated = false;
            }
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (Entities[0].Ypos < Height - 1)
            {
                Entities[0] = new Entity { Xpos = Entities[0].Xpos, Ypos = Entities[0].Ypos + 1, Identifier = Entities[0].Identifier };
                hasUpdated = false;
            }
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (Entities[0].Xpos > 0)
            {
                Entities[0] = new Entity { Xpos = Entities[0].Xpos - 1, Ypos = Entities[0].Ypos, Identifier = Entities[0].Identifier };
                hasUpdated = false;
            }
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (Entities[0].Xpos < Width - 1)
            {
                Entities[0] = new Entity { Xpos = Entities[0].Xpos + 1, Ypos = Entities[0].Ypos, Identifier = Entities[0].Identifier };
                hasUpdated = false;
            }
        }

        for(int i = 0; i < tiles.Count; i++)
        {
            tiles[i].GetComponent<SpriteRenderer>().color = Color.white;
        }
        for (int i = 1; i < EntityCount; i++)
        {
            tiles[Entities[i].Xpos * Width + Entities[i].Ypos].GetComponent<SpriteRenderer>().color = Color.cyan;
            if(Entities[i].Xpos == Entities[0].Xpos && Entities[i].Ypos == Entities[0].Ypos && !hasUpdated)
            {
                hasUpdated = true;
                Debug.Log("Lmao, collision: " + Entities[i].Identifier + " and " + Entities[0].Identifier + " have bonked UwU");
            }
        }
        tiles[Entities[0].Xpos * Width + Entities[0].Ypos].GetComponent<SpriteRenderer>().color = Color.red;
    }
}

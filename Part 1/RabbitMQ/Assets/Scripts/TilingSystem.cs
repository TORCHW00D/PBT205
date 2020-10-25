using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

struct Entity
{
    public int Xpos, Ypos;
    public string Identifier;
    public int MoveTime;
    public Color color;
    public bool HasUpdated;
    public List<string> CollisionList;
}

public class TilingSystem : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject tileableObject;
    public TilingSystemNetcode MessageService;

    public GameObject IDPanel;
    public Text[] texts;

    public int Width, Height;
    public int EntityCount;

    private bool UpdatesAllowed = true;

    private List<GameObject> tiles;
    private List<Entity> Entities;

    public List<string> CollisionCatalogue;

    void Start()
    {
        MessageService.TilingSystemSetup("simpleUser","12345"); //setup message service for NETCODE & RabbitMQ
        
        IDPanel.SetActive(false);
        tiles = new List<GameObject>();
        Entities = new List<Entity>();
        for(int i = 0; i < Width; i++)
        {
            for(int j = 0; j < Height; j++)
            {
                GameObject temp = Instantiate(tileableObject, gameObject.transform);
                temp.transform.position = new Vector3(i * 0.3f, j * 0.3f, 0) + gameObject.transform.position;
                temp.AddComponent<BoxCollider2D>();
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
            temp.MoveTime = (int)Time.time + 5;
            temp.color = new Color(Random.Range(0, 1000) / 1000.0f, Random.Range(0, 1000) / 1000.0f, Random.Range(0, 1000) / 1000.0f);
            temp.HasUpdated = false;
            temp.CollisionList = new List<string>();
            Entities.Add(temp);

            //          takes x pos times witth  + y pos                                       an make it preddy
            tiles[temp.Xpos * Width + temp.Ypos].gameObject.GetComponent<SpriteRenderer>().color = temp.color;
        }       
    }
    
    // Update is called once per frame
    void Update()
    {   
        if(Input.GetMouseButtonDown(0)) //on mouse down left click
        {
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero); //cast form camera to world

            if (hit.collider != null) //if we hit something that has a collider
            {
                GameObject clicked = hit.collider.gameObject; //copy the collider
                if(tiles.Exists(go => go.transform == clicked.transform)) //search the list of tiles for this tile, checking it exists
                {
                    //Debug.Log("Found tile!");
                    if( clicked.GetComponent<SpriteRenderer>().color != Color.white) //if we're not a basic bitch tile
                    {
                        Color SearchCol = clicked.GetComponent<SpriteRenderer>().color; //save the colour of the tile
                        for(int i = 0; i < EntityCount; i++) //search linearly for the entity with this colour 
                        {
                            if(SearchCol == Entities[i].color) //when we find it
                            {
                                string temp = "";
                                for (int j = 0; j < Entities[i].CollisionList.Count; j++)
                                {
                                    temp += Entities[i].CollisionList[j] + ", "; //add each collision 
                                }
                                //Debug.Log("Found tile clicked! Tile ID: " + Entities[i].Identifier + " collision list is: " + temp);
                                IDPanel.SetActive(true);
                                texts[0].text = Entities[i].Identifier;
                                texts[1].text = temp;

                                string SentMessageTemp = texts[0].text + " entity has hit: " + texts[1].text; //really bad compile strat
                                MessageService.SendMessageTilingSystem(SentMessageTemp, messageClass.querySend); //compile and send message to RabbitMQ
                                Debug.Log("Message sent!"); //debugging for sake's sake
                                UpdatesAllowed = false;
                            }
                        }
                    }
                }
            }
        }

        if (UpdatesAllowed)
        {

            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                //Entities.ElementAt<Entity>(0).Xpos += 1;
                if (Entities[0].Ypos > 0)
                {
                    Entities[0] = new Entity { Xpos = Entities[0].Xpos, Ypos = Entities[0].Ypos - 1, Identifier = Entities[0].Identifier, HasUpdated = false, CollisionList = Entities[0].CollisionList };
                }
            }
            else if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                if (Entities[0].Ypos < Height - 1)
                {
                    Entities[0] = new Entity { Xpos = Entities[0].Xpos, Ypos = Entities[0].Ypos + 1, Identifier = Entities[0].Identifier, HasUpdated = false, CollisionList = Entities[0].CollisionList };
                }
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                if (Entities[0].Xpos > 0)
                {
                    Entities[0] = new Entity { Xpos = Entities[0].Xpos - 1, Ypos = Entities[0].Ypos, Identifier = Entities[0].Identifier, HasUpdated = false, CollisionList = Entities[0].CollisionList };
                }
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                if (Entities[0].Xpos < Width - 1)
                {
                    Entities[0] = new Entity { Xpos = Entities[0].Xpos + 1, Ypos = Entities[0].Ypos, Identifier = Entities[0].Identifier, HasUpdated = false, CollisionList = Entities[0].CollisionList };
                }
            }

            for (int i = 0; i < tiles.Count; i++)
            {
                tiles[i].GetComponent<SpriteRenderer>().color = Color.white;
            }
            for (int i = 1; i < EntityCount; i++)
            {
                if (Entities[i].CollisionList.Count > 15)
                {
                    Entities[i].CollisionList.RemoveAt(0);
                }

                if (Entities[i].MoveTime == (int)Time.time)
                {
                    int TempDir = Random.Range(1, 9);
                    switch (TempDir)
                    {
                        case 1: //up
                            if (Entities[i].Ypos < Height - 1)
                                Entities[i] = new Entity { Xpos = Entities[i].Xpos, Ypos = Entities[i].Ypos + 1, Identifier = Entities[i].Identifier, MoveTime = Entities[i].MoveTime, color = Entities[i].color, CollisionList = Entities[0].CollisionList };
                            break;
                        case 2: //up right
                            if (Entities[i].Ypos < Height - 1 && Entities[i].Xpos < Width - 1)
                                Entities[i] = new Entity { Xpos = Entities[i].Xpos + 1, Ypos = Entities[i].Ypos + 1, Identifier = Entities[i].Identifier, MoveTime = Entities[i].MoveTime, color = Entities[i].color, CollisionList = Entities[0].CollisionList };
                            break;
                        case 3: //right
                            if (Entities[i].Xpos < Width - 1)
                                Entities[i] = new Entity { Xpos = Entities[i].Xpos + 1, Ypos = Entities[i].Ypos, Identifier = Entities[i].Identifier, MoveTime = Entities[i].MoveTime, color = Entities[i].color, CollisionList = Entities[0].CollisionList };
                            break;
                        case 4: //down right
                            if (Entities[i].Ypos > 0 && Entities[i].Xpos < Width - 1)
                                Entities[i] = new Entity { Xpos = Entities[i].Xpos + 1, Ypos = Entities[i].Ypos - 1, Identifier = Entities[i].Identifier, MoveTime = Entities[i].MoveTime, color = Entities[i].color, CollisionList = Entities[0].CollisionList };
                            break;
                        case 5: //down
                            if (Entities[i].Ypos > 0)
                                Entities[i] = new Entity { Xpos = Entities[i].Xpos, Ypos = Entities[i].Ypos - 1, Identifier = Entities[i].Identifier, MoveTime = Entities[i].MoveTime, color = Entities[i].color, CollisionList = Entities[0].CollisionList };
                            break;
                        case 6: //down left
                            if (Entities[i].Ypos > 0 && Entities[i].Xpos > 0)
                                Entities[i] = new Entity { Xpos = Entities[i].Xpos - 1, Ypos = Entities[i].Ypos - 1, Identifier = Entities[i].Identifier, MoveTime = Entities[i].MoveTime, color = Entities[i].color, CollisionList = Entities[0].CollisionList };
                            break;
                        case 7: //left
                            if (Entities[i].Xpos > 0)
                                Entities[i] = new Entity { Xpos = Entities[i].Xpos - 1, Ypos = Entities[i].Ypos, Identifier = Entities[i].Identifier, MoveTime = Entities[i].MoveTime, color = Entities[i].color, CollisionList = Entities[0].CollisionList };
                            break;
                        case 8: //up left
                            if (Entities[i].Ypos < Height - 1 && Entities[i].Xpos > 0)
                                Entities[i] = new Entity { Xpos = Entities[i].Xpos - 1, Ypos = Entities[i].Ypos + 1, Identifier = Entities[i].Identifier, MoveTime = Entities[i].MoveTime, color = Entities[i].color, CollisionList = Entities[0].CollisionList };
                            break;
                    }
                    Entities[i] = new Entity { Xpos = Entities[i].Xpos, Ypos = Entities[i].Ypos, Identifier = Entities[i].Identifier, MoveTime = (int)Time.time + Random.Range(1, 6), color = Entities[i].color, HasUpdated = false, CollisionList = Entities[0].CollisionList };
                }
                tiles[Entities[i].Xpos * Width + Entities[i].Ypos].GetComponent<SpriteRenderer>().color = Entities[i].color;
            }

            for (int i = 0; i < EntityCount - 1; i++)
            {
                for (int k = i + 1; k < EntityCount; k++)
                {
                    if (Entities[i].Xpos == Entities[k].Xpos && Entities[i].Ypos == Entities[k].Ypos && !Entities[i].HasUpdated)
                    {
                        Entities[i] = new Entity { Xpos = Entities[i].Xpos, Ypos = Entities[i].Ypos, Identifier = Entities[i].Identifier, MoveTime = Entities[i].MoveTime, color = Entities[i].color, HasUpdated = true, CollisionList = Entities[i].CollisionList };
                        Entities[i].CollisionList.Add(Entities[k].Identifier);
                        Entities[k].CollisionList.Add(Entities[i].Identifier);
                        string temp = Entities[i].Identifier + " hit " + Entities[k].Identifier + " at " + Entities[i].Xpos.ToString() + "@" + Entities[i].Ypos.ToString();

                        MessageService.SendMessageTilingSystem(temp, messageClass.positionUpdate);
                        //Debug.Log(temp);

                        CollisionCatalogue.Add(temp);
                    }
                }
            }
            if (CollisionCatalogue.Count > 15)
            {
                CollisionCatalogue.RemoveAt(0);
            }
            tiles[Entities[0].Xpos * Width + Entities[0].Ypos].GetComponent<SpriteRenderer>().color = Color.red;
        }
    }

    public void CloseButton()
    {
        UpdatesAllowed = true;
        IDPanel.SetActive(false);
    }

    void OnGUI()
    {
        for(int i = 0; i < CollisionCatalogue.Count; i++)
        {
            GUI.Label(new Rect(10, 12 * i, 1000, 50), (CollisionCatalogue[i]));
        }
    }

}

using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using JetBrains.Annotations;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using UnityEngine;
using UnityEngine.UI;

public class SimpleMessage
{
    public string Username;
    public string Message;
}

public class SimpleRabMQSctipt : MonoBehaviour
{
    /*Rabbit MQ learning phase
     * 
     * Before an application can use Rabbit MQ, it has to connect to a RabbitMQ node. This CONNECTION should be long lasting and SHOULD BE USED FOR EACH FOLLOWING ACTION
     * Opening a new connection for pushing a message is INNEFIECIENT and not recommended.
     * 
     * To open a connection, instantiate a [ConnectionFactory] & configure it to use the desired hostname, virtual host, credentials TLS settings and any other params
     * 
     * The code snippets in the following section connect to a RabbitMQ node on [hostname]  
     */

    private ConnectionFactory factory;
    private IConnection conn;
    private IModel channel, TilingChannel, StocksChannel;

    private EventingBasicConsumer consumer, TilingConsumer, StocksConsumer;

    private List<SimpleMessage> List_MessageCollection; //msgs from otehr users

    private List<TilingMessage> List_TilingMessages_Collision; //messages for tile collisions
    private List<TilingMessage> List_TilingMessages_PositionUpdates; //messages for tile updates

    private List<StocksMessageStruct> List_StocksMessaging_MarketPrice; //messages for stock market price updates
    private List<StocksMessageStruct> List_StocksMessaging_Sell_n_BuyRequests; //messages for stock market price sell and buy requests

    public Text TextBoxSend;
    private string Username;
    private string Password;
    private string ConnectionHost;
    private string ConnectionVirtualHost;


    public GameObject messageBox;
    public bool TextInputEnabled = false;

    public void Setup(string UsrName, string pwd, string Host, string VHost)
    {
        
        Username = UsrName;
        Password = pwd;
        ConnectionHost = Host;
        ConnectionVirtualHost = VHost;
        TextBoxSend.text = "Type a message!";

        factory = new ConnectionFactory();
        factory.UserName = Username;
        factory.Password = Password;

        if (ConnectionVirtualHost.Length != 0)
        {
            factory.VirtualHost = ConnectionVirtualHost;
        }
        else
        {
            factory.VirtualHost = "SimpleVirtHost";
        }


        if (ConnectionHost.Length != 0)
        {
            factory.HostName = Host;
        }
        else
        {
            factory.HostName = "localhost";
        }
        
        conn = factory.CreateConnection();

        channel = conn.CreateModel();

        /*above code uses default user & settings, can use -user- provided login details
         * 
         * // this name will be shared by all connections instantiated by
           // this factory
           factory.ClientProvidedName = "app:audit component:event-consumer"
         * 
         * 
         * building blocks of protocol must be defined and linked together before they can be used
         */
        channel.ExchangeDeclare(factory.VirtualHost, ExchangeType.Direct);
        channel.QueueDeclare(queue: "hello", false, false, false, null);

        SimpleMessage Message = new SimpleMessage();      //fill temporary message
        Message.Message = "Hello world!";   //create the simplest hello world message
        Message.Username = Username;        //fill the username out for sending
        string JsonEncoded = JsonUtility.ToJson(Message);   //serialize to JSON for complex data sending
        var body = Encoding.UTF8.GetBytes(JsonEncoded);     //utf-8 encoding for send

        channel.BasicPublish(exchange: "", routingKey: "hello", basicProperties: null, body: body); //send the message

        
        List_MessageCollection = new List<SimpleMessage>();
        
        // above is all the setup needed to send a message on a single channel -- creating one igf there's not one provided.

        consumer = new EventingBasicConsumer(channel);
        consumer.Received += (model, ea) =>
        {
            var body2 = ea.Body.ToArray();
            var message2 = Encoding.UTF8.GetString(body2); //decode from UTF-8; need to de-serialze JSON message format
            
            SimpleMessage recievedMessage = JsonUtility.FromJson<SimpleMessage>(message2);

            List_MessageCollection.Add(recievedMessage); //add to list to print to screen
        };
        channel.BasicConsume(queue: "hello", autoAck: true, consumer: consumer);
        //Debug.Log("Simple tasks finished, necking.");

        //Tiling message rec system in progress
        TilingChannel = conn.CreateModel();
        TilingChannel.ExchangeDeclare(factory.VirtualHost, ExchangeType.Direct);
        TilingChannel.QueueDeclare(queue: "position", false, false, false, null);

        List_TilingMessages_Collision = new List<TilingMessage>();
        List_TilingMessages_PositionUpdates = new List<TilingMessage>();

        TilingConsumer = new EventingBasicConsumer(TilingChannel);
        TilingConsumer.Received += (model, ea) =>
        {
            var body2 = ea.Body.ToArray();
            var Message2 = Encoding.UTF8.GetString(body2);
            TilingMessage temp = JsonUtility.FromJson<TilingMessage>(Message2);
            if(temp.MessageType == messageClass.querySend)
            {
                List_TilingMessages_Collision.Add(temp);
            }
            else if (temp.MessageType == messageClass.positionUpdate)
            {
                List_TilingMessages_PositionUpdates.Add(temp);
            }
        };
        TilingChannel.BasicConsume(queue: "position", autoAck: true, consumer: TilingConsumer);

        //stocks messaging system
        StocksChannel = conn.CreateModel(); //create chanel setup
        StocksChannel.ExchangeDeclare(factory.VirtualHost, ExchangeType.Direct); //channel decleration type
        StocksChannel.QueueDeclare(queue: "stocks", false, false, false, null); 

        List_StocksMessaging_MarketPrice = new List<StocksMessageStruct>(); //fill list
        List_StocksMessaging_Sell_n_BuyRequests = new List<StocksMessageStruct>(); //fill list

        StocksConsumer = new EventingBasicConsumer(StocksChannel); //create consumer system
        StocksConsumer.Received += (model, ea) =>
        {
            var Body = ea.Body.ToArray();
            var StocksMessage = Encoding.UTF8.GetString(Body);
            StocksMessageStruct temp = JsonUtility.FromJson<StocksMessageStruct>(StocksMessage);
            if (temp.type == StocksEnumMessageType.MarketPrice)
            {
                List_StocksMessaging_MarketPrice.Add(temp);
            }
            if (temp.type == StocksEnumMessageType.buyReq || temp.type == StocksEnumMessageType.sellReq)
            {
                List_StocksMessaging_Sell_n_BuyRequests.Add(temp);
            }
        };
        StocksChannel.BasicConsume(queue: "stocks", autoAck: true, consumer: StocksConsumer);


    }

    // Update is called once per frame
    void Update()
    {
        if (TextInputEnabled)
        {
            foreach (char c in Input.inputString)
            {
                if (c == '\b') //backspace / delete lmao
                {
                    if (TextBoxSend.text.Length != 0)
                    {
                        TextBoxSend.text = TextBoxSend.text.Substring(0, TextBoxSend.text.Length - 1);
                    }
                }
                else if (c == '\n' || c == '\r') //enter / return
                {

                    SimpleMessage simpleMessage = new SimpleMessage();
                    simpleMessage.Message = TextBoxSend.text;
                    simpleMessage.Username = Username;
                    List_MessageCollection.Add(simpleMessage);
                    string JsonEncoded = JsonUtility.ToJson(simpleMessage);

                    SendMessageRabbitMQ(JsonEncoded);
                    TextBoxSend.text = "";
                }
                else
                {
                    if (TextBoxSend.text == "Type a message!")
                    {
                        TextBoxSend.text = "";
                    }
                    TextBoxSend.text += c;
                }
            }
            if (List_MessageCollection.Count > 25)
            {
                List_MessageCollection.RemoveAt(0);
            }
            if(List_TilingMessages_Collision.Count > 6)
            {
                List_TilingMessages_Collision.RemoveAt(0);
            }
            if(List_TilingMessages_PositionUpdates.Count > 6)
            {
                List_TilingMessages_PositionUpdates.RemoveAt(0);
            }
            if(List_StocksMessaging_MarketPrice.Count > 6)
            {
                List_StocksMessaging_MarketPrice.RemoveAt(0);
            }
            if(List_StocksMessaging_Sell_n_BuyRequests.Count > 6)
            {
                List_StocksMessaging_Sell_n_BuyRequests.RemoveAt(0);
            }
        }
        
    }

    private void closeSystem() //safety kill for everything :D
    {
        if (TextInputEnabled)
        {
            SimpleMessage simple = new SimpleMessage();
            simple.Message = "[[User left channel!]]";
            simple.Username = Username;
            string JsonEncoded = JsonUtility.ToJson(simple);
            SendMessageRabbitMQ(JsonEncoded);

            channel.Close();
            conn.Close();
        }
        //disconnections can be observed in the node server logs
    }

    public void SendMessageRabbitMQ(string MessageToSend)
    {
        var body = Encoding.UTF8.GetBytes(MessageToSend);
        channel.BasicPublish(exchange: "", routingKey: "hello", basicProperties: null, body: body);
    }

    private void OnGUI()
    {
        if (TextInputEnabled)
        {
            for(int i = 0; i < List_MessageCollection.Count; i++)
            {
                GUI.Label(new Rect(10, 11 * i, 500, 50), (List_MessageCollection[i].Username + ": " + List_MessageCollection[i].Message));
            }
            for(int i = 0; i < List_TilingMessages_Collision.Count; i++)
            {
                GUI.Label(new Rect(500, 11 * i, 500, 50), "Collision: " + (List_TilingMessages_Collision[i].Message)); //print the recived message just off the edge of the screen (?)
            }
            for(int i = 0; i < List_TilingMessages_PositionUpdates.Count; i++)
            {
                GUI.Label(new Rect(1000, 11 * i, 500, 50), "New move: " + (List_TilingMessages_PositionUpdates[i].Message)); //print the recived message just off the edge of the screen (?)
            }
            for(int i = 0; i < List_StocksMessaging_MarketPrice.Count; i++)
            {
                GUI.Label(new Rect(500, (11 * i) + 77, 500, 50), "Market Price update: " +(List_StocksMessaging_MarketPrice[i].Message)); //print the recived message just off the edge of the screen (?)
            }
            for(int i = 0; i < List_StocksMessaging_Sell_n_BuyRequests.Count; i++)
            {
                GUI.Label(new Rect(1000, (11 * i) + 77, 500, 50), "Sell(0) | Buy (1): " + (List_StocksMessaging_Sell_n_BuyRequests[i].type + " req: " + List_StocksMessaging_Sell_n_BuyRequests[i].Message)); //print the recived message just off the edge of the screen (?)
            }
        }
    }

    private void OnApplicationQuit()
    {
        //Debug.Log("Closing systems!");
        closeSystem();
        //Debug.Log("Systems closed.");
    }

}

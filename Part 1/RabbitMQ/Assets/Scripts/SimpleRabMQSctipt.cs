using System.Collections;
using System.Collections.Generic;
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
     * The code snippets in the following section connect to a RabbitMQ node on [hostname]     */
    private ConnectionFactory factory;
    private IConnection conn;
    private IModel channel;

    private EventingBasicConsumer consumer;

    private SimpleMessage Message;

    private List<GameObject> MessageQueue;

    public Text TextBoxSend;
    private string Username;
    private string Password;

    public GameObject messageBox;
    public bool TextInputEnabled = false;

    public void Setup(string UsrName, string pwd)
    {

        Username = UsrName;
        Password = pwd;

        TextBoxSend.text = "Type a message!";
        MessageQueue = new List<GameObject>();

        factory = new ConnectionFactory();
        factory.UserName = Username;
        factory.Password = Password;
        factory.VirtualHost = "SimpleVirtHost";
        factory.HostName = "localhost";
        
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
        channel.ExchangeDeclare("SimpleVirtHost", ExchangeType.Direct);
        channel.QueueDeclare(queue: "hello", false, false, false, null);

        Message = new SimpleMessage();      //fill temporary message
        Message.Message = "Hello world!";   //create the simplest hello world message
        Message.Username = Username;        //fill the username out for sending
        string JsonEncoded = JsonUtility.ToJson(Message);   //serialize to JSON for complex data sending
        var body = Encoding.UTF8.GetBytes(JsonEncoded);     //utf-8 encoding for send

        channel.BasicPublish(exchange: "", routingKey: "hello", basicProperties: null, body: body); //send the message

        
        //Debug.Log("Sent message!");

        // above is all the setup needed to send a message on a single channel -- creating one igf there's not one provided.

        consumer = new EventingBasicConsumer(channel);
        consumer.Received += (model, ea) =>
        {
            
            var body2 = ea.Body.ToArray();
            var message2 = Encoding.UTF8.GetString(body2); //decode from UTF-8; need to de-serialze JSON message format
            Message = JsonUtility.FromJson<SimpleMessage>(message2);
            print(Message.Username + ": " + Message.Message); //debug logging of recieved message
            
        };
        channel.BasicConsume(queue: "hello", autoAck: true, consumer: consumer);
        //Debug.Log("Simple tasks finished, necking.");
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
                    Message.Message = TextBoxSend.text;
                    Message.Username = Username;

                    string JsonEncoded = JsonUtility.ToJson(Message);

                    SendMessage(JsonEncoded);
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
        }
        //RecieveMessage();
    }

    private void closeSystem() //safety kill for everything :D
    {
        if (TextInputEnabled)
        {
            channel.Close();
            conn.Close();
        }
        //disconnections can be observed in the node server logs
    }

    public void SendMessage(string MessageToSend)
    {
        var body = Encoding.UTF8.GetBytes(MessageToSend);
        channel.BasicPublish(exchange: "", routingKey: "hello", basicProperties: null, body: body);
    }

    private void OnGUI()
    {
        if(TextInputEnabled)
            GUI.Label(new Rect(10, 10, 400, 50), Message.Message);
    }

    private void OnApplicationQuit()
    {
        Debug.Log("Closing systems!");
        closeSystem();
        Debug.Log("Systems closed.");
    }

}

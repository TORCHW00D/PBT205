using System.Collections;
using System.Collections.Generic;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using UnityEngine;
using UnityEngine.UI;

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

    public Text TextBoxSend;

    void Start()
    {
        TextBoxSend.text = "Type a message!";


        factory = new ConnectionFactory();
        factory.UserName = "simpleUser";
        factory.Password = "12345";
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
        
        
        string message = "Hello world!"; //simple hello world lmao
        var body = Encoding.UTF8.GetBytes(message);

        channel.BasicPublish(exchange: "", routingKey: "hello", basicProperties: null, body: body);

        Debug.Log("Sent message!");

        // above is all the setup needed to send a message on a single channel -- creating one igf there's not one provided.

        consumer = new EventingBasicConsumer(channel);
        consumer.Received += (model, ea) =>
        {
            var body2 = ea.Body.ToArray();
            var message2 = Encoding.UTF8.GetString(body2);
            Debug.Log("Message rec: " + message2);
        };
        channel.BasicConsume(queue: "hello", autoAck: true, consumer: consumer);
        Debug.Log("Simple tasks finished, necking.");
    }

    // Update is called once per frame
    void Update()
    {
        foreach(char c in Input.inputString)
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
                SendMessage(TextBoxSend.text);
                TextBoxSend.text = "";
            }
            else
            {
                if(TextBoxSend.text == "Type a message!")
                {
                    TextBoxSend.text = ". . .";
                }
                TextBoxSend.text += c;
            }
        }
    }

    private void closeSystem() //safety kill for everything :D
    {
        channel.Close();
        conn.Close();
        //disconnections can be observed in the node server logs
    }

    public void SendMessage(string MessageToSend)
    {
        var body = Encoding.UTF8.GetBytes(MessageToSend);
        channel.BasicPublish(exchange: "", routingKey: "hello", basicProperties: null, body: body);
    }

    public string RecieveMessage()
    {
        string message = "";
        consumer.Received += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            message = Encoding.UTF8.GetString(body);
            Debug.Log("Message rec: " + message);
        };
        channel.BasicConsume(queue: "hello", autoAck: true, consumer: consumer);
        return message;
    }

    private void OnApplicationQuit()
    {
        Debug.Log("Closing systems!");
        closeSystem();
        Debug.Log("Systems closed.");
    }

}

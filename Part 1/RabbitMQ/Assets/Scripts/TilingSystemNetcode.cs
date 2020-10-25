using System.Collections;
using System.Collections.Generic;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using UnityEngine;

public enum messageClass
{
    positionUpdate,
    queryRequest,
    querySend
}

public class TilingMessage
{
    public messageClass MessageType;
    public string Message;
}


public class TilingSystemNetcode : MonoBehaviour
{

    private ConnectionFactory Factory;
    private IConnection connection;
    private IModel channel;

    private EventingBasicConsumer consumer;

    private string username, pass;

    public void TilingSystemSetup(string usrnm = "a", string pwd = "a")
    {
        username = usrnm;
        pass = pwd;
        Factory = new ConnectionFactory();
        Factory.UserName = username;
        Factory.Password = pass;
        Factory.VirtualHost = "SimpleVirtHost";
        Factory.HostName = "localhost";

        connection = Factory.CreateConnection();
        channel = connection.CreateModel();

        channel.ExchangeDeclare(Factory.VirtualHost, ExchangeType.Direct);
        channel.QueueDeclare(queue: "position", false, false, false, null); //position

        TilingMessage temp = new TilingMessage();
        temp.MessageType = messageClass.positionUpdate; //enum type for message
        temp.Message = "pushing message to queue";

        string JsonEncoded = JsonUtility.ToJson(temp);
        var body = Encoding.UTF8.GetBytes(JsonEncoded);

        channel.BasicPublish(exchange: "", routingKey: "position", basicProperties: null, body: body); //posotion
    }

    public void SendMessageTilingSystem(string messageToSend, messageClass Type)
    {
        string JsonEncoded = JsonUtility.ToJson(new TilingMessage { MessageType = Type, Message = messageToSend });
        var body = Encoding.UTF8.GetBytes(JsonEncoded);
        channel.BasicPublish(exchange: "", routingKey: "position", basicProperties: null, body: body); //position
    }
    private void closeSystem() //safety kill for everything :D
    {
        
        channel.Close();
        connection.Close();
        
        //disconnections can be observed in the node server logs
    }
    private void OnApplicationQuit()
    {
        //Debug.Log("Closing systems!");
        closeSystem();
        //Debug.Log("Systems closed.");
    }

}

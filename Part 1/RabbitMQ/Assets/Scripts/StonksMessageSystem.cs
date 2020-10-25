using System.Collections;
using System.Collections.Generic;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using UnityEngine;

public enum StocksEnumMessageType
{
    sellReq,
    buyReq,
    MarketPrice,

}

public struct StocksMessageStruct
{
    public string Message;
    public StocksEnumMessageType type;
}
public class StonksMessageSystem : MonoBehaviour
{
    public SimpleRabMQSctipt MessageService;

    private ConnectionFactory Factory;
    private IConnection connection;
    private IModel channel;
    private EventingBasicConsumer consumer;

    private string username, pass;

    

    

    public void StonksMessageSystemSetup(string usrnm = "a", string pwd = "a")
    {
        username = usrnm;
        pass = pwd;
        Factory = new ConnectionFactory();
        Factory.UserName = "simpleUser";
        Factory.Password = "12345";
        Factory.VirtualHost = "SimpleVirtHost";
        Factory.HostName = "localhost";

        connection = Factory.CreateConnection();
        channel = connection.CreateModel();

        channel.ExchangeDeclare(Factory.VirtualHost, ExchangeType.Direct);
        channel.QueueDeclare(queue: "stocks", false, false, false, null); //position

        StocksMessageStruct temp = new StocksMessageStruct();
        temp.type= StocksEnumMessageType.MarketPrice; //enum type for message
        temp.Message = "Stocks messaging system active!";

        string JsonEncoded = JsonUtility.ToJson(temp);
        var body = Encoding.UTF8.GetBytes(JsonEncoded);

        channel.BasicPublish(exchange: "", routingKey: "stocks", basicProperties: null, body: body); //posotion
    }

    public void SendMessageStonksSystem(string messageToSend, StocksEnumMessageType Type)
    {
        string JsonEncoded = JsonUtility.ToJson(new StocksMessageStruct { type = Type, Message = messageToSend });
        var body = Encoding.UTF8.GetBytes(JsonEncoded);
        channel.BasicPublish(exchange: "", routingKey: "stocks", basicProperties: null, body: body); //position
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

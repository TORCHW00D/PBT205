using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StockMarker : MonoBehaviour
{
    
    private struct OrderForm
    {
        public double price;
        public bool isSalesPoint;
        public int quantity;
    }

    public StonksMessageSystem StockMessagingSystem;

    public Text marketText;
    public Text sellingText;
    public Text buyingText;
    public Text recent;

    private List<OrderForm> List_Orders_Selling = new List<OrderForm>();
    private List<OrderForm> List_Orders_Buying = new List<OrderForm>();
    private OrderForm Sell, Buy;

    private double marketPrice;
    
    public float stockTimer = 1;
    public float marketDecay = 5;
    private float MAXstockTimer; // for reset
    private float MAXmarketDecay; // for reset

    void Start()
    {
        marketPrice = Random.Range(1, 10); // start somewhere
        floorIt(marketPrice);
        MAXstockTimer = stockTimer;
        MAXmarketDecay = marketDecay;
        stockTimer = 0; // right into the action
        Sell.quantity = 0;
        Sell.price = 100000000.0f;
        List_Orders_Selling.Add(Sell);
        Buy.quantity = 0;
        Buy.price = 10000000000000.0f;
        List_Orders_Buying.Add(Buy);

        StockMessagingSystem.StonksMessageSystemSetup();
        string SellerMessage = Sell.price.ToString() + " " + Sell.quantity.ToString();
        StockMessagingSystem.SendMessageStonksSystem(SellerMessage, StocksEnumMessageType.sellReq);

        string BuyerMessage = Buy.price.ToString() + " " + Buy.quantity.ToString();
        StockMessagingSystem.SendMessageStonksSystem(BuyerMessage, StocksEnumMessageType.buyReq);
    }

    void Update()
    {
        if(stockTimer <= 0) // when its time to make a new order
        {
            //buying person
            OrderForm tempBuyer = new OrderForm();
            tempBuyer.price = marketPrice + Random.Range(0.0f, 1.0f);
            tempBuyer.price = floorIt(tempBuyer.price);
            tempBuyer.quantity = 100;
            tempBuyer.isSalesPoint = false;
            Buy = tempBuyer;
            
            string BuyerMessage = Buy.price.ToString() + " " + Buy.quantity.ToString();
            StockMessagingSystem.SendMessageStonksSystem(BuyerMessage, StocksEnumMessageType.buyReq);


            // selling person
            OrderForm tempSeller = new OrderForm();
            tempSeller.price = marketPrice + Random.Range(-0.5f, 1.5f);
            tempSeller.price = floorIt(tempSeller.price);
            tempSeller.quantity = 100;
            tempSeller.isSalesPoint = true;
            Sell = tempSeller;

            string SellerMessage = Sell.price.ToString() + " " + Sell.quantity.ToString();
            StockMessagingSystem.SendMessageStonksSystem(SellerMessage, StocksEnumMessageType.sellReq); //send randomlly gen buy and sell pieces to Buy and Sell req slots
            
            // check if the 2 current orders are equal
            bool marketGood = false;
            if (tempBuyer.price == tempSeller.price && tempBuyer.quantity == tempSeller.quantity)
            {
                marketGood = true;
                //Debug.Log("Sold on generated pair!");
                string saleString = tempBuyer.quantity.ToString() + "# sold at $" + tempBuyer.price.ToString();
                StockMessagingSystem.SendMessageStonksSystem(saleString, StocksEnumMessageType.MarketPrice);
            }
            else // check if the 2 current orders are equal to every other existing order
            {
                ///Comparing buying to all sales records
                bool HistoricSalesMatchFound = false;
                for(int i = 0; i < List_Orders_Selling.Count; i++)
                {
                    if(List_Orders_Selling[i].price == tempBuyer.price) //sales match
                    {
                       // Debug.Log("Found buy match in historic sell records");
                        HistoricSalesMatchFound = true;
                        marketGood = true;
                        List_Orders_Selling.RemoveAt(i); //remove from sales record
                        i--;
                        string saleString = tempBuyer.quantity.ToString() + "# sold at $" + tempBuyer.price.ToString();
                        StockMessagingSystem.SendMessageStonksSystem(saleString, StocksEnumMessageType.MarketPrice);
                    }
                }
                if(!HistoricSalesMatchFound) //if we don't find buy match, add the buy to the historical buys
                {
                    List_Orders_Buying.Add(tempBuyer);
                }

                ///Comparing selling to all buy records
                bool HistoricBuyMatchFound = false;
                for(int i = 0; i < List_Orders_Buying.Count; i++)
                {
                    if(List_Orders_Buying[i].price == tempSeller.price)
                    {
                       // Debug.Log("Found sell match in historic buy records");
                        HistoricSalesMatchFound = true;
                        marketGood = true;
                        List_Orders_Buying.RemoveAt(i);
                        i--;
                        string saleString = tempSeller.quantity.ToString() + "# sold at $" + tempSeller.price.ToString();
                        StockMessagingSystem.SendMessageStonksSystem(saleString, StocksEnumMessageType.MarketPrice);
                    }
                }
                if(!HistoricBuyMatchFound)
                {
                    List_Orders_Selling.Add(tempSeller);
                }

            }

            // change the market if a stock is bought
            if (marketGood)
            {
                marketPrice = Buy.price = Random.Range(0.0f, 0.5f);
                marketPrice = floorIt(marketPrice);
                marketDecay = MAXmarketDecay; // reset the market decay timer

                recent.text = "Most recent trade: " + Buy.quantity.ToString() + " at $" + marketPrice;

                StockMessagingSystem.SendMessageStonksSystem(recent.text, StocksEnumMessageType.MarketPrice);
            }

            // reset timer
            stockTimer = MAXstockTimer;
        }
        else
        {
            // tick downwards
            stockTimer -= Time.deltaTime;
        }

        // stock price going down over time as people dont buy it
        if(marketDecay <= 0)
        {
            marketPrice += Random.Range(-0.5f, -0.1f);
            marketPrice = floorIt(marketPrice);
            marketDecay = MAXmarketDecay;
        }
        else
        {
            marketDecay -= Time.deltaTime;
        }

        // display market stuff to screen
        if (marketPrice <= 0)
            marketText.text = "Market crash: " + marketPrice.ToString();
        else
            marketText.text = marketPrice.ToString();

        sellingText.text = Sell.quantity.ToString() + " for $" + Sell.price.ToString();
        buyingText.text = Buy.quantity.ToString() + " for $" + Buy.price.ToString();

    }

    double floorIt(double number) // round off a number
    {
        number *= 10;
        number = (int)number;
        number /= 10.0f;
        return number;
        
    }
}

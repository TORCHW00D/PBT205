using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StockMarker : MonoBehaviour
{
    public Text marketText;
    public Text sellingText;
    public Text buyingText;

    private float marketPrice;
    private float sellingPrice;
    private int sellingAmount;
    private float buyingPrice;
    private int buyingAmount;

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

    }

    void Update()
    {
        if(stockTimer <= 0)
        {
            // buying person
            buyingPrice = marketPrice + Random.Range(-2.0f, 0.0f);
            buyingPrice = floorIt(buyingPrice);
            buyingAmount = Random.Range(1, 20);

            // selling person
            sellingPrice = marketPrice + Random.Range(-1.0f, 1.0f);
            sellingPrice = floorIt(sellingPrice);
            sellingAmount = Random.Range(1, 20);

            // equal check
            bool marketGood = false;
            if (sellingPrice == buyingPrice && sellingAmount == buyingAmount)
            {
                marketGood = true;
            }

            // market change
            if(marketGood)
            {
                marketPrice += Random.Range(0.1f, 0.5f);
                marketPrice = floorIt(marketPrice);
                marketDecay = MAXmarketDecay; // reset timer
            }

            stockTimer = MAXstockTimer;
        }
        else
        {
            stockTimer -= Time.deltaTime;
        }

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

        marketText.text = marketPrice.ToString();
        sellingText.text = sellingAmount + " for $" + sellingPrice;
        buyingText.text = buyingAmount + " for $" + buyingPrice;


    }

    float floorIt(float number) // round off a number
    {
        number = Mathf.Round(number * 10f) / 10f;
        return number;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SimpleMessageBoxScript : MonoBehaviour
{
    
    public Text UsernamneBox;
    public Text MessageBox;

    public void SetUsername(string UsrName)
    {
        UsernamneBox.text = UsrName;
    }
    public void SetMessageContents(string msg)
    {
        MessageBox.text = msg;
    }

}

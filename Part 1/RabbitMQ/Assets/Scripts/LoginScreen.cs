using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoginScreen : MonoBehaviour
{
    public InputField UsrnameInput;
    public InputField PwdInput;
    public SimpleRabMQSctipt SimpleRab;
    public void SubmissionForm()
    {
        string Username = UsrnameInput.text;
        string Pass = PwdInput.text;
        SimpleRab.Setup(Username, Pass);
        SimpleRab.TextInputEnabled = true;
        gameObject.SetActive(false);
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoginScreen : MonoBehaviour
{
    public InputField HostnameInput;
    public InputField VirtualHost;
    public InputField UsrnameInput;
    public InputField PwdInput;
    
    public SimpleRabMQSctipt SimpleRab;
    public void SubmissionForm()
    {
        string Username = UsrnameInput.text;
        string Pass = PwdInput.text;
        string Host = HostnameInput.text;
        string VHost = VirtualHost.text;
        SimpleRab.Setup(Username, Pass, Host, VHost);
        SimpleRab.TextInputEnabled = true;
        gameObject.SetActive(false);
    }
}

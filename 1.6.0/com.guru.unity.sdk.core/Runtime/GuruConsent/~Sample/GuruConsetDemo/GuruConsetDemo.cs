using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Guru;

/// <summary>
/// Consent流程演示
/// </summary>
public class GuruConsetDemo : MonoBehaviour
{
    public Button _btnRequest;
    public Text _txtInfo;
    public InputField _inputBox;
    
    
    // Start is called before the first frame update
    void Start()
    {
        _btnRequest.onClick.AddListener(OnClickRequest);
    }
    
    /// <summary>
    /// 点击请求
    /// </summary>
    void OnClickRequest()
    {
        // 无需回调的话可直接调用
        // GuruConsent.StartConsent();
        
        var deviceId = _inputBox.text;
        GuruConsent.StartConsent(OnGetGdprResult,  OnGetConsentResult, deviceId);
    }

    
    /// <summary>
    /// 获取到 ConsentStatus
    /// </summary>
    /// <param name="status"></param>
    private void OnGetGdprResult(int status)
    {
        string msg = $"--- [Unity] Get Status: {status}";
        Debug.Log(msg);
        _txtInfo.text = msg;
    }

    private void OnGetConsentResult(ConsentData consentData)
    {
        Debug.Log($"--- Get Consent Data ---");
        Debug.Log($"  isEeaUser: {consentData.IsEea}");
        Debug.Log($"  Consents: \n{consentData.PrintConsents()}");
        
    }
}

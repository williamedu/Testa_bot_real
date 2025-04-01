using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MT5BalanceDisplay : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI balanceText;
    [SerializeField] private TextMeshProUGUI equityText;
    [SerializeField] private TextMeshProUGUI profitText;
    [SerializeField] private TextMeshProUGUI marginText;
    [SerializeField] private TextMeshProUGUI freeMarginText;
    
    [Header("Format Settings")]
    [SerializeField] private string prefix = "$";
    [SerializeField] private string balanceFormat = "Balance: {0}{1:N2}";
    [SerializeField] private string equityFormat = "Equity: {0}{1:N2}";
    [SerializeField] private string profitFormat = "Profit: {0}{1:N2}";
    [SerializeField] private string marginFormat = "Margin: {0}{1:N2}";
    [SerializeField] private string freeMarginFormat = "Free Margin: {0}{1:N2}";
    
    [Header("Colors")]
    [SerializeField] private Color positiveColor = Color.green;
    [SerializeField] private Color negativeColor = Color.red;
    [SerializeField] private Color neutralColor = Color.white;
    
    private string currentAccountId;
    
    private void Start()
    {
        // Obtener el ID de cuenta guardado en el login
        currentAccountId = PlayerPrefs.GetString("CurrentAccountID", "590423106");
        
        // Suscribirse al evento del MT5DataManager
        if (MT5DataManager.Instance != null)
        {
            //MT5DataManager.Instance.onDataReceived.AddListener(OnMT5DataReceived);
        }
        else
        {
            Debug.LogError("MT5DataManager no encontrado en la escena");
        }
    }
    
    private void OnDestroy()
    {
        // Desuscribirse para evitar memory leaks
        if (MT5DataManager.Instance != null)
        {
            //MT5DataManager.Instance.onDataReceived.RemoveListener(OnMT5DataReceived);
        }
    }
    
    private void OnMT5DataReceived(Dictionary<long, MT5AccountData> accountsData)
    {
        // Verificar si tenemos datos para la cuenta actual
        if (long.TryParse(currentAccountId, out long accountId) && 
            accountsData.TryGetValue(accountId, out MT5AccountData accountData))
        {
            // Actualizar los textos con los datos de la cuenta específica
            UpdateUI(accountData);
        }
    }
    
    private void UpdateUI(MT5AccountData accountData)
    {
        // Actualizar texto de balance
        if (balanceText != null)
        {
            balanceText.text = string.Format(balanceFormat, prefix, accountData.balance);
            balanceText.color = neutralColor;
        }
        
        // Actualizar texto de equity
        if (equityText != null)
        {
            equityText.text = string.Format(equityFormat, prefix, accountData.equity);
            equityText.color = neutralColor;
        }
        
        // Actualizar texto de profit con color basado en valor
        if (profitText != null)
        {
            profitText.text = string.Format(profitFormat, prefix, accountData.profit);
            profitText.color = accountData.profit >= 0 ? positiveColor : negativeColor;
        }
        
        // Actualizar texto de margin
        if (marginText != null)
        {
            marginText.text = string.Format(marginFormat, prefix, accountData.margin);
            marginText.color = neutralColor;
        }
        
        // Actualizar texto de free margin
        if (freeMarginText != null)
        {
            freeMarginText.text = string.Format(freeMarginFormat, prefix, accountData.free_margin);
            freeMarginText.color = neutralColor;
        }
    }
    
    // Método público para cambiar manualmente la cuenta a mostrar
    public void SetAccountId(string newAccountId)
    {
        currentAccountId = newAccountId;
        PlayerPrefs.SetString("CurrentAccountID", newAccountId);
        PlayerPrefs.Save();
        
        // Intentar actualizar la UI inmediatamente si hay datos disponibles
        if (MT5DataManager.Instance != null && 
            long.TryParse(currentAccountId, out long accountId) &&
            MT5DataManager.Instance.AccountsData.TryGetValue(accountId, out MT5AccountData accountData))
        {
            UpdateUI(accountData);
        }
    }
}

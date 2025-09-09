using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeliveryUIManager : MonoBehaviour
{
    [Header("UI ���")]
    public Text statusText;
    public Text messageText;
    public Slider batterySlider;
    public Image batteryFill;

    [Header("���� ������Ʈ")]
    public DeliveryDriver driver;

    // Start is called before the first frame update
    void Start()
    {
        if(driver != null)
        {
            driver.driverEvents.OnMoneyChanged.AddListener(UpdateMoney);
            driver.driverEvents.OnBatteryChanged.AddListener(UpdateBattery);
            driver.driverEvents.OnDeliveryCountChanged.AddListener(UpdateDeliveryCount);
            driver.driverEvents.OnMoveStarted.AddListener(OnMoveStarted);
            driver.driverEvents.OnMoveStoped.AddListener(OnMoveStopped);
            driver.driverEvents.OnLowBattery.AddListener(OnLowBattery);
            driver.driverEvents.OnLowBatteryEmpty.AddListener(OnBatteryEmpty);
            driver.driverEvents.OnDeliveryCompleted.AddListener(OnDeliveryCompleted);
        }
        UpdateUI();
    }

    // Update is called once per frame
    void Update()
    {
        if(statusText != null && driver != null)
        {
            statusText.text = driver.GetStatusText();
        }
    }

    void ShowMessage(string message, Color color)
    {
        if(messageText != null)
        {
            messageText.text = message;
            messageText.color = color;
        }
    }

    IEnumerator ClearMessageAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (messageText != null)
        {
            messageText.text = "";
        }
    }
    void UpdateMoney(float money)
    {
        ShowMessage($"�� : {money}��", Color.green);
    }
    void UpdateBattery(float battery)
    {
        if(batterySlider != null)
        {
            batterySlider.value = battery / 100f;
        }
        if (batteryFill != null)
        {
            if(battery > 50f)
                batteryFill.color = Color.green;
            else if (battery > 20f)
                batteryFill.color = Color.yellow;
            else
                batteryFill.color = Color.red;
        }
    }

    void UpdateDeliveryCount(int count)
    {
        ShowMessage($"��� �Ϸ� Ƚ�� : {count}", Color.blue);
    }
    void OnMoveStarted()
    {
        ShowMessage("�̵� ����", Color.cyan);
    }
    void OnMoveStopped()
    {
        ShowMessage("�̵� ����", Color.gray);
    }
    void OnLowBattery()
    {
        ShowMessage("���: ���͸� ����!", Color.red);
    }
    void OnBatteryEmpty()
    {
        ShowMessage("���͸� ����! �̵� �Ұ�!", Color.red);
    }
    void OnDeliveryCompleted()
    {
        ShowMessage("��� �Ϸ�!", Color.green);
    }
    void UpdateUI()
    {
        if (driver != null)
        {
            UpdateMoney(driver.currentMoney);
            UpdateBattery(driver.batteryLevel);
            UpdateDeliveryCount(driver.deliveryCount);
        }
    }
    void OnDestroy()
    {
        if (driver != null)
        {
            driver.driverEvents.OnMoneyChanged.AddListener(UpdateMoney);
            driver.driverEvents.OnBatteryChanged.AddListener(UpdateBattery);
            driver.driverEvents.OnDeliveryCountChanged.AddListener(UpdateDeliveryCount);
            driver.driverEvents.OnMoveStarted.AddListener(OnMoveStarted);
            driver.driverEvents.OnMoveStoped.AddListener(OnMoveStopped);
            driver.driverEvents.OnLowBattery.AddListener(OnLowBattery);
            driver.driverEvents.OnLowBatteryEmpty.AddListener(OnBatteryEmpty);
            driver.driverEvents.OnDeliveryCompleted.AddListener(OnDeliveryCompleted);
        }
    }
}

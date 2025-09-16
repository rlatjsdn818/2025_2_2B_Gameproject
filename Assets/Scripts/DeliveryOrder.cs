using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class DeliveryOrder
{
    //����
    public int orderId;
    public string restaurantName;
    public string customerName;
    public Building restaurantBuilding;
    public Building customerBuilding;
    public float orderTime; // Time when the order was created
    public float timeLimit; // Time limit to complete the order
    public float reward;
    public OrderState state;

    public DeliveryOrder(int id, Building restaurant, Building customer, float rewardAmount)
    {
        orderId = id;
        restaurantBuilding = restaurant;
        customerBuilding = customer;
        restaurantName = restaurant.buildingName;
        customerName = customer.buildingName;
        orderTime = Time.time;
        timeLimit = Random.Range(60, 120f); // 1~2�� ����
        reward = rewardAmount;
        state = OrderState.WaitingPickup;
    }

    public float GetRemainingTime()
    {
        return Mathf.Max(0f, timeLimit - (Time.time - orderTime)); //���� �ð� ����
    }

    public bool IsExpired()
    {
        return GetRemainingTime() <= 0f; //�ð� �ʰ� ���� ����
    }


}

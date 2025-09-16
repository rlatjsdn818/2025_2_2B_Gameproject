using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DeliveryOrderSystem : MonoBehaviour
{
    [Header("�ֹ� ����")]
    public float ordergenrateInterval = 15f; //�ֹ� ���� ���� (15��)
    public int maxActiveOrders = 8; //�ִ� Ȱ�� �ֹ� ��

    [Header("���� ����")]
    public int totalOrdersGenerated = 0;
    public int completedOrders = 0;
    public int expiredOrders = 0;

    //�ֹ� ����Ʈ
    private List<DeliveryOrder> currentOrders = new List<DeliveryOrder>();

    //Building ����
    private List<Building> restaurants = new List<Building>();
    private List<Building> customers = new List<Building>();

    //event �ý���
    [System.Serializable]
    public class OrderSystemEvents
    {
        public UnityEvent<DeliveryOrder> OnNewOrderAdded;
        public UnityEvent<DeliveryOrder> OnOrderPickedUp;
        public UnityEvent<DeliveryOrder> OnOrderCompleted;
        public UnityEvent<DeliveryOrder> OnOrderExpired;
    }

    public OrderSystemEvents orderEvents;
    private DeliveryDriver driver;




    private DeliveryOrderSystem orderSystem;


    // Start is called before the first frame update
    void Start()
    {
        driver = FindObjectOfType<DeliveryDriver>();
        FindAllBuilding();

        //�ʱ� �ֹ� ����
        StartCoroutine(GenerateInitialOrders());
        //�ֱ����� �ֹ� ����
        StartCoroutine(orderGenerator());
        //���� üũ
        StartCoroutine(ExpiredOrderChecker());
    }

    void FindAllBuilding()
    {
        Building[] allBuildings = FindObjectsOfType<Building>();

        foreach (Building building in allBuildings)
        {
            if (building.BuildingType == BuildingType.Restaurant)
            {
                restaurants.Add(building);
            }
            else if (building.BuildingType == BuildingType.Customer)
            {
                customers.Add(building);
            }
        }

        Debug.Log($"������ {restaurants.Count}��, �� {customers.Count}�� ã��.");
    }

    void CreateNewOrder()
    {
        if (restaurants.Count == 0 || customers.Count == 0) return;
        {
            //���� �������� �� ����
            Building randomRestaurant = restaurants[Random.Range(0, restaurants.Count)];
            Building randomCustomer = customers[Random.Range(0, customers.Count)];

            //���� ���� �ǹ��̸� �ٽ� ����
            if (randomRestaurant == randomCustomer)
            {
                randomCustomer = customers[Random.Range(0, customers.Count)];
            }

            float reward = Random.Range(3000f, 8000f);

            DeliveryOrder newOrder = new DeliveryOrder(++totalOrdersGenerated,randomRestaurant,randomCustomer,reward);

            currentOrders.Add(newOrder);
            orderEvents.OnNewOrderAdded?.Invoke(newOrder);


        }
    }

    void PickupOrder(DeliveryOrder order)  //�Ⱦ� �Լ�
    {
        order.state = OrderState.PickedUp;
        orderEvents.OnOrderPickedUp?.Invoke(order);
    }

    void CompleteOrder(DeliveryOrder order) //�Ϸ� �Լ�
    {
        order.state = OrderState.Completed;
        completedOrders++;

        //���� �ִ°�
        if(driver != null)
        {
            driver.AddMoney(order.reward);
        }

        //�Ϸ��ֹ� ����
        currentOrders.Remove(order);
        orderEvents.OnOrderCompleted?.Invoke(order);
    }

    void ExpireOrder(DeliveryOrder order) //���� �Լ�, �ֹ� ��� �Ҹ�
    {
        order.state = OrderState.Expired;
        expiredOrders++;

        //���� �ֹ� ����
        currentOrders.Remove(order);
        orderEvents.OnOrderExpired?.Invoke(order);
    }

    //UI ���� ����
    public List<DeliveryOrder> GetCurrentOrders()
    {
        return new List<DeliveryOrder>(currentOrders);
    }

    public int GetPickWaitingCount() // �Ⱦ� ������� �ֹ� ��
    {
        int count = 0;
        foreach (DeliveryOrder order in currentOrders)
        {
            if (order.state == OrderState.WaitingPickup) count++;
        }
        return count;
    }
    public int GetDeliveryWaitingCount() // ��� ������� �ֹ� ��
    {
        int count = 0;
        foreach (DeliveryOrder order in currentOrders)
        {
            if (order.state == OrderState.PickedUp) count++;
        }
        return count;
    }

    DeliveryOrder FindOrderForPickup(Building restaurant)
    {
        foreach (DeliveryOrder order in currentOrders)
        {
            if (order.restaurantBuilding == restaurant && order.state == OrderState.WaitingPickup)
            {
                return order;
            }
        }
        return null;
    }
    DeliveryOrder FIndOrderForDelivery(Building customer)
    {
        foreach (DeliveryOrder order in currentOrders)
        {
            if (order.customerBuilding == customer && order.state == OrderState.PickedUp)
            {
                return order;
            }
        }
        return null;
    }

    //Event ó��
    public void OnDriverEnteredRestaurant(Building restaurant)
    {
        DeliveryOrder orderToPickup = FindOrderForPickup(restaurant);

        if(orderToPickup != null)
        {
            PickupOrder(orderToPickup);
        }
    }

    public void OnDriverEnteredCustomer(Building customer)
    {
        DeliveryOrder orderToDeliver = FIndOrderForDelivery(customer);

        if (orderToDeliver != null)
        {
            CompleteOrder(orderToDeliver);
        }
    }

    IEnumerator GenerateInitialOrders()
    {
        yield return new WaitForSeconds(1f);

        // ���� �� 3�� �ֹ� ����
        for(int i = 0; i < 3; i++)
        {
            CreateNewOrder();
            yield return new WaitForSeconds(0.5f);
        }
    }

    IEnumerator orderGenerator()
    {
        while(true)
        {
            yield return new WaitForSeconds(ordergenrateInterval);
            if (currentOrders.Count < maxActiveOrders)
            {
                CreateNewOrder();
            }
        }
    }

    IEnumerator ExpiredOrderChecker()
    {
        while (true)
        {
            yield return new WaitForSeconds(5f);
            List<DeliveryOrder> expiredOrders = new List<DeliveryOrder>();
            foreach (DeliveryOrder order in currentOrders)
            {
                if (order.IsExpired() && order.state != OrderState.Completed)
                {
                    expiredOrders.Add(order);
                }
            }
            foreach (DeliveryOrder expired in expiredOrders)
            {
                ExpireOrder(expired);
            }
        }
    }

    private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 400, 1300));

            GUILayout.Label("=== ��� �ֹ� ===");
            GUILayout.Label($"Ȱ�� �ֹ�: {currentOrders.Count}��");
            GUILayout.Label($"�Ⱦ� ���: {GetPickWaitingCount()}��");
            GUILayout.Label($"��� ���: {GetDeliveryWaitingCount()}��");
            GUILayout.Label($"������ �ֹ�: {completedOrders}�� | ����: {expiredOrders}");

            GUILayout.Space(10);

            foreach (DeliveryOrder order in currentOrders)
            {
            string status = order.state == OrderState.WaitingPickup ? "�Ⱦ� ���" : "��� ���";
                float timeLeft = order.GetRemainingTime();

                GUILayout.Label($"#{order.orderId} | {order.restaurantName} -> {order.customerName}");
                GUILayout.Label($"{status} ] {timeLeft:F0} �� ����");
            }

            GUILayout.EndArea();
        }
    }

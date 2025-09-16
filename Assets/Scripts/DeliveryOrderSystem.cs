using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DeliveryOrderSystem : MonoBehaviour
{
    [Header("주문 설정")]
    public float ordergenrateInterval = 15f; //주문 생성 간격 (15초)
    public int maxActiveOrders = 8; //최대 활성 주문 수

    [Header("게임 상태")]
    public int totalOrdersGenerated = 0;
    public int completedOrders = 0;
    public int expiredOrders = 0;

    //주문 리스트
    private List<DeliveryOrder> currentOrders = new List<DeliveryOrder>();

    //Building 참조
    private List<Building> restaurants = new List<Building>();
    private List<Building> customers = new List<Building>();

    //event 시스템
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

        //초기 주문 생성
        StartCoroutine(GenerateInitialOrders());
        //주기적인 주문 생성
        StartCoroutine(orderGenerator());
        //만료 체크
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

        Debug.Log($"음식점 {restaurants.Count}개, 고객 {customers.Count}개 찾음.");
    }

    void CreateNewOrder()
    {
        if (restaurants.Count == 0 || customers.Count == 0) return;
        {
            //랜덤 음식점과 고객 선택
            Building randomRestaurant = restaurants[Random.Range(0, restaurants.Count)];
            Building randomCustomer = customers[Random.Range(0, customers.Count)];

            //만약 같은 건물이면 다시 선택
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

    void PickupOrder(DeliveryOrder order)  //픽업 함수
    {
        order.state = OrderState.PickedUp;
        orderEvents.OnOrderPickedUp?.Invoke(order);
    }

    void CompleteOrder(DeliveryOrder order) //완료 함수
    {
        order.state = OrderState.Completed;
        completedOrders++;

        //보상 주는거
        if(driver != null)
        {
            driver.AddMoney(order.reward);
        }

        //완료주문 제거
        currentOrders.Remove(order);
        orderEvents.OnOrderCompleted?.Invoke(order);
    }

    void ExpireOrder(DeliveryOrder order) //만료 함수, 주문 취소 소멸
    {
        order.state = OrderState.Expired;
        expiredOrders++;

        //만료 주문 제거
        currentOrders.Remove(order);
        orderEvents.OnOrderExpired?.Invoke(order);
    }

    //UI 정보 제공
    public List<DeliveryOrder> GetCurrentOrders()
    {
        return new List<DeliveryOrder>(currentOrders);
    }

    public int GetPickWaitingCount() // 픽업 대기중인 주문 수
    {
        int count = 0;
        foreach (DeliveryOrder order in currentOrders)
        {
            if (order.state == OrderState.WaitingPickup) count++;
        }
        return count;
    }
    public int GetDeliveryWaitingCount() // 배달 대기중인 주문 수
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

    //Event 처리
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

        // 시작 시 3개 주문 생성
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

            GUILayout.Label("=== 배달 주문 ===");
            GUILayout.Label($"활성 주문: {currentOrders.Count}개");
            GUILayout.Label($"픽업 대기: {GetPickWaitingCount()}개");
            GUILayout.Label($"배달 대기: {GetDeliveryWaitingCount()}개");
            GUILayout.Label($"생성된 주문: {completedOrders}개 | 만료: {expiredOrders}");

            GUILayout.Space(10);

            foreach (DeliveryOrder order in currentOrders)
            {
            string status = order.state == OrderState.WaitingPickup ? "픽업 대기" : "배달 대기";
                float timeLeft = order.GetRemainingTime();

                GUILayout.Label($"#{order.orderId} | {order.restaurantName} -> {order.customerName}");
                GUILayout.Label($"{status} ] {timeLeft:F0} 초 남음");
            }

            GUILayout.EndArea();
        }
    }

using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;



public class GarageHndlr : MonoBehaviour
{
    [System.Serializable]
    public class Car
    {
        public string carID; // Unique ID for each car
        public GameObject carModel;
        public float Speed;
        public float Acceleration;
        public float Brake;
        public float Handling;
        public float Durabilty;
        public int carPrice;
        public int CarNumber;
    }


    public List<Car> cars;
    public Text carPriceText;
    public GameObject carPrice;
    public GameObject purchaseButton;
    public GameObject NextBtn;
    public GameObject Locked;
    public GameObject purchasesuccess;
    public GameObject NotenoughCoin;
    public Button arrownxt;
    public Button arrowbck;
    private int currentIndex = 0;



    [Header("Specifications")]
    public Image Speed;
    public Image Acceleration;
    public Image Brake;
    public Image Handling;
    public Image Durabilty;


    private void OnEnable()
    {
        
    }
    private void Start()
    {
        InitializeCarData();
    }

    private void InitializeCarData()
    {
        PlayerPrefs.SetInt(cars[0].carID, 1);
        // Initialize purchase state for each car
        foreach (var car in cars)
        {
            // If the car's purchase state doesn't exist in PlayerPrefs, set it as not purchased
            if (!PlayerPrefs.HasKey(car.carID))
            {
                PlayerPrefs.SetInt(car.carID, 0); // 0 = not purchased, 1 = purchased
            }
        }

        UpdateCarUI();
    }

    private void UpdateCarUI()
    {
        // Hide all car models
        foreach (var car in cars)
        {
            car.carModel.SetActive(false);
        }

        // Show current car model
        Car currentCar = GetCurrCar();
        currentCar.carModel.SetActive(true);




        // Check if the car is purchased
        bool isPurchased = PlayerPrefs.GetInt(currentCar.carID) == 1;
        Debug.Log("IsPurchased   "+isPurchased);
        if (isPurchased)
        {
            Locked.SetActive(false);
            carPrice.SetActive(false);
            purchaseButton.SetActive(false);
            NextBtn.SetActive(true);

        }
        else
        {
            carPriceText.text = $"{currentCar.carPrice}";
            carPrice.SetActive(true);
            Locked.SetActive(true);
            purchaseButton.SetActive(true);
            NextBtn.SetActive(false);
        }


         AnimateSpec(Speed, currentCar.Speed );
         AnimateSpec(Acceleration, currentCar.Acceleration);
         AnimateSpec(Brake, currentCar.Brake);
         AnimateSpec(Handling, currentCar.Handling );
         AnimateSpec(Durabilty, currentCar.Durabilty );
    }

    // This method calls the coroutine for animating the spec bars
    private void AnimateSpec(Image image, float targetValue)
    {
        StartCoroutine(AnimateSpecCoroutine(image, targetValue));
    }

    // Coroutine to animate the fillAmount over time
    private IEnumerator AnimateSpecCoroutine(Image image, float targetValue)
    {
        float startValue = image.fillAmount;
        float duration = 0.5f; // Duration in seconds
        float elapsed = 0f;

        // While the elapsed time is less than the duration, continue the animation
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;  // Update elapsed time
            image.fillAmount = Mathf.Lerp(startValue, targetValue, elapsed / duration);  // Lerp fillAmount
            yield return null;  // Wait for the next frame
        }

        // Ensure the final value is exactly the target value
        image.fillAmount = targetValue;
    }




    public void ShowNextCar()
    {

        if (MySoundManager.instance)
            MySoundManager.instance.PlayButtonClickSound(1f);
        // Increment the index, loop back if needed
        currentIndex = (currentIndex + 1) % cars.Count;
        UpdateCarUI();
    }
    
    public void ShowPrevCar()
    {

        if (MySoundManager.instance)
            MySoundManager.instance.PlayButtonClickSound(1f);
        // Decrement the index, loop back if needed (handling negative index)
        currentIndex = (currentIndex - 1 + cars.Count) % cars.Count;
        UpdateCarUI();
    }

    public void PurchaseCar()
    {
        if (MySoundManager.instance)
            MySoundManager.instance.PlayButtonClickSound(1f);
        Car currentCar = GetCurrCar();

        // Check if the car is already purchased
        if (PlayerPrefs.GetInt(currentCar.carID) == 0 && currentCar.carPrice<=ValStorage.GetCoins())
        {
            purchasesuccess.SetActive(true);

            ValStorage.SetCoins(ValStorage.GetCoins()- currentCar.carPrice);
            MMManager.Instance.SetCoins();

            // Simulate purchase logic (you can replace this with real currency handling)
            PlayerPrefs.SetInt(currentCar.carID, 1); // Mark as purchased
            PlayerPrefs.Save(); // Save PlayerPrefs data
            UpdateCarUI();
        }
        else 
        {
            NotenoughCoin.SetActive(true);
        }
    }

    public Car GetCurrCar() 
    {
        Car currentCar = cars[currentIndex];
        return currentCar;
    }

    public string GetCurrCarId()
    {
        Car currCar = GetCurrCar();
        return currCar.carID;
    }
    public int GetCurrCarNumber()
    {
        Car currCar = GetCurrCar();
        return currCar.CarNumber;
    }
}

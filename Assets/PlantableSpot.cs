using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
public class PlantableSpot : MonoBehaviour
{
    [SerializeField] private SpriteRenderer cropRenderer;
    [SerializeField] RectTransform _UIOptions;
    [SerializeField] List<Button> _options;
    private PlantData currentPlantData;
    private Coroutine growPlantCoroutine;
    [SerializeField] float _moneyDecreaseRate; // The rate at which money earned decreases over time after peak
    float _harvestTime;
    [SerializeField] float _optionsScaleSpeed;
    bool _planted;

    public void PlantCrop(PlantData plantData)
    {
        if(!_planted){
            currentPlantData = plantData;
            StartCoroutine(GrowPlant());
            _UIOptions.parent.gameObject.SetActive(false);
        }
    }


    private IEnumerator GrowPlant()
    {
        cropRenderer.sprite = currentPlantData.seedSprite;
        yield return new WaitForSeconds(currentPlantData.growthTime);
        cropRenderer.sprite = currentPlantData.grownSprite;
        _harvestTime = Time.time + currentPlantData.deadTime;
        yield return new WaitForSeconds(currentPlantData.deadTime);
        cropRenderer.sprite = currentPlantData.deadSprite;
        yield return new WaitForSeconds(currentPlantData.deadTime);
    }

    public int Harvest() {
        if (currentPlantData != null && cropRenderer.sprite == currentPlantData.grownSprite) {
            // Calculate money based on time since harvest peak
            float timeLeftToWilt = _harvestTime - Time.time;
            // Assuming timeSincePeak starts negative (harvest time in the future), and becomes positive once the peak passes
            float fractionOfValueLost = Mathf.Clamp01((currentPlantData.deadTime - timeLeftToWilt) / currentPlantData.deadTime);
            int moneyEarned = Mathf.RoundToInt(currentPlantData.baseMoney * (1f - fractionOfValueLost));
            
            ResetPlantableSpot();
            return moneyEarned;
        } else if (currentPlantData != null && cropRenderer.sprite == currentPlantData.deadSprite) {
            ResetPlantableSpot();
            return 0; // No money earned for dead plants
        }
        return 0; // Default return if neither condition is met
    }

    void ResetPlantableSpot(){
        cropRenderer.sprite = null;
        currentPlantData = null; // Reset plant data after harvesting
        _planted = false;
        _harvestTime = 0;
        StopAllCoroutines();
    }

    public void SpotPressedOn(){
        if(!_planted) StartCoroutine(ShowOptions());
    }

    private IEnumerator ShowOptions() {
        _UIOptions.parent.gameObject.SetActive(true);
        _UIOptions.gameObject.SetActive(true);
        _UIOptions.localScale = Vector3.zero;
        // Scale up the UI options until the mouse button is released
        while (Mouse.current.leftButton.isPressed) {
            _UIOptions.localScale = Vector3.MoveTowards(_UIOptions.localScale, Vector3.one, _optionsScaleSpeed * Time.deltaTime);
            yield return null;
        }
        // After mouse release
        ClickClosestButton();
        _planted = true;
    }

    private void ClickClosestButton()
    {
        if(!_planted){
            Vector2 mouseScreenPosition = Mouse.current.position.ReadValue();
            Button closestButton = null;
            float closestDistance = float.MaxValue;

            foreach (Button button in _options)
            {
                // You may need to convert the button's position to screen space if it's not already
                Vector3 buttonPosition = Camera.main.WorldToScreenPoint(button.transform.position);
                
                // Calculate the squared distance to avoid unnecessary square root calculation
                float distance = (mouseScreenPosition - (Vector2)buttonPosition).sqrMagnitude;

                if (distance < closestDistance)
                {
                    closestButton = button;
                    closestDistance = distance;
                }
            }

            closestButton?.onClick.Invoke(); // Check if closestButton is not null and then invoke click
        }
    }
}

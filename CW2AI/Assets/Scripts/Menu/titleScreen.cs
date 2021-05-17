﻿// The second script to run. Relies on LoadMenu.cs and GameManager.cs in order to initialise.
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class titleScreen : MonoBehaviour
{
    public GridManager Grid;
    public GameObject cameraMain;
    public GameObject cameraObject;
    public GameObject titleButton;
    public GameObject subButton;
    public GameObject tutorialButton;
    public GameObject speedIcon;
    [HideInInspector] public bool Loaded = false;
    public void Update() { if (Loaded) { if (Input.GetKeyDown(KeyCode.R)) RestartScene(); if (Input.GetKeyDown(KeyCode.T)) ChangeSpeed(); } }
    public IEnumerator StartGame()
    {        
        // Reveals the Game.
        LeanTween.alphaCanvas(GameObject.Find("Transition").GetComponent<CanvasGroup>(), 0, 0.5f).setEase(LeanTweenType.easeInOutQuad);
        yield return new WaitForSeconds(0.5f);

        LeanTween.alphaCanvas(titleButton.GetComponent<CanvasGroup>(), 1, 0.5f).setEase(LeanTweenType.easeInOutQuad);
        yield return new WaitForSeconds(0.5f);
        LeanTween.alphaCanvas(subButton.GetComponent<CanvasGroup>(), 1, 0.5f).setEase(LeanTweenType.easeInOutQuad);
        LeanTween.alphaCanvas(tutorialButton.GetComponent<CanvasGroup>(), 1, 0.5f).setEase(LeanTweenType.easeInOutQuad);
        yield return new WaitForSeconds(0.5f);
        yield return new WaitUntil(() => Input.anyKey);
        LeanTween.alphaCanvas(titleButton.GetComponent<CanvasGroup>(), 0, 0.5f).setEase(LeanTweenType.easeInOutQuad);
        LeanTween.alphaCanvas(subButton.GetComponent<CanvasGroup>(), 0, 0.5f).setEase(LeanTweenType.easeInOutQuad);
        LeanTween.alphaCanvas(tutorialButton.GetComponent<CanvasGroup>(), 0, 0.5f).setEase(LeanTweenType.easeInOutQuad);
        yield return new WaitForSeconds(0.5f);
        LeanTween.value(this.gameObject, SpeedUp, 2f, 14f, 5f).setEase(LeanTweenType.easeInOutQuad);
        // Removes all HUD objects.
        titleButton.SetActive(false);
        subButton.SetActive(false);
        tutorialButton.SetActive(false);
        // Activates all Player Inputs and game commands.
        cameraMain.GetComponent<cameraMove>().enabled = true;
        cameraObject.GetComponent<cameraStop>().enabled = true;        
        Grid.Kingdom.LateStart(); Grid.Kingdom.CreateUnit("Captain");
        yield return new WaitForSeconds(0.5f);
        LeanTween.value(this.gameObject, SetAlpha, 0, 0.75f, 5f).setEase(LeanTweenType.easeInOutQuad);
        yield return new WaitForSeconds(5f);
        Loaded = true; speedIcon.SetActive(true);
        FinishedAlpha();
    }
    void SpeedUp(float value) { Time.timeScale = value; }
    void SetAlpha(float value)
    {
        Grid.Kingdom.Buildings[0].GetChild(1).GetChild(0).GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, value);
        Grid.Kingdom.Buildings[0].GetChild(1).GetChild(0).GetChild(0).GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, value);
        Grid.Kingdom.Buildings[0].GetChild(1).GetChild(1).GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, value);
        Grid.Kingdom.Buildings[0].GetChild(1).GetChild(1).GetChild(0).GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, value);
        Grid.Kingdom.Buildings[0].GetChild(1).GetChild(2).GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, value);
        Grid.Kingdom.Buildings[0].GetChild(1).GetChild(2).GetChild(0).GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, value);
        Grid.Kingdom.Buildings[0].GetChild(1).GetChild(3).GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, value);
        Grid.Kingdom.Buildings[0].GetChild(1).GetChild(3).GetChild(0).GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, value);
    }
    void FinishedAlpha()
    {
        Grid.Kingdom.Buildings[0].GetChild(1).GetChild(0).GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.75f);
        Grid.Kingdom.Buildings[0].GetChild(1).GetChild(0).GetChild(0).GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f);
        Grid.Kingdom.Buildings[0].GetChild(1).GetChild(1).GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.75f);
        Grid.Kingdom.Buildings[0].GetChild(1).GetChild(1).GetChild(0).GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f);
        Grid.Kingdom.Buildings[0].GetChild(1).GetChild(2).GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.75f);
        Grid.Kingdom.Buildings[0].GetChild(1).GetChild(2).GetChild(0).GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f);
        Grid.Kingdom.Buildings[0].GetChild(1).GetChild(3).GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.75f);
        Grid.Kingdom.Buildings[0].GetChild(1).GetChild(3).GetChild(0).GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f);
    }
    public void RestartScene() { SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); Time.timeScale = 1; }
    void ChangeSpeed() { if (Time.timeScale == 2) { Time.timeScale = 14; speedIcon.SetActive(true); } else { Time.timeScale = 2; speedIcon.SetActive(false); } }
}
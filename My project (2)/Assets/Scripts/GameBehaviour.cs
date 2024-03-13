using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameBehaviour : MonoBehaviour
{
    public static GameBehaviour Instance;
    [SerializeField] CheesesGot _totalCheese;
    public static int  _onLevel = 1;
    bool updateLevelOnce = true;
    public enum GameState {
        Play,
        Pause,
        Die,
        ScreenMoveUp
    }
    public GameState State = GameState.Play;
    void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
            Destroy(this);
        else
            Instance = this;
    }
    public void CollectItem (int arrayNumber){
        _totalCheese.Score++;
    }
    void Update() {
        if(PlayerMovement._isDead) {
            State = GameState.Die;
        }
        else if(PlayerMovement._isLevelTransition) {
            State = GameState.ScreenMoveUp;
            if(updateLevelOnce){
                _onLevel++;
                updateLevelOnce = false;
            }
        }
        else {
            updateLevelOnce = true;
            State = GameState.Play;
        }
        if(Input.GetKeyDown(KeyCode.Space)) {
            State = State == GameState.Play ? GameState.Pause : GameState.Play;
        }
    }
}

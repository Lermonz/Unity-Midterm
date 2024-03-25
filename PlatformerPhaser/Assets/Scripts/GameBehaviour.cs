using UnityEngine;

public class GameBehaviour : MonoBehaviour
{
    public static GameBehaviour Instance;
    [SerializeField] CheesesGot _totalCheese;
    [SerializeField] DeathCount _totalDeath;
    int _trueCheeses;
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
    public void CollectItem(){
        _totalCheese.Score++;
    }
    public void YouDied() {
        _totalCheese.Score = _trueCheeses;
        _totalDeath.Deaths++;
    }
    void Update() {
        if(PlayerMovement._isDead) {
            State = GameState.Die;
        }
        else if(PlayerMovement._isLevelTransition) {
            _trueCheeses = _totalCheese.Score;
            State = GameState.ScreenMoveUp;
            if(updateLevelOnce){
                _onLevel++;
                updateLevelOnce = false;
            }
        }
        else if (State != GameState.Pause){
            updateLevelOnce = true;
            State = GameState.Play;
        }
        if(Input.GetKeyDown(KeyCode.Space)) {
            Debug.Log("pause");
            if(State == GameState.Play) {
                Time.timeScale = 0f;
                State = GameState.Pause;
            }
            else {
                Time.timeScale = 1f;
                State = GameState.Play;
            }
        }
    }
}

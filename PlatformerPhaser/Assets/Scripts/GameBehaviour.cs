using UnityEngine;

public class GameBehaviour : MonoBehaviour
{
    public static GameBehaviour Instance;
    [SerializeField] CheesesGot _totalCheese;
    [SerializeField] DeathCount _totalDeath;
    [SerializeField] FinalScore _finalScore;
    int _trueCheeses;
    public static int _onLevel = 1;
    bool updateLevelOnce = true;
    public static int _score = 100000;
    int _timeScore;
    int _cheeseScore;
    int _deathScore;
    bool _updateScore = true;
    public enum GameState {
        Play,
        Pause,
        Die,
        ScreenMoveUp,
        Finale,
        Title
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
    public void FinalizeScore() {
        Debug.Log(_score);
        _finalScore.TheScore = _score;
    }
    void Update() {
        int ms = Mathf.FloorToInt(Timer.ms*.1f);
        _timeScore = (Timer.m*6000+Timer.s*100+ms);
        _cheeseScore = _totalCheese.Score * 10000;
        _deathScore = _totalDeath.Deaths * 2000;
        if(_updateScore) {
            _score = Mathf.FloorToInt(100000 - _timeScore - _deathScore + _cheeseScore);
            if(_score < 0) {
                _score = 0;
            }
        }
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
        else if(_onLevel == 10) {
            State = GameState.Finale;
            _updateScore = false;
            FinalizeScore();
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

using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] float _hSpeed = 8f;
    [SerializeField] float _hSpeedAir = 3.8f;
    [SerializeField] float _hSpeedWJ = 0.8f;
    [SerializeField] float _jumpVelocity;
    float _initJumpVelocity = 14f;
    [SerializeField] float _wallJumpVelocity;
    float _initWallJumpVelocity = 11f;
    float wallJumpDirection;
    [SerializeField] float _traction;
    [SerializeField] bool _isGrounded;
    [SerializeField] bool _isJumping;
    [SerializeField] bool _isWallJumping = false;
    [SerializeField] bool _canWallJump;
    bool _isCrouch = false;
    bool _touchingDeath = false;
    bool _isPower = false;
    bool _isPowerDisplay = false;
    float _powerTime = 5f;
    Coroutine _powerCoroutine;
    float _finalWallJumpSpeed;
    [SerializeField] float _runInput;
    [SerializeField] float _maxRunSpeed = 7.5f;
    Rigidbody2D _body;
    BoxCollider2D _collider;
    SpriteRenderer _renderer;
    [SerializeField] bool _isPhaseMode;
    [SerializeField] AudioClip jumpClip;
    Camera _camera;
    Animator _animator;
    float _vertCameraBounds;
    bool _ignoreVCB = false;
    public static bool _isDead;
    bool _isDeadDisplay;
    public static bool _isLevelTransition;
    Vector3 _origin;
    void Awake()
    {
        _body = GetComponent<Rigidbody2D>();
        _collider = GetComponent<BoxCollider2D>();
        _renderer = GetComponent<SpriteRenderer>();
        _animator = GetComponent<Animator>();
        _camera = GameObject.Find("Main Camera").GetComponent<Camera>();
        _vertCameraBounds = Utilities.SetYLimit(_camera, GetComponent<SpriteRenderer>());
        _isJumping = false;
        _origin = transform.position;
    }
    void Update()
    {
        if(GameBehaviour.Instance.State == GameBehaviour.GameState.Play) {
            // checking nearest point of Ground
            Vector2 rayFloorOriginL = new Vector2(_collider.bounds.center.x-transform.localScale.x*0.49f,_collider.bounds.min.y); // set origin point for ray to check ground
            Vector2 rayFloorOriginR = new Vector2(_collider.bounds.center.x+transform.localScale.x*0.49f,_collider.bounds.min.y); // set origin point for ray to check ground
            LayerMask maskSolids;
            if(_isPhaseMode) {
                maskSolids = LayerMask.GetMask("Solids","ReversePhasable","SpikeCollision"); // only register floors and walls
            }
            else {
                maskSolids = LayerMask.GetMask("Solids","Phasable","SpikeCollision"); // only register floors and walls
            }
            RaycastHit2D nearestGroundL = Physics2D.Raycast(rayFloorOriginL, Vector2.down, Mathf.Infinity, maskSolids);
            RaycastHit2D nearestGroundR = Physics2D.Raycast(rayFloorOriginR, Vector2.down, Mathf.Infinity, maskSolids);
            _isGrounded = nearestGroundL.distance < 0.1f || nearestGroundR.distance < 0.1f; // if ray to floor is small enough, then you are touching ground
            // checking nearest point of Wall
            float rayWallXPosition = !_renderer.flipX ? _collider.bounds.max.x : _collider.bounds.min.x;
            Vector2 rayWallOriginT = new Vector2(rayWallXPosition,_collider.bounds.max.y);
            Vector2 rayWallOriginB = new Vector2(rayWallXPosition,_collider.bounds.min.y);
            RaycastHit2D nearestWallTop = Physics2D.Raycast(rayWallOriginT, !_renderer.flipX ? Vector2.right : Vector2.left, Mathf.Infinity, maskSolids);
            RaycastHit2D nearestWallBottom = Physics2D.Raycast(rayWallOriginB, !_renderer.flipX ? Vector2.right : Vector2.left, Mathf.Infinity, maskSolids);
        
            _runInput = Input.GetAxisRaw("Horizontal");
            _runInput = _isDead ? 0 : _runInput;
            _runInput = _isCrouch && _isGrounded ? 0 : _runInput;
            if(Mathf.Abs(transform.position.x) > Mathf.Abs(Utilities.SetXLimit(_camera,_renderer))) {
                float stayAway = (Utilities.SetXLimit(_camera,_renderer)-0.01f)*Mathf.Sign(transform.position.x);
                transform.position = new Vector3(stayAway, transform.position.y, transform.position.z);
            }
            //animation
            if(!_isGrounded) {
                if(_body.velocity.y <= 0) {
                    _animator.SetTrigger("JumpAnimDown");
                }
                else {
                    _animator.SetTrigger("JumpAnimUp");
                }
            }
            else if(_isGrounded) {
                if(_runInput != 0) {
                    _animator.SetTrigger("WalkingAnim");
                }
                else {
                    _animator.SetTrigger("IdleGrounded");
                }
            }
            //prevent negative speed
            if(_hSpeed < 0) {
                _hSpeed = 0;
            }
            if(_jumpVelocity < 0) {
                _jumpVelocity = 0;
            }
            if(_wallJumpVelocity < 0) {
                _wallJumpVelocity = 0;
            }
            //phase collision ignores
            if(Input.GetKey(KeyCode.P)) {
                Physics2D.IgnoreLayerCollision(0,3);
                Physics2D.IgnoreLayerCollision(0,8,false);
                _isPhaseMode = true;
            }
            else {
                Physics2D.IgnoreLayerCollision(0,3,false);
                Physics2D.IgnoreLayerCollision(0,8);
                _isPhaseMode = false;
            }
            if(_isPower) {
                Physics2D.IgnoreLayerCollision(0,9,false);
            }
            else {
                Physics2D.IgnoreLayerCollision(0,9);
            }
            //Color
            if(_isPowerDisplay){
                _renderer.color = Color.Lerp(Color.red, Color.green, Mathf.PingPong(Time.time*5, 1f));
            }
            else if(_isPhaseMode) {
                _renderer.color = new Color(0.5f,0.5f,1f,0.85f);
            }
            else {
                _renderer.color = new Color(1f,1f,1f,1f);
            }
            //checks if overlapping a wall when in the wrong phase type
            LayerMask maskPhasables = LayerMask.GetMask("Phasable");
            Collider2D checkWallOverlap = Physics2D.OverlapBox(_collider.bounds.center, new Vector2(0.01f,0.01f), 180, maskPhasables);
            LayerMask maskReversePhasables = LayerMask.GetMask("ReversePhasable");
            Collider2D checkReverseWallOverlap = Physics2D.OverlapBox(_collider.bounds.center, new Vector2(0.01f,0.01f), 180, maskReversePhasables);
            bool overlapWall = false;
            if((checkWallOverlap != null && !_isPhaseMode) || (checkReverseWallOverlap != null && _isPhaseMode)) {
                overlapWall = true;
            }
            //crouch
            if(Input.GetKeyDown(KeyCode.S)) {
                transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y*0.5f, 1);
                transform.position += Vector3.down*0.15f;
                _isCrouch = true;
                _hSpeed = 0f;
                _traction += 0.05f;
                _maxRunSpeed *= 0.5f;
            }
            if(Input.GetKeyUp(KeyCode.S)) {
                transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y*2, 1);
                _isCrouch = false;
                _hSpeed = 8f;
                _traction -= 0.05f;
                _maxRunSpeed *= 2;
            }
            // move horizontally
            if(_runInput*_body.velocity.x < _maxRunSpeed) {
                if(_isGrounded) {
                    _body.AddForce(Vector2.right*_runInput*_hSpeed);
                }
                else if(_isWallJumping) {
                    _body.AddForce(Vector2.right*_runInput*_hSpeedWJ);
                }
                else {
                    _body.AddForce(Vector2.right*_runInput*_hSpeedAir);
                }
            }
            // caps run speed at _maxRunSpeed
            if(Mathf.Abs(_runInput*_body.velocity.x) >= _maxRunSpeed && !_isWallJumping) {
                _body.velocity = new Vector2(Mathf.Sign(_body.velocity.x) * _maxRunSpeed,_body.velocity.y);
            }
            if(_runInput == 0 && !_isWallJumping) {
                _body.velocity = new Vector2(_body.velocity.x * _traction,_body.velocity.y);
            }
            //transform.Translate(Vector3.right*Time.deltaTime*_runInput*_hSpeed);
            // flips player sprite
            if (((_runInput > 0 && _renderer.flipX) || (_runInput < 0 && !_renderer.flipX)) && !_isWallJumping) {
                _renderer.flipX = !_renderer.flipX;
            }
            
            // if you are grounded, you are not jumping
            if(_isGrounded) {
                //_isJumping = false;
                _isWallJumping = false;
            }
            // Grounded Jump
            if(Input.GetKeyDown(KeyCode.O) && _isGrounded) {
                _jumpVelocity = _initJumpVelocity;
                _isJumping = true;
                _body.AddForce(transform.up*_jumpVelocity, ForceMode2D.Impulse);
                GetComponent<AudioSource>().clip = jumpClip;
                GetComponent<AudioSource>().Play();
            }
            if(_isJumping && Input.GetKeyUp(KeyCode.O)) {
                _body.velocity = new Vector2(_body.velocity.x, _body.velocity.y*0.6f);
                _isJumping = false;
            }
            if((nearestWallTop.distance < 0.1f || nearestWallBottom.distance < 0.1f)&& !_isGrounded) {
                _canWallJump = true;
            }
            else {
                _canWallJump = false;
            }
            // Wall Jump
            
            if(Input.GetKeyDown(KeyCode.O) && _canWallJump) {
                _body.velocity = Vector2.zero;
                _wallJumpVelocity = _initWallJumpVelocity;
                wallJumpDirection = _renderer.flipX ? 1 : -1;   
                _renderer.flipX = !_renderer.flipX; //flip sprite on walljump
                _isWallJumping = true;
                _body.AddForce(new Vector2(_wallJumpVelocity*wallJumpDirection, _wallJumpVelocity), ForceMode2D.Impulse);
                StartCoroutine(WallJumpHorizontalDecrease());
                GetComponent<AudioSource>().clip = jumpClip;
                GetComponent<AudioSource>().Play();
            }
            if(Mathf.Abs(_body.velocity.x) < _finalWallJumpSpeed && !_isGrounded) {
                float alpha = (((_finalWallJumpSpeed/Mathf.Abs(_body.velocity.x))-1)*0.95f)+1; // multiplier to decrease movement speed in the opposite direction from where you wall jumped
                _body.velocity = new Vector2(_body.velocity.x*alpha, _body.velocity.y);
                _finalWallJumpSpeed = Mathf.Abs(_body.velocity.x);
            }
            else if(Mathf.Abs(_body.velocity.x) > _finalWallJumpSpeed || _body.velocity.y < 0) {
                _finalWallJumpSpeed = 0;
            }
            if(transform.position.y < (_vertCameraBounds-_camera.orthographicSize*2+1) || ((_touchingDeath && !_isPower)|| overlapWall)) {
                _isDead = true;
                _isDeadDisplay = true;
            }
            if(_isDead){
                StartCoroutine(Respawn());
                GameBehaviour.Instance.YouDied();
                if(_powerCoroutine != null && _isPower){
                    StopCoroutine(_powerCoroutine);
                }
                //kill player
                //reset position to origin point of screen
            }
            if(_isDeadDisplay){
                _animator.SetTrigger("DeathAnim");
            }
            if(transform.position.y > (_vertCameraBounds+0.5f) && !_ignoreVCB) {
                _isLevelTransition = true;
                StartCoroutine(IgnoreVertBounds());
                _origin = new Vector3(transform.position.x, -7.5f+20*GameBehaviour._onLevel, transform.position.z);
                transform.position = _origin-Vector3.up*2.5f;
                _body.velocity = Vector2.up*_initJumpVelocity;
                //move camera up to next screen somehow????
                //reset _vertCameraBounds
                //reset player origin point
            }
        }
    }
    IEnumerator WallJumpHorizontalDecrease() {
        for(int i = 4; i > 0; i--) {
            _body.velocity = new Vector2(_body.velocity.x*0.85f, _body.velocity.y);
        }
        yield return new WaitForSeconds(0.55f);
        _finalWallJumpSpeed = Mathf.Abs(_body.velocity.x);
        _isWallJumping = false;
    }
    //DEATH
    IEnumerator Respawn() {
        yield return new WaitForSeconds(0.015f);
        _body.gravityScale = 0;
        _body.velocity = Vector2.zero;
        _touchingDeath = false;
        yield return new WaitForSeconds(0.3f);
        _isDeadDisplay = false;
        transform.position = _origin;
        _body.gravityScale = 3;
        yield return new WaitForSeconds(0.15f);
        _isDead = false;
    }
    IEnumerator IgnoreVertBounds() {
        _ignoreVCB = true;
        Physics2D.IgnoreLayerCollision(0,6);
        yield return new WaitForSeconds(0.1f);
        Physics2D.IgnoreLayerCollision(0,6,false);
        yield return new WaitForSeconds(0.9f);
        _vertCameraBounds = Utilities.SetYLimit(_camera, GetComponent<SpriteRenderer>());
        _ignoreVCB = false;
    }
    IEnumerator Invincible() {
        yield return new WaitForSeconds(_powerTime);
        _isPowerDisplay = false;
        yield return new WaitForSeconds(0.5f);
        _isPower = false;
    }
    void OnTriggerEnter2D(Collider2D other) {
        if(other.CompareTag("Kill")) {
            _touchingDeath = true;
        }
        if(other.CompareTag("Item")) {
            _isPower = true;
            _isPowerDisplay = true;
            StartCoroutine(Invincible());
        }
        if(other.CompareTag("Sign")) {
            GameObject dialogue = GameObject.Find("Sign Dialogue");
            dialogue.transform.position = Vector3.zero;
        }
    }
    void OnTriggerExit2D(Collider2D other) {
        if(other.CompareTag("Kill")) {
            _touchingDeath = false;
        }
        if(other.CompareTag("Sign")) {
            GameObject dialogue = GameObject.Find("Sign Dialogue");
            dialogue.transform.position = new Vector3(0,0,1000);
        }
    }
}

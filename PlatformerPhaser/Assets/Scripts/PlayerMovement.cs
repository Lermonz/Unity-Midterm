using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] float _hSpeed = 80f;
    [SerializeField] float _hSpeedAir = 38f;
    [SerializeField] float _hSpeedWJ = 8f;
    [SerializeField] float _jumpVelocity;
    float _initJumpVelocity = 14f;
    [SerializeField] float _wallJumpVelocity;
    float _initWallJumpVelocity = 11.5f;
    float wallJumpDirection;
    [SerializeField] float _traction;
    [SerializeField] bool _isGrounded;
    bool _canJump;
    bool _forceNoJump;
    [SerializeField] bool _isJumping;
    [SerializeField] bool _isWallJumping = false;
    [SerializeField] bool _canWallJump;
    bool _wallJumpCoroutine;
    bool _doJumpNow;
    bool _stopJumpEarly;
    bool _doWallJumpNow;
    bool _isCrouch = false;
    bool _touchingDeath = false;
    bool _isPower = false;
    bool _isPowerDisplay = false;
    float _powerTime = 5f;
    bool _powerCoroutine = false;
    bool overlapWall = false;
    bool _overlapWallKill = false;
    float _finalWallJumpSpeed;
    [SerializeField] float _runInput;
    [SerializeField] float _maxRunSpeed = 7.5f;
    Rigidbody2D _body;
    BoxCollider2D _collider;
    SpriteRenderer _renderer;
    [SerializeField] bool _isPhaseMode;
    [SerializeField] AudioClip jumpClip;
    [SerializeField] AudioClip[] deathClips;
    AudioSource _audioSource;
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
        _audioSource = GetComponent<AudioSource>();
        _camera = GameObject.Find("Main Camera").GetComponent<Camera>();
        _vertCameraBounds = Utilities.SetYLimit(_camera, GetComponent<SpriteRenderer>());
        _isJumping = false;
        _origin = transform.position;
    }
    void Update()
    {
        if(GameBehaviour.Instance.State == GameBehaviour.GameState.Play || GameBehaviour.Instance.State == GameBehaviour.GameState.Finale) {
            // checking nearest point of Ground
            Vector2 rayFloorOriginL = new Vector2(_collider.bounds.center.x-transform.localScale.x*0.49f,_collider.bounds.min.y); // set origin point for ray to check ground
            Vector2 rayFloorOriginR = new Vector2(_collider.bounds.center.x+transform.localScale.x*0.49f,_collider.bounds.min.y); // set origin point for ray to check ground
            LayerMask maskFloors;
            if(_isPower) {
                if(_isPhaseMode) {
                    maskFloors = LayerMask.GetMask("Solids","ReversePhasable","SpikeCollision","Oneway"); // only register floors and walls
                }
                else {
                    maskFloors = LayerMask.GetMask("Solids","Phasable","SpikeCollision","Oneway"); // only register floors and walls
                }
            }
            else {
                if(_isPhaseMode) {
                    maskFloors = LayerMask.GetMask("Solids","ReversePhasable","Oneway"); // only register floors and walls
                }
                else {
                    maskFloors = LayerMask.GetMask("Solids","Phasable","Oneway"); // only register floors and walls
                }
            }
            RaycastHit2D nearestGroundL = Physics2D.Raycast(rayFloorOriginL, Vector2.down, Mathf.Infinity, maskFloors);
            RaycastHit2D nearestGroundR = Physics2D.Raycast(rayFloorOriginR, Vector2.down, Mathf.Infinity, maskFloors);
            // if ray to floor is small enough, then you are touching ground
            
            // checking nearest point of Wall
            LayerMask maskWalls;
            if(_isPower) {
                if(_isPhaseMode) {
                    maskWalls = LayerMask.GetMask("Solids","ReversePhasable","SpikeCollision"); // only register walls
                }
                else {
                    maskWalls = LayerMask.GetMask("Solids","Phasable","SpikeCollision"); // only register walls
                }
            }
            else {
                if(_isPhaseMode) {
                    maskWalls = LayerMask.GetMask("Solids","ReversePhasable"); // only register walls
                }
                else {
                    maskWalls = LayerMask.GetMask("Solids","Phasable"); // only register walls
                }
            }
            float rayWallXPosition = !_renderer.flipX ? _collider.bounds.max.x : _collider.bounds.min.x;
            Vector2 rayWallOriginT = new Vector2(rayWallXPosition,_collider.bounds.max.y);
            Vector2 rayWallOriginB = new Vector2(rayWallXPosition,_collider.bounds.min.y);
            RaycastHit2D nearestWallTop = Physics2D.Raycast(rayWallOriginT, !_renderer.flipX ? Vector2.right : Vector2.left, Mathf.Infinity, maskWalls);
            RaycastHit2D nearestWallBottom = Physics2D.Raycast(rayWallOriginB, !_renderer.flipX ? Vector2.right : Vector2.left, Mathf.Infinity, maskWalls);
            if(nearestGroundL.distance < 0.1f || nearestGroundR.distance < 0.1f) {
                if(nearestWallTop.distance < 0.3f || nearestWallBottom.distance < 0.3f) {
                    if(nearestGroundL.distance < 0.1f && nearestGroundR.distance < 0.1f) {
                        _isGrounded = true;
                    }
                    else {
                        _isGrounded =  false;
                    }
                }
                _isGrounded = true;
            }
            else {
                _isGrounded = false;
            }
            _runInput = Input.GetAxisRaw("Horizontal");
            _runInput = _isDead ? 0 : _runInput;
            _runInput = _isCrouch && _isGrounded ? 0 : _runInput;
            if(Mathf.Abs(transform.position.x) > Mathf.Abs(Utilities.SetXLimit(_camera,_renderer))) {
                float stayAway = (Utilities.SetXLimit(_camera,_renderer)-0.01f)*Mathf.Sign(transform.position.x);
                transform.position = new Vector3(stayAway, transform.position.y, transform.position.z);
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
            if(Mathf.Abs(_body.velocity.x) < 0) {
                _body.velocity = new Vector2(0,_body.velocity.y);
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
                
                if(_isPhaseMode) {
                    _renderer.color = Color.Lerp(new Color(1f,0f,0.5f,0.85f), new Color(0f,1f,0.5f,0.85f), Mathf.PingPong(Time.time*5, 1f));
                }
                else {
                    _renderer.color = Color.Lerp(Color.red, Color.green, Mathf.PingPong(Time.time*5, 1f));
                }
            }
            else if(_isPhaseMode) {
                _renderer.color = new Color(0.5f,0.5f,1f,0.85f);
            }
            else {
                _renderer.color = new Color(1f,1f,1f,1f);
            }
            //checks if overlapping a wall when in the wrong phase type
            LayerMask maskPhasables = LayerMask.GetMask("Phasable");
            Collider2D checkWallOverlap = Physics2D.OverlapBox(_collider.bounds.center, new Vector2(.1f,.1f), 180, maskPhasables);
            LayerMask maskReversePhasables = LayerMask.GetMask("ReversePhasable");
            Collider2D checkReverseWallOverlap = Physics2D.OverlapBox(_collider.bounds.center, new Vector2(.1f,.1f), 180, maskReversePhasables);
            
            if((checkWallOverlap != null && !_isPhaseMode) || (checkReverseWallOverlap != null && _isPhaseMode)) {
                overlapWall = true;
            }
            else {
                overlapWall = false;
            }
            if(overlapWall) {
                StartCoroutine(KillDelay());
            }
            else {
                _overlapWallKill = false;
            }
            //crouch
            if(Input.GetKeyDown(KeyCode.S)) {
                transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y*0.5f, 1);
                transform.position += Vector3.down*0.15f;
                _isCrouch = true;
                _hSpeed = 0f;
                _maxRunSpeed *= 0.5f;
            }
            if(Input.GetKeyUp(KeyCode.S) && _isCrouch) {
                _isCrouch = false;
                transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y*2, 1);
                _hSpeed = 30f;
                _maxRunSpeed *= 2;
            }
            
            // flips player sprite
            if (((_runInput > 0 && _renderer.flipX) || (_runInput < 0 && !_renderer.flipX)) && !_isWallJumping) {
                _renderer.flipX = !_renderer.flipX;
            }
            if(!_forceNoJump) {
                // if you are grounded, you are not jumping
                if(_isGrounded) {
                    _canJump = true;
                    _isJumping = false;
                    _isWallJumping = false;
                    _traction = 0.81f;
                }
                else {
                    _traction = 0.9f;
                }
                if((nearestWallTop.distance < 0.2f || nearestWallBottom.distance < 0.2f) && !_isGrounded) {
                    _canWallJump = true;
                }
                else {
                    _canWallJump = false;
                }
                if(_canWallJump || _isJumping) {
                    _canJump = false;
                }
                if(_canJump && !_isGrounded) {
                    StartCoroutine(CoyoteTime());
                }
                // Grounded Jump
                if(Input.GetKeyDown(KeyCode.O) && _canJump) {
                    _jumpVelocity = _initJumpVelocity;
                    _canJump = false;
                    _doJumpNow = true;
                    _isJumping = true;
                    _audioSource.PlayOneShot(jumpClip);
                }
                if(Input.GetKeyUp(KeyCode.O)) {
                    _stopJumpEarly = true;
                }
                // Wall Jump
                
                if(Input.GetKeyDown(KeyCode.O) && _canWallJump) {
                    _body.velocity = Vector2.zero;
                    _wallJumpVelocity = _initWallJumpVelocity;
                    wallJumpDirection = _renderer.flipX ? 1 : -1;   
                    _renderer.flipX = !_renderer.flipX; //flip sprite on walljump
                    _isWallJumping = true;
                    _doWallJumpNow = true;
                    StartCoroutine(WallJumpHorizontalDecrease());
                    _audioSource.PlayOneShot(jumpClip);
                }
                /*Debug.Log("body velocity: "+_body.velocity);
                if(_finalWallJumpSpeed != 0) {
                    Debug.Log(_finalWallJumpSpeed);
                }*/
                if(Mathf.Abs(_body.velocity.x) < _finalWallJumpSpeed && !_isGrounded) {
                    //Debug.Log("PRE alpha: "+_body.velocity);
                    float alpha = _body.velocity.x != 0 ? (((_finalWallJumpSpeed/Mathf.Abs(_body.velocity.x))-1)*0.95f)+1 : 0; // multiplier to decrease movement speed in the opposite direction from where you wall jumped
                    _body.velocity = new Vector2(_body.velocity.x*alpha, _body.velocity.y);
                    _finalWallJumpSpeed = Mathf.Abs(_body.velocity.x);
                    //Debug.Log("Alpha: "+alpha+"\nFinal Wall Jump Speed: "+_finalWallJumpSpeed);
                    //Debug.Log("POST alpha: "+_body.velocity);
                }
                else if(Mathf.Abs(_body.velocity.x) > _finalWallJumpSpeed || _body.velocity.y < 0) {
                    _finalWallJumpSpeed = 0;
                }
                if(!_isWallJumping) {
                    _finalWallJumpSpeed = 0;
                    if(_wallJumpCoroutine) {
                        _wallJumpCoroutine = false;
                        StopCoroutine(WallJumpHorizontalDecrease());
                    }
                }
            }
            //dies
            if(transform.position.y < (_vertCameraBounds-_camera.orthographicSize*2+1) || ((_touchingDeath && !_isPower)|| _overlapWallKill) || transform.position.y < -9.5f) {
                _isDead = true;
                _isDeadDisplay = true;
                _overlapWallKill = false;
            }
            if(_isDead){
                StartCoroutine(Respawn());
                GameBehaviour.Instance.YouDied();
                if(_powerCoroutine && _isPower){
                    StopCoroutine(Invincible());
                    _isPower = false;
                    _isPowerDisplay = false;
                    _powerCoroutine = false;
                }
                //kill player
                //reset position to origin point of screen
            }
            
            if(transform.position.y > (_vertCameraBounds+0.5f) && !_ignoreVCB && transform.position.y > 10) {
                _isLevelTransition = true;
                StartCoroutine(IgnoreVertBounds());
                _origin = new Vector3(transform.position.x, -7.5f+20*GameBehaviour._onLevel, transform.position.z);
                transform.position = _origin-Vector3.up*2.5f;
                _body.velocity = Vector2.up*_initJumpVelocity;
                //move camera up to next screen somehow????
                //reset _vertCameraBounds
                //reset player origin point
            }
            //animation
             if(!_isGrounded) {
                if(_canWallJump && GameBehaviour.Instance.State != GameBehaviour.GameState.ScreenMoveUp) {
                    _animator.SetTrigger("WallJumpDisplay");
                }
                else if(_body.velocity.y <= 0) {
                    _animator.SetTrigger("JumpAnimDown");
                }
                else {
                    _animator.SetTrigger("JumpAnimUp");
                }
            }
            else if(_isGrounded) {
                if(_runInput != 0 && _body.velocity.x != 0) {
                    _animator.SetTrigger("WalkingAnim");
                }
                else {
                    _animator.SetTrigger("IdleGrounded");
                }
            }
            
        }
        if(_isDeadDisplay){
                _animator.SetTrigger("DeathAnim");
            }
    }
    IEnumerator WallJumpHorizontalDecrease() {
        _wallJumpCoroutine = true;
        for(int i = 4; i > 0; i--) {
            _body.velocity = new Vector2(_body.velocity.x*0.85f, _body.velocity.y);
        }
        yield return new WaitForSeconds(0.5f);
        _finalWallJumpSpeed = 0;
        _isWallJumping = false;
        _wallJumpCoroutine = false;
    }
    //DEATH
    IEnumerator Respawn() {
        int i = Random.Range(0, deathClips.Length);
		_audioSource.PlayOneShot(deathClips[i], 0.9f);
        yield return new WaitForSeconds(0.015f);
        if(_isCrouch) {
            _isCrouch = false;
            transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y*2, 1);
            _hSpeed = 30f;
            _maxRunSpeed *= 2;
        }
        _body.gravityScale = 0;
        _body.velocity = Vector2.zero;
        _touchingDeath = false;
        yield return new WaitForSeconds(0.4f);
        _isDeadDisplay = false;
        transform.position = _origin;
        _body.gravityScale = 3;
        yield return new WaitForSeconds(0.2f);
        _isDead = false;
        _overlapWallKill = false;
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
        _powerCoroutine = true;
        yield return new WaitForSeconds(_powerTime);
        _isPowerDisplay = false;
        yield return new WaitForSeconds(0.5f);
        _isPower = false;
        _powerCoroutine = false;
    }
    IEnumerator CoyoteTime() {
        yield return new WaitForSeconds(0.1f);
        _canJump = false;
    }
    IEnumerator KillDelay() {
        _forceNoJump = true;
        yield return new WaitForSeconds(0.03f);
        if(overlapWall) {
            _overlapWallKill = true;
        }
        else {
            _overlapWallKill = false;
        }
        StartCoroutine(JumpDelay());
    }
    IEnumerator JumpDelay() {
        _forceNoJump = true;
        yield return new WaitForSeconds(0.05f);
        _forceNoJump = false;
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
    void FixedUpdate() {
        if(!_isDead && GameBehaviour.Instance.State != GameBehaviour.GameState.ScreenMoveUp){
            // caps run speed at _maxRunSpeed
            if(Mathf.Abs(_runInput*_body.velocity.x) >= _maxRunSpeed && !_isWallJumping) {
                _body.velocity = new Vector2(Mathf.Sign(_body.velocity.x) * _maxRunSpeed,_body.velocity.y);
            }
            if((_runInput == 0 || (_runInput < 0 && _body.velocity.x > 0) || (_runInput > 0 && _body.velocity.x < 0)) && !_isWallJumping) {
                _body.velocity = new Vector2(_body.velocity.x * _traction,_body.velocity.y);
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
            //jump physics
            if(_doJumpNow) {
                _body.AddForce(transform.up*_jumpVelocity, ForceMode2D.Impulse);
                _doJumpNow = false;
            }
            if(_stopJumpEarly) {
                _body.velocity = new Vector2(_body.velocity.x, _body.velocity.y*0.6f);
                _stopJumpEarly = false;
            }
            //walljump physics
            if(_doWallJumpNow) {
                _body.AddForce(new Vector2(_wallJumpVelocity*wallJumpDirection*0.5f, _wallJumpVelocity), ForceMode2D.Impulse);
                _doWallJumpNow = false;
            }
            //slows down descent while on walls
            if(_canWallJump && _body.velocity.y < 0) {
                _body.velocity = new Vector2(_body.velocity.x, _body.velocity.y*0.85f);
            }
        }
        else{
            _body.velocity = new Vector2(0, _body.velocity.y);
        }
        
    }
}
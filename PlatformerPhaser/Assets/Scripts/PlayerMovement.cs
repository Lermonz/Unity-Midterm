using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] float _hSpeed = 8f;
    float _hSpeedAir = 4.2f;
    [SerializeField] float _jumpVelocity;
    float _initJumpVelocity = 12f;
    [SerializeField] float _wallJumpVelocity;
    float _initWallJumpVelocity = 9.5f;
    float wallJumpDirection;
    float _traction = 0.92f;
    [SerializeField] bool _isGrounded;
    [SerializeField] bool _isJumping;
    [SerializeField] bool _isWallJumping = false;
    [SerializeField] bool _canWallJump;
    bool _touchingDeath = false;
    bool _isSmall = false;
    Coroutine _smallCoroutine;
    float _finalWallJumpSpeed;
    [SerializeField] float _runInput;
    float _maxRunSpeed = 7.5f;
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
                maskSolids = LayerMask.GetMask("Solids"); // only register floors and walls
            }
            else {
                maskSolids = LayerMask.GetMask("Solids","Phasable"); // only register floors and walls
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
            if(Mathf.Abs(transform.position.x) > Mathf.Abs(Utilities.SetXLimit(_camera,_renderer))) {
                float stayAway = (Utilities.SetXLimit(_camera,_renderer)-0.01f)*Mathf.Sign(transform.position.x);
                transform.position = new Vector3(stayAway, transform.position.y, transform.position.z);
            }
            if(!_isGrounded) {
                if(_body.velocity.y <= 0) {
                    _animator.SetTrigger("JumpAnimDown");
                }
                else {
                    _animator.SetTrigger("JumpAnimUp");
                }
            }
            else {
                if(_runInput != 0) {
                    _animator.SetTrigger("WalkingAnim");
                }
                else {
                    _animator.SetTrigger("IdleGrounded");
                }
            }
            if(_hSpeed < 0) {
                _hSpeed = 0;
            }
            if(_jumpVelocity < 0) {
                _jumpVelocity = 0;
            }
            if(_wallJumpVelocity < 0) {
                _wallJumpVelocity = 0;
            }
            if(Input.GetKey(KeyCode.P)) {
                Physics2D.IgnoreLayerCollision(0,3);
                _renderer.color = new Color(0.5f,0.5f,1f,0.85f);
                _isPhaseMode = true;
            }
            else {
                Physics2D.IgnoreLayerCollision(0,3,false);
                _renderer.color = new Color(1f,1f,1f,1f);
                _isPhaseMode = false;
            }
            if(_isSmall) {
                _smallCoroutine = StartCoroutine(BeSmall());
            }
            //crouch
            if(Input.GetKeyDown(KeyCode.S)) {
                transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y*0.5f, 1);
                transform.position += Vector3.down*0.15f;
                _hSpeed = 1f;
                _maxRunSpeed *= 0.5f;
            }
            if(Input.GetKeyUp(KeyCode.S)) {
                transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y*2, 1);
                _hSpeed = 8f;
                _maxRunSpeed *= 2;
            }
            // move horizontally
            if(_runInput*_body.velocity.x < _maxRunSpeed && !_isWallJumping) {
                if(_isGrounded) {
                    _body.AddForce(Vector2.right*_runInput*_hSpeed);
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
                float alpha = (((_finalWallJumpSpeed/Mathf.Abs(_body.velocity.x))-1)*0.75f)+1; // multiplier to decrease movement speed in the opposite direction from where you wall jumped
                _body.velocity = new Vector2(_body.velocity.x*alpha, _body.velocity.y);
                _finalWallJumpSpeed = Mathf.Abs(_body.velocity.x);
            }
            else if(Mathf.Abs(_body.velocity.x) > _finalWallJumpSpeed || _isGrounded) {
                _finalWallJumpSpeed = 0;
            }
            if(transform.position.y < (_vertCameraBounds-_camera.orthographicSize*2) || _touchingDeath) {
                _isDead = true;
                _animator.SetTrigger("DeathAnim");
                Debug.Log("Death anim");
                StartCoroutine(Respawn());
                if(_smallCoroutine != null && _isSmall){
                    StopCoroutine(_smallCoroutine);
                    _isSmall = false;
                    RevertSizeToNormal();
                }
                //kill player
                //reset position to origin point of screen
            }
            if(transform.position.y > (_vertCameraBounds+0.5f) && !_ignoreVCB) {
                _isLevelTransition = true;
                
                StartCoroutine(IgnoreVertBounds());
                _origin = new Vector3(transform.position.x, -7.5f+20*GameBehaviour._onLevel, transform.position.z);
                transform.position = _origin-Vector3.up*1.5f;
                _body.velocity = Vector2.up*9.5f;
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
        _isDead = false;
        transform.position = _origin;
        _body.gravityScale = 2;
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
    IEnumerator BeSmall() {
        _isSmall = false;
        transform.localScale = transform.localScale*0.5f;
        yield return new WaitForSeconds(5f);
        RevertSizeToNormal();
    }
    void OnTriggerEnter2D(Collider2D other) {
        if(other.CompareTag("Kill")) {
            _touchingDeath = true;
        }
        if(other.CompareTag("Item")) {
            _isSmall = true;
            other.gameObject.transform.position = other.gameObject.transform.position - Vector3.up*20;
        }
    }
    void RevertSizeToNormal() {
        if(transform.localScale.x <= 1.0f) {
            transform.localScale = transform.localScale*2;
        }
    }
}

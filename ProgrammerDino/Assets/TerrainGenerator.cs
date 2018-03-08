using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    public GameObject WallPrefab;
    public TextMesh Score;
    public float JumpForce;

    private bool _isAlive;
    private Rigidbody _rigidbody;

    private List<GameObject> _wallsPool;
    private List<GameObject> _currentWalls;

    public float WallSpeed;
    public GameObject StartPos;
    public GameObject EndPos;

    private Vector3 _wallsMoveVector;
    private bool _isJumping;
    private Collider _collider;
    public bool IsGrounded
    {
        get;
        private set;
    }

    // Use this for initialization
    void Start()
    {
        _collider = this.gameObject.GetComponent<Collider>();
        _wallsMoveVector = (EndPos.transform.position - StartPos.transform.position).normalized;
        _wallsPool = new List<GameObject>();
        _currentWalls = new List<GameObject>();
        _rigidbody = this.gameObject.GetComponent<Rigidbody>();
        StartGame();
    }

    void StartGame()
    {
        _isAlive = true;
        StartCoroutine("SpawnWalls");
        this.gameObject.transform.position = Vector3.zero;
    }

    private GameObject GetAvailableWall()
    {
        if (_wallsPool.Count == 0)
            return Instantiate(WallPrefab);
        var wall = _wallsPool[0];
        _wallsPool.RemoveAt(0);
        return wall;
    }

    IEnumerator SpawnWalls()
    {
        while (true)
        {
            var wall = GetAvailableWall();
            wall.transform.position = new Vector3(StartPos.transform.position.x, wall.transform.position.y, wall.transform.position.z);
            wall.SetActive(true);
            _currentWalls.Add(wall);
            yield return new WaitForSeconds(2.0f);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (_isAlive)
        {
            if (Input.GetKey(KeyCode.Space) && IsGrounded && !_isJumping)
            {
                _rigidbody.AddForce(Vector3.up * JumpForce, ForceMode.VelocityChange);
                _isJumping = true;
            }
            List<GameObject> toRemove = new List<GameObject>();
            foreach (var wall in _currentWalls)
            {
                wall.transform.position += _wallsMoveVector * Time.deltaTime * WallSpeed;
                if (wall.transform.position.x < EndPos.transform.position.x)
                {
                    wall.SetActive(false);
                    _wallsPool.Add(wall);
                    toRemove.Add(wall);
                }
            }
            foreach (var wall in toRemove)
                _currentWalls.Remove(wall);
        }
        else if (Input.anyKey)
            StartGame();
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.collider.tag == "Ground")
        {
            _isJumping = false;
            IsGrounded = false;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.tag == "Ground")
            IsGrounded = true;
        if (collision.collider.tag == "Wall")
        {
            StopCoroutine("SpawnWalls");
            _isAlive = false;
        }
    }
}

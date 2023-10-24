using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rowMarkerScript : MonoBehaviour
{
    private Transform tr;
    public GameObject gm;
    GameManager gameM;
    public float moveSpeed = 0.01f;
    

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void Awake()
    {
        tr = GetComponent<Transform>();
        gm = GameObject.Find("SceneCore");
        gameM = gm.GetComponent<GameManager>();
        moveSpeed = gameM.moveSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 target = new Vector3(tr.position.x, tr.position.y, gm.transform.position.z - 20);
        tr.position = Vector3.MoveTowards(tr.position, target, moveSpeed * Time.deltaTime);
        if (tr.position.z < -10)
        {
            gameM.rows--;
            Destroy(this.gameObject);
        }
    }
}

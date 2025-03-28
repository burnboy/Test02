using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

using GameObjectType = Defines.GameObjectType;
using ZombieState = Defines.ZombieState;

public class Box : MonoBehaviour
{
    public GameObjectType GameType = GameObjectType.BOX;

    [SerializeField] private BoxCollider2D myBoxCollider2D = null;
    [SerializeField] private List<Zombie> targetZombielist = new List<Zombie>();


    private void Awake()
    {
        myBoxCollider2D = GetComponent<BoxCollider2D>();
    }

    float timer = 0.0f;
    private void Update()
    {
        if(targetZombielist.Count == 0)
        {
            return; 
        }

        timer += Time.deltaTime;
        if(timer > 3.0f)
        { 
            for(int i = 0; i < targetZombielist.Count; ++i)
            {
                targetZombielist[i].GetComponent<Rigidbody2D>().velocity = new Vector2(targetZombielist[i].GetComponent<Rigidbody2D>().velocity.x, 5.0f);
            }

            timer = 0.0f;
            //Debug.Log("타이머 초기화");
        }
    }





    bool check = false;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        check = collision.TryGetComponent<Zombie>(out Zombie zombie);
        if(check == true)
        {
            targetZombielist.Add(zombie);
            zombie.TargetBox = this;
        }

        //if(targetZombielist.Count < 5)
        //    return;

        //for(int i = 0; i < targetZombielist.Count; ++i)
        //{
        //    //zombie.GetComponent<Rigidbody2D>().velocity = new Vector2(zombie.GetComponent<Rigidbody2D>().velocity.x, 5.0f);

        //    //zombie?.ActivateHeadBoxCollider(true);
        //    //zombie?.ActivateBodyTrigger(true);

        //}

        //if(check == true)
        //{
        //    if(Vector3.Distance(transform.position, zombie.transform.position) < 1.5f)
        //    {
        //        zombie.GetComponent<Rigidbody2D>().velocity = new Vector2(zombie.GetComponent<Rigidbody2D>().velocity.x, 1.0f);
        //    }
        //}
    }

    //배열로 모았다가 위로 보내주자!

    private void OnTriggerStay2D(Collider2D collision)
    {
        check = collision.TryGetComponent<Zombie>(out Zombie zombie);

        if(check == true)
        {
            zombie.TargetBox = this;
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        check = collision.TryGetComponent<Zombie>(out Zombie zombie);
        if(check == true)
        {
            targetZombielist.Remove(zombie);
            zombie.TargetBox = ZombieManager.Instance.GetBoxList(0);
        }

    }
}

using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


/// <summary>
/// 좀비들을 건물에 들러붙고 어느정도 뭉치게 되면
/// 한칸씩 올라가게 된다.
/// 해당 칸 박스에 어느정도 허용치를 넘어가게 되면 좀비들이
/// 올라오도록 설정을 해주는 것이 관건
/// 
/// 
/// 피라미드처럼
/// 1
/// 11
/// 111
/// 1111....
/// 그리고 좀비가 가다가 자기앞에 좀비가 있다면 멈춰야한다.
/// 
/// </summary>
public class ZombieManager : MonoBehaviour
{
    [Header("Zombie")]
    [SerializeField] private GameObject[] zombiePrefabs = null;
    [SerializeField] private List<Zombie> zombieList = new List<Zombie>();
    [SerializeField] private List<Box> boxList = new List<Box>();
    public Box GetBoxList(int cnt)
    {
        return boxList[cnt];
    }

    [SerializeField] private List<List<Zombie>> ZombieRowsList = new List<List<Zombie>>();

    [Space(20)]
    [Header("Transforms")]
    [SerializeField] private Transform boxTransform = null;

    private static ZombieManager instance = null;
    public static ZombieManager Instance { get => instance; }


    //빈방 미리 만들어서 그 위치에다가 좀비 배치!
    public GameObject roomPrefab;
    public List<Transform> roomList = new List<Transform>();
    public Transform TargetRoomTransform;
    //public float spacing = 1.5f;



    private void Awake()
    {
        if(instance == null)
            instance = this;

        zombiePrefabs[0] = Resources.Load("3.Prefabs/Monster/ZombieMelee01").GameObject();
        zombiePrefabs[1] = Resources.Load("3.Prefabs/Monster/ZombieMelee02").GameObject();
        zombiePrefabs[2] = Resources.Load("3.Prefabs/Monster/ZombieMelee03").GameObject();

        GameObject temp = null;
        Box tempBox = null;

        boxTransform = GameObject.Find("Truck/BoxTrans").transform;
        for(int i = 0; i < boxTransform.childCount; ++i)
        {
            temp = boxTransform.GetChild(i).gameObject;
            if(temp.GetComponent<Box>() == true)
            {
                tempBox = temp.GetComponent<Box>();
                boxList.Add(tempBox);
            }

        }
        tempBox = boxList[0];
    }

    private void Start()
    {
        SpawnZombies();
    }


    float timer = 0.0f;

    private void Update()
    {
#if UNITY_EDITOR && true

        if(Input.GetKeyDown(KeyCode.A))
        {
            SpawnZombies();
        }
        if(Input.GetKeyDown(KeyCode.S))
        {
            StopZombieSpawn();
        }

#endif
        timer += Time.deltaTime;
        if(timer > 5.0f)
        {
            ZombieHail();
            timer = 0.0f;
        }
    }

    /// <summary>
    /// 맨아래에 있는 좀비들 뒤로 살짝 빼주기
    /// 주기적으로 ㅇㅇ
    /// 10마리 이상
    /// </summary>

    List<Zombie> underZombieList = new List<Zombie>();
    private void ZombieHail()
    {
        underZombieList.Clear();
        for(int i = 0; i < zombieList.Count; ++i)
        {
            if(-3.9f < zombieList[i].transform.position.y && zombieList[i].transform.position.y < -3.5f
                && Vector3.Distance(zombieList[i].transform.position, boxList[0].transform.position)<7.0f)
            {
                underZombieList.Add(zombieList[i]);
            }
        }

        if(underZombieList.Count > 6)
        {
            if(isActiveHail == false)
            {
                isActiveHail = true;
                hailCor = StartCoroutine(StartHail());
            }
        }

    }
    Coroutine hailCor = null;
    bool isActiveHail = false;
    IEnumerator StartHail()
    {
        for(int i = 0; i < underZombieList.Count; ++i)
        {
            underZombieList[i].MoveRight();
        }
        Debug.Log("헤일발동!");
        yield return new WaitForSeconds(1.5f);
        isActiveHail = false;
    }

    public void StopZombieSpawn()
    {
        if(ZombieSpawnCoroutine != null)
            StopCoroutine(ZombieSpawnCoroutine);
    }

    private void SpawnZombies()
    {
        if(ZombieSpawnCoroutine != null)
            StopCoroutine(ZombieSpawnCoroutine);

        ZombieSpawnCoroutine = StartCoroutine(nameof(ZombieSpawn));
    }

    Coroutine ZombieSpawnCoroutine = null;

    [SerializeField] private int ZombieCount = 0;
    //좀비는 일단 1초에 하나씩 생성하도록 하자!
    Vector3 pos = new Vector3(8.0f, -3.22f, 0.0f);

    Zombie tempZombie = null;
    WaitForSeconds spawnTime = new WaitForSeconds(1.0f);
    int t = 0;
    private IEnumerator ZombieSpawn()
    {
        while(ZombieCount < 200)
        {
            t++;
            if(t > 2)
                t = 0;

            tempZombie = Instantiate(zombiePrefabs[t], pos, Quaternion.identity).GetComponent<Zombie>();
            tempZombie.InitializeZombie(boxList[0]);
            zombieList.Add(tempZombie);
            //totalMonsters++;

            yield return spawnTime;
            tempZombie.GoToBox();
            yield return new WaitForSeconds(0.5f);
        }

        yield return null;
    }


    //float distance = 10000.0f;
    //float checkDistance = 0.0f;
    ////Box targetBox = null;
    //Box tempBox = null;
    //public Box GetNearBox(Zombie zombie)
    //{

    //    foreach(Box targetBox in boxList)
    //    {
    //        checkDistance = Vector3.Distance(zombie.transform.position, targetBox.transform.position);
    //        if(checkDistance < distance)
    //        {
    //            distance = checkDistance;
    //            return tempBox = targetBox;
    //        }
    //    }
    //    return tempBox;
    //}

    public void DeleteZombie(Zombie targetZombie)
    {
        ZombieCount--;
        zombieList.Remove(targetZombie);
    }

    Coroutine GoingUpZombieCoroutine = null;
    public void SetupGoingUpZombie(Zombie lower, Zombie front)
    {
        GoingUpZombieCoroutine = StartCoroutine(GoingUpZombie(lower, front));
    }

    /// <summary>
    /// 맨뒤의 좀비가 자기 앞의 좀비의 머리위를 지나야함 
    /// </summary>
    IEnumerator GoingUpZombie(Zombie lowerZombie, Zombie frontOfZombie)
    {
        lowerZombie.Idle();
        //lowerZombie.GetRigidBody2D().velocity = new Vector2(lowerZombie.GetRigidBody2D().velocity.x,
        //                                                    lowerZombie.GetRigidBody2D().velocity.y + 5.0f);


        lowerZombie.GetRigidBody2D().velocity = new Vector2(lowerZombie.GetRigidBody2D().velocity.x,
                                                            7.0f);

        yield return new WaitForSeconds(1.0f);
        float percent = 1.0f;
        float landingTime = 0.5f;
        while(percent < 1.0f)
        {
            if(frontOfZombie == null)
            {
                StopCoroutine(GoingUpZombieCoroutine);
                yield break;
            }


            percent += Time.deltaTime / landingTime;
            Vector3 pos = Vector3.Lerp(lowerZombie.transform.position, frontOfZombie.transform.position + new Vector3(0.0f, 2.0f, 0.0f), percent);
            lowerZombie.transform.position = pos;
            yield return null;
        }

        lowerZombie.Walk();

        yield return null;
    }


}

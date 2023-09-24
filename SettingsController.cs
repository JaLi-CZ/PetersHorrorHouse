using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System;
using Random = System.Random;

public class Key
{
    public static int keysFound = 0;

    public static List<Key> keys = new List<Key>(), activeKeys = null;
    private static Random random = new Random();

    public GameObject obj;
    public int floor = -1, probability = -1;

    public Key(GameObject obj)
    {
        this.obj = obj;
        try {
            string name = obj.name;
            int dotIdx = name.IndexOf('.');
            if (dotIdx >= 0) name = name.Substring(0, dotIdx);
            string[] data = name.Split('_');
            char floorC = data[1][0];
            if(floorC == 'F') floor = 2;
            else if(floorC == 'G') floor = 1;
            else if(floorC == 'B') floor = 0;
            probability = int.Parse(data[2]);
        } catch (Exception e)
        {
            Debug.Log("ERROR while creating Key object: " + e);
        }
        if(floor < 0 || probability < 0)
        {
            Debug.Log("Something went weong while creating Key object (prob: " + probability + ", floor: " + floor + ") :/");
        }
        keys.Add(this);
    }

    public void SetVisible(bool b)
    {
        if(obj != null) obj.SetActive(b);
    }

    public static Key RemoveFromActive(GameObject keyObj)
    {
        Key keyToRemove = null;
        foreach(Key key in activeKeys)
        {
            if(key.obj == keyObj)
            {
                keyToRemove = key;
                break;
            }
        }
        activeKeys.Remove(keyToRemove);
        return keyToRemove;
    }

    public static List<Key> SelectKeysRandomly()
    {
        List<Key> keys = new List<Key>(Key.keys);
        List<Key> selectedKeys = new List<Key>();
        int b = 0, f = 0, g = 0;
        int keysCount = Settings.current.hiddenKeys;
        int currFloor = random.Next(0, 3);
        for (int i=0; i<keysCount; i++)
        {
            int current = 0;
            int sum = 0;
            foreach (Key key in keys) if (key.floor == currFloor) sum += key.probability;
            int rd = random.Next(0, sum);
            Key selectedKey = null;

            foreach(Key key in keys)
            {
                if (key.floor != currFloor) continue;
                current += key.probability;
                if(rd < current)
                {
                    selectedKey = key;
                    break;
                }
            }
            selectedKeys.Add(selectedKey);
            keys.Remove(selectedKey);

            if (++currFloor == 3) currFloor = 0;

            if (selectedKey.floor == 0) b++;
            else if (selectedKey.floor == 1) g++;
            else if (selectedKey.floor == 2) f++;
        }
        activeKeys = selectedKeys;
        return selectedKeys;
    }

    public static void HideOtherKeys(List<Key> keys)
    {
        foreach (Key key in Key.keys) key.SetVisible(keys.Contains(key));
    }

    public static void HideAllKeys()
    {
        foreach (Key key in Key.keys) key.SetVisible(false);
    }
}

public class SettingsController : MonoBehaviour
{
    public GameObject petrModel;
    public Light playerLight;

    // Start is called before the first frame update
    void Start()
    {
        Key.keys.Clear();
        Key.activeKeys = null;
        foreach(GameObject obj in GameObject.FindObjectsOfType<GameObject>())
            if(obj != null && obj.name.StartsWith("KEY_")) new Key(obj);
        List<Key> keys = Key.SelectKeysRandomly();
        Key.HideOtherKeys(keys);
    }

    // Update is called once per frame
    void Update()
    {
        float speed = Settings.current.speed;
        petrModel.GetComponent<NavMeshAgent>().speed = speed;
        playerLight.GetComponent<Light>().range = Settings.current.visibility;
    }
}

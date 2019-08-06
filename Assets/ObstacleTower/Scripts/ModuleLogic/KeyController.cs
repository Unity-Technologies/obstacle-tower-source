using UnityEngine;

/// <summary>
/// Responsible for managing the keys an agent may have in its possession.
/// </summary>
public class KeyController : MonoBehaviour
{

    public int currentNumberOfKeys;
    public GameObject[] keysArray = new GameObject[5];

    void Start()
    {
        ResetKeys();
    }

    //Increase the current num of keys and update the UI
    public void AddKey()
    {
        if (currentNumberOfKeys < keysArray.Length)
        {
            currentNumberOfKeys++;
            SetKeysUI(currentNumberOfKeys);
        }

    }

    //Decrease the current num of keys and update the UI
    public void UseKey()
    {
        if (currentNumberOfKeys > 0)
        {
            currentNumberOfKeys--;
            SetKeysUI(currentNumberOfKeys);
        }
    }

    //Reset keys to zero
    public void ResetKeys()
    {
        currentNumberOfKeys = 0;
        SetKeysUI(0);
    }

    public void SetKeysUI(int num)
    {
        for (var i = 0; i < keysArray.Length; i++) //4 because we have a max of 5 keys
        {
            //if the key index is less than the current number of keys,
            //enable it, otherwise disable it.
            if (i < num)
            {
                if (!keysArray[i].activeInHierarchy)
                {
                    keysArray[i].SetActive(true);
                }
            }
            else
            {
                keysArray[i].SetActive(false);
            }
        }
    }
}

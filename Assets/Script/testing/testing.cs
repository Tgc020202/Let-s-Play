using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class testing : MonoBehaviour
{
    public Button bt1;
    public Button bt2;
    public Button bt3;
    // public GameObject test;

    // Start is called before the first frame update
    void Start()
    {
        // test.SetActive(false);
        bt1.onClick.AddListener(() =>
        {
            bt1.gameObject.SetActive(false);
        });
        bt2.onClick.AddListener(onShowNumber);
        bt3.onClick.AddListener(() =>
        {
            bt3.gameObject.SetActive(false);
            // test.gameObject.SetActive(true);

        });
    }

    // Update is called once per frame
    void Update()
    {

    }

    void onShowNumber()
    {
        Debug.Log("1");
    }
}

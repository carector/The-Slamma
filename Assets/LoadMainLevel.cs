﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadMainLevel : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(this.gameObject);
        Application.LoadLevel(1);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

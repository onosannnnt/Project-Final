using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoaderCallback : MonoBehaviour
{
    private bool isLoaded = false;

    private void Update()
    {
        if (!isLoaded)
        {
            Loader.LoadCallback();
            isLoaded = true;
        }
    }
}

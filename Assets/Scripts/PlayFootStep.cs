using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayFootStep : MonoBehaviour
{
    public AudioSource sound;

    public void playFootStep(){
        sound.Play();
    }
}

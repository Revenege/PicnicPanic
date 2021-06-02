using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/**
 * <summary>
 * Handles game sound effects
 * </summary>
 * 
 */
public class SoundManager : MonoBehaviour
{
    /// <summary>
    /// Source for all audioclips
    /// </summary>
    public AudioSource sfxSource;
    /// <summary>
    /// Source for game music
    /// </summary>
    public AudioSource musicSource;

    /// <summary>
    /// Allows for calling the class statically
    /// </summary>
    public static SoundManager instance = null;
    /// <summary>
    /// Sound effects that plays on every move
    /// </summary>
    public AudioClip moveSound;
    /// <summary>
    /// Sound effect that plays when the player collided with impassible terrain
    /// </summary>
    public AudioClip bumpSound;
    
    /// <summary>
    /// Create an instance of the sound manage on load
    /// </summary>
    private void Awake()
    {
        // if a sound manager already exists, destroy it. otherwise set the game object
        if (instance == null)
        {  
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Play the sound of the player moving
    /// </summary>
    public void PlayMoveSound()
    {
        sfxSource.clip = moveSound;
        sfxSource.Play();
    }

    /// <summary>
    /// Play the sound of the player colliding with a game object
    /// </summary>
    public void PlayBumpSound()
    {
        sfxSource.clip = bumpSound;
        sfxSource.Play();
    }
}

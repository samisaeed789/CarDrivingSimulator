using UnityEngine;
using System.Collections;

public class MySoundManager : MonoBehaviour {

	

	public static MySoundManager instance = null;



	[Range(0f, 1f)]
	public float musicValue = 1f;
	
	
	[Range(0f, 1f)]
	public float soundValue = 1f;




	[Header("AudioSource")]
	public AudioSource BGM;
	public AudioSource Effectsource;
	





    [Header("AudioClips")]
    public AudioClip ss;
    public AudioClip mm;
    public AudioClip bgm;
    public AudioClip Indi;
    public AudioClip Horn;
    public AudioClip Excellent;
    public AudioClip DiscourageS;
    public AudioClip click;
    public AudioClip complete;
    public AudioClip fail;
    public AudioClip popup;
    public AudioClip pop;
    public AudioClip emoji;
    public AudioClip cash;
    public AudioClip coin;
    public AudioClip CP;
    public AudioClip RampSound;
    public AudioClip parkedSound;
    public AudioClip FireWork;
    public AudioClip Applaud;
    public AudioClip GreatJob;
    public AudioClip Splash;
    public AudioClip PlayEngine;
    public AudioClip Collect;
    public AudioClip CollectCoin;


  

	[Header("Booleans")]
	public bool levelcompleted;
	public bool levelfailed;

	void Awake()
	{
		
		if(instance==null)
			instance=this;

		
		BGM = this.GetComponent<AudioSource>();
		Effectsource = transform.GetChild(0).GetComponent<AudioSource>();
	}

	public void musicValueChanged (float val)
	{

		musicValue = val;
        BGM.volume = musicValue;
	
	}

	public void soundValueChanged (float val)
	{

		soundValue = val;
		Effectsource.volume = soundValue;

	}

	public void SetMainMenuMusic (bool check, float val)
	{
		if (check) {
			BGM.clip = mm;
			musicValue = val;
			BGM.volume = musicValue;
			BGM.Play ();
		} else {
			BGM.Pause ();
		}
	}
	public void PlayLevelCompleteSound(bool check,float val)
	{
		if (check) {
			//BGM.clip = complete;
			//musicValue = val;
			//BGM.volume = musicValue;
			BGM.clip = null;
			BGM.PlayOneShot(complete);
		} else {
			BGM.Pause ();
		}
	}

	public void PlayCompleteSound(bool check)
	{
		if (check)
		{
			BGM.clip = complete;
			BGM.Play();
		}
		else
		{
			BGM.Pause();
		}
	}

	public void PlayLevelFailSound(bool check, float val)
	{
		if (check) {
			musicValue = val;
			BGM.volume = musicValue;
			BGM.PlayOneShot(fail);
		} else {
			BGM.Pause ();
		}
	}

	public void SetSelectionScreenMusic (bool check, float val)
	{
		if (check) {
			    BGM.clip = ss;
				musicValue = val;
				BGM.volume = musicValue;
				BGM.Play ();
			
		} else {
			if (BGM) {
				BGM.Pause ();
			}
		}
	}	
	
	public void SetModeScreenMusic (bool check, float val)
	{
		if (check) {
			    BGM.clip = ss;
				musicValue = val;
				BGM.volume = musicValue;
				BGM.Play ();
			
		} else {
			if (BGM) {
				BGM.Pause ();
			}
		}
	}
	public void SetBGM (bool check)
	{
		if (check) {
			    BGM.clip = bgm;
				BGM.Play ();
			
		}
		else {
			if (BGM) {
				BGM.Pause ();
			}
		}
	}

	public void PlayHorn()
	{
		Effectsource.clip = Horn;
		Effectsource.loop = true;  // Set looping to true while button is held down
		Effectsource.Play();
	}
	
	public void StopHorn()
	{
		if (Effectsource.isPlaying)  // Stop the sound if it's playing
		{
		Effectsource.loop = false;

			Effectsource.Stop();
		}
	}
	
	public void ExcellentSound()
	{
		Effectsource.PlayOneShot(Excellent);

	}
	
	public void DiscourageSound()
	{
		Effectsource.PlayOneShot(DiscourageS);

	}

	public void PlayEngineSound()
	{
		Effectsource.PlayOneShot(PlayEngine);
	} 
	
	public void PlayUIPopSound(float val)
	{
		soundValue = val;
		Effectsource.volume = soundValue;
		Effectsource.PlayOneShot(pop);
	}

	public void PlayCollectSound()
	{
		
		Effectsource.PlayOneShot(Collect);
	}

	public void PlayCollectCoin()
	{
	
		Effectsource.PlayOneShot(CollectCoin);
	}
	 public void PlayEmojiSound(float val)
	{
		soundValue = val;
		Effectsource.volume = soundValue;
		Effectsource.PlayOneShot(emoji);
	}

	public void PlayCashSound(float val)
	{
		soundValue = val;
		Effectsource.volume = soundValue;
		Effectsource.PlayOneShot(cash);
	}

	public void PlaycoinSound()
	{
	
		Effectsource.PlayOneShot (coin);
		Effectsource.loop=true;
	}

	public void StopcoinSound()
	{
		
		Effectsource.Stop();
		Effectsource.loop=false;
	}

	public void PlayButtonClickSound(float val)
	{
		//soundValue = val;
		//Effectsource.volume = soundValue;
		Effectsource.PlayOneShot(click);
	}
	
	public void PlayCPSound(float val)
	{
		soundValue = val;
		Effectsource.volume = soundValue;
		Effectsource.PlayOneShot(CP);
	}



	public void PlayRampSound(float val)
	{
		soundValue = val;
		Effectsource.volume = soundValue;
		Effectsource.PlayOneShot(RampSound);
	}
	
	public void StopRampSound()
	{
		Effectsource.clip = null;
		Effectsource.Stop();
	}

	public void PlayParkedSound(float val)
	{

		soundValue = val;
		Effectsource.volume = soundValue;
		Effectsource.PlayOneShot(parkedSound);
	}


	public void PlayFireworkSound(float val)
	{

		soundValue = val;
		Effectsource.volume = soundValue;
		Effectsource.clip=FireWork;
		Effectsource.loop = true;
		Effectsource.Play();
	}


	public void PlayApplaudSound(bool check,float val)
	{
		if (check)
		{
			BGM.clip = Applaud;
			musicValue = val;
			BGM.volume = musicValue;
			BGM.Play();
		}
		else
		{
			BGM.Pause();
		}
	}


	public void playindiSound(bool check) 
	{
		if (check)
		{
			Effectsource.clip = Indi;
			Effectsource.loop = true;
			Effectsource.Play();
		}
		else
		{
		
			Effectsource.loop = false;
			Effectsource.Stop();
		}
	}
	public void PlayVO(float val)
	{

		soundValue = val;
		Effectsource.volume = soundValue;
		Effectsource.PlayOneShot(GreatJob);

	}
	public void PlaySplash(float val)
	{

		soundValue = val;
		Effectsource.volume = soundValue;
		Effectsource.PlayOneShot(Splash);

	}

    public void OnVolumeChanged(float value)
    {
        // Set the volume based on the slider's value
        BGM.volume = value;
    }

	public void PauseSounds() 
	{
		SetBGM(false);
		playindiSound(false);

	}
	
	public void ResumeSounds() 
	{
		SetBGM(true);

		bool play = GameMngr.instance.IsIndsiactive();
		
		playindiSound(play);
		
	}


}

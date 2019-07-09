using UnityEngine;
using System.Collections;

public class Footsteptouch : MonoBehaviour {

    void ftMale () 
    {
        //Debug.Log("FootOnGround,stepSoundOn!");
        ISoundMan soundMan = EntryPoint.Instance.SoundMan;
        if (soundMan != null && soundMan.CanPlayEffect)
        {
            int footStepId = GetFootStepId();
            string sound = EntryPoint.Instance.WwiseSoundConfigParams.GetMaleFootStepAudio(footStepId);

            if (!string.IsNullOrEmpty(sound))
                soundMan.PlayFootStepAudio(sound, gameObject.transform.position);
        }
    }

    void ftFemale () 
    {
        //Debug.Log("FootOnGround,stepSoundOn!");
        ISoundMan soundMan = EntryPoint.Instance.SoundMan;
        if (soundMan != null && soundMan.CanPlayEffect)
        {
            int footStepId = GetFootStepId();
            string sound = EntryPoint.Instance.WwiseSoundConfigParams.GetFemaleFootStepAudio(footStepId);

            if (!string.IsNullOrEmpty(sound))
                soundMan.PlayFootStepAudio(sound, gameObject.transform.position);
        }
    }

    void ftHoofed () 
    {
        //Debug.Log("FootOnGround,stepSoundOn!");
        ISoundMan soundMan = EntryPoint.Instance.SoundMan;
        if (soundMan != null && soundMan.CanPlayEffect)
        {
            int footStepId = GetFootStepId();
            string sound = EntryPoint.Instance.WwiseSoundConfigParams.GetHoofedFootStepAudio(footStepId);
            if (!string.IsNullOrEmpty(sound))
                soundMan.PlayFootStepAudio(sound, gameObject.transform.position);
        }
    }

    void ftDragon () 
    {
        //Debug.Log("FootOnGround,stepSoundOn!");
        ISoundMan soundMan = EntryPoint.Instance.SoundMan;
        if (soundMan != null && soundMan.CanPlayEffect)
        {
            int footStepId = GetFootStepId();
            string sound = EntryPoint.Instance.WwiseSoundConfigParams.GetDragonFootStepAudio(footStepId);

            if (!string.IsNullOrEmpty(sound))
                soundMan.PlayFootStepAudio(sound, gameObject.transform.position);
        }
    }

    void ftFelidae () 
    {
        //Debug.Log("FootOnGround,stepSoundOn!");
        ISoundMan soundMan = EntryPoint.Instance.SoundMan;
        if (soundMan != null && soundMan.CanPlayEffect)
        {
            int footStepId = GetFootStepId();
            string sound = EntryPoint.Instance.WwiseSoundConfigParams.GetFelidaeFootStepAudio(footStepId);

            if (!string.IsNullOrEmpty(sound))
                soundMan.PlayFootStepAudio(sound, gameObject.transform.position);
        }
    }

    void ftdunk () 
    {
        ISoundMan soundMan = EntryPoint.Instance.SoundMan;
        if (soundMan != null && soundMan.CanPlayEffect)
        {
            int footStepId = GetFootStepId();
            string sound = EntryPoint.Instance.WwiseSoundConfigParams.GetdunkFootStepAudio(footStepId);

            if (!string.IsNullOrEmpty(sound))
                soundMan.PlayFootStepAudio(sound, gameObject.transform.position);
        }
	}
	void ftpig () 
    {
        ISoundMan soundMan = EntryPoint.Instance.SoundMan;
        if (soundMan != null && soundMan.CanPlayEffect)
        {
            int footStepId = GetFootStepId();
            string sound = EntryPoint.Instance.WwiseSoundConfigParams.GetpigFootStepAudio(footStepId);

            if (!string.IsNullOrEmpty(sound))
                soundMan.PlayFootStepAudio(sound, gameObject.transform.position);
        }
	}
	void ftlion () 
	{
        ISoundMan soundMan = EntryPoint.Instance.SoundMan;
        if (soundMan != null && soundMan.CanPlayEffect)
        {
            int footStepId = GetFootStepId();
            string sound = EntryPoint.Instance.WwiseSoundConfigParams.GetlionFootStepAudio(footStepId);

            if (!string.IsNullOrEmpty(sound))
                soundMan.PlayFootStepAudio(sound, gameObject.transform.position);
        }
	}
	void ftwolf () 
	{
        ISoundMan soundMan = EntryPoint.Instance.SoundMan;
        if (soundMan != null && soundMan.CanPlayEffect)
        {
            int footStepId = GetFootStepId();
            string sound = EntryPoint.Instance.WwiseSoundConfigParams.GetwolfFootStepAudio(footStepId);

            if (!string.IsNullOrEmpty(sound))
                soundMan.PlayFootStepAudio(sound, gameObject.transform.position);
        }
	}

    void ftBDragon()
    {
        ISoundMan soundMan = EntryPoint.Instance.SoundMan;
        if (soundMan != null && soundMan.CanPlayEffect)
        {
            int footStepId = GetFootStepId();
            string sound = EntryPoint.Instance.WwiseSoundConfigParams.GetBDragonFootStepAudio(footStepId);

            if (!string.IsNullOrEmpty(sound))
                soundMan.PlayFootStepAudio(sound, gameObject.transform.position);
        }
    }

    void ftCarpet()
    {

    }

    void ftMotobike()
    {

    }

    private int GetFootStepId()
    {
        if (GFXConfig.Instance.IsUseDetailFootStepSound)
        {
            byte footstep_id = 0;
            if (PathFindingManager.Instance.GetFootStepID(gameObject.transform.position, ref footstep_id))
                return (int)footstep_id;
        }
        return 0;
    }
}

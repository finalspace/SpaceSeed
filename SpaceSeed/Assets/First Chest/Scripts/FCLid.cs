﻿// First Chest
// Version: 1.4.0
// Compatilble: Unity 5.6.1 or higher, see more info in Readme.txt file.
//
// Developer:			Gold Experience Team (https://www.assetstore.unity3d.com/en/#!/search/page=1/sortby=popularity/query=publisher:4162)
// Unity Asset Store:	https://www.assetstore.unity3d.com/en/#!/content/18353
//
// Please direct any bugs/comments/suggestions to geteamdev@gmail.com

#region Namespaces

using UnityEngine;
using System.Collections;

#if USE_DOTWEEN     // use DOTween: https://www.assetstore.unity3d.com/en/#!/content/27676 Documentation: http://dotween.demigiant.com/documentation.php			// use DOTween: https://www.assetstore.unity3d.com/en/#!/content/27676 Documentation: http://dotween.demigiant.com/documentation.php
	using DG.Tweening;
#elif USE_HOTWEEN   // use HOTween: https://www.assetstore.unity3d.com/#/content/3311 Documentation:  http://hotween.demigiant.com/documentation.html
	using Holoville.HOTween;
#elif USE_LEANTWEEN // use LeanTween: https://www.assetstore.unity3d.com/#/content/3595 Documentation: http://dentedpixel.com/LeanTweenDocumentation/classes/LeanTween.html
#else // use iTween: https://www.assetstore.unity3d.com/#/content/84 Documentation: http://itween.pixelplacement.com/documentation.php
#endif

#endregion

// ######################################################################
// First Chest Lid Handler.
// This class does open/close the Lid, lock/unlock, create/remove particles and play/stop sounds.
// It works both in Editor and Play mode.
// ######################################################################

[ExecuteInEditMode]
public class FCLid : MonoBehaviour
{
	// ########################################
	// Variables
	// ########################################

	#region Variables

	// Repository of FCMain component
	FCMain m_Main = null;

	// Lid GameObject
	GameObject m_Lid;

	// Search string for Lid GameObject name
	string m_LidString = "Lid";

	// Type of Lid turning status
	public enum eState
	{
		Open,
		Close
	}
	;
	eState m_LidStatus = eState.Close;
	eState m_LidStatusOld = eState.Close;

	// Variable
	bool m_ChangingState = false;
	float m_CurrentOpenAngle = 0.0f;

	// Type of Lock status
	public enum eLockStatus
	{
		Locked,
		Free
	}
	;
	eLockStatus m_LockStatus = eLockStatus.Free;
	eLockStatus m_LockStatusOld = eLockStatus.Free;

	// Euler degrees variables
	[Range(90, 170)]
	public float
			m_MaxOpenAngle = 120.0f;
	float m_EulerWhenClose = 0;

	// Open value (0.0-1.0)
	[Range(0, 1)]
	public float
			m_OpenValue;

	// Open variables
	public FCEaseType.eEaseType m_OpenEaseType = FCEaseType.eEaseType.OutBounce;
	[Range(0, 2)]
	public float
			m_OpenDuration = 0.75f;

	// Close variables
	public FCEaseType.eEaseType m_CloseEaseType = FCEaseType.eEaseType.InOutQuad;
	[Range(0, 2)]
	public float
			m_CloseDuration = 0.5f;

	#endregion // Variables

	// ########################################
	// MonoBehaviour Functions
	// http://docs.unity3d.com/ScriptReference/MonoBehaviour.html
	// ########################################

	#region MonoBehaviour

	// Awake is called when the script instance is being loaded.
	// http://docs.unity3d.com/ScriptReference/MonoBehaviour.Awake.html
	void Awake()
	{
#if USE_DOTWEEN        // use DOTween: https://www.assetstore.unity3d.com/en/#!/content/27676 Documentation: http://dotween.demigiant.com/documentation.php
#elif USE_HOTWEEN  // use HOTween: https://www.assetstore.unity3d.com/#/content/3311 Documentation:  http://hotween.demigiant.com/documentation.html
#elif USE_LEANTWEEN // use LeanTween: https://www.assetstore.unity3d.com/#/content/3595 Documentation: http://dentedpixel.com/LeanTweenDocumentation/classes/LeanTween.html
	// LeanTween.init(3200); // This line is optional. Here you can specify the maximum number of tweens you will use (the default is 400).  This must be called before any use of LeanTween is made for it to be effective.
#else // use iTween: https://www.assetstore.unity3d.com/#/content/84 Documentation: http://itween.pixelplacement.com/documentation.php
#endif
	}

	// Start is called on the frame when a script is enabled just before any of the Update methods is called the first time.
	// http://docs.unity3d.com/ScriptReference/MonoBehaviour.Start.html
	void Start()
	{
#if USE_DOTWEEN        // use DOTween: https://www.assetstore.unity3d.com/en/#!/content/27676 Documentation: http://dotween.demigiant.com/documentation.php
			// DOTWEEN INITIALIZATION
			// Initialize DOTween (needs to be done only once).
			// If you don't initialize DOTween yourself,
			// it will be automatically initialized with default values.
			// DOTween.Init(false, true, LogBehaviour.ErrorsOnly);
#elif USE_HOTWEEN  // use HOTween: https://www.assetstore.unity3d.com/#/content/3311 Documentation:  http://hotween.demigiant.com/documentation.html
			// HOTWEEN INITIALIZATION
			// Must be done only once, before the creation of your first tween
			// (you can skip this if you want, and HOTween will be initialized automatically
			// when you create your first tween - using default values)
			HOTween.Init(true, true, true);
#elif USE_LEANTWEEN // use LeanTween: https://www.assetstore.unity3d.com/#/content/3595 Documentation: http://dentedpixel.com/LeanTweenDocumentation/classes/LeanTween.html
#else // use iTween: https://www.assetstore.unity3d.com/#/content/84 Documentation: http://itween.pixelplacement.com/documentation.php
#endif

		// Get FCMain compoment
		m_Main = this.GetComponent<FCMain>();

		// Find Lid GameObject with m_LidString search string
		FindLid();
	}

	// Update is called every frame, if the MonoBehaviour is enabled.
	// http://docs.unity3d.com/ScriptReference/MonoBehaviour.Update.html
	void Update()
	{
		// Find Lid GameObject with m_LidString search string
		FindLid();

#if UNITY_EDITOR

		// Update in editor mode
		if (m_Lid != null)
		{
			UpdateUnityEditor_ChestOpeningOrClosing();
		}
#endif

		// Update in play mode
		if (Application.isPlaying == true)
		{
			// Play Lock or Unlock sound when lock state is changed
			PlaySoundWhenLockStateChanged();

			// Update when Lid is finished a turn
			if (m_LidStatus != m_LidStatusOld)
			{
				if (m_ChangingState == false)
				{
					Update_LidStateIsChanged();
				}
			}

			// Create Particle for Chest
			if (IsClosed() == true)
			{
				if (m_Main.getChestParticle() != null)
				{
					if (m_Main.getChestParticle().enabled == true && m_Main.getChestParticle().getParticleGameObject() == null)
					{
						if (m_Main.getChestParticle().m_CreateCount == 0 && m_Main.getChestParticle().getPrefab() != null)
						{
							CreateChestParticle();
						}
					}
				}
			}
		}
	}

	#endregion // MonoBehaviour

	// ########################################
	// Update function in Editor mode
	// ########################################

	#region Update function in Editor mode

	// Update in editor mode
	void UpdateUnityEditor_ChestOpeningOrClosing()
	{
#if UNITY_EDITOR

		bool EditorPlayOpenSound = false;
		bool EditorPlayCloseSound = false;

		// Turn Lid how much it open by using m_OpenValue
		if (Application.isPlaying == false)
		{
			// Rotate Lid
			m_CurrentOpenAngle = Mathf.LerpAngle(m_EulerWhenClose, -m_MaxOpenAngle, m_OpenValue);
			m_Lid.transform.localRotation = Quaternion.AngleAxis(m_CurrentOpenAngle, Vector3.right);

			// Update m_LidStatus and mark EditorPlayCloseSound
			if (m_ChangingState == false)
			{
				// Lid is closing
				if (m_CurrentOpenAngle == m_EulerWhenClose)
				{
					// If m_LidStatus is Open state then make it Close
					if (m_LidStatus == eState.Open)
					{
						m_LidStatusOld = eState.Close;
						m_LidStatus = eState.Close;

						EditorPlayCloseSound = true;
					}
				}
				// Lid is opening
				else
				{
					// If m_LidStatus is Close state then make it Open
					if (m_LidStatus == eState.Close)
					{
						m_LidStatusOld = eState.Open;
						m_LidStatus = eState.Open;

						EditorPlayOpenSound = true;
					}
				}

				// Play sound
				if (m_Main.getSound() != null)
				{
					if (m_Main.getSound().enabled == true)
					{
						// Play Close sound
						if (m_Main.getSound().m_SoundClose != null && EditorPlayCloseSound == true && m_LidStatusOld == m_LidStatus)
						{
							GetComponent<AudioSource>().PlayOneShot(m_Main.getSound().m_SoundClose);
						}
						// Play Open sound
						if (m_Main.getSound().m_SoundOpen != null && EditorPlayOpenSound == true && m_LidStatusOld == m_LidStatus)
						{
							GetComponent<AudioSource>().PlayOneShot(m_Main.getSound().m_SoundOpen);
						}
					}
				}

			}
		}
#endif
	}

	#endregion // Update function in Editor mode

	// ########################################
	// Lid State Functions
	// ########################################

	#region Lid State Functions

	// Lid is finished turning 
	void Update_LidStateIsChanged()
	{
		// Turn open
		if (m_LidStatus == eState.Open)
		{
			Update_LidStateIsChanged_Open();
		}
		// Turn close
		else if (m_LidStatus == eState.Close)
		{
			Update_LidStateIsChanged_Close();
		}
	}

	#endregion // Lid State Functions

	// ########################################
	// Open Lid Functions
	// ########################################

	#region Open Lid Functions

	// Open
	public void Open()
	{
		if (IsClosed())
		{
			Open(m_OpenDuration);
		}
	}

	// Open in a period of time
	public void Open(float time)
	{
		// Lid can be opened when it is not between opening or closing
		if (IsClosed())
		{
			m_OpenDuration = time;
			m_LidStatus = eState.Open;
		}
	}

	// Open Lid by m_OpenValue (0.0 to 1.0)
	void OpenLid()
	{
		OpenLidByValue(m_OpenValue);
	}

	// Open Lib by m_OpenValue
	void OpenLidByValue(float OpenValue)
	{
		m_OpenValue = OpenValue;
		m_CurrentOpenAngle = Mathf.LerpAngle(m_EulerWhenClose, -m_MaxOpenAngle, OpenValue);
		m_Lid.transform.localRotation = Quaternion.AngleAxis(m_CurrentOpenAngle, Vector3.right);
	}

	// Lid is Opened
	void Update_LidStateIsChanged_Open()
	{
		// Open failed when Chest is locked
		if (m_LockStatus == eLockStatus.Locked)
		{
			// Set m_LidStatus to close
			m_LidStatus = eState.Close;
			m_LidStatusOld = m_LidStatus;

			// Set m_ChangingState to false
			m_ChangingState = false;

			//Debug.Log(this.name + ": Open failed");

			// Play Bounce animation
			PlayChestOpenFailedBounceAnimation(m_Main.m_BounceForce / 2);

			// Play Open Failed sound
			if (m_Main.getSound() != null)
			{
				if (m_Main.getSound().enabled == true)
				{
					m_Main.getSound().PlaySoundOpenFailed();
				}
			}
		}
		// Open succeed when Chest is unlocked
		else
		{
			m_OpenValue = 0;
			m_LidStatusOld = m_LidStatus;

			// Set m_ChangingState to true
			m_ChangingState = true;

			// Display Open/Close status on Console
			//Debug.Log(this.name + ": Opened");

#if USE_DOTWEEN      // use DOTween: https://www.assetstore.unity3d.com/en/#!/content/27676 Documentation: http://dotween.demigiant.com/documentation.php
#elif USE_HOTWEEN    // use HOTween: https://www.assetstore.unity3d.com/#/content/3311 Documentation:  http://hotween.demigiant.com/documentation.html

		HOTween.To(this, m_OpenDuration, new TweenParms()
			.Prop("m_OpenValue", 1.0f, false)
			.Delay(0)
			.Ease(FCEaseType.EaseTypeConvert(m_OpenEaseType))
			.OnUpdate(OpenLid)
			.OnStepComplete(Update_LidIsOpeningOrClosing_Opening_Finished)
);

#elif USE_LEANTWEEN // use LeanTween: https://www.assetstore.unity3d.com/#/content/3595 Documentation: http://dentedpixel.com/LeanTweenDocumentation/classes/LeanTween.html

		LeanTween.value(this.gameObject, OpenLidByValue, 0.0f, 1.0f, m_OpenDuration)
			.setDelay(0.0f)
			.setEase(FCEaseType.EaseTypeConvert(m_OpenEaseType))
			.setOnComplete(Update_LidIsOpeningOrClosing_Opening_Finished);

#else // use iTween: https://www.assetstore.unity3d.com/#/content/84 Documentation: http://itween.pixelplacement.com/documentation.php

			iTween.ValueTo(this.gameObject, iTween.Hash("from", 0.0f, "to", 1.0f,
"time", m_OpenDuration,
"delay", 0,
"easeType", FCEaseType.EaseTypeConvert(m_OpenEaseType),
"onupdate", "OpenLidByValue",
"onupdatetarget", this.gameObject,
"oncomplete", "Update_LidIsOpeningOrClosing_Opening_Finished"));

#endif

			// Play Bounce animation
			PlayChestOpenBounceAnimation();

			// Create Prop and Particles
			if (m_Main.getProp() != null)
			{
				if (m_Main.getProp().enabled == true)
				{
					if (m_Main.getProp().getPrefab() != null)
					{
						if (m_Main.getProp().isShowing() == false)
						{
							// Create Prop
							if (m_Main.getProp() != null)
							{
								m_Main.getProp().CreateProp();
							}

							// Create Prop particle
							if (m_Main.getPropParticle() != null)
							{
								if (m_Main.getPropParticle().m_CreateWhenChestOpen == true)
									CreatePropParticle();
							}

							// Show Prop
							if (m_Main.getProp() != null)
							{
								m_Main.getProp().Show();
							}
						}
						else
						{
							m_LidStatus = m_LidStatusOld;
						}
					}
				}
			}

			// Stop Chest particle
			if (m_Main.getChestParticle() != null)
			{
				if (m_Main.getChestParticle().m_RemoveWhenChestOpen == true)
				{
					RemoveChestParticle();
				}
			}

			// Play Open sound and Prop sound even there is no Prop
			if (m_Main.getSound() != null)
			{
				if (m_Main.getSound().enabled == true)
				{
					m_Main.getSound().PlaySoundOpen();
					m_Main.getSound().PlaySoundProp();
				}
			}
		}
	}

	// Play Bounce animation while opening
	void PlayChestOpenBounceAnimation()
	{
		if (m_Main.m_Elastic == true)
		{
			float BounceCount = 2;
			float durationStep = m_Main.m_BounceDuration / ((BounceCount * 2 * 2) + 1);
			float duration = durationStep;
			float delay = 0;
			float BounceForce = m_Main.m_BounceForce;

#if USE_DOTWEEN        // use DOTween: https://www.assetstore.unity3d.com/en/#!/content/27676 Documentation: http://dotween.demigiant.com/documentation.php
#elif USE_HOTWEEN  // use HOTween: https://www.assetstore.unity3d.com/#/content/3311 Documentation:  http://hotween.demigiant.com/documentation.html

for (int i = 0; i < BounceCount; i++)
{
			HOTween.To(this.gameObject.transform, duration, new TweenParms()
	.Prop("localScale", new Vector3(m_Main.m_Scale.x + BounceForce, m_Main.m_Scale.y - BounceForce, m_Main.m_Scale.z + BounceForce), false)
	.Ease(EaseType.EaseInOutSine)
	.Delay(delay)
	);
			delay += duration;

			HOTween.To(this.gameObject.transform, duration, new TweenParms()
	.Prop("localScale", new Vector3(m_Main.m_Scale.x - BounceForce, m_Main.m_Scale.y + BounceForce, m_Main.m_Scale.z - BounceForce), false)
	.Ease(EaseType.EaseInOutSine)
	.Delay(delay)
	);
			delay += duration;

			duration += durationStep;
			BounceForce = BounceForce / 3;
}

HOTween.To(this.gameObject.transform, duration, new TweenParms()
	.Prop("localScale", new Vector3(m_Main.m_Scale.x + BounceForce, m_Main.m_Scale.y - BounceForce, m_Main.m_Scale.z + BounceForce), false)
			.Ease(EaseType.EaseInOutSine)
			.Delay(delay)
			);
delay += duration;

HOTween.To(this.gameObject.transform, duration, new TweenParms()
	.Prop("localScale", new Vector3(m_Main.m_Scale.x, m_Main.m_Scale.y, m_Main.m_Scale.z), false)
			.Ease(EaseType.EaseInOutSine)
			.Delay(delay)
			);

#elif USE_LEANTWEEN // use LeanTween: https://www.assetstore.unity3d.com/#/content/3595 Documentation: http://dentedpixel.com/LeanTweenDocumentation/classes/LeanTween.html

for (int i = 0; i < BounceCount;i++ )
{
				LeanTween.scale(this.gameObject, new Vector3(m_Main.m_Scale.x + BounceForce, m_Main.m_Scale.y - BounceForce, m_Main.m_Scale.z + BounceForce), duration)
			.setDelay(delay)
			.setEase(LeanTweenType.easeInOutBounce);
			delay += duration;

				LeanTween.scale(this.gameObject, new Vector3(m_Main.m_Scale.x - BounceForce, m_Main.m_Scale.y + BounceForce, m_Main.m_Scale.z - BounceForce), duration)
			.setDelay(delay)
			.setEase(LeanTweenType.easeInOutBounce);
			delay += duration;

			duration += durationStep;
			BounceForce = BounceForce / 3;
}

			LeanTween.scale(this.gameObject, new Vector3(m_Main.m_Scale.x + BounceForce, m_Main.m_Scale.y - BounceForce, m_Main.m_Scale.z + BounceForce), duration)
.setDelay(delay)
.setEase(LeanTweenType.easeInOutBounce);
delay += duration;

			LeanTween.scale(this.gameObject, new Vector3(m_Main.m_Scale.x, m_Main.m_Scale.y, m_Main.m_Scale.z), duration)
.setDelay(delay)
.setEase(LeanTweenType.easeInOutBounce);

#else // use iTween: https://www.assetstore.unity3d.com/#/content/84 Documentation: http://itween.pixelplacement.com/documentation.php

			for (int i = 0; i < BounceCount; i++)
			{
				iTween.ScaleTo(this.gameObject, iTween.Hash("scale", new Vector3(m_Main.m_Scale.x + BounceForce, m_Main.m_Scale.y - BounceForce, m_Main.m_Scale.z + BounceForce),
"time", duration, "delay", delay,
"easetype", FCEaseType.EaseTypeConvert(FCEaseType.eEaseType.spring)));
				delay += duration;

				iTween.ScaleTo(this.gameObject, iTween.Hash("scale", new Vector3(m_Main.m_Scale.x - BounceForce, m_Main.m_Scale.y + BounceForce, m_Main.m_Scale.z - BounceForce),
"time", duration, "delay", delay,
"easetype", FCEaseType.EaseTypeConvert(FCEaseType.eEaseType.spring)));
				delay += duration;

				duration += durationStep;
				BounceForce = BounceForce / 3;
			}

			iTween.ScaleTo(this.gameObject, iTween.Hash("scale", new Vector3(m_Main.m_Scale.x + BounceForce, m_Main.m_Scale.y - BounceForce, m_Main.m_Scale.z + BounceForce),
"time", duration, "delay", delay,
"easetype", FCEaseType.EaseTypeConvert(FCEaseType.eEaseType.spring)));
			delay += duration;

			iTween.ScaleTo(this.gameObject, iTween.Hash("scale", new Vector3(m_Main.m_Scale.x, m_Main.m_Scale.y, m_Main.m_Scale.z),
"time", duration, "delay", delay,
"easetype", FCEaseType.EaseTypeConvert(FCEaseType.eEaseType.spring)));

#endif
		}
	}

	// Play Bounce animation when open is failed
	void PlayChestOpenFailedBounceAnimation(float BounceForce)
	{
		if (m_Main.m_Elastic == true)
		{

			float BounceCount = 1;
			float durationStep = m_Main.m_BounceDuration / ((BounceCount * 2 * 2) + 1);
			float duration = durationStep;
			float delay = 0;

#if USE_DOTWEEN     // use DOTween: https://www.assetstore.unity3d.com/en/#!/content/27676 Documentation: http://dotween.demigiant.com/documentation.php
#elif USE_HOTWEEN   // use HOTween: https://www.assetstore.unity3d.com/#/content/3311 Documentation:  http://hotween.demigiant.com/documentation.html

		HOTween.To(this.gameObject.transform, duration, new TweenParms()
		.Prop("localScale", new Vector3(m_Main.m_Scale.x + BounceForce, m_Main.m_Scale.y - BounceForce, m_Main.m_Scale.z + BounceForce), false)
.Ease(EaseType.EaseInOutSine)
.Delay(delay)
);
		delay += duration;

		for (int i = 0; i < BounceCount; i++)
		{

HOTween.To(this.gameObject.transform, duration, new TweenParms()
			.Prop("localScale", new Vector3(m_Main.m_Scale.x - BounceForce, m_Main.m_Scale.y + BounceForce, m_Main.m_Scale.z - BounceForce), false)
			.Ease(EaseType.EaseInOutSine)
			.Delay(delay)
			);
delay += duration;

HOTween.To(this.gameObject.transform, duration, new TweenParms()
			.Prop("localScale", new Vector3(m_Main.m_Scale.x + BounceForce, m_Main.m_Scale.y - BounceForce, m_Main.m_Scale.z + BounceForce), false)
			.Ease(EaseType.EaseInOutSine)
			.Delay(delay)
			);
delay += duration;

duration += durationStep;
BounceForce = BounceForce / 4;
		}

		HOTween.To(this.gameObject.transform, duration, new TweenParms()
		.Prop("localScale", new Vector3(m_Main.m_Scale.x, m_Main.m_Scale.y, m_Main.m_Scale.z), false)
.Ease(EaseType.EaseInOutSine)
.Delay(delay)
);
		delay += duration;

#elif USE_LEANTWEEN // use LeanTween: https://www.assetstore.unity3d.com/#/content/3595 Documentation: http://dentedpixel.com/LeanTweenDocumentation/classes/LeanTween.html

			LeanTween.scale(this.gameObject, new Vector3(m_Main.m_Scale.x + BounceForce, m_Main.m_Scale.y - BounceForce, m_Main.m_Scale.z + BounceForce), duration)
		.setDelay(delay)
		.setEase(LeanTweenType.easeInOutBounce);
		delay += duration;

		for (int i = 0; i < BounceCount; i++)
		{

				LeanTween.scale(this.gameObject, new Vector3(m_Main.m_Scale.x - BounceForce, m_Main.m_Scale.y + BounceForce, m_Main.m_Scale.z - BounceForce), duration)
.setDelay(delay)
.setEase(LeanTweenType.easeInOutBounce);
delay += duration;

				LeanTween.scale(this.gameObject, new Vector3(m_Main.m_Scale.x + BounceForce, m_Main.m_Scale.y - BounceForce, m_Main.m_Scale.z + BounceForce), duration)
.setDelay(delay)
.setEase(LeanTweenType.easeInOutBounce);
delay += duration;

duration += durationStep;
BounceForce = BounceForce / 4;
		}

			LeanTween.scale(this.gameObject, new Vector3(m_Main.m_Scale.x, m_Main.m_Scale.y, 1.0f), duration)
		.setDelay(delay)
		.setEase(LeanTweenType.easeInOutBounce);
		delay += duration;


#else // use iTween: https://www.assetstore.unity3d.com/#/content/84 Documentation: http://itween.pixelplacement.com/documentation.php

			iTween.ScaleTo(this.gameObject, iTween.Hash("scale", new Vector3(m_Main.m_Scale.x + BounceForce, m_Main.m_Scale.y - BounceForce, m_Main.m_Scale.z + BounceForce),
"time", duration, "delay", delay + 0.05f,
"easetype", FCEaseType.EaseTypeConvert(FCEaseType.eEaseType.spring)));
			delay += duration + 0.05f;

			for (int i = 0; i < BounceCount; i++)
			{

				iTween.ScaleTo(this.gameObject, iTween.Hash("scale", new Vector3(m_Main.m_Scale.x - BounceForce, m_Main.m_Scale.y + BounceForce, m_Main.m_Scale.z - BounceForce),
"time", duration, "delay", delay,
"easetype", FCEaseType.EaseTypeConvert(FCEaseType.eEaseType.spring)));
				delay += duration;

				iTween.ScaleTo(this.gameObject, iTween.Hash("scale", new Vector3(m_Main.m_Scale.x + BounceForce, m_Main.m_Scale.y - BounceForce, m_Main.m_Scale.z + BounceForce),
"time", duration, "delay", delay,
"easetype", FCEaseType.EaseTypeConvert(FCEaseType.eEaseType.spring)));
				delay += duration;

				duration += durationStep;
				BounceForce = BounceForce / 4;
			}

			iTween.ScaleTo(this.gameObject, iTween.Hash("scale", new Vector3(m_Main.m_Scale.x, m_Main.m_Scale.y, m_Main.m_Scale.z),
"time", duration, "delay", delay,
"easetype", FCEaseType.EaseTypeConvert(FCEaseType.eEaseType.spring)));

#endif

		}
	}

	// Update some variables after Opening is finished
	void Update_LidIsOpeningOrClosing_Opening_Finished()
	{
		m_OpenValue = 1.0f;
		m_ChangingState = false;
	}

	#endregion // Open Lid Functions

	// ########################################
	// Close Lid Functions
	// ########################################

	#region Close Lid Functions

	// Lid is Closed
	void Update_LidStateIsChanged_Close()
	{
		m_OpenValue = 1;
		m_LidStatusOld = m_LidStatus;

		m_ChangingState = true;

		// Display Open/Close status on Console
		//Debug.Log(this.name + ": Closed");

#if USE_DOTWEEN      // use DOTween: https://www.assetstore.unity3d.com/en/#!/content/27676 Documentation: http://dotween.demigiant.com/documentation.php
#elif USE_HOTWEEN    // use HOTween: https://www.assetstore.unity3d.com/#/content/3311 Documentation:  http://hotween.demigiant.com/documentation.html

		HOTween.To(this, m_CloseDuration, new TweenParms()
.Prop("m_OpenValue", 0.0f, false)
.Delay(0)
.Ease(FCEaseType.EaseTypeConvert(m_CloseEaseType))
.OnUpdate(OpenLid)
.OnStepComplete(Update_LidIsOpeningOrClosing_Closing_Finished)
		);

#elif USE_LEANTWEEN // use LeanTween: https://www.assetstore.unity3d.com/#/content/3595 Documentation: http://dentedpixel.com/LeanTweenDocumentation/classes/LeanTween.html

		LeanTween.value(this.gameObject, OpenLidByValue, 1.0f, 0.0f, m_CloseDuration)
.setDelay(0.0f)
.setEase(FCEaseType.EaseTypeConvert(m_CloseEaseType))
.setOnComplete(Update_LidIsOpeningOrClosing_Closing_Finished);

#else // use iTween: https://www.assetstore.unity3d.com/#/content/84 Documentation: http://itween.pixelplacement.com/documentation.php

		iTween.ValueTo(this.gameObject, iTween.Hash("from", 1.0f, "to", 0.0f,
"time", m_CloseDuration,
"delay", 0,
"easeType", FCEaseType.EaseTypeConvert(m_CloseEaseType),
"onupdate", "OpenLidByValue",
"onupdatetarget", this.gameObject,
"oncomplete", "Update_LidIsOpeningOrClosing_Closing_Finished"));

#endif

		if (m_Main.getProp() != null)
		{
			if (m_Main.getProp().enabled == true)
			{
				if (m_Main.getProp().getPrefab() != null)
				{
					// Make Prop to a none-parent object
					m_Main.getProp().getPropGameObject().transform.parent = null;

					if (m_Main.getProp().isShowing() == false)
					{
						// Stop Prop
						if (m_Main.getProp() != null)
						{
							if (m_Main.getProp().m_RemovedWhenChestClose == true)
							{
								m_Main.getProp().Remove();
							}
						}

						// Stop Prop particle
						if (m_Main.getPropParticle() != null)
						{
							if (m_Main.getPropParticle().m_RemovedWhenChestClose == true)
							{
								RemovePropParticle();
							}
						}
					}
					else
					{
						m_LidStatus = m_LidStatusOld;
					}
				}
			}
		}

		// Create Chest particle
		if (m_Main.getChestParticle() != null)
		{
			if (m_Main.getChestParticle().m_CreateWhenChestClose == true)
			{
				CreateChestParticle();
			}
		}

		// Play Bounce animation
		PlayChestCloseBounceAnimation();

		// Play Close sound
		if (m_Main.getSound() != null)
		{
			if (m_Main.getSound().enabled == true)
			{
				// Play Close sound
				m_Main.getSound().PlaySoundClose();

				// Play Lock sound if Chest is locked
				if (m_LockStatus == eLockStatus.Locked)
				{
					m_Main.getSound().PlaySoundSetLock();
				}
			}
		}
	}

	// Close
	public void Close()
	{
		if (IsOpened())
		{
			Close(m_CloseDuration, eLockStatus.Free);
		}
	}

	// Close in a period of time
	public void Close(float time)
	{
		if (IsOpened())
		{
			Close(time, eLockStatus.Free);
		}
	}

	// Close Lid
	public void Close(eLockStatus LockStatus)
	{
		if (IsOpened())
		{
			Close(m_CloseDuration, LockStatus);
		}
	}

	// Close Lid
	public void Close(float time, eLockStatus LockStatus)
	{
		// Lid can be closed when it is not between opening or closing
		if (IsOpened())
		{
			m_CloseDuration = time;
			m_LidStatus = eState.Close;

			if (m_LockStatus != LockStatus)
				SetLock(LockStatus);
		}
	}

	// Play bounce animation for closing
	void PlayChestCloseBounceAnimation()
	{
		if (m_Main.m_Elastic == true)
		{

			float BounceCount = 1;
			float durationStep = m_Main.m_BounceDuration / ((BounceCount * 2 * 2) + 1);
			float duration = durationStep;
			float delay = 0;
			if (m_Lid != null)
				delay = m_CloseDuration;
			float BounceForce = m_Main.m_BounceForce / 2;

#if USE_DOTWEEN        // use DOTween: https://www.assetstore.unity3d.com/en/#!/content/27676 Documentation: http://dotween.demigiant.com/documentation.php
#elif USE_HOTWEEN  // use HOTween: https://www.assetstore.unity3d.com/#/content/3311 Documentation:  http://hotween.demigiant.com/documentation.html

		HOTween.To(this.gameObject.transform, duration, new TweenParms()
		.Prop("localScale", new Vector3(m_Main.m_Scale.x + BounceForce, m_Main.m_Scale.y - BounceForce, m_Main.m_Scale.z + BounceForce), false)
.Ease(EaseType.EaseInOutSine)
.Delay(delay)
);
		delay += duration;

		for (int i = 0; i < BounceCount; i++)
		{

HOTween.To(this.gameObject.transform, duration, new TweenParms()
			.Prop("localScale", new Vector3(m_Main.m_Scale.x - BounceForce, m_Main.m_Scale.y + BounceForce, m_Main.m_Scale.z - BounceForce), false)
			.Ease(EaseType.EaseInOutSine)
			.Delay(delay)
			);
delay += duration;

HOTween.To(this.gameObject.transform, duration, new TweenParms()
			.Prop("localScale", new Vector3(m_Main.m_Scale.x + BounceForce, m_Main.m_Scale.y - BounceForce, m_Main.m_Scale.z + BounceForce), false)
			.Ease(EaseType.EaseInOutSine)
			.Delay(delay)
			);
delay += duration;

duration += durationStep;
BounceForce = BounceForce / 3;
		}

		HOTween.To(this.gameObject.transform, duration, new TweenParms()
		.Prop("localScale", new Vector3(m_Main.m_Scale.x, m_Main.m_Scale.y, m_Main.m_Scale.z), false)
.Ease(EaseType.EaseInOutSine)
.Delay(delay)
);
		delay += duration;

#elif USE_LEANTWEEN // use LeanTween: https://www.assetstore.unity3d.com/#/content/3595 Documentation: http://dentedpixel.com/LeanTweenDocumentation/classes/LeanTween.html

			LeanTween.scale(this.gameObject, new Vector3(m_Main.m_Scale.x + BounceForce, m_Main.m_Scale.y - BounceForce, m_Main.m_Scale.z + BounceForce), duration)
		.setDelay(delay)
		.setEase(LeanTweenType.easeInOutBounce);
		delay += duration;

		for (int i = 0; i < BounceCount; i++)
		{

				LeanTween.scale(this.gameObject, new Vector3(m_Main.m_Scale.x - BounceForce, m_Main.m_Scale.y + BounceForce, m_Main.m_Scale.z - BounceForce), duration)
.setDelay(delay)
.setEase(LeanTweenType.easeInOutBounce);
delay += duration;

				LeanTween.scale(this.gameObject, new Vector3(m_Main.m_Scale.x + BounceForce, m_Main.m_Scale.y - BounceForce, m_Main.m_Scale.z + BounceForce), duration)
.setDelay(delay)
.setEase(LeanTweenType.easeInOutBounce);
delay += duration;

duration += durationStep;
BounceForce = BounceForce / 3;
		}

			LeanTween.scale(this.gameObject, new Vector3(m_Main.m_Scale.x, m_Main.m_Scale.y, m_Main.m_Scale.z), duration)
		.setDelay(delay)
		.setEase(LeanTweenType.easeInOutBounce);
		delay += duration;


#else // use iTween: https://www.assetstore.unity3d.com/#/content/84 Documentation: http://itween.pixelplacement.com/documentation.php

			iTween.ScaleTo(this.gameObject, iTween.Hash("scale", new Vector3(m_Main.m_Scale.x + BounceForce, m_Main.m_Scale.y - BounceForce, m_Main.m_Scale.z + BounceForce),
"time", duration, "delay", delay + 0.05f,
"easetype", FCEaseType.EaseTypeConvert(FCEaseType.eEaseType.spring)));
			delay += duration + 0.05f;

			for (int i = 0; i < BounceCount; i++)
			{

				iTween.ScaleTo(this.gameObject, iTween.Hash("scale", new Vector3(m_Main.m_Scale.x - BounceForce, m_Main.m_Scale.y + BounceForce, m_Main.m_Scale.z - BounceForce),
"time", duration, "delay", delay,
"easetype", FCEaseType.EaseTypeConvert(FCEaseType.eEaseType.spring)));
				delay += duration;

				iTween.ScaleTo(this.gameObject, iTween.Hash("scale", new Vector3(m_Main.m_Scale.x + BounceForce, m_Main.m_Scale.y - BounceForce, m_Main.m_Scale.z + BounceForce),
"time", duration, "delay", delay,
"easetype", FCEaseType.EaseTypeConvert(FCEaseType.eEaseType.spring)));
				delay += duration;

				duration += durationStep;
				BounceForce = BounceForce / 3;
			}

			iTween.ScaleTo(this.gameObject, iTween.Hash("scale", new Vector3(m_Main.m_Scale.x, m_Main.m_Scale.y, m_Main.m_Scale.z),
"time", duration, "delay", delay,
"easetype", FCEaseType.EaseTypeConvert(FCEaseType.eEaseType.spring)));

#endif

		}
	}

	// Update some variables after Closing is finished
	void Update_LidIsOpeningOrClosing_Closing_Finished()
	{
		m_OpenValue = 0.0f;
		m_ChangingState = false;
	}

	#endregion // Close Lid Functions

	// ########################################
	// Lock/Unlock Functions
	// ########################################

	#region Lock/Unlock Functions

	// Lock
	public void Lock()
	{
		if (m_ChangingState == false && m_LidStatus == eState.Close)
			SetLock(eLockStatus.Locked);
	}

	// Unlock
	public void Unlock()
	{
		if (m_ChangingState == false && m_LidStatus == eState.Close)
			SetLock(eLockStatus.Free);
	}

	// Set Lock Status
	public void SetLock(eLockStatus LockStatus)
	{
		if (m_ChangingState == false && m_LidStatus == eState.Close)
		{
			m_LockStatus = LockStatus;
			/*
// Display Lock status on Console
if (m_LockStatus != m_LockStatusOld)
{
if(m_LockStatus==eLockStatus.Free)
Debug.Log(this.name + ": Unlocked");
else
Debug.Log(this.name + ": Locked");			
}*/
		}


		// Play chest open failed bounce animation
		PlayChestOpenFailedBounceAnimation(m_Main.m_BounceForce / 5);
	}

	#endregion // Lock/Unlock Functions

	// ########################################
	// Get Functions
	// ########################################

	#region Get Functions

	// Is Lid opened?
	public bool IsOpened()
	{
		if (m_LidStatus == eState.Open && m_ChangingState == false)
			return true;

		return false;
	}

	// Is Lid closed?
	public bool IsClosed()
	{
		if (m_LidStatus == eState.Close && m_ChangingState == false)
			return true;

		return false;
	}

	// Is Lid between closing or opening?
	public bool IsOpeningOrClosing()
	{
		return m_ChangingState;
	}

	// Return Lid Status
	public eState getLidStatus()
	{
		return m_LidStatus;
	}

	// Return Lock status
	public bool IsLocked()
	{
		if (m_LockStatus == eLockStatus.Locked)
			return true;

		return false;
	}

	#endregion // Get Functions

	// ########################################
	// Create/Remove Particle Functions
	// ########################################

	#region Create/Remove Particle

	// Create Chest Particle
	void CreateChestParticle()
	{
		if (m_Main.getChestParticle() != null)
		{
			if (m_Main.getChestParticle().enabled == true)
			{
				if (m_Main.getChestParticle().getPrefab() != null)
				{
					m_Main.getChestParticle().CreateParticle();
				}
			}
		}
	}

	// Remove Chest Particle
	void RemoveChestParticle()
	{
		if (m_Main.getChestParticle() != null)
		{
			if (m_Main.getChestParticle().getParticleGameObject() != null)
			{
				m_Main.getChestParticle().Remove();
			}
		}
	}

	// Create Prop Particle
	void CreatePropParticle()
	{
		if (m_Main.getPropParticle() != null)
		{
			if (m_Main.getPropParticle().enabled == true)
			{
				if (m_Main.getPropParticle().getPrefab() != null)
				{
					m_Main.getPropParticle().CreateParticle();
				}
			}
		}
	}

	// Remove Chest Particle
	void RemovePropParticle()
	{
		if (m_Main.getPropParticle() != null)
		{
			if (m_Main.getProp().enabled == true)
			{
				if (m_Main.getProp().getPrefab() != null)
				{
					if (m_Main.getPropParticle().getParticleGameObject() != null)
					{
						m_Main.getPropParticle().Remove();
					}
				}
			}
		}
	}

	#endregion // Create/Remove Particle

	// ########################################
	// Play Sound Functions
	// ########################################

	#region Playsound Functions

	// Play Lock/Unlock sound when lock state is changed
	void PlaySoundWhenLockStateChanged()
	{
		if (m_LockStatus != m_LockStatusOld)
		{
			// Plack Lock sound
			if (m_LockStatus == eLockStatus.Locked)
			{
				if (m_Main.getSound() != null)
				{
					if (m_Main.getSound().enabled == true)
					{
						if (m_LidStatus == eState.Close)
						{
							m_Main.getSound().PlaySoundSetLock();
						}
					}
				}
			}
			// Play Unlock sound
			else if (m_LockStatus == eLockStatus.Free)
			{
				if (m_Main.getSound() != null)
				{
					if (m_Main.getSound().enabled == true)
					{
						if (m_LidStatus == eState.Close)
						{
							m_Main.getSound().PlaySoundSetUnlock();
						}
					}
				}
			}

			// Replace old lock status with current status
			m_LockStatusOld = m_LockStatus;
		}
	}

	#endregion // Play Sound Functions

	// ########################################
	// Utilities functions
	// ########################################

	#region Utilities functions

	// Find Lid GameObject with m_LidString search string
	public void FindLid()
	{
		if (m_Lid == null)
		{
			m_Lid = Find(this.transform, m_LidString);
		}
	}

	// Find Lid GameObject
	GameObject Find(Transform tran, string ContainsString)
	{
		string tranName = tran.name;
		if (tranName.Contains(ContainsString) == true)
		{
			return tran.gameObject;
		}
		else
		{
			// continue find Lid in children GameObject
			foreach (Transform child in tran)
			{
				GameObject resultGameObject = Find(child, ContainsString);
				if (resultGameObject != null)
				{
					return resultGameObject;
				}
			}
		}

		return null;
	}

	#endregion // Utilities functions
}

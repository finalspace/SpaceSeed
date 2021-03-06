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

#if USE_DOTWEEN        // use DOTween: https://www.assetstore.unity3d.com/en/#!/content/27676 Documentation: http://dotween.demigiant.com/documentation.php
	using DG.Tweening;
#elif USE_HOTWEEN  // use HOTween: https://www.assetstore.unity3d.com/#/content/3311 Documentation:  http://hotween.demigiant.com/documentation.html
	using Holoville.HOTween;
#elif USE_LEANTWEEN // use LeanTween: https://www.assetstore.unity3d.com/#/content/3595 Documentation: http://dentedpixel.com/LeanTweenDocumentation/classes/LeanTween.html
#else // use iTween: https://www.assetstore.unity3d.com/#/content/84 Documentation: http://itween.pixelplacement.com/documentation.php
#endif

#endregion

// ######################################################################
// First Chest Main handler.
// Scene's event handlers can Access Variables and call Functions in this class to control the Chest.
// It works both in Editor and Play mode.
// ######################################################################

[ExecuteInEditMode]
public class FCMain : MonoBehaviour
{
	// ########################################
	// Variables
	// ########################################

	#region Variables

	// Collider
	public bool m_CreateBoxCollider = true;

	// Chest Scale
	[HideInInspector]
	public Vector3
			m_Scale;

	// Elastic and Bounce
	public bool m_Elastic = true;
	[Range(0.1f, 0.5f)]
	public float
			m_BounceForce = 0.3f;
	[Range(0.5f, 1.5f)]
	public float
			m_BounceDuration = 1.0f;

	// Other First Chest component handlers
	FCLid m_Lid = null;
	FCProp m_Prop = null;
	FCPropParticle m_PropParticle = null;
	FCChestParticle m_ChestParticle = null;
	FCSound m_Sound = null;

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
		m_Scale = gameObject.transform.localScale;
	}

	// Use this for initialization
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

		if (Application.isPlaying == true)
		{
			// Add mesh collider and make it fit to the chest
			if (m_CreateBoxCollider == true)
			{
				AddFitSizeBoxCollider();
				//SimpleConsole.print(this.name + ":AddFitSizeBoxCollider");
			}
		}

		// Get First Chest objects
		m_Lid = getLid();
		m_Prop = getProp();
		m_PropParticle = getPropParticle();
		m_ChestParticle = getChestParticle();
		m_Sound = getSound();

#if UNITY_EDITOR
		// Update in editor mode
		if (Application.isPlaying == false)
		{
			CreateFCLid();
			CreateFCProp();
			CreateFCPropParticle();
			CreateFCChestParticle();
			CreateFCSound();
		}
#endif
	}

	// Update is called every frame, if the MonoBehaviour is enabled.
	// http://docs.unity3d.com/ScriptReference/MonoBehaviour.Update.html
	void Update()
	{
	}

	#endregion // MonoBehaviour

	// ########################################
	// Open/Close Functions
	// ########################################

	#region Open/Close Functions

	// Toggle Open/Close
	public void ToggleOpen()
	{
		if (m_Lid == null)
		{
			m_Lid = getLid();
		}

		if (m_Lid != null)
		{
			if (m_Lid.enabled == true)
			{
				// Open/Close when the Lid is not turning
				if (m_Lid.IsOpeningOrClosing() == false)
				{
					// Close
					if (m_Lid.IsOpened())
					{
						Close();
					}
					// Open
					else
					{
						Open();
					}
				}
			}
		}
	}

	// Toggle Lock/Unlock
	public void ToggleLock()
	{
		if (m_Lid == null)
		{
			m_Lid = getLid();
		}

		if (m_Lid != null)
		{
			if (m_Lid.enabled == true)
			{
				// Lock/Unlock only when Chest is closed
				if (m_Lid.IsOpeningOrClosing() == false)
				{
					// Switch to Lock
					if (m_Lid.IsLocked() == false)
					{
						m_Lid.Lock();
					}
					// Switch to Unlock
					else
					{
						m_Lid.Unlock();
					}
				}
			}
		}
	}

	// Open chest
	public void Open()
	{
		if (m_Lid != null)
		{
			if (m_Lid.enabled == true)
			{
				m_Lid.Open();
			}
		}
	}

	// Close chest
	public void Close()
	{
		if (m_Lid != null)
		{
			if (m_Lid.enabled == true)
			{
				m_Lid.Close();
			}
		}
	}

	// Check chest is locked
	public bool IsLocked()
	{
		if (m_Lid != null)
		{
			if (m_Lid.enabled == true)
			{
				m_Lid.IsLocked();
			}
		}
		return false;
	}

	// Lock chest
	public void Lock()
	{
		if (m_Lid.enabled == true)
		{
			m_Lid.Lock();
		}
	}

	// Unlock chest
	public void Unlock()
	{
		if (m_Lid.enabled == true)
		{
			m_Lid.Unlock();
		}
	}

	#endregion // Open/Close Functions

	// ########################################
	// Create FirstChest components functions
	// ########################################

	#region Create FirstChest components functions


#if UNITY_EDITOR

	// Create FCLid component
	void CreateFCLid()
	{
		// Create FCLid
		if (m_Lid == null)
		{
			m_Lid = this.GetComponent<FCLid>();
		}
		if (m_Lid == null)
		{
			m_Lid = this.gameObject.AddComponent<FCLid>();
		}
		//m_Lid.enabled = true;
		m_Lid.FindLid();
	}

	// Create FCProp component
	void CreateFCProp()
	{
		// Create FCProp
		if (m_Prop == null)
		{
			m_Prop = this.GetComponent<FCProp>();
		}
		if (m_Prop == null)
		{
			m_Prop = this.gameObject.AddComponent<FCProp>();
		}
		//m_Prop.enabled = true;
	}

	// Create FCPropParticle component
	void CreateFCPropParticle()
	{
		// Create FCParticle
		if (m_PropParticle == null)
		{
			m_PropParticle = this.GetComponent<FCPropParticle>();
		}
		if (m_PropParticle == null)
		{
			m_PropParticle = this.gameObject.AddComponent<FCPropParticle>();
		}
		//m_PropParticle.enabled = true;
	}

	// Create FCChestParticle component
	void CreateFCChestParticle()
	{
		// Create FCParticle
		if (m_ChestParticle == null)
		{
			m_ChestParticle = this.GetComponent<FCChestParticle>();
		}
		if (m_ChestParticle == null)
		{
			m_ChestParticle = this.gameObject.AddComponent<FCChestParticle>();
		}
		//m_ChestParticle.enabled = true;
	}

	// Create FCSound component
	void CreateFCSound()
	{
		// Create FCSound
		if (m_Sound == null)
		{
			m_Sound = this.GetComponent<FCSound>();
		}
		if (m_Sound == null)
		{
			m_Sound = this.gameObject.AddComponent<FCSound>();
		}
		m_Sound.CreateAudioSource();
		//m_Sound.enabled = true;
	}
#endif
	#endregion // Create FirstChest components functions

	// ########################################
	// Get Funtions
	// ########################################

	#region Get Funtions

	// Get FCLid object
	public FCLid getLid()
	{
		if (m_Lid != null)
		{
			if (m_Lid.enabled == false)
			{
				return null;
			}
		}
		else
		{
			m_Lid = this.GetComponent<FCLid>();
		}

		return m_Lid;
	}

	// Get FCProp object
	public FCProp getProp()
	{
		if (m_Prop != null)
		{
			if (m_Prop.enabled == false)
				return null;
		}
		else
		{
			m_Prop = this.GetComponent<FCProp>();
		}

		return m_Prop;
	}

	// Get FCParticle object form PropParticle
	public FCPropParticle getPropParticle()
	{
		if (m_PropParticle != null)
		{
			if (m_PropParticle.enabled == false)
				return null;
		}
		else
		{
			m_PropParticle = this.GetComponent<FCPropParticle>();
		}

		return m_PropParticle;
	}

	// Get FCParticle object from ChestParticle
	public FCChestParticle getChestParticle()
	{
		if (m_ChestParticle != null)
		{
			if (m_ChestParticle.enabled == false)
				return null;
		}
		else
		{
			m_ChestParticle = this.GetComponent<FCChestParticle>();
		}

		return m_ChestParticle;
	}

	// Get FCSound object
	public FCSound getSound()
	{
		if (m_Sound != null)
		{
			if (m_Sound.enabled == false)
				return null;
		}
		else
		{
			m_Sound = this.GetComponent<FCSound>();
		}

		return m_Sound;
	}

	// Check if Lid is Opened status
	public bool IsOpened()
	{
		if (m_Lid != null)
		{
			if (m_Lid.enabled == true)
			{
				return m_Lid.IsOpened();
			}
		}
		return false;
	}

	// Check if Lid is Closed status
	public bool IsClosed()
	{
		if (m_Lid != null)
		{
			if (m_Lid.enabled == true)
			{
				return m_Lid.IsClosed();
			}
		}
		return false;
	}

	// Get Lid status
	public FCLid.eState LidStatus()
	{
		if (m_Lid != null)
		{
			if (m_Lid.enabled == true)
			{
				return m_Lid.getLidStatus();
			}
		}
		return FCLid.eState.Close;
	}

	#endregion // Get Funtions

	// ########################################
	// Utilities functions
	// ########################################

	#region Utilities functions

	// Making a bounding Box to this Chest and its children
	void AddFitSizeBoxCollider()
	{
		BoxCollider pBoxCollider = gameObject.AddComponent<BoxCollider>();

		// Fit the BoxCollider
		Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);
		bool hasBounds = false;
		foreach (Renderer render in transform.GetComponentsInChildren<Renderer>())
		{
			if (hasBounds)
			{
				bounds.Encapsulate(render.bounds);
			}
			else
			{
				bounds = render.bounds;
				hasBounds = true;
			}
		}

		// Set center and resize the BoxCollider
		if (hasBounds)
		{
			pBoxCollider.center = bounds.center - transform.position;
			pBoxCollider.size = bounds.size;
		}
		else
		{
			pBoxCollider.center = Vector3.zero;
			pBoxCollider.size = Vector3.zero;
		}
	}

	#endregion // Utilities functions

}

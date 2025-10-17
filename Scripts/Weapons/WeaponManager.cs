using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using NeonDepths.Weapons;

public class WeaponManager : MonoBehaviour
{
    private void Awake()
    {
        Debug.Log("--- WEAPON MANAGER SCRIPT IS AWAKE AND RUNNING ---");
    }

    [Header("General")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject player;
    private Rigidbody playerRb;
    private AudioSource SFX;
    [SerializeField] private AudioClip[] shotSounds;
    private float nextFireTime = 0f;
    private string weaponType = "basic";

    [Header("Charge System")]
    [SerializeField] private float maxCharge = 100f;
    [SerializeField] private float movementReloadSpeed = 1.0f;
    [SerializeField] private float idleReloadSpeed = 5.0f;
    private float currentCharge;

    [Header("Basic")]
    [SerializeField] private GameObject basicBulletPrefab;
    [SerializeField] private float basicFireRate = 1.5f;
    [SerializeField] private float basicChargeCost = 0f;
    [SerializeField] private int bulletDamage = 10;

    [Header("Pistol")]
    [SerializeField] private GameObject pistolBulletPrefab;
    [SerializeField] private int Pistol_Damage = 10;
    [SerializeField] private float Pistol_fireRate = 0.5f;
    [SerializeField] private float pistolChargeCost = 5f;
    [SerializeField] private float pistolAbilityCost = 25f;
    [SerializeField] private float pistolAbilityForce = 10f;
    [SerializeField] private Animator pistolAnimator;
    private bool abilityUsed = false;

    [Header("AR")]
    [SerializeField] private GameObject ARBulletPrefab;
    [SerializeField] private int ARDamage = 5;
    [SerializeField] private float ARFireRate = 0.1f;
    [SerializeField] private float arChargeCost = 2f;
    [SerializeField] private float arAbilityCost = 20f;
    [SerializeField] private GameObject ARAbilityPrefab; // This is the bounce pad
    [SerializeField] private LayerMask bouncePadPlacementLayer;


    [Header("Sniper")]
    [SerializeField] private GameObject sniperBullet;
    [SerializeField] private int sniperDamage = 50;
    [SerializeField] private float sniperFireRate = 1f;
    [SerializeField] private float sniperChargeCost = 20f;
    [SerializeField] private float sniperAbilityCost = 30f;
    [SerializeField] private Animator sniperAnimator;
    [SerializeField] private float dashForce = 20f;
    [SerializeField] private float dashTime = 0.3f;
    [SerializeField] private AnimationCurve dashCurve;
    [SerializeField] private int dashDamage = 25;
    private bool isDashing = false;

    [Header("Shotgun")]
    [SerializeField] private GameObject shotgunBulletPrefab;
    [SerializeField] private int shotgunDamage = 8;
    [SerializeField] private int pelletsPerShot = 10;
    [SerializeField] private float shotgunFireRate = 0.8f;
    [SerializeField] private float shotgunSpread = 0.1f;
    [SerializeField] private float shotgunChargeCost = 15f;

    [Header("Shotgun Rewind Ability")]
    [SerializeField] private float rewindAbilityCost = 50f;
    [SerializeField] private float rewindDelay = 3f;
    [SerializeField] private float rewindTravelTime = 1f;
    [SerializeField] private GameObject rewindMarkerPrefab;
    [SerializeField] private Volume rewindVolume;
    [SerializeField] private float distortionIntensity = -0.5f;
    [SerializeField] private float distortionFadeTime = 0.5f;
    private bool isRewindActive = false;
    private GameObject rewindMarkerInstance;
    private List<RewindState> pathHistory = new List<RewindState>();
    private LensDistortion lensDistortion;

    [Header("Grapple")]
    [SerializeField] private float grappleRange = 50f;
    [SerializeField] private float grappleSpring = 4.5f;
    [SerializeField] private float grappleDamper = 7f;
    [SerializeField] private float grappleMassScale = 4.5f;
    [SerializeField] private float grappleChargeCost = 15f;
    [SerializeField] private float grappleExtendTime = 1f; // Time to extend the rope
    [SerializeField] private float grappleRetractTime = 0.5f; // Time to retract if no hit
    [SerializeField] private LineRenderer grappleRope;
    private SpringJoint joint;
    private Vector3 grapplePoint;
    private bool isGrappling = false;
    private Coroutine grappleCoroutine;

    [Header("Burst Shot (Grapple Primary)")]
    [SerializeField] private GameObject burstBulletPrefab;
    [SerializeField] private int bulletsPerBurst = 3;
    [SerializeField] private float timeBetweenBursts = 0.5f;
    [SerializeField] private float timeBetweenShotsInBurst = 0.1f;
    [SerializeField] private float burstChargeCost = 10f;

    [Header("Rocket Launcher")]
    [SerializeField] private GameObject rocketPrefab;
    [SerializeField] private int rocketDamage = 50;
    [SerializeField] private float rocketFireRate = 1.5f;
    [SerializeField] private float rocketChargeCost = 30f;
    [SerializeField] private float rocketSpeed = 30f;
    
    [Header("Wall Climb Ability")]
    [SerializeField] private float wallClimbRange = 2f;
    [SerializeField] private float wallClimbDrainRate = 10f; // Charge per second
    private bool isWallClimbMode = false;
    private bool isClimbingWall = false;

    private PlayerMovement playerMovement;


    public static event Action ARLost;

    private void Start()
    {
        GroundPistol.pistolPicked += () => SetWeaponType("pistol");
        GroundAR.ARPicked += () => SetWeaponType("AR");
        groundSniper.SniperPicked += () => SetWeaponType("Sniper");
        GroundShotgun.shotgunPicked += () => SetWeaponType("shotgun");
        GroundGrapple.grapplePicked += () => SetWeaponType("grapple");
        GroundRocketLauncher.rocketLauncherPicked += () => SetWeaponType("rocketLauncher");
        LevelReset.LvlReset += ResetWeapons;

        if (SFX == null)
        {
            SFX = GetComponent<AudioSource>();
        }

        if (player != null)
        {
            playerRb = player.GetComponent<Rigidbody>();
            playerMovement = player.GetComponent<PlayerMovement>();
            if (playerMovement == null)
            {
                Debug.LogError("WeaponManager: PlayerMovement component not found on the assigned player object!");
            }
        }
        else
        {
            Debug.LogError("WeaponManager: The 'Player' GameObject has not been assigned in the inspector!");
        }
        
        currentCharge = maxCharge;

        if (rewindVolume != null)
        {
            rewindVolume.profile.TryGet(out lensDistortion);
            if (lensDistortion != null)
            {
                lensDistortion.intensity.value = 0f; 
            }
        }
        
        // Initialize object pools for all projectile types
        InitializeProjectilePools();
        
        SetWeaponType("basic");
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from events to prevent memory leaks
        // Note: Lambda expressions cannot be unsubscribed directly like this,
        // so these lines are commented out for now
        // For a proper implementation, we would need to store references to delegate methods
        /*
        GroundPistol.pistolPicked -= () => SetWeaponType("pistol");
        GroundAR.ARPicked -= () => SetWeaponType("AR");
        groundSniper.SniperPicked -= () => SetWeaponType("Sniper");
        GroundShotgun.shotgunPicked -= () => SetWeaponType("shotgun");
        GroundGrapple.grapplePicked -= () => SetWeaponType("grapple");
        GroundRocketLauncher.rocketLauncherPicked -= () => SetWeaponType("rocketLauncher");
        */
        
        // Unsubscribe from the LevelReset event
        LevelReset.LvlReset -= ResetWeapons;
    }

    /// <summary>
    /// Initializes object pools for all weapon projectiles to reduce instantiation overhead.
    /// </summary>
    private void InitializeProjectilePools()
    {
        if (basicBulletPrefab != null)
            ProjectilePoolManager.Instance.InitializePool("basic", basicBulletPrefab);
        
        if (pistolBulletPrefab != null)
            ProjectilePoolManager.Instance.InitializePool("pistol", pistolBulletPrefab);
        
        if (ARBulletPrefab != null)
            ProjectilePoolManager.Instance.InitializePool("ar", ARBulletPrefab);
        
        if (sniperBullet != null)
            ProjectilePoolManager.Instance.InitializePool("sniper", sniperBullet);
        
        if (shotgunBulletPrefab != null)
            ProjectilePoolManager.Instance.InitializePool("shotgun", shotgunBulletPrefab);
        
        if (burstBulletPrefab != null)
            ProjectilePoolManager.Instance.InitializePool("burst", burstBulletPrefab);
        
        if (rocketPrefab != null)
            ProjectilePoolManager.Instance.InitializePool("rocket", rocketPrefab);
    }

    void Update()
    {
        HandleShooting();
        HandleAbility();

        if (playerRb != null)
        {
            // Don't recharge from movement if in wall climb mode
            if (isClimbingWall)
            {
                // Drain charge while climbing
                currentCharge -= wallClimbDrainRate * Time.deltaTime;
                
                // If out of charge, stop climbing
                if (currentCharge <= 0)
                {
                    currentCharge = 0;
                    StopWallClimb(true);
                }
            }
            else if (!isWallClimbMode)
            {
                float speed = playerRb.linearVelocity.magnitude;
                if (speed > 0.1f) 
                {
                    currentCharge += speed * movementReloadSpeed * Time.deltaTime;
                }
                else 
                {
                    currentCharge += idleReloadSpeed * Time.deltaTime;
                }
            }
            
            currentCharge = Mathf.Clamp(currentCharge, 0, maxCharge);
            UIManager.Instance.UpdateChargeBar(currentCharge, maxCharge);
        }
    }

    private void HandleAbility()
    {
        if (Input.GetButtonDown("Fire2"))
        {
            Debug.Log($"Fire2 pressed! Current weaponType: '{weaponType}'");
            
            // Check for wall climb ability first (works with any weapon)
            HandleWallClimbAbility();
            
            // Then handle weapon-specific abilities
            switch (weaponType)
            {
                case "shotgun":
                    if (!isRewindActive && currentCharge >= rewindAbilityCost)
                    {
                        StartCoroutine(RewindSequence());
                    }
                    break;
                case "pistol":
                     if (!abilityUsed && currentCharge >= pistolAbilityCost)
                    {
                        pistolAbility();
                    }
                    break;
                case "AR":
                    if (!abilityUsed && currentCharge >= arAbilityCost)
                    {
                        abilityUsed = true;
                        FireBouncePad();
                        StartCoroutine(ResetARAbilityUsed());
                    }
                    break;
                case "Sniper":
                     if (!abilityUsed && currentCharge >= sniperAbilityCost)
                    {
                        sniperAbility();
                    }
                    break;
                case "grapple":
                    StartGrapple();
                    break;
            }
        }
        else if (Input.GetButtonUp("Fire2"))
        {
            Debug.Log($"Fire2 released. Weapon: {weaponType}, isClimbingWall: {isClimbingWall}");
            
            // Check for wall climb jump first (works with any weapon)
            if (isClimbingWall)
            {
                Debug.Log("Attempting wall climb jump!");
                StopWallClimb(false);
                playerMovement.WallClimbJump();
            }
            else if (weaponType == "grapple" && joint != null)
            {
                StopGrapple();
            }
        }
    }

    private void HandleShooting()
    {
        switch (weaponType)
        {
            case "basic":
                BasicLogic();
                break;
            case "pistol":
                PistolLogic();
                break;
            case "AR":
                ARLogic();
                break;
            case "Sniper":
                SniperLogic();
                break;
            case "shotgun":
                ShotgunLogic();
                break;
            case "grapple":
                BurstShotLogic();
                break;
            case "rocketLauncher":
                RocketLauncherLogic();
                break;
        }
    }
    
    private void LateUpdate()
    {
        DrawRope();
    }

    void StartGrapple()
    {
        if (isGrappling) return; 
        if (currentCharge < grappleChargeCost) return;

        currentCharge -= grappleChargeCost;
        UIManager.Instance.UpdateChargeBar(currentCharge, maxCharge);

        // Start the grapple animation
        if (grappleCoroutine != null)
        {
            StopCoroutine(grappleCoroutine);
        }
        grappleCoroutine = StartCoroutine(GrappleSequence());
    }

    void StopGrapple()
    {
        if (grappleCoroutine != null)
        {
            StopCoroutine(grappleCoroutine);
            grappleCoroutine = null;
        }
        
        grappleRope.positionCount = 0;
        if (joint != null)
        {
            Destroy(joint);
            joint = null;
        }
        isGrappling = false;
    }

    private IEnumerator GrappleSequence()
    {
        isGrappling = true;
        Vector3 startPoint = firePoint.position;
        Vector3 targetDirection = playerCamera.transform.forward;
        
        // Raycast to see if we hit something
        RaycastHit hit;
        bool didHit = Physics.Raycast(playerCamera.transform.position, targetDirection, out hit, grappleRange);
        
        Vector3 finalTargetPoint = didHit ? hit.point : (playerCamera.transform.position + targetDirection * grappleRange);
        
        grappleRope.positionCount = 2;
        
        // Extend the rope over time
        float extendElapsed = 0f;
        while (extendElapsed < grappleExtendTime)
        {
            extendElapsed += Time.deltaTime;
            float t = extendElapsed / grappleExtendTime;
            
            // Always extend from current firePoint position to target
            Vector3 currentRopeEnd = Vector3.Lerp(startPoint, finalTargetPoint, t);
            grappleRope.SetPosition(0, firePoint.position);
            grappleRope.SetPosition(1, currentRopeEnd);
            
            yield return null;
        }
        
        grapplePoint = finalTargetPoint;
        
        // If we hit something, attach the spring joint
        if (didHit)
        {
            joint = player.AddComponent<SpringJoint>();
            joint.autoConfigureConnectedAnchor = false;
            joint.connectedAnchor = grapplePoint;

            float distanceFromPoint = Vector3.Distance(player.transform.position, grapplePoint);

            joint.maxDistance = distanceFromPoint * 0.8f;
            joint.minDistance = distanceFromPoint * 0.25f;

            joint.spring = grappleSpring;
            joint.damper = grappleDamper;
            joint.massScale = grappleMassScale;
            
            // Keep the rope active while grappling
            while (joint != null && isGrappling)
            {
                grappleRope.SetPosition(0, firePoint.position);
                grappleRope.SetPosition(1, grapplePoint);
                yield return null;
            }
        }
        else
        {
            // No hit, retract the rope
            yield return new WaitForSeconds(0.1f); // Brief pause at max extension
            
            float retractElapsed = 0f;
            while (retractElapsed < grappleRetractTime)
            {
                retractElapsed += Time.deltaTime;
                float t = 1f - (retractElapsed / grappleRetractTime);
                
                // Retract from current firePoint back toward player
                Vector3 currentRopeEnd = firePoint.position + (finalTargetPoint - firePoint.position) * t;
                grappleRope.SetPosition(0, firePoint.position);
                grappleRope.SetPosition(1, currentRopeEnd);
                
                yield return null;
            }
            
            // Clean up
            StopGrapple();
        }
    }

    void DrawRope()
    {
        // Drawing is now handled in the coroutine, but keep this for any manual updates
        if (joint != null && grappleRope.positionCount > 0)
        {
            grappleRope.SetPosition(0, firePoint.position);
            grappleRope.SetPosition(1, grapplePoint);
        }
    }

    private void BurstShotLogic()
    {
        if (Input.GetButtonDown("Fire1") && Time.time >= nextFireTime && currentCharge >= burstChargeCost)
        {
            currentCharge -= burstChargeCost;
            UIManager.Instance.UpdateChargeBar(currentCharge, maxCharge);
            StartCoroutine(FireBurst());
            nextFireTime = Time.time + timeBetweenBursts; 
        }
    }

    private IEnumerator FireBurst()
    {
        for (int i = 0; i < bulletsPerBurst; i++)
        {
            Shoot(burstBulletPrefab, bulletDamage, false);
            yield return new WaitForSeconds(timeBetweenShotsInBurst);
        }
    }

    private void ShotgunLogic()
    {
        if (Input.GetButton("Fire1") && Time.time >= nextFireTime && currentCharge >= shotgunChargeCost)
        {
            currentCharge -= shotgunChargeCost;
            
            for (int i = 0; i < pelletsPerShot; i++)
            {
                Shoot(shotgunBulletPrefab, shotgunDamage, true, shotgunSpread);
            }

            if (SFX != null)
            {
                SFX.clip = shotSounds[2];
                SFX.Play();
            }
            nextFireTime = Time.time + shotgunFireRate;
            UIManager.Instance.UpdateChargeBar(currentCharge, maxCharge);
        }
    }

    private IEnumerator RewindSequence()
    {
        isRewindActive = true;
        currentCharge -= rewindAbilityCost;
        UIManager.Instance.UpdateChargeBar(currentCharge, maxCharge);

        pathHistory.Clear();
        StartCoroutine(RecordPlayerPath());

        if (rewindMarkerPrefab != null)
        {
            rewindMarkerInstance = Instantiate(rewindMarkerPrefab, player.transform.position, Quaternion.identity);
        }
        Debug.Log("Rewind activated. Recording path for " + rewindDelay + " seconds.");

        yield return new WaitForSeconds(rewindDelay);

        Debug.Log("Rewinding player along path.");
        
        StartCoroutine(FadeLensDistortion(true));
        
        if (playerRb != null) playerRb.isKinematic = true;

        if (pathHistory.Count > 1)
        {
            for (int i = pathHistory.Count - 1; i > 0; i--)
            {
                RewindState from = pathHistory[i];
                RewindState to = pathHistory[i - 1];
                float travelDuration = rewindTravelTime / (pathHistory.Count - 1);
                float elapsedTime = 0f;

                while (elapsedTime < travelDuration)
                {
                    float progress = elapsedTime / travelDuration;
                    player.transform.position = Vector3.Lerp(from.position, to.position, progress);
                    playerCamera.transform.rotation = Quaternion.Slerp(from.rotation, to.rotation, progress);
                    elapsedTime += Time.deltaTime;
                    yield return null;
                }
            }
        }
        else if (pathHistory.Count == 1)
        {
            player.transform.position = pathHistory[0].position;
            playerCamera.transform.rotation = pathHistory[0].rotation;
        }

        if (pathHistory.Count > 0)
        {
            RewindState finalState = pathHistory[0];
            player.transform.position = finalState.position;
            playerCamera.transform.rotation = finalState.rotation;
        }

        if (playerRb != null) playerRb.isKinematic = false;
        if (rewindMarkerInstance != null)
        {
            Destroy(rewindMarkerInstance);
        }
        isRewindActive = false;
        pathHistory.Clear();
        Debug.Log("Rewind complete.");
        
        StartCoroutine(FadeLensDistortion(false));
    }

    private IEnumerator RecordPlayerPath()
    {
        float timer = 0f;
        while (timer < rewindDelay)
        {
            pathHistory.Add(new RewindState { position = player.transform.position, rotation = playerCamera.transform.rotation });
            yield return new WaitForSeconds(0.1f);
            timer += 0.1f;
        }
    }

    private IEnumerator FadeLensDistortion(bool fadeIn)
    {
        if (lensDistortion == null)
        {
            if (rewindVolume == null)
            {
                Debug.LogWarning("Rewind Effect Failed: The 'Rewind Volume' has not been assigned in the WeaponManager inspector.");
            }
            yield break;
        }

        float startIntensity = lensDistortion.intensity.value;
        float endIntensity = fadeIn ? distortionIntensity : 0f;
        float elapsedTime = 0f;

        while (elapsedTime < distortionFadeTime)
        {
            elapsedTime += Time.deltaTime;
            float newIntensity = Mathf.Lerp(startIntensity, endIntensity, elapsedTime / distortionFadeTime);
            lensDistortion.intensity.value = newIntensity;
            yield return null;
        }

        lensDistortion.intensity.value = endIntensity;
    }

    private void BasicLogic()
    {
        if (Input.GetButton("Fire1") && Time.time >= nextFireTime && currentCharge >= basicChargeCost)
        {
            currentCharge -= basicChargeCost;
            Shoot(basicBulletPrefab, bulletDamage, false);
            if (SFX != null)
            {
                SFX.clip = shotSounds[0];
                SFX.Play();
            }
            nextFireTime = Time.time + basicFireRate;
            UIManager.Instance.UpdateChargeBar(currentCharge, maxCharge);
        }
    }

    private void PistolLogic()
    {
        if (Input.GetButton("Fire1") && Time.time >= nextFireTime && currentCharge >= pistolChargeCost)
        {
            currentCharge -= pistolChargeCost;
            Shoot(pistolBulletPrefab, Pistol_Damage, false);
            if (SFX != null)
            {
                SFX.clip = shotSounds[0];
                SFX.Play();
            }
            nextFireTime = Time.time + Pistol_fireRate;
            UIManager.Instance.UpdateChargeBar(currentCharge, maxCharge);
        }
    }

    private void ARLogic()
    {
        if (Input.GetButton("Fire1") && Time.time >= nextFireTime && currentCharge >= arChargeCost)
        {
            currentCharge -= arChargeCost;
            Shoot(ARBulletPrefab, ARDamage, true, shotgunSpread); // Note: Using shotgunSpread for AR
            if (SFX != null)
            {
                SFX.clip = shotSounds[1];
                SFX.Play();
            }
            nextFireTime = Time.time + ARFireRate;
            UIManager.Instance.UpdateChargeBar(currentCharge, maxCharge);
        }
    }

    private void SniperLogic()
    {
        if (Input.GetButton("Fire1") && Time.time >= nextFireTime && currentCharge >= sniperChargeCost)
        {
            currentCharge -= sniperChargeCost;
            Shoot(sniperBullet, sniperDamage, false);
            nextFireTime = Time.time + sniperFireRate;
            UIManager.Instance.UpdateChargeBar(currentCharge, maxCharge);
        }
    }

    private void ResetWeapons()
    {
        // Stop any active grapple
        StopGrapple();
        
        currentCharge = maxCharge;
        SetWeaponType("basic");
    }

    public void SetWeaponType(string newType)
    {
        weaponType = newType;
        Debug.Log("Weapon type set to: " + newType);
        UIManager.Instance.UpdateWeaponUI(weaponType);

        float cost = 0;
        switch (newType)
        {
            case "basic":
                cost = basicChargeCost;
                break;
            case "pistol":
                cost = pistolChargeCost;
                break;
            case "AR":
                cost = arChargeCost;
                break;
            case "Sniper":
                cost = sniperChargeCost;
                break;
            case "shotgun":
                cost = shotgunChargeCost;
                break;
            case "grapple":
                cost = burstChargeCost;
                break;
            case "rocketLauncher":
                cost = rocketChargeCost;
                break;
        }
        UIManager.Instance.UpdateNotches(maxCharge, cost);
    }

    private void pistolAbility()
    {
        currentCharge -= pistolAbilityCost;
        if (pistolAnimator != null)
            pistolAnimator.SetTrigger("Break");

        abilityUsed = true;
        Rigidbody rb = player.GetComponent<Rigidbody>();
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(transform.up * pistolAbilityForce, ForceMode.Impulse);
        
        StartCoroutine(ResetAbilityUsed());
        UIManager.Instance.UpdateChargeBar(currentCharge, maxCharge);
    }

    private IEnumerator ResetAbilityUsed()
    {
        yield return new WaitForSeconds(0.2f);
        abilityUsed = false;
    }

    private void sniperAbility()
    {
        currentCharge -= sniperAbilityCost;
        if (sniperAnimator != null)
            sniperAnimator.SetTrigger("SniperDash");

        abilityUsed = true;
        
        Vector3 dashDirection = playerCamera.transform.forward;
        dashDirection.y = 0f;
        dashDirection.Normalize();
        
        if (player.TryGetComponent(out Rigidbody rb))
        {
            rb.useGravity = false;
            StartCoroutine(DashCoroutine(dashDirection));
        }
        
        StartCoroutine(ResetSniperAbilityUsed());
        UIManager.Instance.UpdateChargeBar(currentCharge, maxCharge);
    }

    private IEnumerator ResetSniperAbilityUsed()
    {
        yield return new WaitForSeconds(1.01f);
        abilityUsed = false;
    }

    public bool IsDashing()
    {
        return isDashing;
    }

    public int GetDashDamage()
    {
        return dashDamage;
    }

    private IEnumerator ResetARAbilityUsed()
    {
        yield return new WaitForSeconds(0.1f);
        abilityUsed = false;
    }
    
    private IEnumerator DashCoroutine(Vector3 dashDirection)
    {
        float elapsedTime = 0f;
        
        if (player.TryGetComponent(out Rigidbody rb))
        {
            if (sniperAnimator != null)
                sniperAnimator.SetBool("IsDashing", true);
                
            isDashing = true;
                
            while (elapsedTime < dashTime)
            {
                float dashProgress = elapsedTime / dashTime;
                float dashForceMultiplier = dashCurve.Evaluate(dashProgress);

                Vector3 dash = dashDirection * dashForce * dashForceMultiplier;
                dash.y = 0f;
                rb.linearVelocity = dash;

                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            if (sniperAnimator != null)
                sniperAnimator.SetBool("IsDashing", false);
                
            isDashing = false;
            rb.useGravity = true;
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        HandleDashCollision(hit.gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        HandleDashCollision(collision.gameObject);
    }

    private void HandleDashCollision(GameObject hitObject)
    {
        if (isDashing)
        {
            Enemy enemy = hitObject.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(dashDamage);
                Debug.Log($"Hit enemy with dash! Applying {dashDamage} damage");
                
                if (hitObject.TryGetComponent<Rigidbody>(out Rigidbody enemyRb))
                {
                    Vector3 dashDirection = playerCamera.transform.forward;
                    dashDirection.y = 0f;
                    dashDirection.Normalize();
                    enemyRb.AddForce(dashDirection * dashForce, ForceMode.Impulse);
                }
            }
        }
    }

    /// <summary>
    /// Fires a projectile using object pooling for improved performance.
    /// </summary>
    void Shoot(GameObject prefab, int damage, bool spread, float customSpread = 0f)
    {
        if (prefab == null)
        {
            Debug.LogError("Shoot failed: Projectile prefab is not assigned!");
            return;
        }

        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;
        
        Vector3 direction = ray.direction;
        if (spread)
        {
            direction += new Vector3(
                UnityEngine.Random.Range(-customSpread, customSpread),
                UnityEngine.Random.Range(-customSpread, customSpread),
                UnityEngine.Random.Range(-customSpread, customSpread)
            );
            direction.Normalize();
        }

        Vector3 targetPoint = playerCamera.transform.position + direction * 100; 

        if (Physics.Raycast(playerCamera.transform.position, direction, out hit))
        {
            targetPoint = hit.point;
            Enemy enemy = hit.collider.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }
        }
        
        // Get projectile from pool instead of instantiating
        string poolName = GetPoolNameForPrefab(prefab);
        PistolBullet projectile = ProjectilePoolManager.Instance.GetProjectile(poolName);
        
        if (projectile != null)
        {
            // Position and orient the projectile
            projectile.transform.position = firePoint.position;
            projectile.transform.rotation = Quaternion.LookRotation(targetPoint - firePoint.position);
            
            // Set damage and pool name
            projectile.SetDamage(damage);
            projectile.SetPoolName(poolName);
            
            // Apply velocity
            Rigidbody rb = projectile.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 shootDirection = (targetPoint - firePoint.position).normalized;
                rb.linearVelocity = shootDirection * 50f;
            }
        }
    }

    /// <summary>
    /// Gets the pool name for a given prefab.
    /// </summary>
    private string GetPoolNameForPrefab(GameObject prefab)
    {
        if (prefab == basicBulletPrefab) return "basic";
        if (prefab == pistolBulletPrefab) return "pistol";
        if (prefab == ARBulletPrefab) return "ar";
        if (prefab == sniperBullet) return "sniper";
        if (prefab == shotgunBulletPrefab) return "shotgun";
        if (prefab == burstBulletPrefab) return "burst";
        if (prefab == rocketPrefab) return "rocket";
        
        Debug.LogWarning($"Unknown prefab, defaulting to 'basic' pool");
        return "basic";
    }

    private void FireBouncePad()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, 100f, bouncePadPlacementLayer))
        {
            Vector3 targetPoint = hit.point + hit.normal * 0.1f; 
            Quaternion rotation = Quaternion.LookRotation(hit.normal) * Quaternion.Euler(-90, 0, 0);
            Instantiate(ARAbilityPrefab, targetPoint, rotation);
            currentCharge -= arAbilityCost;
            UIManager.Instance.UpdateChargeBar(currentCharge, maxCharge);
        }
    }

    // ========== ROCKET LAUNCHER METHODS ==========
    
    private void RocketLauncherLogic()
    {
        if (Input.GetButton("Fire1") && Time.time >= nextFireTime && currentCharge >= rocketChargeCost)
        {
            currentCharge -= rocketChargeCost;
            FireRocket();
            nextFireTime = Time.time + rocketFireRate;
            UIManager.Instance.UpdateChargeBar(currentCharge, maxCharge);
        }
    }

    private void FireRocket()
    {
        if (rocketPrefab == null)
        {
            Debug.LogError("Rocket prefab is not assigned!");
            return;
        }

        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        Vector3 direction = ray.direction;
        
        // Get rocket from pool
        PistolBullet rocket = ProjectilePoolManager.Instance.GetProjectile("rocket");
        
        if (rocket != null)
        {
            // Position and orient
            rocket.transform.position = firePoint.position;
            rocket.transform.rotation = Quaternion.LookRotation(direction);
            
            // Set damage and pool name
            rocket.SetDamage(rocketDamage);
            rocket.SetPoolName("rocket");
            
            // Give it velocity
            Rigidbody rb = rocket.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = direction * rocketSpeed;
            }
        }

        if (SFX != null && shotSounds.Length > 0)
        {
            SFX.clip = shotSounds[0];
            SFX.Play();
        }
    }

    // ========== WALL CLIMB METHODS ==========
    
    private void HandleWallClimbAbility()
    {
        if (playerMovement == null)
        {
            Debug.LogError("WeaponManager: Cannot handle wall climb, playerMovement is null!");
            return;
        }

        RaycastHit hit;
        Vector3 raycastOrigin = playerCamera.transform.position + playerCamera.transform.forward * 0.5f;
        
        if (Physics.Raycast(raycastOrigin, playerCamera.transform.forward, out hit, wallClimbRange))
        {
            Debug.Log($"Raycast hit: {hit.collider.name}, Tag: {hit.collider.tag}");
            if (hit.collider.CompareTag("Wall"))
            {
                Debug.Log("Wall detected with correct tag!");
                // Start climbing the wall
                if (!isClimbingWall)
                {
                    Debug.Log("Starting wall climb...");
                    StartWallClimb();
                }
                else
                {
                    Debug.Log("Already climbing");
                }
            }
        }
        else
        {
            Debug.Log("No raycast hit detected");
        }
    }

    private void StartWallClimb()
    {
        if (isClimbingWall)
        {
            Debug.Log("Already climbing, returning");
            return;
        }

        Debug.Log($"StartWallClimb called");
        isClimbingWall = true;
        isWallClimbMode = true;
        playerMovement.ActivateWallClimbMode(true);
        playerMovement.StartWallClimb();
        Debug.Log("Wall climb started successfully");
    }

    private void StopWallClimb(bool slideDown)
    {
        if (!isClimbingWall) return;

        isClimbingWall = false;
        isWallClimbMode = false;
        playerMovement.ActivateWallClimbMode(false);
        playerMovement.StopWallClimb(slideDown);
    }

    private void OnDrawGizmosSelected()
    {
        if (Application.isPlaying && playerCamera != null && weaponType == "rocketLauncher")
        {
            RaycastHit hit;
            Vector3 raycastOrigin = playerCamera.transform.position + playerCamera.transform.forward * 0.5f;
            if (Physics.Raycast(raycastOrigin, playerCamera.transform.forward, out hit, wallClimbRange))
            {
                if (hit.collider.CompareTag("Wall"))
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawLine(raycastOrigin, hit.point);
                }
                else
                {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawLine(raycastOrigin, hit.point);
                }
            }
            else
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(raycastOrigin, raycastOrigin + playerCamera.transform.forward * wallClimbRange);
            }
        }
    }
}
struct RewindState
{
    public Vector3 position;
    public Quaternion rotation;
}
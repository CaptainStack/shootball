﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AssemblyCSharp;
using UnityEngine.UI;

public class Tds_Character : MonoBehaviour
{
    public enum DirectionType{Right, Left, Up, Down};
    public enum WalkDirection{Right, Left, Up, Down, RightUp, RightDown, LeftUp, LeftDown};
    public enum FactionType {FriendlyP1, FriendlyP2, Hostile}

    public string vName = "";
    public FactionType vFactionType = FactionType.Hostile;
    public int HP = 5;
    public int Score = 0;
    public bool IsAlive = true;
    public bool IsPlayer = false;
    public bool IsPlayer2 = false;
    public bool IsCharacter = false;
    public List<WeaponName> ListWeaponsName;                //choose which weapon will be instantied for this player
    public float WalkSpeed = 1f;
    public Animator vLegAnimator;
    public GameObject vBodyObj;
    public Animator vBodyAnimator;
    public GameObject vLeftHandObj;
    public GameObject vRightHandObj;
    public bool IsReady = false;
    private bool LootNearby = false;
    public AudioClip ReloadAudio;

    // Control Mapping
    public string Fire;
    public string ChangeWeaponKey;
    public string UseKey;
    public string MovementX;
    public string MovementY;
    public string CameraX;
    public string CameraY;

    private bool IsWalking = false;
    private bool LastWalkingStatus = false;
    private bool IsChasing = false;                            //check if we are running at the player
    private Vector3 pos;

    //initialize character direction
    private WalkDirection CurWalkDirection = WalkDirection.Down;
    public List<Tds_Weapons> ListWeapons;
    public List<Tds_WeaponMenu> PlayerWeaponMenu;
    private GameObject CurWeaponObj = null;
    private int vCurWeapIndex = 0;                //start with the 1st weapon
    //private int vCurAmmo = 0;
    private List<Tds_Tile> vGroundList;
    private bool CanWalk = true;
    private AudioSource vAudioSource;
    private bool IsReloading = false;
    private bool CanAttack = true;
    private bool CanMelee = false;
    private bool IsAggro = false;
    //private float TimeWaited = 0f;
    private bool SeePlayer = false;
    private bool CursorIsNear = false;            //prevent the player to rotate on itself when the cursor is near the player.
    private int MaxHP = 0;                         //replace this variable with HP so we know how much HP the player has when FULL HP.

    //if player, we show these icon on mouse position
    private GameObject vAimIcon = null;
    private GameObject vCurrentIcon = null;
    private Tds_GameManager vGameManager = null;
    private Tds_Loot vCurLoot = null;
    private float TimeToReload = 0f;
    private GameObject vMainPlayer;

    private bool GameStarted = false;

    private Vector3 LastMouseLocation;
    private bool UsingGamepad = false;
    private Vector3 LastPositiveGamepadInput;

    private Vector3 startPos;

    private float speedBoostDuration = 0f;
    private float speedBoostFactor = 1.75f;

    public GameObject glowingEffectPrefab;
    private ParticleSystem playerGlowingEffect;

    private Vector3 startingLocation;
    
    // Use this for initialization
    void Start()
    {
        startingLocation = this.transform.position;
        playerGlowingEffect = Instantiate(glowingEffectPrefab, this.transform.position, Quaternion.identity).GetComponent<ParticleSystem>();
        playerGlowingEffect.Stop();
        LastMouseLocation = Input.mousePosition;
        vAudioSource = GetComponent<AudioSource>();

        vMainPlayer = GameObject.Find(vName);

        //initialise variables
        IsWalking = false;
        LastWalkingStatus = false;

        if (IsPlayer || IsPlayer2)
        {
            SeePlayer = true;
        }

        //get the MaxHP
        MaxHP = HP;

        startPos = this.transform.position;
    }
    
    // Update is called once per frame
    void Update()
    {
        if (playerGlowingEffect)
        {
            playerGlowingEffect.transform.position = this.transform.position;
        }
        //check if the character is ready
        if (vGameManager != null && IsAlive)
        {
            if (vGameManager.IsReady)
            {
                //get it's weapon ONLY when the game start
                if (!GameStarted)
                {
                    GameStarted = true;
                    ChangeWeapon();
                }

                //check if attacking
                bool IsAttacking = false;
                Vector3 vTargetPosition =  Camera.main.WorldToScreenPoint(vMainPlayer.transform.position);

                //reduce the waiting time for the next bullet!
                if (!CanAttack && ListWeapons [vCurWeapIndex].vTimeWaited > 0f)
                {
                    ListWeapons [vCurWeapIndex].vTimeWaited -= Time.deltaTime;

                    //check if we waited enought
                    if (ListWeapons [vCurWeapIndex].vTimeWaited <= 0f)
                    {
                        CanAttack = true;
                    }
                }

                if (IsPlayer || IsPlayer2)
                {
                    //player get mouse position instead of its' own position
                    if (IsPlayer)
                    {
                        vTargetPosition = Input.mousePosition;
                    }

                    //calculate how far is the cursor from the player
                    Vector3 v3Pos = new Vector3(0f, 0f, 0f);
                    v3Pos = Camera.main.ScreenToWorldPoint(v3Pos);

                    //prevent the player to rotate on itself when the cursor is above him
                    if (Vector3.Distance(new Vector3(v3Pos.x,v3Pos.y, 0f), new Vector3(transform.position.x, transform.position.y, 0f)) <= vGameManager.vCursorRange)
                    {
                        CursorIsNear = true;
                    }
                    else
                    {
                        CursorIsNear = false;
                    }

                    if (Input.GetAxis(MovementY) > 0 || Input.GetAxis(MovementY) < 0 || Input.GetAxis(MovementX) > 0 || Input.GetAxis(MovementX) < 0)
                    {
                        IsWalking = true;
                    }
                    else {
                        IsWalking = false;
                    }

                    //check if we changed walking status
                    if (IsWalking != LastWalkingStatus)
                    {
                        LastWalkingStatus = IsWalking;
                        vLegAnimator.SetBool("IsWalking", IsWalking);
                    }

                    if (vAimIcon != null && vCurrentIcon == null)
                    {
                        vCurrentIcon = Instantiate(vAimIcon);
                    }
                    else if (IsPlayer && vCurrentIcon != null)
                    {
                        Vector3 pz = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                        pz.z = 0;
                        pz.x -= 0.1f;
                        pz.y -= 0.2f;
                        vCurrentIcon.transform.position = pz;
                    }

                    if (Input.GetButton(Fire))
                    {
                        IsAttacking = true;
                    }

                    //check if the user want to change weapon
                    if (Input.GetButtonDown(ChangeWeaponKey))
                    {
                        GetNextWeapon(1);
                    }

                    //reduce it's time
                    if (IsReloading && TimeToReload > 0f)
                    {
                        TimeToReload -= Time.deltaTime;
                        RefreshWeaponUI();
                    }

                    //get the item
                    if (LootNearby && vCurLoot != null)
                    {
                        //check if we already have the weapon and show it
                        bool HasAlreadyWeapon = false;

                        //check if looting give a weapon
                        if (vCurLoot.vItems.GiveWeapon)
                        {
                            //give the right weapon
                            foreach (Tds_Weapons vCurWeapon in vGameManager.vWeaponList)
                            {
                                if (vCurLoot.vItems.vWeaponName.ToString() == vCurWeapon.vWeaponName.ToString())
                                {
                                    foreach (Tds_Weapons vWeapon in ListWeapons)
                                    {
                                        if (vWeapon.vWeaponName == vCurWeapon.vWeaponName)
                                        {
                                            HasAlreadyWeapon = true;
                                        }
                                    }

                                    if (!HasAlreadyWeapon)
                                    {
                                        //get a copy of this weapon
                                        Tds_Weapons vNewWeapon = CopyWeapon(vCurWeapon);

                                        //new weapon is already recharge
                                        vNewWeapon.vAmmoCur = vNewWeapon.vAmmoSize;

                                        //found the weapon, add it for the player
                                        ListWeapons.Add(vNewWeapon);
                                    }
                                }
                            }
                        }

                        if (vGameManager.vItemLootedAnim != null)
                        {
                            //show the itemlooted going up
                            GameObject vNewObj = vGameManager.vItemLootedAnim;

                            //enable it for the very first use
                            vNewObj.SetActive(true);

                            Text vLabel = vNewObj.transform.Find("Label").GetComponent<Text>();

                            //show different message
                            if (HasAlreadyWeapon)
                            {
                                vLabel.text = "Already have :";
                                vLabel.color = Color.red;
                            }
                            else
                            {
                                vLabel.color = Color.white;
                                vLabel.text = "Received :"; //change info on it
                            }

                            //change info on it
                            Text vText = vNewObj.transform.Find("ItemName").GetComponent<Text>();

                            //show the right item name
                            vText.text = vCurLoot.vItems.vName;

                            //default color
                            vText.color = Color.green;
                            if (vCurLoot.vItems.vDmgType == WeaponValueType.Average)
                            {
                                vText.color = Color.yellow;
                            }
                            else if (vCurLoot.vItems.vDmgType == WeaponValueType.High)
                            {
                                vText.color = Color.red;
                            }
                            else if (vCurLoot.vItems.vDmgType == WeaponValueType.GODLY)
                            {
                                vText.color = new Color(255f, 0f, 195f, 0f);
                            }

                            //reshow the whole animation
                            vNewObj.GetComponent<Animator>().SetTrigger("Show");
                        }

                        //destroy loot
                        if (!HasAlreadyWeapon)
                        {
                            GameObject.Destroy(vCurLoot.gameObject);
                            RefreshWeaponUI();
                        }

                        //clear loot
                        vCurLoot = null;
                    }
                }
                else
                {
                    float vDistance = Vector2.Distance(vMainPlayer.transform.position, transform.position);

                    ///////////////AI////////////////
                    if ((vDistance <= 20f || IsAggro) && vDistance >= vGameManager.vMeleeRange)
                    {
                        IsWalking = true;
                        IsChasing = true;
                        SeePlayer = true;
                        IsAggro = true;
                    }
                    else
                    {

                        //if (vDistance >= vGameManager.vMeleeRange && IsChasing)
                        IsWalking = false;

                        //check if can Melee Attack
                        if (IsChasing && vDistance <= vGameManager.vMeleeRange)
                        {
                            CanMelee = true;
                            IsAttacking = true;
                        }
                        else
                        {
                            CanMelee = false;
                        }
                    }
                    /////////////////////////////////
                }

                //check if we shoot
                if (IsAttacking && CanAttack)
                {
                    //Melee (don't use any ammo)
                    if (ListWeapons [vCurWeapIndex].vWeaponType == WeaponType.Melee && CanMelee)
                    {
                        //get the amount of time to wait until we can shoot again
                        ListWeapons [vCurWeapIndex].vTimeWaited = ListWeapons [vCurWeapIndex].vTimeBtwShot;

                        //prevent from shooting too many time and wait for the animation to be done
                        CanAttack = false;

                        //animate the hand
                        if (ListWeapons [vCurWeapIndex].AttackAnimationUsed != "")
                        {
                            vBodyAnimator.SetTrigger(ListWeapons [vCurWeapIndex].AttackAnimationUsed);
                        }
                    }
                    else
                    {
                        //RANGED
                        //check if has enought ammo
                        if (ListWeapons [vCurWeapIndex].vAmmoCur > 0)
                        {
                            //get the amount of time to wait until we can shoot again
                            ListWeapons [vCurWeapIndex].vTimeWaited = ListWeapons [vCurWeapIndex].vTimeBtwShot;

                            //prevent from shooting too many time and wait for the animation to be done
                            CanAttack = false;

                            //reduce the ammo by 1
                            ListWeapons [vCurWeapIndex].vAmmoCur--;

                            //animate the hand
                            if (ListWeapons [vCurWeapIndex].AttackAnimationUsed != "")
                            {
                                vBodyAnimator.SetTrigger(ListWeapons [vCurWeapIndex].AttackAnimationUsed);
                            }

                            //create the shot FX
                            GameObject vShotFX = Instantiate(ListWeapons [vCurWeapIndex].vShotFX);
                            vShotFX.transform.position = CurWeaponObj.transform.Find("BulletPos").position;

                            //create the projectile on the aim obj IF EXIST
                            if (ListWeapons [vCurWeapIndex].vProjectile != null)
                            {
                                //create as many shot with the specific angle
                                foreach (float vAngle in ListWeapons [vCurWeapIndex].vBulletAngleList)
                                {
                                    //create the projectile
                                    GameObject vNewProj = Instantiate(ListWeapons [vCurWeapIndex].vProjectile);
                                    vNewProj.transform.position = CurWeaponObj.transform.Find("BulletPos").position;

                                    //calculate the new angle for every shot
                                    Quaternion vtemp = CurWeaponObj.transform.rotation;
                                    vtemp.z += vAngle;
                                    vNewProj.transform.rotation = vtemp;

                                    //send to the projectile everything it need to kill
                                    Tds_Projectile vProj = vNewProj.GetComponent<Tds_Projectile>();
                                    vGameManager.vProjectileList.Add(vProj);
                                    vProj.vProjFactionType = vFactionType;
                                    vProj.Speed = ListWeapons [vCurWeapIndex].vProjectileSpeed;
                                    vProj.vDmg = ListWeapons [vCurWeapIndex].vDmg;
                                    vProj.vGameManager = vGameManager;
                                    vProj.vRebounce = ListWeapons [vCurWeapIndex].Rebounce;
                                    vProj.vImpactFX = ListWeapons [vCurWeapIndex].vImpactFX;
                                    vProj.IsReady = true;
                                }
                            }
                        }
                        else
                        {
                            if (IsReloading == false)
                            {
                                if (vCurWeapIndex == 0)
                                {
                                    IsReloading = true;
                                    //time until we have fully reloaded
                                    TimeToReload = 1f;

                                    //recharging animation // then we wait until the animation is complete and tell the character we have ammo!
                                    if (vBodyAnimator != null)
                                    {
                                        vBodyAnimator.SetTrigger("Reload");
                                    }
                                }
                                else
                                {
                                    Tds_Weapons pistol = ListWeapons[0];
                                    ListWeapons = new List<Tds_Weapons>();
                                    ListWeapons.Add(pistol);
                                    vCurWeapIndex = 0;
                                    ChangeWeapon();
                                }
                            }
                        }

                        //refresh all the weapon on top
                        if (IsPlayer || IsPlayer2)
                        {
                            RefreshWeaponUI();
                        }
                    }
                }
    
                //rotate weapon if have one
                Vector3 vBodyPosition = vLeftHandObj.transform.position;

                //calcualte the angle
                Vector3 pos = Camera.main.WorldToScreenPoint(vBodyPosition);
                Vector3 joyInput = new Vector3(Input.GetAxis(CameraX), Input.GetAxis(CameraY), 0f);

                Vector3 dir = new Vector3();
                bool rotationChanged = false;

                if (joyInput.magnitude != 0f)
                {
                    dir = joyInput;
                    UsingGamepad = true;
                    rotationChanged = true;
                    LastPositiveGamepadInput = joyInput;
                }
                else if (IsPlayer && (LastMouseLocation != Input.mousePosition))
                {
                    dir = vTargetPosition - pos;
                    LastMouseLocation = Input.mousePosition;
                    rotationChanged = true;
                    UsingGamepad = false;
                }
                else
                {
                    rotationChanged = true;
                    if (UsingGamepad)
                    {
                        dir = LastPositiveGamepadInput;
                    }
                    else
                    {
                        if (IsPlayer)
                        {
                            dir = vTargetPosition - pos;
                        }
                    }
                }
                
                //check if walking
                if (IsWalking)
                {
                    //initialise variable
                    bool vMoveUP = false;
                    bool vMoveRight = false;
                    bool vMoveLeft = false;
                    bool vMoveDown = false;

                    if (IsPlayer || IsPlayer2)
                    {
                        if (Input.GetAxis(MovementY) > 0 && !Input.GetButtonUp(MovementY))
                        {
                            vMoveUP = true;
                        }
                        if (Input.GetAxis(MovementY) < 0 && !Input.GetButtonUp(MovementY))
                        {
                            vMoveDown = true;
                        }
                        if (Input.GetAxis(MovementX) > 0 && !Input.GetButtonUp(MovementX))
                        {
                            vMoveRight = true;
                        }
                        if (Input.GetAxis(MovementX) < 0 && !Input.GetButtonUp(MovementX))
                        {
                            vMoveLeft = true;
                        }
                    }
                    else
                    {
                        //shorten variables
                        float vX = transform.position.x;
                        float vY = transform.position.y;

                        //NPC
                        if (vX <= vMainPlayer.transform.position.x /*&& vXValue*/)
                        {
                            vMoveRight = true;
                        }
                        else
                        {
                            vMoveLeft = true;
                        }

                        if (vY <= vMainPlayer.transform.position.y /*&& vYValue*/)
                        {
                            vMoveUP = true;
                        }
                        else
                        {// if (vYValue)
                            vMoveDown = true;
                        }
                    }

                    //check which position we are rotating
                    if (vMoveUP && vMoveRight)
                    {
                        CurWalkDirection = WalkDirection.RightUp;
                    }
                    else if (vMoveUP && vMoveLeft)
                    {
                        CurWalkDirection = WalkDirection.LeftUp;
                    }
                    else if (vMoveDown && vMoveLeft)
                    {
                        CurWalkDirection = WalkDirection.LeftDown;
                    }
                    else if (vMoveDown && vMoveRight)
                    {
                        CurWalkDirection = WalkDirection.RightDown;
                    }
                    else if (vMoveUP)
                    {
                        CurWalkDirection = WalkDirection.Up;
                    }
                    else if (vMoveLeft)
                    {
                        CurWalkDirection = WalkDirection.Left;
                    }
                    else if (vMoveDown)
                    {
                        CurWalkDirection = WalkDirection.Down;
                    }
                    else if (vMoveRight)
                    {
                        CurWalkDirection = WalkDirection.Right;
                    }

                    //calculate the new rotation
                    Vector3 temp = transform.rotation.eulerAngles;
                    temp.x = 0f;
                    temp.y = 0f;
                    temp.z = GetWalkingRotation();
                    transform.rotation = Quaternion.Euler(temp);

                    //play leg animation backward
                    float vBackForward = 1;
                    if (vMoveDown)
                    {
                        vBackForward = -1;
                    }

                    //show the animation on the right direction
                    vLegAnimator.SetFloat("Direction", vBackForward);

                    //check where it's heading

                    float moveDistance = 1.0f;
                    if (speedBoostDuration > 0f)
                    {
                        moveDistance *= speedBoostFactor;
                        speedBoostDuration -= Time.deltaTime;
                    }
                    else
                    {
                        playerGlowingEffect.Stop();
                    }

                    Vector2 vDestination = new Vector2(0f, moveDistance) * WalkSpeed * Time.deltaTime;
                    //move the character in this direction
                    if (CanWalk)
                    {
                        transform.Translate(vDestination);
                    }
                }
            
                Quaternion newRotation = new Quaternion();
                if (rotationChanged && dir != Vector3.zero)
                {
                    newRotation = Quaternion.LookRotation(dir, Vector3.back);
                    newRotation.x = 0f;
                    newRotation.y = 0f;
                }

                //rotate the body correctly
                //can only look at the player if he can see it
                if (rotationChanged && CanRotateBody())
                {
                    vBodyObj.transform.rotation = Quaternion.Slerp(vBodyObj.transform.rotation, newRotation, 1f);
                }
            }
        }
    }

    void GetNextWeapon(int vNext)
    {
        //check if we can change weapon before
        int vNewIndex = vCurWeapIndex + vNext;
        if (vNewIndex < 0)
        {
            vNewIndex = ListWeapons.Count - 1;
        }
        else if (vCurWeapIndex + vNext > ListWeapons.Count-1)
        {
            vNewIndex = 0;
        }

        //change selected index
        vCurWeapIndex = vNewIndex;

        //then change weapon
        ChangeWeapon();
    }

    bool CanRotateBody()
    {
        bool CanRotate = true;
        if ((!IsPlayer && !SeePlayer) || (IsPlayer && CursorIsNear) || (!IsPlayer2 && !SeePlayer) || (IsPlayer2 && CursorIsNear))
        {
            CanRotate = false;
        }
        return CanRotate;
    }

    public void MeleeAttack()
    {
        //try to get a hostile target around
        if (Vector2.Distance(vMainPlayer.transform.position, transform.position) <= vGameManager.vMeleeRange)
        {
            vMainPlayer.GetComponent<Tds_Character>().ApplyDamage(ListWeapons[vCurWeapIndex].vDmg);
        }
    }

    //character has new ammo!
    public void RechargeWeapon()
    {
        //leave on ground a clip if there is any
        if (ListWeapons [vCurWeapIndex].vClipObj != null)
        {
            GameObject vClip = Instantiate(ListWeapons [vCurWeapIndex].vClipObj);
            vClip.transform.position = transform.position;
        }

        //reload is complete
        IsReloading = false;
        TimeToReload = 0f;

        //recharge
        ListWeapons [vCurWeapIndex].vAmmoCur = ListWeapons [vCurWeapIndex].vAmmoSize;

        //refresh recharge
        if (IsPlayer || IsPlayer2)
        {
            RefreshWeaponUI();
        }
    }

    void PlayAudio(AudioClip vClip)
    {
        if (vAudioSource != null && vClip != null)
        {
            vAudioSource.clip = vClip;
            vAudioSource.Play();
        }
    }

    public void RefreshLoot(Tds_Loot vLoot)
    {
        //show the loot items on ground
        if (vLoot != null)
        {
            vLoot.ShowHide(true);
        }
        else if (vCurLoot != null)
        {
            vCurLoot.ShowHide(false);
        }

        //check loot
        vCurLoot = vLoot;

        if (vLoot != null && (IsPlayer || IsPlayer2))
        {
            LootNearby = true;
            vGameManager.vPressSpaceObj.SetActive(true);
        }
        else if ((IsPlayer || IsPlayer2))
        {
            LootNearby = false;
            vGameManager.vPressSpaceObj.SetActive(false);
        }
    }

    //replace the list
    public void RefreshVariables(List<Tds_Tile> vNewList)
    {
        //check if there is a wall forward.
        bool hasBall = false;
        CanWalk = true;

        for (int i = 0; i < vNewList.Count; i++)
        {
            if (vNewList[i].vTileType == Tds_Tile.cTileType.Ball)
            {
                hasBall = true;
            }
        }

        if (vNewList.Count > 0 && !hasBall)
        {
            CanWalk = false;
        }
    }

    //make the character die
    void CharacterDie()
    {
        this.transform.position = startingLocation;
        this.HP = MaxHP;
        int playerNumber = IsPlayer ? 1 : 2;
        vGameManager.RefreshPlayerHP(playerNumber, (float)HP / (float)MaxHP);
    }

    void ShowDamage(int vDmg)
    {
        //create dmg text on target
        GameObject vDmgText = Instantiate(vGameManager.vDmgText);
        vDmgText.transform.position = transform.position;
        vDmgText.transform.SetParent(vGameManager.vWorldObj.transform);
        vDmgText.GetComponent<Tds_DmgText>().StartMoving(vDmg.ToString(), vGameManager.vDamageTextColor);    //make the dmg text starting moving
    }

    public void ApplyDamage(int vDmg)
    {
        //when receiving dmg. see the player wherever it is
        SeePlayer = true;
        IsAggro = true;

        //show current Dmg or not
        if (vGameManager.ShowDamageNumber)
        {
            ShowDamage(vDmg);
        }

        //reduce dmg
        HP -= vDmg;
        if (HP < 0)
        {
            HP = 0;
        }

        //if main player
        if (IsPlayer)
        {
            vGameManager.RefreshPlayerHP(1, (float)HP / (float)MaxHP);
        }
        else if (IsPlayer2)
        {
            vGameManager.RefreshPlayerHP(2, (float)HP / (float)MaxHP);
        }

        //check if the character died
        if (HP <= 0)
        {
            CharacterDie();
        }
        else
        {
            StartCoroutine(vGameManager.BlinkEffect(transform));
        }
    }

    public void InitialiseChar(Tds_GameManager vvGameManager)
    {
        //get the gamemanager
        vGameManager = vvGameManager;

        //replace the mouse cursor with this obj
        if ((IsPlayer || IsPlayer2) && vGameManager.AimObj != null)
        {
            vAimIcon = vGameManager.AimObj;
            // vGameManager.vPlayerText.text = vName;
        }

        //initialise list
        ListWeapons = new List<Tds_Weapons>();

        //initialise weapons from game manager. One place to handle all the game
        foreach (WeaponName vCurName in ListWeaponsName)
        {
            foreach (Tds_Weapons vCurWeapon in vGameManager.vWeaponList)
            {
                if (vCurName.ToString() == vCurWeapon.vWeaponName.ToString())
                {
                    //begin with full Ammo
                    Tds_Weapons vNewWeapon = CopyWeapon(vCurWeapon);
                    vNewWeapon.vAmmoCur = vNewWeapon.vAmmoSize;
                    ListWeapons.Add(vNewWeapon); //found the weapon, add it for the player
                }
            }
        }

        //make the 1st weapon the one you have in your hand.
        vCurWeapIndex = 0;

        //now the character is ready
        IsReady = true;
    }

    void ChangeWeapon()
    {
        //destroy old weapon if exist
        if (CurWeaponObj != null)
        {
            GameObject.Destroy(CurWeaponObj);
            CurWeaponObj = null;
        }

        //create the weapon
        GameObject vNewWeapon = Instantiate(ListWeapons [vCurWeapIndex].vWeaponObj, vLeftHandObj.transform);

        //check if weapon is 2handed
        if (vBodyAnimator.gameObject.activeSelf)
        {
            vBodyAnimator.SetBool("Is2Handed",ListWeapons [vCurWeapIndex].Is2Handed);
        }

        //positionnate the new weapon on the right hand
        vNewWeapon.transform.localScale = new Vector3(1f, 1f, 1f);
        vNewWeapon.transform.localPosition = new Vector3(0f, 0f, 0f);
        vNewWeapon.transform.rotation = new Quaternion(0f, 0f, 0f, 0f);

        //get the new weapon
        CurWeaponObj = vNewWeapon;

        //refresh all the weapon on top
        if (IsPlayer || IsPlayer2)
        {
            RefreshWeaponUI();
            CanAttack = true;
        }
    }

    //refresh all the weapon on top
    void RefreshWeaponUI()
    {
        //clear everything
        foreach (Tds_WeaponMenu vCurMenu in PlayerWeaponMenu)
        {
            UpdateWeaponMenu(vCurMenu, null);
        }

        int vcpt = 0;
        foreach (Tds_Weapons vCurWeapon in ListWeapons)
        {
            //get the right menu
            Tds_WeaponMenu vCurMenu = PlayerWeaponMenu[vcpt];

            //check if we are on the selected weapon
            if (vcpt == vCurWeapIndex)
            {
                vCurMenu.WepPanel.color = Color.yellow;
            }
            else
            {
                vCurMenu.WepPanel.color = Color.white;
            }

            //update the menu
            UpdateWeaponMenu(vCurMenu, vCurWeapon);

            //increase counter
            vcpt++;
        }
    }

    //update current weapon menu
    void UpdateWeaponMenu(Tds_WeaponMenu vCurMenu, Tds_Weapons vCurWeapon)
    {
        if (vCurWeapon != null)
        {
            //enable current menu
            vCurMenu.WepPanel.gameObject.SetActive(true);

            //weapon sprite
            vCurMenu.WeaponSprite.sprite = vCurWeapon.vWeaponIcon;

            //ammo text
            vCurMenu.AmmoValue.text = vCurWeapon.vAmmoCur.ToString() + " / " + vCurWeapon.vAmmoSize;

            //ammo text
            vCurMenu.AmmoValue.text = vCurWeapon.vAmmoCur.ToString() + " / " + vCurWeapon.vAmmoSize;

            //ammo bar % shown
            vCurMenu.AmmoBar.fillAmount = (float)vCurWeapon.vAmmoCur/(float)vCurWeapon.vAmmoSize;

            //check if it's reloading current weapon
            if (IsReloading && vCurWeapon == ListWeapons[vCurWeapIndex])
            {
                vCurMenu.ReloadImg.fillAmount =  ((float)TimeToReload / 1f);
            }
            else if (!IsReloading && vCurWeapon.vAmmoCur == 0)
            {
                vCurMenu.ReloadImg.fillAmount = 1f;
            }
            else {
                vCurMenu.ReloadImg.fillAmount = 0f;
            }
        }
        else
        {
            //disable the whole menu if there is no weapon there
            vCurMenu.WepPanel.gameObject.SetActive(false);
        }
    }

    //make a TRUE copy of this weapon to be used
    public Tds_Weapons CopyWeapon(Tds_Weapons vOld)
    {
        //copy everything for the weapon
        Tds_Weapons vNew = new Tds_Weapons();
        vNew.Rebounce = vOld.Rebounce;
        vNew.vAmmoSize = vOld.vAmmoSize;
        vNew.vDmg = vOld.vDmg;
        vNew.vName = vOld.vName;
        vNew.vProjectile = vOld.vProjectile;
        vNew.vWeaponIcon = vOld.vWeaponIcon;
        vNew.vWeaponObj = vOld.vWeaponObj;
        vNew.vAimObj = vOld.vAimObj;
        vNew.vImpactFX = vOld.vImpactFX;
        vNew.vProjectileSpeed = vOld.vProjectileSpeed;
        vNew.vShotFX = vOld.vShotFX;
        vNew.vClipObj = vOld.vClipObj;
        vNew.vTimeBtwShot = vOld.vTimeBtwShot;
        vNew.AttackAnimationUsed = vOld.AttackAnimationUsed;
        vNew.vWeaponType = vOld.vWeaponType;
        vNew.vTimeWaited = vOld.vTimeWaited;
        vNew.vAmmoCur = vOld.vAmmoCur;
        vNew.Is2Handed = vOld.Is2Handed;
        vNew.vBulletAngleList = vOld.vBulletAngleList;

        //return new tds_weapons
        return vNew;
    }

    //calculate the angle to rotate
    float GetWalkingRotation()
    {
        float vangle = 0f;
        switch (CurWalkDirection)
        {
            case WalkDirection.Down:
                vangle = 180f;
                break;
            case WalkDirection.Right:
                vangle = -90f;
                break;
            case WalkDirection.Left:
                vangle = 90f;
                break;
            case WalkDirection.Up:
                vangle = 0f;
                break;
            case WalkDirection.LeftUp:
                vangle = 45f;
                break;
            case WalkDirection.LeftDown:
                vangle = 135f;
                break;
            case WalkDirection.RightDown:
                vangle = -135;
                break;
            case WalkDirection.RightUp:
                vangle = -45f;
                break;
        }
        return vangle;
    }

    //Reset the player position
    public void ResetPosition()
    {
        this.transform.position = startPos;
        this.transform.GetComponent<Rigidbody2D>().velocity = Vector3.zero;
        this.transform.GetComponent<Rigidbody2D>().angularVelocity = 0f;
    }

    public void EnableSpeedBoost(float boostTime = 8.0f)
    {
        playerGlowingEffect.Play();
        speedBoostDuration = boostTime;
    }
}

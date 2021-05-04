using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatistics : MonoBehaviour
{
    public StatBar healthBar, hungerBar, radiationBar, staminaBar;
    public Text[] statTexts;
    public Color textColor;
    public float showTextTimeNormal = 15f;
    public float clearlyShowTextTimeNormal = 10f;
    public float showTextTimeShort = 6f;
    public float clearlyShowTextTimeShort = 4f;
    public string[] healthMessages, hungerMessages, radiationMessages, staminaMessages;
    public float statUpdateFreq = 1f;

    public bool IsSprinting { get; set; }
    public bool StaminaCooldown { get; private set; }

    private StatController playerHealth, playerHunger, playerRadiation, playerStamina;
    private bool developerMode;

    private float nextTimeStep = 0f;

    private WorldSettings worldSettings;

    private float startTime;

    private float lastRadiationLevel = 0;

    private RadiatePlayer radZone;

    // Ordering of each message number, its message,and the time since it was updated.
    private Tuple<int, string, float>[] messageOrder = new Tuple<int, string, float>[4];

    private bool hasMask = false;

    // Start is called before the first frame update
    void Start()
    {
        StaminaCooldown = false;

        developerMode = FindObjectOfType<WorldSettings>().developerMode;
        worldSettings = FindObjectOfType<WorldSettings>();

        // Initializes player stats.
        playerHealth = new StatController(100f, 0f, statUpdateFreq, 5f * 60f, false);
        playerHealth.SetMessages(healthMessages);
        messageOrder[0] = new Tuple<int, string, float>(0, healthMessages[0], 0);
        statTexts[0].text = healthMessages[0];

        playerHunger = new StatController(100f, 0f, statUpdateFreq, 5f * 60f, true);
        playerHunger.SetMessages(hungerMessages);
        messageOrder[1] = new Tuple<int, string, float>(1, hungerMessages[0], 0);
        statTexts[1].text = hungerMessages[0];

        playerRadiation = new StatController(0f, 100f);
        playerRadiation.SetMessages(radiationMessages);
        messageOrder[2] = new Tuple<int, string, float>(2, radiationMessages[0], 0);
        statTexts[2].text = radiationMessages[0];

        playerStamina = new StatController(100f, 0f, statUpdateFreq, 20f, false);
        playerStamina.SetMessages(staminaMessages);
        messageOrder[3] = new Tuple<int, string, float>(3, staminaMessages[0], 0);
        statTexts[3].text = staminaMessages[0];


        if (developerMode)
        {
            healthBar.SetMaxValue(playerHealth.GetCurrentValue());
            hungerBar.SetMaxValue(playerHunger.GetCurrentValue());
            radiationBar.SetMaxValue(playerRadiation.GetMaxValue());
            radiationBar.SetValue(playerRadiation.GetCurrentValue()); // Makes sure radiation starts at 0;
            staminaBar.SetMaxValue(playerStamina.GetCurrentValue());
        }

        radZone = GameObject.FindGameObjectWithTag("RadiationZone").GetComponentInChildren<RadiatePlayer>();

        startTime = Time.time;

    }

    // Update is called once per frame
    void Update()
    {
        if (!worldSettings.IsGameOver())
        {
            if (developerMode)
            {
                // Uncomment for testing purposes. Delete after demonstration.
                if (Input.GetKeyDown(KeyCode.Alpha6))
                    ChangePlayerHealth(-5f);
                if (Input.GetKeyDown(KeyCode.Alpha7))
                    ChangePlayerHealth(5f);
                if (Input.GetKeyDown(KeyCode.Y))
                    ChangePlayerHunger(-5f);
                if (Input.GetKeyDown(KeyCode.U))
                    ChangePlayerHunger(5f);
                if (Input.GetKeyDown(KeyCode.N))
                    ChangePlayerRadiation(-5f);
                if (Input.GetKeyDown(KeyCode.M))
                    ChangePlayerRadiation(5f);
                if (Input.GetKeyDown(KeyCode.Alpha8))
                    ChangePlayerStamina(-5f);
                if (Input.GetKeyDown(KeyCode.Alpha9))
                    ChangePlayerStamina(5f);
            }

            float health = playerHealth.GetCurrentValue();
            float hunger = playerHunger.GetCurrentValue();
            float radiation = playerRadiation.GetCurrentValue();

            UpdateMessageTexts();
            UpdateMessageAlphas(Time.deltaTime);

            // Checks if the player is dead.
            if (health == 0 || hunger == 0 || radiation == playerRadiation.GetMaxValue())
            {
                GetComponent<PlayerController>().PlayerDies();
            }

            if (playerStamina.GetCurrentValue() == 0)
                StaminaCooldown = true;
            else if (StaminaCooldown && playerStamina.GetCurrentValue() >= playerStamina.GetMaxValue() / 5)
            {
                // If the player just got the ability to use stamina again, change the stamina message from the
                // "stamina gone" message to the "stamina low" message.
                playerStamina.MessageChanged = false;
                messageOrder[3] = Tuple.Create(messageOrder[3].Item1, staminaMessages[3], messageOrder[3].Item3);
                UpdateStatText(3);
                StaminaCooldown = false;
            }
        }

    }

    void FixedUpdate()
    {
        if (!worldSettings.IsGameOver())
        {
            float hunger = playerHunger.GetCurrentValue();
            float radiation = playerRadiation.GetCurrentValue();
            // Updates players stats (subtracts startTime as Time.time does not reset after
            // death or loading a new level).
            if (Time.time - startTime >= nextTimeStep)
            {
                nextTimeStep += statUpdateFreq;
                HandleHealthTick(hunger);
                HandleHungerTick(radiation);
                HandleRadiationTick();
                HandleStaminaTick(radiation);
            }
        }

    }

    // Updates the players health.
    public void ChangePlayerHealth(float amount)
    {
        float radiationEffect = 1;

        // Taking damage
        if (amount < 0)
            radiationEffect = Mathf.Pow(2, playerRadiation.GetCurrentValue() / playerRadiation.GetMaxValue());
        else // Healing
            radiationEffect = Mathf.Clamp((-7f / 6f) * playerRadiation.GetCurrentValue() / playerRadiation.GetMaxValue() + 1.2334f, 0, 1);

        playerHealth.UpdateStatByAmount(amount * radiationEffect);

        if (developerMode)
            healthBar.UpdateValue(amount * radiationEffect);
    }

    // Updates the players hunger.
    public void ChangePlayerHunger(float amount)
    {
        playerHunger.UpdateStatByAmount(amount);

        if (developerMode)
            hungerBar.UpdateValue(amount);
    }

    // Updates the players radiation.
    public void ChangePlayerRadiation(float amount)
    {
        playerRadiation.UpdateStatByAmount(amount);

        if (developerMode)
            radiationBar.UpdateValue(amount);
    }

    // Updates the players stamina.
    public void ChangePlayerStamina(float amount)
    {
        playerStamina.UpdateStatByAmount(amount);
        if (developerMode)
            staminaBar.UpdateValue(amount);
    }

    // Regenerates the player based on the current level of hunger, thirst, and radiation.
    private void HandleHealthTick(float hunger)
    {
        // 0 if less than 1/4th maxHunger, 1 if greater than 3/4 maxHunger, varies linearly between.
        float hungerEffect = Mathf.Clamp(2f * hunger / playerHunger.GetMaxValue() - 0.5f, 0, 1);

        playerHealth.UpdateStatOnTick(hungerEffect);

        if (developerMode)
            healthBar.SetValue(playerHealth.GetCurrentValue());
    }

    private void HandleHungerTick(float radiation)
    {
        float radiationEffect = Mathf.Pow(5, radiation / playerRadiation.GetMaxValue());

        playerHunger.UpdateStatOnTick(radiationEffect);

        if (developerMode)
            hungerBar.SetValue(playerHunger.GetCurrentValue());
    }

    private void HandleRadiationTick()
    {
        float radiationDamage = (worldSettings.AmbientRadiationLevel - lastRadiationLevel) * playerRadiation.GetMaxValue();
        lastRadiationLevel = worldSettings.AmbientRadiationLevel;


        if (radZone.Radiate) // If player within radiation zone, make them take radiation quicker.
            radiationDamage = Mathf.Clamp(radiationDamage * radZone.RadiationMultiplier, radZone.MinDamage, worldSettings.RadiationMax);

        // If has a mask, takes a lot less radiation damage.
        if (hasMask)
            radiationDamage = radiationDamage / 10;

        playerRadiation.UpdateStatByAmount(radiationDamage);



        if (developerMode)
            radiationBar.SetValue(playerRadiation.GetCurrentValue());
    }

    private void HandleStaminaTick(float radiation)
    {
        if (!IsSprinting)
        {
            playerStamina.UpdateStatOnTick(1);
        }
        else
        {
            float radiationEffect = Mathf.Pow(5, radiation / playerRadiation.GetMaxValue());
            playerStamina.UpdateStatOnTick(-1.5f * radiationEffect);

            if (playerStamina.GetCurrentValue() == 0)
                IsSprinting = false;
        }

        if (developerMode)
            staminaBar.SetValue(playerStamina.GetCurrentValue());

    }

    // Checks if the player can begin sprinting. Ensures the player can only sprint if they have
    // at least 20% of their stamina full.
    public bool CanStartSprinting()
    {
        return playerStamina.GetCurrentValue() >= playerStamina.GetMaxValue() / 5f;
    }

    public void AquiredMask()
    {
        hasMask = true;
    }

    private void UpdateMessageAlphas(float delatTime)
    {
        for (int i = 0; i < messageOrder.Length; i++)
        {
            // Makes the time shorter if it is a common stat (only used for stamina currently).
            float showTextTime = (i == 3) ? showTextTimeShort : showTextTimeNormal;
            float clearlyShowTextTime = (i == 3) ? clearlyShowTextTimeShort : clearlyShowTextTimeNormal;

            // If the color is not yet clear
            if (messageOrder[i].Item3 < showTextTime)
            {

                // Updates the time for the message, clamped between 0 and showTextTime.
                messageOrder[i] = Tuple.Create(messageOrder[i].Item1, messageOrder[i].Item2,
                    Mathf.Clamp(messageOrder[i].Item3 + delatTime, 0, showTextTime));

                // Controls how far through the color change the message is. The equation makes it
                // stay the initial color for clearlyShowTextTime seconds, and makes it clear after showTextTime seconds
                float lerpTime = Mathf.Clamp((1f / (showTextTime - clearlyShowTextTime) * messageOrder[i].Item3) -
                    (clearlyShowTextTime / (showTextTime - clearlyShowTextTime)), 0, 1);

                // Lerps the text color to clear
                statTexts[messageOrder[i].Item1].color = new Color(textColor.r, textColor.g, textColor.b,
                    Mathf.Lerp(textColor.a, 0, lerpTime));
            }
        }
    }

    private void UpdateMessageTexts()
    {
        if (playerHealth.MessageChanged)
        {
            playerHealth.MessageChanged = false;
            messageOrder[0] = Tuple.Create(messageOrder[0].Item1, playerHealth.CurrentMessage, messageOrder[0].Item3);
            UpdateStatText(0);
        }

        if (playerHunger.MessageChanged)
        {
            playerHunger.MessageChanged = false;
            messageOrder[1] = Tuple.Create(messageOrder[1].Item1, playerHunger.CurrentMessage, messageOrder[1].Item3);
            UpdateStatText(1);
        }

        if (playerRadiation.MessageChanged)
        {
            playerRadiation.MessageChanged = false;
            messageOrder[2] = Tuple.Create(messageOrder[2].Item1, playerRadiation.CurrentMessage, messageOrder[2].Item3);
            UpdateStatText(2);
        }

        if (playerStamina.MessageChanged && !StaminaCooldown) // If the stamina message changes and the player has stamina.
        {
            playerStamina.MessageChanged = false;
            messageOrder[3] = Tuple.Create(messageOrder[3].Item1, playerStamina.CurrentMessage, messageOrder[3].Item3);
            UpdateStatText(3);
        }
        else if (playerStamina.MessageChanged) // If the player runs out of stamina
        {
            playerStamina.MessageChanged = false;
            messageOrder[3] = Tuple.Create(messageOrder[3].Item1, staminaMessages[4], messageOrder[3].Item3);
            UpdateStatText(3);
        }
    }

    private void UpdateStatText(int statNum)
    {
        int i = messageOrder[statNum].Item1 - 1;

        while (i >= 0)
        {
            for (int j = 0; j < messageOrder.Length; j++)
            {
                if (messageOrder[j].Item1 == i)
                {
                    messageOrder[j] = Tuple.Create(i + 1, messageOrder[j].Item2, messageOrder[j].Item3);
                    statTexts[i + 1].text = messageOrder[j].Item2;
                    i--;
                    break;
                }
            }
        }

        messageOrder[statNum] = Tuple.Create(0, messageOrder[statNum].Item2, 0f);
        statTexts[0].text = messageOrder[statNum].Item2;
    }
}
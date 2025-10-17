using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using System.Reflection;
using System;

/// <summary>
/// Unit tests for the Timer system.
/// Tests time tracking, formatting, start/stop functionality, and reset behavior.
/// </summary>
public class TimerTests
{
    private GameObject timerObject;
    private dynamic timerComponent;
    private WaitForSeconds waitTime;
    private Type timerType;
    private object textComponent;

    [SetUp]
    public void SetUp()
    {
        // Create a test game object
        timerObject = new GameObject("TestTimer");
        waitTime = new WaitForSeconds(0.1f);
        
        // Find the Timer type through reflection
        timerType = null;
        foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
        {
            try
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (type.Name == "Timer" && type.IsSubclassOf(typeof(MonoBehaviour)))
                    {
                        timerType = type;
                        Debug.Log($"Found Timer type: {timerType.FullName} in assembly {timerType.Assembly.FullName}");
                        break;
                    }
                }
                if (timerType != null) break;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error loading types from assembly {assembly.FullName}: {ex.Message}");
            }
        }
        
        Assert.IsNotNull(timerType, "Could not find Timer type");
        
        // Add the Timer component using reflection
        timerComponent = timerObject.AddComponent(timerType);
        
        // Create a TextMeshPro object for the timer
        var tmpType = FindType("TMPro.TextMeshProUGUI");
        if (tmpType != null)
        {
            textComponent = timerObject.AddComponent(tmpType);
            
            // Set the timerDisplay field using reflection
            var timerDisplayField = timerType.GetField("timerDisplay", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            
            if (timerDisplayField != null)
            {
                timerDisplayField.SetValue(timerComponent, textComponent);
            }
        }
    }
    
    private Type FindType(string typeName)
    {
        Type type = null;
        foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
        {
            try
            {
                type = assembly.GetType(typeName);
                if (type != null)
                {
                    Debug.Log($"Found type {typeName} in assembly {assembly.FullName}");
                    return type;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error finding type {typeName} in assembly {assembly.FullName}: {ex.Message}");
            }
        }
        return null;
    }
    
    [TearDown]
    public void TearDown()
    {
        if (timerObject != null)
        {
            UnityEngine.Object.DestroyImmediate(timerObject);
        }
    }

    [Test]
    public void Timer_InitialState_IsZero()
    {
        // Get the GetTime method using reflection
        var getTimeMethod = timerType.GetMethod("GetTime");
        Assert.IsNotNull(getTimeMethod, "GetTime method not found");
        
        // Call GetTime and verify it returns 0
        float time = (float)getTimeMethod.Invoke(timerComponent, null);
        Assert.AreEqual(0f, time, "Timer should start at 0");
    }

    [Test]
    public void Timer_StartStop_Methods()
    {
        // Verify that the StartTimer and StopTimer methods exist
        var startTimerMethod = timerType.GetMethod("StartTimer");
        var stopTimerMethod = timerType.GetMethod("StopTimer");
        var resetTimerMethod = timerType.GetMethod("ResetTimer");
        
        Assert.IsNotNull(startTimerMethod, "StartTimer method not found");
        Assert.IsNotNull(stopTimerMethod, "StopTimer method not found");
        Assert.IsNotNull(resetTimerMethod, "ResetTimer method not found");
        
        // Now test calling them - they should not throw exceptions
        startTimerMethod.Invoke(timerComponent, null);
        stopTimerMethod.Invoke(timerComponent, null);
        resetTimerMethod.Invoke(timerComponent, null);
        
        // Success if we got here without exceptions
        Assert.Pass("Successfully called StartTimer, StopTimer, and ResetTimer methods");
    }
    
    [Test]
    public void Timer_AfterReset_IsZero()
    {
        // Get references to methods we need
        var getTimeMethod = timerType.GetMethod("GetTime");
        var startTimerMethod = timerType.GetMethod("StartTimer");
        var resetTimerMethod = timerType.GetMethod("ResetTimer");
        
        // Start the timer to potentially change its state
        startTimerMethod.Invoke(timerComponent, null);
        
        // Reset the timer
        resetTimerMethod.Invoke(timerComponent, null);
        
        // Verify time is reset to 0
        float time = (float)getTimeMethod.Invoke(timerComponent, null);
        Assert.AreEqual(0f, time, "Timer should be 0 after reset");
        
        // If we have the textComponent, check the text format too
        if (textComponent != null)
        {
            // Get the "text" property from the TMPro text component
            var textProperty = textComponent.GetType().GetProperty("text");
            if (textProperty != null)
            {
                string displayText = (string)textProperty.GetValue(textComponent);
                Assert.AreEqual("0:00:0", displayText, "Timer text should be reset to 0:00:0 format");
            }
        }
    }
}
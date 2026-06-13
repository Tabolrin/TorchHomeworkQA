using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class TorchTests
{
    [UnityTest]
    public IEnumerator UnitTest_Torch_Interact_TurnsOnLightAndChangesSprite()
    {
        // Arrange
        GameObject torchObj = new GameObject("TestTorch");
        SpriteRenderer renderer = torchObj.AddComponent<SpriteRenderer>();
        Light2D light = torchObj.AddComponent<Light2D>();
        Torch torch = torchObj.AddComponent<Torch>();
        
        torch.unlitSprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), Vector2.zero);
        torch.litSprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), Vector2.zero);
        
        Color expectedLightColor;
        ColorUtility.TryParseHtmlString("#FFFF80", out expectedLightColor);
        light.color = expectedLightColor; 
        
        // Wait for one frame so Start() can finish setting up the unlit state
        yield return null; 

        // Act
        torch.Interact();

        // Assert
        Assert.IsTrue(torch.isLit, "Unit Test Failed: Torch state should be true.");
        Assert.AreEqual(torch.litSprite, renderer.sprite, "Unit Test Failed: Sprite did not change.");
        Assert.IsTrue(light.enabled, "Unit Test Failed: Light2D did not turn on.");
        Assert.AreEqual(expectedLightColor, light.color, "Unit Test Failed: Light is not the expected #FFFF80 color.");
        
        // Cleanup
        Object.DestroyImmediate(torchObj);
    }

    [UnityTest]
    public IEnumerator IntegrationTest_Torch_DetectsPlayer_AndAllowsInteraction()
    {
        // Arrange
        GameObject playerObj = new GameObject("Player");
        playerObj.tag = "Player";
        playerObj.AddComponent<Rigidbody2D>().gravityScale = 0;
        playerObj.AddComponent<CircleCollider2D>();

        GameObject torchObj = new GameObject("Torch");
        CircleCollider2D trigger = torchObj.AddComponent<CircleCollider2D>();
        trigger.isTrigger = true;
        Torch torch = torchObj.AddComponent<Torch>();

        // Act - Move player onto the torch
        torchObj.transform.position = Vector2.zero;
        playerObj.transform.position = Vector2.zero;

        // Wait for physics
        yield return new WaitForFixedUpdate(); 
        yield return new WaitForFixedUpdate();

        // Assert Trigger Detection
        Assert.IsTrue(torch.isPlayerInRange, "Integration Failed: Torch did not detect player.");
        
        // Act - Simulate interaction while in range
        torch.Interact();
        yield return null;

        // Assert Interaction
        Assert.IsTrue(torch.isLit, "Integration Failed: Torch did not light when interacted with.");

        Object.DestroyImmediate(playerObj);
        Object.DestroyImmediate(torchObj);
    }

    [UnityTest]
    public IEnumerator RegressionTest_Torch_StaysLit_WhenPlayerWalksAway()
    {
        // Arrange
        GameObject torchObj = new GameObject("Torch");
        Torch torch = torchObj.AddComponent<Torch>();
        
        // Act
        torch.Interact(); // Light it up
        torch.isPlayerInRange = false; // Simulate player leaving trigger
        yield return null;

        // Assert
        Assert.IsTrue(torch.isLit, "Regression Bug: Torch turned off when player walked away.");
        
        Object.DestroyImmediate(torchObj);
    }

    [UnityTest]
    public IEnumerator SmokeTest_Level_BootsWithThreeTorches()
    {
        // Arrange
        yield return SceneManager.LoadSceneAsync("TorchLevel");
        yield return null; 

        // Act
        Torch[] torchesInScene = Object.FindObjectsByType<Torch>(FindObjectsSortMode.None);

        // Assert
        Assert.AreEqual(3, torchesInScene.Length, "Smoke Test Failed: Expected exactly 3 torches.");
        Assert.IsNotNull(GameObject.FindGameObjectWithTag("Player"), "Smoke Test Failed: Player is missing.");
    }

    [UnityTest]
    public IEnumerator FunctionalTest_FullFlow_PlayerLightsMultipleTorches()
    {
        // Arrange
        GameObject torch1Obj = new GameObject("Torch1");
        Torch torch1 = torch1Obj.AddComponent<Torch>();
        
        GameObject torch2Obj = new GameObject("Torch2");
        Torch torch2 = torch2Obj.AddComponent<Torch>();

        // Act 1: Light first torch
        torch1.Interact();
        yield return null;

        // Act 2: Light second torch
        torch2.Interact();
        yield return null;

        // Assert
        bool bothLit = torch1.isLit && torch2.isLit;
        Assert.IsTrue(bothLit, "Functional Test Failed: Torches did not remain lit during full flow.");
        
        Object.DestroyImmediate(torch1Obj);
        Object.DestroyImmediate(torch2Obj);
    }
    
    [UnityTest]
    public IEnumerator UnitTest_Torch_Initializes_InUnlitState()
    {
        // Arrange
        GameObject torchObj = new GameObject("TestTorch");
        SpriteRenderer renderer = torchObj.AddComponent<SpriteRenderer>();
        Light2D light = torchObj.AddComponent<Light2D>();
        Torch torch = torchObj.AddComponent<Torch>();
        
        torch.unlitSprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), Vector2.zero);
        torch.litSprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), Vector2.zero);
        
        // Act
        yield return null;

        // Assert
        Assert.IsFalse(torch.isLit, "Initialization Failed: Torch boolean started as true.");
        Assert.AreEqual(torch.unlitSprite, renderer.sprite, "Initialization Failed: Torch did not start with the unlit sprite.");
        Assert.IsFalse(light.enabled, "Initialization Failed: Light2D was turned on by default.");
        
        // Cleanup
        Object.DestroyImmediate(torchObj);
    }

    [UnityTest]
    public IEnumerator UnitTest_Torch_Interact_WhenAlreadyLit_RemainsLit()
    {
        // Arrange
        GameObject torchObj = new GameObject("TestTorch");
        torchObj.AddComponent<SpriteRenderer>();
        torchObj.AddComponent<Light2D>();
        Torch torch = torchObj.AddComponent<Torch>();
        
        torch.unlitSprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), Vector2.zero);
        torch.litSprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), Vector2.zero);
        
        // Act - Calling it twice to simulate mashing the interaction key
        torch.Interact(); 
        torch.Interact(); 
        yield return null;

        // Assert
        Assert.IsTrue(torch.isLit, "Idempotency Failed: Interacting a second time turned the torch off.");
        
        // Cleanup
        Object.DestroyImmediate(torchObj);
    }
}
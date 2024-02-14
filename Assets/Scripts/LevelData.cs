using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// PRE-ALPHA VERSION
/* stores level checkpoint/default spawn locations and keeps track of
 *  the number of checkpoints reached. Can be used to store data that
 *  is PER LEVEL like stardust counts for the current level.
 */
public static class LevelData
{
    // set these manually
    private static Vector3[] OrangeRespawnArray = new[] {
        new Vector3(-40.4f, 22.3f, 41.2f),
        new Vector3(144.26f, 66.95f, 42.81f)
    };

    private static Vector3[] TutorialRespawnArray = new[]
    {
        new Vector3(0,3,0),
        new Vector3(38.2f,37.6f,32.7f)
    };

    private static int checkpointReached; // stores latest checkpoint reached

    // player related data
    public static int starsparkleCount;

    // resets temporary data. Do this when loading into a level or leaving a level.
    public static void resetLevelData()
    {
        checkpointReached = 0;
    }

    // specifically for when loading to a checkpoint. This is so the player
    // cant farm star sparkles by dying over and over again.
    // TODO - Implemnent this when star sparkles are moved from game jam build
    public static void resetSparkles() {
        starsparkleCount = 0;
    }

    // get respawn position for player based on last checkpoint reached.
    // TODO - works for the base level rn, expand to other levels as they are made
    public static Vector3 getRespawnPos() {
        Debug.Log("GIVING RESPAWN POSITION TO PLAYER CONTROLLER");
        switch (SceneManager.GetActiveScene().name)
        {
            case "Orange Level First Playable":
                return OrangeRespawnArray[checkpointReached];
            case "Tutorial Level":
                return TutorialRespawnArray[checkpointReached];
            default:
                return Vector3.zero;

        }
    }

    public static void setCheckpoint(int c) {
        /* prevent player from setting themselves back
         * by only storing if they've found a "greater"
         * checkpoint. */
        if (c > checkpointReached)
            checkpointReached = c;
    }
}
using UnityEngine;

public class LevelExitByChoice : MonoBehaviour
{
    public float timeUntilChoice = 3f;
    private float timeEnteredCollider = 0f;
    private bool triggerMethodCalled = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            timeEnteredCollider = Time.time;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") && !triggerMethodCalled)
        {
            if (Time.time - timeEnteredCollider >= timeUntilChoice)
            {
                UIManager.get.ShowLeaveByChoiceScreen();
                triggerMethodCalled = true;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            timeEnteredCollider = 0f;
            triggerMethodCalled = false;
        }
    }
}

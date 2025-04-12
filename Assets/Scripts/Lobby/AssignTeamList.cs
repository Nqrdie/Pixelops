using UnityEngine;

public class AssignTeamList : MonoBehaviour
{
    public int id;

    private void Start()
    {
        id = Random.Range(0, 10000000);
    }
}

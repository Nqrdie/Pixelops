using UnityEngine;
using Unity.Netcode;

public class PlayerCam : NetworkBehaviour
{

    public Transform player;
    public float sensX;
    public float sensY;

    private float xRotation;
    private float yRotation;

    bool cams;
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (player != null)
        {


            float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensX;
            float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensY;

            yRotation += mouseX;
            xRotation -= mouseY;

            xRotation = Mathf.Clamp(xRotation, -90, 90);

            player.rotation = Quaternion.Euler(0, yRotation, 0);
            transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
            transform.position = new Vector3(player.position.x, player.position.y + 0.6f, player.position.z);
        }
    }
}

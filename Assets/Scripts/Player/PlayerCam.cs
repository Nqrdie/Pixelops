using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using TMPro;
using System;

public class PlayerCam : NetworkBehaviour
{

    public Transform player;
    public float sensX = 800;
    public float sensY = 800;


    private float xRotation;
    private float yRotation;

    [SerializeField] private Slider sensSlider;
    [SerializeField] private TextMeshProUGUI sensText;

    bool cams;
    void Start()
    {
        
    }

    void Update()
    {
        LookCam();
    }

    public void LookCam()
    {
        if (player != null && !GameSystem.Instance.settingsTriggered)
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

    public void SetSensitivity()
    {
        sensX = sensSlider.value;
        sensY = sensSlider.value;

        sensText.text = (sensSlider.value / 100).ToString("0.0");
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;
using UnityEngine.Rendering;
using static OVRInput;
using SelfButton = UnityEngine.UI.Button;

public class PanelsScript : MonoBehaviour
{
    private Texture2D[] textures;

    public GameObject content;

    // public GameObject panel;
    public GameObject buttonPrefab;

    private Texture2D currentTexture;

    // [SerializeField] private Texture2D texture;
    [SerializeField] private Shader shader;

    private GameObject[] borderPanels;
    int width = 1200;
    int height = 13874;

    public OVRHand hand;
    // Start is called before the first frame update
    void Start()
    {
        textures = Resources.LoadAll<Texture2D>("Textures");
        borderPanels = new GameObject[textures.Length];
        // Iterate through the children array and set their Image component's material
        for (int i = 0; i < textures.Length; i++)
        {
            // GameObject newPannel = Instantiate(panel, content.transform);
            // Image childImage = newPannel.GetComponent<Image>();
            // childImage.sprite = Sprite.Create(textures[i], new Rect(0, 0, textures[i].width, textures[i].height), Vector2.one*0.5f);

            // GameObject buttonGameObject = Instantiate(buttonPrefab, content.transform);

            // Create a new button
            GameObject buttonGameObject = Instantiate(buttonPrefab);
            borderPanels[i] = buttonGameObject.transform.GetChild(0).gameObject;
            buttonGameObject.transform.SetParent(content.transform, false);

            // Assign the click function
            SelfButton button = buttonGameObject.GetComponent<SelfButton>();
            int temp = i;
            button.onClick.AddListener(() => OnPanelClick(temp));

            // Add the image to the button
            Image imageComponent = buttonGameObject.GetComponent<Image>();
            Sprite sprite = Sprite.Create(textures[i], new Rect(0, 0, textures[i].width, textures[i].height), Vector2.one * 0.5f);

            imageComponent.sprite = sprite;
        }

        currentTexture = textures[0];
     }

    // Update is called once per fram
    void Update()
    {
        if (OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger) || OVRInput.Get(OVRInput.Button.SecondaryIndexTrigger))
        {
            Vector3 controllerPosition;
            Quaternion controllerRotation;
            if (OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger))
            {
                controllerPosition = OVRInput.GetLocalControllerPosition(OVRInput.Controller.LTouch);
                controllerRotation = OVRInput.GetLocalControllerRotation(OVRInput.Controller.LTouch);
            }
            else
            {
                controllerPosition = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch);
                controllerRotation = OVRInput.GetLocalControllerRotation(OVRInput.Controller.RTouch);
            }
            Vector3 rayDirection = controllerRotation * Vector3.forward;
            
            RaycastHit hit;
            if (Physics.Raycast(controllerPosition, rayDirection, out hit))
            {
                GameObject hitPlane = hit.collider.gameObject;
                if (hitPlane.transform.parent == null) return;
                if (hitPlane.transform.parent.name == "WALL_FACE")
                {
                    Debug.Log($"Hit: {hitPlane.transform.parent.name}");

                    Material material = new Material(shader);
                    material.mainTexture = currentTexture;
                    material.SetFloat("_Cull", (float) CullMode.Off);

                    float scaleFactor = 0.0001f;
                    // Constrain the dimensions
                    float imageWidth = width * scaleFactor;
                    float imageHeight = height * scaleFactor;
                    
                    // Obtain the plane's dimensions
                    float planeWidth = hitPlane.transform.localScale.x;
                    float planeHeight = hitPlane.transform.localScale.z;

                    // Set the tiling values
                    float tileX = planeWidth / imageWidth;
                    float tileY = planeHeight / imageHeight;
                    material.mainTextureScale = new Vector2(tileX, tileY);

                    // Apply the material to the plane
                    MeshRenderer planeRenderer = hitPlane.GetComponentInParent<MeshRenderer>();
                    planeRenderer.material = material;
                }
                else
                {
                    Debug.Log($"{hitPlane.transform.parent.name} is not a wall");
                }
            }
        }

        if (hand.GetFingerIsPinching(OVRHand.HandFinger.Index))
        {
            Debug.Log("Index finger is pinching");
        }
    }

    void OnPanelClick(int t) {
        Debug.Log(t);
        currentTexture = textures[t];
        for (int i = 0; i < textures.Length; i++) {
            if (i == t) borderPanels[i].SetActive(true);
            else borderPanels[i].SetActive(false);
        }
    }
}
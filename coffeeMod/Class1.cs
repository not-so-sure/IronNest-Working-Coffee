using MelonLoader;
using UnityEngine;
using UnityEngine.InputSystem;

[assembly: MelonInfo(typeof(CoffeeMod), "Working Coffee machine", "0.5", "LukaSyntax")]

public class CoffeeMod : MelonMod
{
    private GameObject heldCup = null;
    private GameObject spawnButton = null;
    private Vector3 restPos = new Vector3(0.1436f, -0.1582f, 0.24f);
    private Quaternion restRot = Quaternion.Euler(90, 180, 0);
    private Vector3 sipPos = new Vector3(0, -0.1f, 0.14f);
    private Quaternion sipRot = Quaternion.Euler(45, 0, 300);
    private bool isSipping = false;
    private float sipTimer = 0f;
    private float lerpSpeed = 5f;

    private Vector3 buttonPosition = new Vector3(-5.6922f, -11.6465f,7.8508f);

    public override void OnSceneWasLoaded(int buildIndex, string sceneName)
    {
        CreatePhysicalButton();
    }

    private void CreatePhysicalButton()
    {
        if (spawnButton != null) return;
        spawnButton = GameObject.CreatePrimitive(PrimitiveType.Cube);
        spawnButton.name = "Coffee_Button";
        spawnButton.transform.position = buttonPosition;
        spawnButton.transform.localScale = new Vector3(0.05f,0.05f,0.05f);
        spawnButton.transform.rotation = Quaternion.Euler(-0, 30, 0);
    }

    public override void OnUpdate()
    {
        var keyboard = Keyboard.current;
        var mouse = Mouse.current;
        if (keyboard == null || mouse == null) return;

        if (mouse.leftButton.wasPressedThisFrame)
        {
            HandleLeftClick();
        }

        if (keyboard.qKey.wasPressedThisFrame && heldCup != null)
        {
            DropCup();
        }

        if (heldCup != null)
        {
            UpdateCupAnimation();
        }
    }

    private void HandleLeftClick()
    {
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit, 3.5f))
        {
            GameObject hitObj = hit.collider.gameObject;

            if (hitObj.name == "Coffee_Button")
            {
                SpawnCoffeeNearButton();
                return;
            }

            if (heldCup == null && hitObj.name == "Physical_Coffee_Cup")
            {
                PickUpCup(hitObj);
                return;
            }
        }

        if (heldCup != null)
        {
            StartSip();
        }
    }

    private void SpawnCoffeeNearButton()
    {
        GameObject masterCup = GameObject.Find(".Coffee Cup.glb") ??
                               GameObject.FindObjectsOfType<GameObject>().FirstOrDefault(g => g.name.Contains("Coffee Cup"));

        if (masterCup != null)
        {
            // -5.873f, -11.7312f, 7.9523f is the perfect position, change it if you find a better one and contact me
            Vector3 spawnPos = new Vector3(-5.873f, -11.7312f, 7.9523f);
            SpawnPhysicalCup(spawnPos, masterCup);
        }
    }

    private void PickUpCup(GameObject cup)
    {
        heldCup = cup;
        Rigidbody rb = heldCup.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;

        heldCup.transform.SetParent(Camera.main.transform);
        heldCup.transform.localPosition = restPos;
        heldCup.transform.localRotation = restRot;
    }

    private void StartSip()
    {
        if (isSipping) return;
        isSipping = true;
        sipTimer = 3f;
    }

    private void UpdateCupAnimation()
    {
        Vector3 targetPos = isSipping ? sipPos : restPos;
        Quaternion targetRot = isSipping ? sipRot : restRot;

        heldCup.transform.localPosition = Vector3.Lerp(heldCup.transform.localPosition, targetPos, Time.deltaTime * lerpSpeed);
        heldCup.transform.localRotation = Quaternion.Slerp(heldCup.transform.localRotation, targetRot, Time.deltaTime * lerpSpeed);

        if (isSipping)
        {
            sipTimer -= Time.deltaTime;
            if (sipTimer <= 0) isSipping = false;
        }
    }

    private void DropCup()
    {
        isSipping = false;
        heldCup.transform.SetParent(null);
        Rigidbody rb = heldCup.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = false;
        heldCup = null;
    }

    private void SpawnPhysicalCup(Vector3 spawnPos, GameObject Cup)
    {
        GameObject newCup = GameObject.Instantiate(Cup, spawnPos, Quaternion.Euler(90,0,0));
        newCup.name = "Physical_Coffee_Cup";
        newCup.SetActive(true);
        Rigidbody rb = newCup.GetComponent<Rigidbody>() ?? newCup.AddComponent<Rigidbody>();
        rb.useGravity = true;
        MeshCollider col = newCup.GetComponent<MeshCollider>() ?? newCup.AddComponent<MeshCollider>();
        col.convex = true;
    }
}
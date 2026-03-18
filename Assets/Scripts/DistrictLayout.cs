using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Procedural generation of a low-poly Khortitskiy District (Baburka) street scene.
/// Generates panelka buildings, a street with sidewalks, benches, trash bins,
/// and a school entrance with a barrier zone.
/// </summary>
public class DistrictLayout : MonoBehaviour
{
    [Header("Street Settings")]
    [SerializeField] private float streetLength = 120f;
    [SerializeField] private float streetWidth = 12f;
    [SerializeField] private float sidewalkWidth = 3f;

    [Header("Building Settings")]
    [SerializeField] private int buildingsPerSide = 4;
    [SerializeField] private float buildingWidth = 18f;
    [SerializeField] private float buildingDepth = 10f;
    [SerializeField] private float buildingHeight = 30f;
    [SerializeField] private float buildingSpacing = 8f;
    [SerializeField] private int floorsPerBuilding = 9;

    [Header("Furniture Settings")]
    [SerializeField] private int benchCount = 6;
    [SerializeField] private int trashBinCount = 8;

    [Header("School Settings")]
    [SerializeField] private Vector3 schoolPosition = new Vector3(0f, 0f, 50f);
    [SerializeField] private float schoolWidth = 30f;
    [SerializeField] private float schoolDepth = 15f;
    [SerializeField] private float schoolHeight = 12f;

    [Header("Materials")]
    [SerializeField] private Material panelkaWallMaterial1;
    [SerializeField] private Material panelkaWallMaterial2;
    [SerializeField] private Material roadMaterial;
    [SerializeField] private Material sidewalkMaterial;
    [SerializeField] private Material benchMaterial;
    [SerializeField] private Material trashBinMaterial;
    [SerializeField] private Material roofMaterial;
    [SerializeField] private Material schoolMaterial;

    [Header("Character Prefabs")]
    [SerializeField] private GameObject vitaliyPrefab;
    [SerializeField] private GameObject kirillPrefab;
    [SerializeField] private GameObject ulianaPrefab;
    [SerializeField] private GameObject zavkhozPrefab;

    private readonly List<GameObject> generatedObjects = new List<GameObject>();

    private void Start()
    {
        GenerateDistrict();
    }

    public void GenerateDistrict()
    {
        ClearGenerated();
        GenerateGround();
        GenerateStreet();
        GenerateSidewalks();
        GenerateBuildings();
        GenerateSchool();
        GenerateBenches();
        GenerateTrashBins();
        PlaceCharacters();
    }

    public void ClearGenerated()
    {
        foreach (GameObject obj in generatedObjects)
        {
            if (obj != null)
            {
                Destroy(obj);
            }
        }
        generatedObjects.Clear();
    }

    private void GenerateGround()
    {
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ground.name = "Ground";
        ground.transform.parent = transform;
        ground.transform.localPosition = new Vector3(0f, -0.05f, streetLength * 0.5f);
        ground.transform.localScale = new Vector3(200f, 0.1f, 200f);
        ground.isStatic = true;

        Renderer groundRenderer = ground.GetComponent<Renderer>();
        if (groundRenderer != null)
        {
            groundRenderer.material.color = new Color(0.35f, 0.42f, 0.25f);
        }
        generatedObjects.Add(ground);
    }

    private void GenerateStreet()
    {
        GameObject road = GameObject.CreatePrimitive(PrimitiveType.Cube);
        road.name = "Road";
        road.transform.parent = transform;
        road.transform.localPosition = new Vector3(0f, 0.01f, streetLength * 0.5f);
        road.transform.localScale = new Vector3(streetWidth, 0.05f, streetLength);
        road.isStatic = true;

        Renderer roadRenderer = road.GetComponent<Renderer>();
        if (roadRenderer != null)
        {
            if (roadMaterial != null)
                roadRenderer.material = roadMaterial;
            else
                roadRenderer.material.color = new Color(0.25f, 0.25f, 0.28f);
        }
        generatedObjects.Add(road);

        // Dashed center line
        float dashLength = 3f;
        float dashGap = 2f;
        float z = 0f;
        while (z < streetLength)
        {
            GameObject dash = GameObject.CreatePrimitive(PrimitiveType.Cube);
            dash.name = "RoadDash";
            dash.transform.parent = transform;
            dash.transform.localPosition = new Vector3(0f, 0.02f, z + dashLength * 0.5f);
            dash.transform.localScale = new Vector3(0.2f, 0.06f, dashLength);
            dash.isStatic = true;

            Renderer dashRenderer = dash.GetComponent<Renderer>();
            if (dashRenderer != null)
            {
                dashRenderer.material.color = Color.white;
            }

            Collider dashCollider = dash.GetComponent<Collider>();
            if (dashCollider != null) Destroy(dashCollider);

            generatedObjects.Add(dash);
            z += dashLength + dashGap;
        }
    }

    private void GenerateSidewalks()
    {
        float leftX = -(streetWidth * 0.5f + sidewalkWidth * 0.5f);
        float rightX = streetWidth * 0.5f + sidewalkWidth * 0.5f;

        for (int side = 0; side < 2; side++)
        {
            float x = (side == 0) ? leftX : rightX;
            GameObject sidewalk = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sidewalk.name = "Sidewalk_" + (side == 0 ? "Left" : "Right");
            sidewalk.transform.parent = transform;
            sidewalk.transform.localPosition = new Vector3(x, 0.06f, streetLength * 0.5f);
            sidewalk.transform.localScale = new Vector3(sidewalkWidth, 0.12f, streetLength);
            sidewalk.isStatic = true;

            Renderer renderer = sidewalk.GetComponent<Renderer>();
            if (renderer != null)
            {
                if (sidewalkMaterial != null)
                    renderer.material = sidewalkMaterial;
                else
                    renderer.material.color = new Color(0.6f, 0.58f, 0.55f);
            }
            generatedObjects.Add(sidewalk);
        }
    }

    private void GenerateBuildings()
    {
        float startZ = 5f;
        float leftX = -(streetWidth * 0.5f + sidewalkWidth + buildingDepth * 0.5f);
        float rightX = streetWidth * 0.5f + sidewalkWidth + buildingDepth * 0.5f;

        for (int i = 0; i < buildingsPerSide; i++)
        {
            float z = startZ + i * (buildingWidth + buildingSpacing) + buildingWidth * 0.5f;
            Material wallMat = (i % 2 == 0) ? panelkaWallMaterial1 : panelkaWallMaterial2;

            CreatePanelkaBuilding("Panelka_L" + i, new Vector3(leftX, 0f, z), wallMat, true);
            CreatePanelkaBuilding("Panelka_R" + i, new Vector3(rightX, 0f, z), wallMat, false);
        }
    }

    private void CreatePanelkaBuilding(string name, Vector3 position, Material wallMaterial, bool faceRight)
    {
        GameObject building = new GameObject(name);
        building.transform.parent = transform;
        building.transform.position = position;
        generatedObjects.Add(building);

        // Main body
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
        body.name = name + "_Body";
        body.transform.parent = building.transform;
        body.transform.localPosition = new Vector3(0f, buildingHeight * 0.5f, 0f);
        body.transform.localScale = new Vector3(buildingDepth, buildingHeight, buildingWidth);
        body.isStatic = true;

        Renderer bodyRenderer = body.GetComponent<Renderer>();
        if (bodyRenderer != null)
        {
            if (wallMaterial != null)
            {
                bodyRenderer.material = wallMaterial;
                bodyRenderer.material.mainTextureScale = new Vector2(2f, floorsPerBuilding);
            }
            else
            {
                bodyRenderer.material.color = new Color(0.75f, 0.72f, 0.68f);
            }
        }

        // Roof
        GameObject roof = GameObject.CreatePrimitive(PrimitiveType.Cube);
        roof.name = name + "_Roof";
        roof.transform.parent = building.transform;
        roof.transform.localPosition = new Vector3(0f, buildingHeight + 0.25f, 0f);
        roof.transform.localScale = new Vector3(buildingDepth + 0.5f, 0.5f, buildingWidth + 0.5f);
        roof.isStatic = true;

        Renderer roofRenderer = roof.GetComponent<Renderer>();
        if (roofRenderer != null)
        {
            if (roofMaterial != null)
                roofRenderer.material = roofMaterial;
            else
                roofRenderer.material.color = new Color(0.4f, 0.38f, 0.35f);
        }

        // Windows
        float floorHeight = buildingHeight / floorsPerBuilding;
        int windowsPerFloor = Mathf.FloorToInt(buildingWidth / 3f);

        for (int floor = 0; floor < floorsPerBuilding; floor++)
        {
            for (int w = 0; w < windowsPerFloor; w++)
            {
                float wy = (floor + 0.5f) * floorHeight + 0.5f;
                float wz = -buildingWidth * 0.5f + (w + 0.5f) * (buildingWidth / windowsPerFloor);
                float facingX = faceRight ? buildingDepth * 0.5f + 0.01f : -buildingDepth * 0.5f - 0.01f;

                GameObject window = GameObject.CreatePrimitive(PrimitiveType.Cube);
                window.name = name + "_Win_" + floor + "_" + w;
                window.transform.parent = building.transform;
                window.transform.localPosition = new Vector3(facingX, wy, wz);
                window.transform.localScale = new Vector3(0.1f, 1.8f, 1.5f);
                window.isStatic = true;

                Renderer windowRenderer = window.GetComponent<Renderer>();
                if (windowRenderer != null)
                {
                    windowRenderer.material.color = new Color(0.6f, 0.75f, 0.85f, 0.8f);
                }

                Collider windowCollider = window.GetComponent<Collider>();
                if (windowCollider != null) Destroy(windowCollider);
            }
        }

        // Entrance
        float entranceX = faceRight ? buildingDepth * 0.5f + 0.02f : -buildingDepth * 0.5f - 0.02f;
        GameObject entrance = GameObject.CreatePrimitive(PrimitiveType.Cube);
        entrance.name = name + "_Entrance";
        entrance.transform.parent = building.transform;
        entrance.transform.localPosition = new Vector3(entranceX, 1.5f, 0f);
        entrance.transform.localScale = new Vector3(0.15f, 3f, 2f);
        entrance.isStatic = true;

        Renderer entranceRenderer = entrance.GetComponent<Renderer>();
        if (entranceRenderer != null)
        {
            entranceRenderer.material.color = new Color(0.3f, 0.2f, 0.15f);
        }
    }

    private void GenerateSchool()
    {
        GameObject school = new GameObject("School");
        school.transform.parent = transform;
        school.transform.position = schoolPosition;
        generatedObjects.Add(school);

        // Main school building
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
        body.name = "School_Body";
        body.transform.parent = school.transform;
        body.transform.localPosition = new Vector3(0f, schoolHeight * 0.5f, 0f);
        body.transform.localScale = new Vector3(schoolWidth, schoolHeight, schoolDepth);
        body.isStatic = true;

        Renderer bodyRenderer = body.GetComponent<Renderer>();
        if (bodyRenderer != null)
        {
            if (schoolMaterial != null)
                bodyRenderer.material = schoolMaterial;
            else
                bodyRenderer.material.color = new Color(0.85f, 0.82f, 0.75f);
        }

        // School entrance trigger zone for BakhilyBarrier
        GameObject entranceZone = new GameObject("SchoolEntrance");
        entranceZone.transform.parent = school.transform;
        entranceZone.transform.localPosition = new Vector3(0f, 1.5f, -schoolDepth * 0.5f - 1f);

        BoxCollider triggerCollider = entranceZone.AddComponent<BoxCollider>();
        triggerCollider.size = new Vector3(4f, 3f, 2f);
        triggerCollider.isTrigger = true;

        entranceZone.AddComponent<BakhilyBarrier>();

        // School door
        GameObject door = GameObject.CreatePrimitive(PrimitiveType.Cube);
        door.name = "School_Door";
        door.transform.parent = school.transform;
        door.transform.localPosition = new Vector3(0f, 1.5f, -schoolDepth * 0.5f - 0.01f);
        door.transform.localScale = new Vector3(3f, 3f, 0.2f);
        door.isStatic = true;

        Renderer doorRenderer = door.GetComponent<Renderer>();
        if (doorRenderer != null)
        {
            doorRenderer.material.color = new Color(0.4f, 0.25f, 0.15f);
        }

        // School sign
        GameObject sign = GameObject.CreatePrimitive(PrimitiveType.Cube);
        sign.name = "School_Sign";
        sign.transform.parent = school.transform;
        sign.transform.localPosition = new Vector3(0f, schoolHeight - 1f, -schoolDepth * 0.5f - 0.1f);
        sign.transform.localScale = new Vector3(8f, 1.5f, 0.1f);
        sign.isStatic = true;

        Renderer signRenderer = sign.GetComponent<Renderer>();
        if (signRenderer != null)
        {
            signRenderer.material.color = new Color(0.2f, 0.4f, 0.7f);
        }
    }

    private void GenerateBenches()
    {
        float sidewalkCenterLeft = -(streetWidth * 0.5f + sidewalkWidth * 0.5f);
        float sidewalkCenterRight = streetWidth * 0.5f + sidewalkWidth * 0.5f;

        for (int i = 0; i < benchCount; i++)
        {
            float z = (i + 1) * (streetLength / (benchCount + 1));
            float x = (i % 2 == 0) ? sidewalkCenterLeft : sidewalkCenterRight;
            CreateBench("Bench_" + i, new Vector3(x, 0.12f, z));
        }
    }

    private void CreateBench(string name, Vector3 position)
    {
        GameObject bench = new GameObject(name);
        bench.transform.parent = transform;
        bench.transform.position = position;
        generatedObjects.Add(bench);

        // Seat
        GameObject seat = GameObject.CreatePrimitive(PrimitiveType.Cube);
        seat.name = name + "_Seat";
        seat.transform.parent = bench.transform;
        seat.transform.localPosition = new Vector3(0f, 0.45f, 0f);
        seat.transform.localScale = new Vector3(1.8f, 0.08f, 0.5f);
        seat.isStatic = true;

        Renderer seatRenderer = seat.GetComponent<Renderer>();
        if (seatRenderer != null)
        {
            if (benchMaterial != null) seatRenderer.material = benchMaterial;
            else seatRenderer.material.color = new Color(0.45f, 0.3f, 0.15f);
        }

        // Backrest
        GameObject backrest = GameObject.CreatePrimitive(PrimitiveType.Cube);
        backrest.name = name + "_Backrest";
        backrest.transform.parent = bench.transform;
        backrest.transform.localPosition = new Vector3(0f, 0.7f, -0.2f);
        backrest.transform.localScale = new Vector3(1.8f, 0.5f, 0.06f);
        backrest.isStatic = true;

        Renderer backRenderer = backrest.GetComponent<Renderer>();
        if (backRenderer != null)
        {
            if (benchMaterial != null) backRenderer.material = benchMaterial;
            else backRenderer.material.color = new Color(0.45f, 0.3f, 0.15f);
        }

        // Legs
        Vector3[] legPositions = new Vector3[]
        {
            new Vector3(-0.75f, 0.22f, 0.15f),
            new Vector3(0.75f, 0.22f, 0.15f),
            new Vector3(-0.75f, 0.22f, -0.15f),
            new Vector3(0.75f, 0.22f, -0.15f)
        };

        for (int i = 0; i < legPositions.Length; i++)
        {
            GameObject leg = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leg.name = name + "_Leg_" + i;
            leg.transform.parent = bench.transform;
            leg.transform.localPosition = legPositions[i];
            leg.transform.localScale = new Vector3(0.06f, 0.44f, 0.06f);
            leg.isStatic = true;

            Renderer legRenderer = leg.GetComponent<Renderer>();
            if (legRenderer != null) legRenderer.material.color = new Color(0.2f, 0.2f, 0.2f);
        }
    }

    private void GenerateTrashBins()
    {
        float edgeLeft = -(streetWidth * 0.5f + sidewalkWidth * 0.75f);
        float edgeRight = streetWidth * 0.5f + sidewalkWidth * 0.75f;

        for (int i = 0; i < trashBinCount; i++)
        {
            float z = (i + 0.5f) * (streetLength / trashBinCount);
            float x = (i % 2 == 0) ? edgeLeft : edgeRight;
            CreateTrashBin("TrashBin_" + i, new Vector3(x, 0.12f, z));
        }
    }

    private void CreateTrashBin(string name, Vector3 position)
    {
        GameObject bin = new GameObject(name);
        bin.transform.parent = transform;
        bin.transform.position = position;
        generatedObjects.Add(bin);

        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        body.name = name + "_Body";
        body.transform.parent = bin.transform;
        body.transform.localPosition = new Vector3(0f, 0.4f, 0f);
        body.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
        body.isStatic = true;

        Renderer bodyRenderer = body.GetComponent<Renderer>();
        if (bodyRenderer != null)
        {
            if (trashBinMaterial != null) bodyRenderer.material = trashBinMaterial;
            else bodyRenderer.material.color = new Color(0.3f, 0.35f, 0.3f);
        }

        GameObject post = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        post.name = name + "_Post";
        post.transform.parent = bin.transform;
        post.transform.localPosition = new Vector3(0f, 0.15f, 0f);
        post.transform.localScale = new Vector3(0.06f, 0.15f, 0.06f);
        post.isStatic = true;

        Renderer postRenderer = post.GetComponent<Renderer>();
        if (postRenderer != null) postRenderer.material.color = new Color(0.25f, 0.25f, 0.25f);
    }

    private void PlaceCharacters()
    {
        float leftSidewalk = -(streetWidth * 0.5f + sidewalkWidth * 0.5f);
        float rightSidewalk = streetWidth * 0.5f + sidewalkWidth * 0.5f;

        // Vitaliy at a laptop, sitting near a bench
        if (vitaliyPrefab != null)
        {
            GameObject vitaliy = Instantiate(vitaliyPrefab, new Vector3(leftSidewalk, 0.12f, streetLength * 0.4f), Quaternion.identity, transform);
            vitaliy.name = "Vitaliy";
            generatedObjects.Add(vitaliy);
        }
        else
        {
            CreatePlaceholderCharacter("Vitaliy", new Vector3(leftSidewalk, 0.12f, streetLength * 0.4f), new Color(0.3f, 0.5f, 0.7f));
            CreateLaptop(new Vector3(leftSidewalk + 0.5f, 0.55f, streetLength * 0.4f));
        }

        // Kirill near camera equipment
        if (kirillPrefab != null)
        {
            GameObject kirill = Instantiate(kirillPrefab, new Vector3(rightSidewalk, 0.12f, streetLength * 0.35f), Quaternion.identity, transform);
            kirill.name = "Kirill";
            generatedObjects.Add(kirill);
        }
        else
        {
            CreatePlaceholderCharacter("Kirill", new Vector3(rightSidewalk, 0.12f, streetLength * 0.35f), new Color(0.6f, 0.4f, 0.3f));
            CreateCameraEquipment(new Vector3(rightSidewalk + 0.8f, 0.12f, streetLength * 0.35f));
        }

        // Uliana with a suitcase
        if (ulianaPrefab != null)
        {
            GameObject uliana = Instantiate(ulianaPrefab, new Vector3(leftSidewalk + 0.5f, 0.12f, streetLength * 0.6f), Quaternion.identity, transform);
            uliana.name = "Uliana";
            generatedObjects.Add(uliana);
        }
        else
        {
            CreatePlaceholderCharacter("Uliana", new Vector3(leftSidewalk + 0.5f, 0.12f, streetLength * 0.6f), new Color(0.8f, 0.5f, 0.6f));
            CreateSuitcase(new Vector3(leftSidewalk + 1.1f, 0.12f, streetLength * 0.6f));
        }

        // Zavkhoz blocking the school entrance
        Vector3 zavkhozPos = schoolPosition + new Vector3(0f, 0.12f, -schoolDepth * 0.5f - 1.5f);
        if (zavkhozPrefab != null)
        {
            GameObject zavkhoz = Instantiate(zavkhozPrefab, zavkhozPos, Quaternion.identity, transform);
            zavkhoz.name = "Zavkhoz";
            generatedObjects.Add(zavkhoz);
        }
        else
        {
            GameObject zavkhoz = CreatePlaceholderCharacter("Zavkhoz", zavkhozPos, new Color(0.3f, 0.4f, 0.7f));
            zavkhoz.tag = "NPC";
            if (zavkhoz.GetComponent<Animator>() == null)
                zavkhoz.AddComponent<Animator>();
        }
    }

    private GameObject CreatePlaceholderCharacter(string name, Vector3 position, Color clothingColor)
    {
        GameObject character = new GameObject(name);
        character.transform.parent = transform;
        character.transform.position = position;
        generatedObjects.Add(character);

        // Torso
        GameObject torso = GameObject.CreatePrimitive(PrimitiveType.Cube);
        torso.name = name + "_Torso";
        torso.transform.parent = character.transform;
        torso.transform.localPosition = new Vector3(0f, 1.1f, 0f);
        torso.transform.localScale = new Vector3(0.5f, 0.7f, 0.3f);
        SetColor(torso, clothingColor);

        // Head
        GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        head.name = name + "_Head";
        head.transform.parent = character.transform;
        head.transform.localPosition = new Vector3(0f, 1.7f, 0f);
        head.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
        SetColor(head, new Color(0.9f, 0.75f, 0.65f));

        // Legs
        for (int i = 0; i < 2; i++)
        {
            GameObject leg = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leg.name = name + "_Leg_" + i;
            leg.transform.parent = character.transform;
            leg.transform.localPosition = new Vector3((i == 0) ? -0.12f : 0.12f, 0.4f, 0f);
            leg.transform.localScale = new Vector3(0.18f, 0.7f, 0.22f);
            SetColor(leg, new Color(0.2f, 0.2f, 0.3f));
        }

        // Arms
        for (int i = 0; i < 2; i++)
        {
            GameObject arm = GameObject.CreatePrimitive(PrimitiveType.Cube);
            arm.name = name + "_Arm_" + i;
            arm.transform.parent = character.transform;
            arm.transform.localPosition = new Vector3((i == 0) ? -0.35f : 0.35f, 1.1f, 0f);
            arm.transform.localScale = new Vector3(0.15f, 0.6f, 0.18f);
            SetColor(arm, clothingColor);
        }

        CapsuleCollider capsule = character.AddComponent<CapsuleCollider>();
        capsule.center = new Vector3(0f, 1f, 0f);
        capsule.height = 2f;
        capsule.radius = 0.3f;

        return character;
    }

    private void SetColor(GameObject obj, Color color)
    {
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = color;
        }
    }

    private void CreateLaptop(Vector3 position)
    {
        GameObject laptop = new GameObject("Laptop");
        laptop.transform.parent = transform;
        laptop.transform.position = position;
        generatedObjects.Add(laptop);

        GameObject laptopBase = GameObject.CreatePrimitive(PrimitiveType.Cube);
        laptopBase.name = "Laptop_Base";
        laptopBase.transform.parent = laptop.transform;
        laptopBase.transform.localPosition = Vector3.zero;
        laptopBase.transform.localScale = new Vector3(0.35f, 0.02f, 0.25f);
        SetColor(laptopBase, new Color(0.15f, 0.15f, 0.15f));

        GameObject screen = GameObject.CreatePrimitive(PrimitiveType.Cube);
        screen.name = "Laptop_Screen";
        screen.transform.parent = laptop.transform;
        screen.transform.localPosition = new Vector3(0f, 0.12f, -0.11f);
        screen.transform.localRotation = Quaternion.Euler(-70f, 0f, 0f);
        screen.transform.localScale = new Vector3(0.33f, 0.22f, 0.01f);
        SetColor(screen, new Color(0.4f, 0.6f, 0.9f));
    }

    private void CreateCameraEquipment(Vector3 position)
    {
        GameObject equipment = new GameObject("CameraEquipment");
        equipment.transform.parent = transform;
        equipment.transform.position = position;
        generatedObjects.Add(equipment);

        for (int i = 0; i < 3; i++)
        {
            GameObject leg = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            leg.name = "Tripod_Leg_" + i;
            leg.transform.parent = equipment.transform;
            float angle = i * 120f * Mathf.Deg2Rad;
            leg.transform.localPosition = new Vector3(Mathf.Sin(angle) * 0.2f, 0.5f, Mathf.Cos(angle) * 0.2f);
            leg.transform.localRotation = Quaternion.Euler(Mathf.Cos(angle) * 15f, 0f, Mathf.Sin(angle) * 15f);
            leg.transform.localScale = new Vector3(0.03f, 0.5f, 0.03f);
            SetColor(leg, new Color(0.15f, 0.15f, 0.15f));
        }

        GameObject cameraBody = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cameraBody.name = "Camera_Body";
        cameraBody.transform.parent = equipment.transform;
        cameraBody.transform.localPosition = new Vector3(0f, 1.1f, 0f);
        cameraBody.transform.localScale = new Vector3(0.2f, 0.12f, 0.15f);
        SetColor(cameraBody, new Color(0.1f, 0.1f, 0.1f));

        GameObject lens = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        lens.name = "Camera_Lens";
        lens.transform.parent = equipment.transform;
        lens.transform.localPosition = new Vector3(0f, 1.1f, -0.12f);
        lens.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        lens.transform.localScale = new Vector3(0.06f, 0.08f, 0.06f);
        SetColor(lens, new Color(0.05f, 0.05f, 0.05f));
    }

    private void CreateSuitcase(Vector3 position)
    {
        GameObject suitcase = GameObject.CreatePrimitive(PrimitiveType.Cube);
        suitcase.name = "Suitcase";
        suitcase.transform.parent = transform;
        suitcase.transform.position = position;
        suitcase.transform.localScale = new Vector3(0.45f, 0.6f, 0.2f);
        SetColor(suitcase, new Color(0.5f, 0.15f, 0.15f));
        generatedObjects.Add(suitcase);

        GameObject handle = GameObject.CreatePrimitive(PrimitiveType.Cube);
        handle.name = "Suitcase_Handle";
        handle.transform.parent = suitcase.transform;
        handle.transform.localPosition = new Vector3(0f, 0.55f, 0f);
        handle.transform.localScale = new Vector3(0.4f, 0.08f, 0.5f);
        SetColor(handle, new Color(0.3f, 0.1f, 0.1f));
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using TMPro;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine.UI;

public class MapManager : MonoBehaviour
{
    private readonly Dictionary<string, GameObject> regions = new();
    private readonly Dictionary<string, int> regionTroops = new();
    private readonly Dictionary<string, string> regionOwners = new();
    private readonly Dictionary<GameObject, Material> ownerMaterials = new();
    private readonly Dictionary<string, Material> continentMaterials = new();
    private readonly Dictionary<string, GameObject> hiddenTroopTexts = new();

    public Material saMaterial;
    public Material naMaterial;
    public Material afMaterial;
    public Material euMaterial;
    public Material asMaterial;
    public Material auMaterial;

    private GameObject currentlySelectedRegion;
    private GameObject previouslySelectedSecondRegion;

    public GameObject winRateUI;
    public TMP_Text winRateText;
    public GameObject rollDiceButton;
    public GameObject continentToggleButton;

    public GameObject troopsTransferUI;
    public TMP_Text takviyeAmountText;

    public GameObject troopsTakviyeUI;
    public TMP_Text transferAmountText;
    private int maxTransferableTroops = 0;
    private int currentTransferCount = 1;

    public GameObject loadingSpinner;

    public GameObject PhaseOneContainer;
    public TMP_Text addTroopsInfoText;
    public GameObject addTroopsButton;

    public Material selectedMaterial;
    public Material redMaterial;
    public Material greenMaterial;
    public Material blueMaterial;

    public Material attackerMaterial; // turuncu
    public Material defenderMaterial; // sarı

    // Game Winner UI
    public GameObject winnerPanel;
    public TMP_Text winnerText;

    private DatabaseReference dbReference;
    private Vector3 defaultScale = Vector3.one;

    public int GlobalPhase;
    public int GlobalTurn;
    private bool isHighlighting = false;
    private bool isTransferingAfterAttack = false;
    private bool isContinentViewActive = false;

    private Vector3 as8OriginalScale = Vector3.zero;

    void Start()
    {
        StartCoroutine(FindMapElementsCoroutine());
        InitializeFirebase();
    }

    private IEnumerator FindMapElementsCoroutine()
    {
        float searchTime = 0f;
        float maxSearchTime = 30f;

        while (searchTime < maxSearchTime)
        {
            GameObject[] regionObjects = GameObject.FindGameObjectsWithTag("Region");

            if (regionObjects.Length > 0)
            {
                foreach (GameObject region in regionObjects)
                {
                    if (!regions.ContainsKey(region.name))
                    {
                        regions[region.name] = region;
                    }
                }

                Debug.Log($"Bütün bölgeler yüklendi: {regions.Count} bölge bulundu.");
                yield return null;

                LoadRegionOwnersFromFirebase();
                yield break;
            }

            searchTime += 1f;
            yield return new WaitForSeconds(1f);
        }

        Debug.LogWarning("Bölgeler zamanında yüklenemedi!");
    }

    private void InitializeFirebase()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                dbReference = FirebaseDatabase.DefaultInstance.RootReference;
                ListenForSelectionChanges();
                ListenForSecondSelection();
                ListenForWinRate();
                ListenForTroopsChanges();
                ListenForOwnerChanges();
                ListenForPhaseOneAskerCount();
                ListenForWinner();
            }
            else
            {
                Debug.LogError("Firebase dependency error: " + task.Result);
            }
        });
    }

    public void LoadRegionOwnersFromFirebase()
    {
        FirebaseDatabase.DefaultInstance
            .GetReference("Games/Room1/Map")
            .GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && task.Result.Exists)
            {
                DataSnapshot snapshot = task.Result;

                foreach (DataSnapshot regionSnap in snapshot.Children)
                {
                    string regionName = regionSnap.Key;
                    string owner = regionSnap.Child("Owner").Value?.ToString();
                    regionOwners[regionName] = owner;
                    int troops = int.Parse(regionSnap.Child("Troops").Value?.ToString() ?? "0");

                    regionTroops[regionName] = troops;

                    if (regions.TryGetValue(regionName, out GameObject regionObj))
                    {
                        if (regionObj.TryGetComponent<Renderer>(out var renderer))
                        {
                            Material mat = GetMaterialByOwner(owner);
                            renderer.material = mat;
                            ownerMaterials[regionObj] = mat;
                            if (regionName == "AS8" && as8OriginalScale == Vector3.zero)
                            {
                                as8OriginalScale = regionObj.transform.localScale;
                                Debug.Log("AS8 orijinal scale kaydedildi: " + as8OriginalScale);
                            }

                            if (regionName == "AS8")
                            {
                                regionObj.transform.localScale = as8OriginalScale;
                            }
                            else
                            {
                                regionObj.transform.localScale = defaultScale;
                            }
                        }
                    }

                    UpdateTroopsText(regionName, troops);
                    StartCoroutine(WaitAndUpdateTroopUI());
                }
            }
            else
            {
                Debug.LogError("Bölge sahipleri alınamadı.");
            }
        });
    }

    private void UpdateTroopsText(string regionName, int troops)
    {
        // Her bölgenin 3D TextMeshPro nesnesini isminin sonuna "T" ekleyerek buluyoruz
        string textObjectName = regionName + "T";
        GameObject textObject = GameObject.Find(textObjectName);

        if (textObject != null)
        {
            // TextMeshPro bileşenini buluyoruz ve asker sayısını yazdırıyoruz
            if (textObject.TryGetComponent<TextMeshPro>(out var textMesh))
            {
                textMesh.text = troops.ToString(); // Asker sayısını güncelliyoruz
            }
            else
            {
                Debug.LogWarning($"TextMeshPro bileşeni bulunamadı: {textObjectName}");
            }
        }
        else
        {
            Debug.LogWarning($"Asker sayısı için 3D TextMeshPro nesnesi bulunamadı: {"regionName:" + regionName + " textObjectName: " + textObjectName}");
        }
    }

    private Material GetMaterialByOwner(string owner)
    {
        return owner switch
        {
            "Player1" => blueMaterial,
            "Player2" => greenMaterial,
            "Player3" => redMaterial,
            _ => null
        };
    }

    private void ListenForSelectionChanges()
    {
        FirebaseDatabase.DefaultInstance
            .GetReference("Games/Room1/Action/First")
            .ValueChanged += HandleFirstValueChanged;
    }

    private void ListenForWinner()
    {
        FirebaseDatabase.DefaultInstance
            .GetReference("Games/Room1/Action/Winner")
            .ValueChanged += async (sender, args) =>
            {
                if (args.Snapshot.Exists)
                {
                    string winner = args.Snapshot.Value.ToString();
                    if (winner == "-")
                        return;

                    await ShowWinnerUIAsync(winner);
                }
            };
    }

    private void ListenForPhaseOneAskerCount()
    {
        FirebaseDatabase.DefaultInstance
            .GetReference("Games/Room1/Action/PhaseOneAskerCount")
            .ValueChanged += async (sender, args) =>
        {
            if (args.DatabaseError != null)
            {
                Debug.LogError("PhaseOneAskerCount dinlenemedi: " + args.DatabaseError.Message);
                return;
            }

            if (args.Snapshot.Exists && int.TryParse(args.Snapshot.Value.ToString(), out int count) && count > 0)
            {
                UpdatePhaseOneInfoText(count);
                if (PhaseOneContainer != null && addTroopsButton != null)
                {
                    PhaseOneContainer.SetActive(true);
                    addTroopsButton.SetActive(false);
                }

                await GetValues();
                if (GameManager.PlayerOneText == null ||
                    GameManager.PlayerTwoText == null ||
                    GameManager.PlayerThreeText == null)
                {
                    Debug.LogWarning("GameManager player text'leri hazır değil. PhaseOneAskerCount UI atlandı.");
                    return;
                }

                if (GlobalTurn != 0 && IsCurrentUsersTurn(GlobalTurn))
                {
                    addTroopsButton.SetActive(true);
                }
            }
            else
            {
                if (PhaseOneContainer != null)
                    PhaseOneContainer.SetActive(false);
            }
        };
    }

    private void ListenForSecondSelection()
    {
        FirebaseDatabase.DefaultInstance
            .GetReference("Games/Room1/Action/Second")
            .ValueChanged += HandleSecondValueChanged;
    }

    private void ListenForWinRate()
    {
        FirebaseDatabase.DefaultInstance
            .GetReference("Games/Room1/Action/WinRate")
            .ValueChanged += HandleWinRateChanged;
    }

    private void ListenForTroopsChanges()
    {
        FirebaseDatabase.DefaultInstance
            .GetReference("Games/Room1/Map")
            .ChildChanged += HandleTroopsChange;
    }

    private void ListenForOwnerChanges()
    {
        FirebaseDatabase.DefaultInstance
            .GetReference("Games/Room1/Map")
            .ChildChanged += HandleOwnerChange;
    }

    private void HandleOwnerChange(object sender, ChildChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError("Firebase read error: " + args.DatabaseError.Message);
            return;
        }

        if (!args.Snapshot.Exists || !args.Snapshot.HasChild("Owner"))
        {
            // Owner alanı yoksa bu değişiklik Owner değişikliği değildir
            return;
        }

        string regionName = args.Snapshot.Key;
        string newOwner = args.Snapshot.Child("Owner").Value.ToString();

        regionOwners[regionName] = newOwner;

        if (regions.TryGetValue(regionName, out GameObject regionObj))
        {
            Material newMaterial = GetMaterialByOwner(newOwner);
            if (regionObj.TryGetComponent<Renderer>(out var renderer))
            {
                renderer.material = newMaterial;
            }
            ownerMaterials[regionObj] = newMaterial;
        }

        UpdateAllPlayersTotalTroopsUI();
        CheckForWinner();

        Debug.Log($"Owner değişti: {regionName} - Yeni owner: {newOwner}");
    }

    private void HandleTroopsChange(object sender, ChildChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError("Firebase read error: " + args.DatabaseError.Message);
            return;
        }

        if (args.Snapshot.Exists)
        {
            string regionName = args.Snapshot.Key;
            int updatedTroops = int.Parse(args.Snapshot.Child("Troops").Value.ToString());

            // Asker sayısını güncelle
            UpdateTroopsText(regionName, updatedTroops);
            UpdateAllPlayersTotalTroopsUI();

            // Diğer işlemleri yap
            Debug.Log($"Troops sayısı güncellendi: {regionName} - Yeni sayısı: {updatedTroops}");
        }
    }

    private async void HandleWinRateChanged(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError("Firebase read error: " + args.DatabaseError.Message);
            return;
        }

        if (!args.Snapshot.Exists)
        {
            CloseWinRateUI();
            return;
        }

        string winRateStr = args.Snapshot.Value.ToString();
        if (winRateStr == "0")
        {
            CloseWinRateUI();
            return;
        }

        if (int.TryParse(winRateStr, out int winRate))
        {
            await GetValues();

            var firstSnap = await dbReference.Child("Games/Room1/Action/First").GetValueAsync();
            var secondSnap = await dbReference.Child("Games/Room1/Action/Second").GetValueAsync();

            string firstVal = firstSnap.Value?.ToString() ?? "-";
            string secondVal = secondSnap.Value?.ToString() ?? "-";

            if (GlobalPhase == 2 && firstVal != "-" && secondVal != "-")
            {
                ShowWinRateUI(winRate);
            }
            else
            {
                CloseWinRateUI();
            }
        }
        else
        {
            CloseWinRateUI(); // Sayısal değilse kapat
        }
    }

    private void HandleFirstValueChanged(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError("Firebase read error: " + args.DatabaseError.Message);
            return;
        }

        string newFirstRegionName = args.Snapshot.Value?.ToString();

        // Eğer değer boşsa veya "-" ise seçimi kaldır
        if (string.IsNullOrEmpty(newFirstRegionName) || newFirstRegionName == "-")
        {
            if (currentlySelectedRegion != null)
            {
                DeselectRegion(currentlySelectedRegion);
                currentlySelectedRegion = null;
            }
            return;
        }

        // Geçerli bir bölge varsa highlight işlemi yap
        if (regions.TryGetValue(newFirstRegionName, out GameObject selectedRegion))
        {
            ShowOtherPlayersHighlight(selectedRegion);
            currentlySelectedRegion = selectedRegion; // seçimi güncelle
        }
    }

    private void HandleSecondValueChanged(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError("Firebase read error: " + args.DatabaseError.Message);
            return;
        }

        string newSecondRegionName = args.Snapshot.Value?.ToString();

        // Eğer yeni değer boşsa ya da "-" ise, eski highlight'ı temizle
        if (string.IsNullOrEmpty(newSecondRegionName) || newSecondRegionName == "-")
        {
            if (previouslySelectedSecondRegion != null)
            {
                DeselectRegion(previouslySelectedSecondRegion);
                previouslySelectedSecondRegion = null;
            }
            return;
        }

        // Yeni bölge sahnedeki objelerden bulunur
        if (regions.TryGetValue(newSecondRegionName, out GameObject newRegion))
        {
            // Eski region'ı eski haline döndür
            if (previouslySelectedSecondRegion != null && previouslySelectedSecondRegion != newRegion)
            {
                DeselectRegion(previouslySelectedSecondRegion);
            }

            // Yeni region'ı sarı renkle göster
            HighlightAsDefender(newRegion);
            previouslySelectedSecondRegion = newRegion;

            Debug.Log("Second değişti, yeni sarı bölge: " + newSecondRegionName);
        }
    }

    private void ShowOtherPlayersHighlight(GameObject region)
    {
        if (region == null)
            return;

        if (currentlySelectedRegion != null && currentlySelectedRegion != region)
        {
            DeselectRegion(currentlySelectedRegion);
        }

        if (region.TryGetComponent<Renderer>(out var renderer))
        {
            renderer.material = selectedMaterial;
            if (region.name == "AS8")
            {
                region.transform.localScale = as8OriginalScale + new Vector3(0, 0.1f, 0);
            }
            else
            {
                region.transform.localScale = defaultScale + new Vector3(0, 0, 0.8f);
            }
        }

        currentlySelectedRegion = region;
    }

    void Update()
    {
        HandleInput();
    }

    private void HandleInput()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            HandleRaycast(Mouse.current.position.ReadValue());
        }
        else if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            HandleRaycast(Touchscreen.current.primaryTouch.position.ReadValue());
        }
    }

    private void HandleRaycast(Vector2 screenPosition)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            GameObject clickedRegion = hit.collider.gameObject;

            if (regions.ContainsValue(clickedRegion))
            {
                if (currentlySelectedRegion == clickedRegion && GlobalPhase != 3)
                    return;

                HighlightRegion(clickedRegion, true);
            }
        }
    }

    private bool IsCurrentUsersTurn(int turn)
    {
        string userName = PlayerData.Instance.GetPlayerName();

        string currentPlayerName;
        switch (turn)
        {
            case 1:
                currentPlayerName = GameManager.PlayerOneText.text;
                break;
            case 2:
                currentPlayerName = GameManager.PlayerTwoText.text;
                break;
            case 3:
                currentPlayerName = GameManager.PlayerThreeText.text;
                break;
            default:
                Debug.LogError("Geçersiz Turn değeri! Turn: " + turn);
                return false;
        }

        if (currentPlayerName != userName)
        {
            Debug.Log("Senin sıran değil. " + currentPlayerName + "'s sırası. Sen: " + userName);
            return false;
        }
        else
            return true;

    }

    private async Task GetValues()
    {
        dbReference = FirebaseDatabase.DefaultInstance.RootReference;

        var phaseSnapshot = await dbReference.Child("Games").Child("Room1").Child("Phase").GetValueAsync();
        var turnSnapshot = await dbReference.Child("Games").Child("Room1").Child("Turn").GetValueAsync();

        if (phaseSnapshot.Exists && turnSnapshot.Exists)
        {
            GlobalPhase = int.Parse(phaseSnapshot.Value.ToString());
            GlobalTurn = int.Parse(turnSnapshot.Value.ToString());
        }
        else
        {
            Debug.Log("VERİLER ALINAMADI. TEKRAR DENENİYOR...");
            await GetValues();
        }
    }

    private async void HighlightRegion(GameObject region, bool shouldUpdateFirebase)
    {
        if (isHighlighting)
            return;

        if (isTransferingAfterAttack)
        {
            Debug.Log("Şu anda transfer UI aktif. Haritayla etkileşim kapalı.");
            return;
        }

        isHighlighting = true;
        if (loadingSpinner != null)
            loadingSpinner.SetActive(true);

        await GetValues();
        Debug.Log("HighlightRegion");
        if (!IsCurrentUsersTurn(GlobalTurn))
        {
            if (loadingSpinner != null)
                loadingSpinner.SetActive(false);
            isHighlighting = false;
            return;
        }

        if (GlobalPhase == 1)
        {
            string player = GetActivePlayer();

            string regionName = region.name;
            string regionOwner = await GetRegionOwner(regionName);

            if (regionOwner == player)
            {
                // Seçimi yap
                if (currentlySelectedRegion != null)
                {
                    DeselectRegion(currentlySelectedRegion);
                }

                // Seçilen bölgeyi highlight et
                if (region.TryGetComponent<Renderer>(out var renderer))
                {
                    renderer.material = selectedMaterial;
                    if (region.name == "AS8")
                    {
                        region.transform.localScale = as8OriginalScale + new Vector3(0, 0.1f, 0);
                    }
                    else
                    {
                        region.transform.localScale = defaultScale + new Vector3(0, 0, 0.8f);
                    }
                }

                currentlySelectedRegion = region;

                // Firebase'e yazma işlemi
                if (shouldUpdateFirebase && dbReference != null)
                {
                    await dbReference.Child("Games").Child("Room1").Child("Action").Child("First")
                        .SetValueAsync(region.name);
                }

                int deploymentTroops = CalculateDeploymentTroops(player);
                await dbReference.Child("Games").Child("Room1").Child("Action").Child("PhaseOneAskerCount").SetValueAsync(deploymentTroops);
                UpdatePhaseOneInfoText(deploymentTroops);
                if (PhaseOneContainer != null)
                    PhaseOneContainer.SetActive(true);
            }

        }
        else if (GlobalPhase == 2)
        {
            string player = GetActivePlayer();
            string regionName = region.name;
            string regionOwner = await GetRegionOwner(regionName);

            var actionRef = dbReference.Child("Games").Child("Room1").Child("Action");

            var firstSnapshot = await actionRef.Child("First").GetValueAsync();
            var firstValue = firstSnapshot.Value?.ToString() ?? "-";

            var secondSnapshot = await actionRef.Child("Second").GetValueAsync();
            var secondValue = secondSnapshot.Value?.ToString() ?? "-";

            // Eğer first boşsa (normal ilk seçim gibi)
            if (firstValue == "-")
            {
                if (regionOwner == player)
                {
                    await actionRef.Child("First").SetValueAsync(regionName);
                    HighlightAsAttacker(region);
                    CloseSpinner();
                    return;
                }
                else
                {
                    Debug.Log("Bu bölge sana ait değil, ilk seçim için geçersiz.");
                    CloseSpinner();
                    return;
                }
            }

            // Eğer first doluysa ve tıklanan bölge kullanıcıya aitse
            if (regionOwner == player)
            {
                // Mevcut seçimleri değiştiriyoruz
                await actionRef.Child("First").SetValueAsync(regionName);
                await actionRef.Child("Second").SetValueAsync("-");
                HighlightAsAttacker(region);
                await actionRef.Child("WinRate").SetValueAsync("0");
                await actionRef.Child("PhaseOneAskerCount").SetValueAsync("0");
                CloseWinRateUI();
                CloseSpinner();
                return;
            }

            // Eğer second boşsa (ve buraya kadar geldiyse) savunulan bölge seçiliyor
            if (secondValue == "-" || (firstValue != "-" && secondValue != "-"))
            {
                string attackerRegionName = firstValue;

                if (regionName == attackerRegionName)
                {
                    CloseSpinner();
                    return;
                }

                if (regionOwner == player)
                {
                    Debug.Log("Kendi bölgene saldırı yapamazsın.");
                    CloseSpinner();
                    return;
                }

                if (!MapUtils.Neighbors.TryGetValue(attackerRegionName, out var neighborsList) ||
                    !neighborsList.Contains(regionName))
                {
                    Debug.Log("Bu bölge komşu değil. Saldırı yapılamaz.");
                    CloseSpinner();
                    return;
                }

                await actionRef.Child("Second").SetValueAsync(regionName);
                HighlightAsDefender(region);
                await CalculateWinRate();
                Debug.Log("Saldırmak istediğin düşman bölge seçildi.");
            }
        }
        else if (GlobalPhase == 3)
        {
            string player = GetActivePlayer();
            string regionName = region.name;
            string regionOwner = await GetRegionOwner(regionName);

            var actionRef = dbReference.Child("Games").Child("Room1").Child("Action");

            var firstSnapshot = await actionRef.Child("First").GetValueAsync();
            var secondSnapshot = await actionRef.Child("Second").GetValueAsync();

            string firstValue = firstSnapshot.Value?.ToString() ?? "-";
            string secondValue = secondSnapshot.Value?.ToString() ?? "-";

            Debug.Log($"[DEBUG] FirstValue: {firstValue}, RegionName: {regionName}");
            if (firstValue == regionName)
            {
                await actionRef.Child("First").SetValueAsync("-");
                await actionRef.Child("Second").SetValueAsync("-");
                troopsTakviyeUI.SetActive(false);
                DeselectRegion(currentlySelectedRegion); // Seçimi UI'dan da temizle
                currentlySelectedRegion = null;
                CloseSpinner();
                return;
            }

            if (firstValue == "-")
            {
                if (regionOwner == player)
                {
                    await actionRef.Child("First").SetValueAsync(regionName);
                    HighlightAsAttacker(region); // seçilen kaynak bölge
                    CloseSpinner();
                    return;
                }
                else
                {
                    Debug.Log("Bu bölge sana ait değil.");
                    CloseSpinner();
                    return;
                }
            }

            // Eğer both first ve second doluysa ve başka bir kendi bölgene tıklarsan: first güncelle, second sıfırla
            if (firstValue != "-" && secondValue != "-" && regionOwner == player)
            {
                troopsTakviyeUI.SetActive(false);
                await actionRef.Child("First").SetValueAsync(regionName);
                await actionRef.Child("Second").SetValueAsync("-");
                HighlightAsAttacker(region);
                CloseSpinner();
                return;
            }

            if (secondValue == "-" && firstValue != "-")
            {
                if (regionOwner != player)
                {
                    CloseSpinner();
                    return;
                }

                bool reachable = AreRegionsConnected(firstValue, regionName, player);
                if (!reachable)
                {
                    Debug.Log("Bu iki bölge arasında bağlantı yok.");
                    CloseSpinner();
                    return;
                }

                await actionRef.Child("Second").SetValueAsync(regionName);
                HighlightAsDefender(region);
                ShowTroopsTakviyeUI();
            }
        }

        CloseSpinner();
    }

    private bool AreRegionsConnected(string from, string to, string owner)
    {
        HashSet<string> visited = new();
        Queue<string> queue = new();
        queue.Enqueue(from);
        visited.Add(from);

        while (queue.Count > 0)
        {
            string current = queue.Dequeue();

            if (current == to)
                return true;

            if (!MapUtils.Neighbors.TryGetValue(current, out var neighbors))
                continue;

            foreach (string neighbor in neighbors)
            {
                if (visited.Contains(neighbor))
                    continue;

                if (regionOwners.TryGetValue(neighbor, out string neighborOwner) && neighborOwner == owner)
                {
                    visited.Add(neighbor);
                    queue.Enqueue(neighbor);
                }
            }
        }

        return false;
    }

    public void CloseSpinner()
    {
        if (loadingSpinner != null)
            loadingSpinner.SetActive(false);
        isHighlighting = false;
    }

    public void CloseWinRateUI()
    {
        if (winRateUI != null)
            winRateUI.SetActive(false);

        if (rollDiceButton != null)
            rollDiceButton.SetActive(false);
    }

    public async void AddTroopsToRegion()
    {
        if (isHighlighting)
            return;

        if (currentlySelectedRegion == null)
            return;

        isHighlighting = true;
        if (loadingSpinner != null)
            loadingSpinner.SetActive(true);


        string regionName = currentlySelectedRegion.name;

        var troopsSnapshot = await dbReference.Child("Games").Child("Room1").Child("Map").Child(regionName).Child("Troops").GetValueAsync();
        if (troopsSnapshot.Exists)
        {
            int currentTroops = int.Parse(troopsSnapshot.Value.ToString());
            string owner = await GetRegionOwner(regionName);
            int deploymentTroops = CalculateDeploymentTroops(owner);
            int newTroops = currentTroops + deploymentTroops;

            // Yeni asker sayısını Firebase'e yaz
            await dbReference.Child("Games").Child("Room1").Child("Map").Child(regionName).Child("Troops").SetValueAsync(newTroops);

            // Yeni asker sayısını ekranda güncelle
            UpdateTroopsText(regionName, newTroops);
            regionTroops[regionName] = newTroops;
            UpdateAllPlayersTotalTroopsUI();

            Debug.Log($"{regionName} bölgesine {deploymentTroops} asker eklendi.");
        }

        var phaseSnapshot = await dbReference.Child("Games").Child("Room1").Child("Phase").GetValueAsync();
        if (phaseSnapshot.Exists)
        {
            int currentPhase = int.Parse(phaseSnapshot.Value.ToString());

            // nextPhase hesaplama
            int nextPhase = (currentPhase % 3) + 1;

            // Firebase'e yeni phase'i yaz
            await dbReference.Child("Games").Child("Room1").Child("Phase").SetValueAsync(nextPhase);

            Debug.Log($"Phase güncellendi. Yeni Phase: {nextPhase}");
        }

        var actionRef = dbReference.Child("Games").Child("Room1").Child("Action");
        await actionRef.Child("First").SetValueAsync("-");
        await actionRef.Child("Second").SetValueAsync("-");
        await actionRef.Child("PhaseOneAskerCount").SetValueAsync(0);
        ResetLocalSelections();

        if (PhaseOneContainer != null)
            PhaseOneContainer.SetActive(false);

        if (loadingSpinner != null)
            loadingSpinner.SetActive(false);
        isHighlighting = false;
    }

    private int CalculateDeploymentTroops(string playerName)
    {
        Dictionary<string, List<string>> continents = new()
        {
            { "SA", new() { "SA1", "SA2", "SA3", "SA4" } },
            { "NA", new() { "NA1", "NA2", "NA3", "NA4", "NA5", "NA6", "NA7", "NA8", "NA9" } },
            { "AF", new() { "AF1", "AF2", "AF3", "AF4", "AF5", "AF6" } },
            { "EU", new() { "EU1", "EU2", "EU3", "EU4", "EU5", "EU6", "EU7", "EU8" } },
            { "AS", new() { "AS1", "AS2", "AS3", "AS4", "AS5", "AS6", "AS7", "AS8" } },
            { "AU", new() { "AU1", "AU2", "AU3", "AU4" } }
        };

        int bonus = 0;

        foreach (var entry in continents)
        {
            bool ownsAll = true;
            foreach (string region in entry.Value)
            {
                if (!regionOwners.ContainsKey(region) || regionOwners[region] != playerName)
                {
                    ownsAll = false;
                    break;
                }
            }

            if (ownsAll)
            {
                bonus += entry.Key switch
                {
                    "SA" => 3,
                    "NA" => 5,
                    "AF" => 4,
                    "EU" => 6,
                    "AS" => 8,
                    "AU" => 2,
                    _ => 0
                };
            }
        }

        return 5 + bonus;
    }

    private string GetActivePlayer()
    {
        string userName = PlayerData.Instance.GetPlayerName();
        string thisPlayer = "";

        string playerOne = GameManager.PlayerOneText.text;
        string playerTwo = GameManager.PlayerTwoText.text;
        string playerThree = GameManager.PlayerThreeText.text;

        if (userName == playerOne)
        {
            thisPlayer = "Player1";
        }
        else if (userName == playerTwo)
        {
            thisPlayer = "Player2";
        }
        else if (userName == playerThree)
        {
            thisPlayer = "Player3";
        }

        return thisPlayer;
    }

    private async Task<string> GetRegionOwner(string regionName)
    {
        var snapshot = await dbReference.Child("Games").Child("Room1").Child("Map").Child(regionName).Child("Owner").GetValueAsync();
        if (snapshot.Exists)
        {
            return snapshot.Value.ToString();
        }
        return null;
    }

    private void DeselectRegion(GameObject region)
    {
        if (region == null)
        {
            Debug.Log("[DEBUG] Region is null.");
            return;
        }

        if (region.TryGetComponent<Renderer>(out var renderer))
        {
            if (ownerMaterials.TryGetValue(region, out var originalMat))
            {
                renderer.material = originalMat;

                if (region.name == "AS8")
                {
                    region.transform.localScale = as8OriginalScale;
                }
                else
                {
                    region.transform.localScale = defaultScale;
                }
            }
        }
    }

    private void HighlightAsAttacker(GameObject region)
    {
        if (region.TryGetComponent<Renderer>(out var renderer))
        {
            renderer.material = attackerMaterial;
            if (region.name == "AS8")
            {
                region.transform.localScale = as8OriginalScale + new Vector3(0, 0.1f, 0);
            }
            else
            {
                region.transform.localScale = defaultScale + new Vector3(0, 0, 0.8f);
            }
        }
    }

    private void HighlightAsDefender(GameObject region)
    {
        if (region.TryGetComponent<Renderer>(out var renderer))
        {
            renderer.material = defenderMaterial;
            if (region.name == "AS8")
            {
                region.transform.localScale = as8OriginalScale + new Vector3(0, 0.1f, 0);
            }
            else
            {
                region.transform.localScale = defaultScale + new Vector3(0, 0, 0.8f);
            }
        }
    }

    public async void OnAttackButtonPressed()
    {
        var actionRef = dbReference.Child("Games").Child("Room1").Child("Action");

        string attackerRegion = (await actionRef.Child("First").GetValueAsync()).Value?.ToString();
        string defenderRegion = (await actionRef.Child("Second").GetValueAsync()).Value?.ToString();

        if (attackerRegion == null || defenderRegion == null)
        {
            Debug.LogError("Bölge verileri alınamadı.");
            return;
        }

        var attackerTroopSnapshot = await dbReference.Child("Games").Child("Room1").Child("Map").Child(attackerRegion).Child("Troops").GetValueAsync();
        var defenderTroopSnapshot = await dbReference.Child("Games").Child("Room1").Child("Map").Child(defenderRegion).Child("Troops").GetValueAsync();

        int attackerTroops = int.Parse(attackerTroopSnapshot.Value.ToString());
        int defenderTroops = int.Parse(defenderTroopSnapshot.Value.ToString());

        if (attackerTroops >= defenderTroops * 3)
        {
            Debug.Log("Asker sayısı farkı çok büyük. Saldıran otomatik kazandı.");

            int updatedAttackerTroops = attackerTroops - 1;
            if (updatedAttackerTroops < 1) updatedAttackerTroops = 1;

            regionTroops[attackerRegion] = updatedAttackerTroops;
            regionTroops[defenderRegion] = 0;

            var attackerOwnerSnapshot = await dbReference.Child("Games").Child("Room1").Child("Map").Child(attackerRegion).Child("Owner").GetValueAsync();
            string attackerOwner = attackerOwnerSnapshot.Value.ToString();

            await dbReference.Child("Games").Child("Room1").Child("Map").Child(attackerRegion).Child("Troops").SetValueAsync(updatedAttackerTroops);
            await dbReference.Child("Games").Child("Room1").Child("Map").Child(defenderRegion).Child("Owner").SetValueAsync(attackerOwner);
            await dbReference.Child("Games").Child("Room1").Child("Map").Child(defenderRegion).Child("Troops").SetValueAsync(0);

            regionOwners[defenderRegion] = attackerOwner;

            CheckForWinner();
            var winnerSnapshot = await dbReference.Child("Games").Child("Room1").Child("Action").Child("Winner").GetValueAsync();
            if (winnerSnapshot.Exists && winnerSnapshot.Value.ToString() != "-")
                return;

            ShowTroopTransferSlider();
            UpdateAllPlayersTotalTroopsUI();
            CloseWinRateUI();
            return;
        }

        int attackDice = Mathf.Min(3, attackerTroops - 1);
        int defendDice = Mathf.Min(2, defenderTroops);

        if (attackDice <= 0 || defendDice <= 0)
        {
            Debug.Log("Zar atılamaz. Geçerli asker sayısı yok.");
            return;
        }

        var attackerRolls = RollDice(attackDice);
        var defenderRolls = RollDice(defendDice);

        int attackerLosses = 0;
        int defenderLosses = 0;

        for (int i = 0; i < Mathf.Min(attackDice, defendDice); i++)
        {
            if (defenderRolls[i] >= attackerRolls[i])
                attackerLosses++;
            else
                defenderLosses++;
        }

        Debug.Log($"Zar sonucu → Saldıran: [{string.Join(",", attackerRolls)}] | Savunan: [{string.Join(",", defenderRolls)}]");

        if (defenderLosses > 0)
        {
            Debug.Log("Saldırı başarılı.");

            // Savunucu yenildi, bölge ele geçirilecek
            int updatedAttackerTroops = attackerTroops - attackerLosses;
            if (updatedAttackerTroops < 1)
                updatedAttackerTroops = 1;

            await dbReference.Child("Games").Child("Room1").Child("Map").Child(attackerRegion).Child("Troops").SetValueAsync(updatedAttackerTroops);

            regionTroops[attackerRegion] = updatedAttackerTroops;
            regionTroops[defenderRegion] = 0;

            var attackerOwnerSnapshot = await dbReference.Child("Games").Child("Room1").Child("Map").Child(attackerRegion).Child("Owner").GetValueAsync();
            string attackerOwner = attackerOwnerSnapshot.Value.ToString();

            await dbReference.Child("Games").Child("Room1").Child("Map").Child(defenderRegion).Child("Owner").SetValueAsync(attackerOwner);
            await dbReference.Child("Games").Child("Room1").Child("Map").Child(defenderRegion).Child("Troops").SetValueAsync(0);

            regionOwners[defenderRegion] = attackerOwner; 

            CheckForWinner();
            var winnerSnapshot = await dbReference.Child("Games").Child("Room1").Child("Action").Child("Winner").GetValueAsync();
            if (winnerSnapshot.Exists && winnerSnapshot.Value.ToString() != "-")
                return;

            ShowTroopTransferSlider();
        }
        else
        {
            Debug.Log("Saldırı başarısız. Saldıran kaybetti.");
            int updatedDefenderTroops = defenderTroops - defenderLosses;
            if (updatedDefenderTroops < 1)
                updatedDefenderTroops = 1;

            await dbReference.Child("Games").Child("Room1").Child("Map").Child(defenderRegion).Child("Troops").SetValueAsync(updatedDefenderTroops);
            await dbReference.Child("Games").Child("Room1").Child("Map").Child(attackerRegion).Child("Troops").SetValueAsync(1);

            regionTroops[defenderRegion] = updatedDefenderTroops;
            regionTroops[attackerRegion] = 1;
        }

        UpdateAllPlayersTotalTroopsUI();
        CloseWinRateUI();
    }

    private async void ShowTroopTransferSlider()
    {
        isTransferingAfterAttack = true; // Haritayla etkileşimi engelle
        troopsTransferUI.SetActive(true);

        var firstRegion = (await dbReference.Child("Games").Child("Room1").Child("Action").Child("First").GetValueAsync()).Value.ToString();
        var troopSnapshot = await dbReference.Child("Games").Child("Room1").Child("Map").Child(firstRegion).Child("Troops").GetValueAsync();
        int troops = int.Parse(troopSnapshot.Value.ToString());

        maxTransferableTroops = Mathf.Max(1, troops - 1);
        currentTransferCount = 1;
        UpdateTransferAmountText();
    }

    public async void OnConfirmTakviyeTransferButtonPressed()
    {
        var actionRef = dbReference.Child("Games").Child("Room1").Child("Action");
        Debug.Log("OnConfirmTakviyeTransferButtonPressed()");

        string fromRegion = (await actionRef.Child("First").GetValueAsync()).Value?.ToString();
        string toRegion = (await actionRef.Child("Second").GetValueAsync()).Value?.ToString();

        if (string.IsNullOrEmpty(fromRegion) || string.IsNullOrEmpty(toRegion) || fromRegion == "-" || toRegion == "-")
        {
            Debug.LogWarning("First veya Second bölgesi seçili değil.");
            return;
        }

        // Asker verilerini çek
        var fromSnapshot = await dbReference.Child("Games").Child("Room1").Child("Map").Child(fromRegion).Child("Troops").GetValueAsync();
        var toSnapshot = await dbReference.Child("Games").Child("Room1").Child("Map").Child(toRegion).Child("Troops").GetValueAsync();

        if (!fromSnapshot.Exists || !toSnapshot.Exists)
        {
            Debug.LogError("Bölgelerin asker verileri alınamadı.");
            return;
        }

        int fromTroops = int.Parse(fromSnapshot.Value.ToString());
        int toTroops = int.Parse(toSnapshot.Value.ToString());

        // Takviye yapılamaz: kaynak bölgede sadece 1 asker varsa
        if (fromTroops <= 1)
        {
            Debug.LogWarning("Kaynak bölgede sadece 1 asker var. Takviye yapılamaz.");
            return;
        }

        // Max takviye sayısı kontrolü
        int maxTakviye = fromTroops - 1;
        int transferAmount = Mathf.Min(currentTransferCount, maxTakviye);

        int updatedFromTroops = fromTroops - transferAmount;
        int updatedToTroops = toTroops + transferAmount;

        var mapRef = dbReference.Child("Games").Child("Room1").Child("Map");

        // Güncelleme
        await mapRef.Child(fromRegion).Child("Troops").SetValueAsync(updatedFromTroops);
        await mapRef.Child(toRegion).Child("Troops").SetValueAsync(updatedToTroops);

        // Action temizleniyor
        await actionRef.Child("First").SetValueAsync("-");
        await actionRef.Child("Second").SetValueAsync("-");
        ResetLocalSelections();

        // Phase ve Turn ilerletiliyor
        var turnSnapshot = await dbReference.Child("Games").Child("Room1").Child("Turn").GetValueAsync();

        if (turnSnapshot.Exists && int.TryParse(turnSnapshot.Value.ToString(), out int currentTurn))
        {
            int newTurn = (currentTurn % 3) + 1; // 1 → 2 → 3 → 1 döngüsü

            await dbReference.Child("Games").Child("Room1").Child("Turn").SetValueAsync(newTurn);
            await dbReference.Child("Games").Child("Room1").Child("Phase").SetValueAsync(1);
        }
        else
        {
            Debug.LogError("Turn verisi alınamadı veya geçersiz.");
        }

        troopsTakviyeUI.SetActive(false);
        Debug.Log($"Takviye tamamlandı. {fromRegion} -> {toRegion} | {transferAmount} asker.");
    }

    private async void ShowTroopsTakviyeUI()
    {
        var actionRef = dbReference.Child("Games").Child("Room1").Child("Action");

        string firstRegion = (await actionRef.Child("First").GetValueAsync()).Value?.ToString();
        string secondRegion = (await actionRef.Child("Second").GetValueAsync()).Value?.ToString();

        if (string.IsNullOrEmpty(firstRegion) || string.IsNullOrEmpty(secondRegion) ||
            firstRegion == "-" || secondRegion == "-")
        {
            Debug.LogWarning("First veya Second bölgesi geçerli değil. Takviye UI gösterilemedi.");
            troopsTakviyeUI.SetActive(false); // UI kapat
            return;
        }

        // Asker sayılarını çek
        var firstTroopsSnapshot = await dbReference
            .Child("Games").Child("Room1").Child("Map").Child(firstRegion).Child("Troops")
            .GetValueAsync();

        if (!firstTroopsSnapshot.Exists)
        {
            Debug.LogError("First bölgesinin asker sayısı alınamadı.");
            troopsTakviyeUI.SetActive(false);
            return;
        }

        int firstTroops = int.Parse(firstTroopsSnapshot.Value.ToString());

        // En az 1 asker kalmalı, 1 veya daha az varsa takviye yapılamaz
        if (firstTroops <= 1)
        {
            Debug.LogWarning("Bu bölgede sadece 1 asker var, takviye yapılamaz.");
            troopsTakviyeUI.SetActive(false);
            return;
        }

        troopsTakviyeUI.SetActive(true); // UI panelini göster

        // Takviye yapılabilecek maksimum asker sayısı: total - 1
        maxTransferableTroops = firstTroops - 1;
        currentTransferCount = 1;

        UpdateTakviyeAmountText();
        takviyeAmountText.text = "1";
    }

    private void UpdateTakviyeAmountText()
    {
        if (takviyeAmountText != null)
            takviyeAmountText.text = currentTransferCount.ToString();
    }

    public void IncreaseTakviyeCount()
    {
        if (currentTransferCount < maxTransferableTroops)
        {
            currentTransferCount++;
            UpdateTakviyeAmountText();
        }
    }

    public void DecreaseTakviyeCount()
    {
        if (currentTransferCount > 1)
        {
            currentTransferCount--;
            UpdateTakviyeAmountText();
        }
    }

    private void UpdateTransferAmountText()
    {
        if (transferAmountText != null)
            transferAmountText.text = currentTransferCount.ToString();
    }

    public void IncreaseTransferCount()
    {
        if (currentTransferCount < maxTransferableTroops)
        {
            currentTransferCount++;
            UpdateTransferAmountText();
        }
    }

    public void DecreaseTransferCount()
    {
        if (currentTransferCount > 1)
        {
            currentTransferCount--;
            UpdateTransferAmountText();
        }
    }

    public async void OnConfirmTransferButtonPressed()
    {
        int transferAmount = currentTransferCount;

        var actionRef = dbReference.Child("Games").Child("Room1").Child("Action");

        string attackerRegion = (await actionRef.Child("First").GetValueAsync()).Value.ToString();
        string defenderRegion = (await actionRef.Child("Second").GetValueAsync()).Value.ToString();
        string attacker = GetActivePlayer();

        // First bölgeden asker çıkar
        var attackerTroopSnapshot = await dbReference.Child("Games").Child("Room1").Child("Map").Child(attackerRegion).Child("Troops").GetValueAsync();
        int attackerTroops = int.Parse(attackerTroopSnapshot.Value.ToString());
        int updatedAttackerTroops = attackerTroops - transferAmount;

        // Veritabanı güncellemeleri
        var mapRef = dbReference.Child("Games").Child("Room1").Child("Map");

        await mapRef.Child(attackerRegion).Child("Troops").SetValueAsync(updatedAttackerTroops);
        await mapRef.Child(defenderRegion).Child("Troops").SetValueAsync(transferAmount);
        await mapRef.Child(defenderRegion).Child("Owner").SetValueAsync(attacker);

        // Reset
        await dbReference.Child("Games").Child("Room1").Child("Action").Child("First").SetValueAsync("-");
        await dbReference.Child("Games").Child("Room1").Child("Action").Child("Second").SetValueAsync("-");
        ResetLocalSelections();
        LoadRegionOwnersFromFirebase();
        Debug.Log("Transfer işlemi tamamlandı.");

        // UI kapat
        troopsTransferUI.SetActive(false);
        CloseWinRateUI();
        isTransferingAfterAttack = false;
    }

    private async Task CalculateWinRate()
    {
        var actionRef = dbReference.Child("Games").Child("Room1").Child("Action");

        // First ve Second bölgelerini oku
        var firstSnapshot = await actionRef.Child("First").GetValueAsync();
        var secondSnapshot = await actionRef.Child("Second").GetValueAsync();

        if (!firstSnapshot.Exists || !secondSnapshot.Exists)
        {
            Debug.LogError("WinRate hesaplanamıyor, First veya Second seçilmemiş.");
            return;
        }

        string firstRegion = firstSnapshot.Value.ToString();
        string secondRegion = secondSnapshot.Value.ToString();

        if (firstRegion == "-" || secondRegion == "-")
        {
            Debug.LogError("WinRate hesaplanamıyor, seçimler boş.");
            return;
        }

        // Asker sayılarını oku
        var firstTroopsSnapshot = await dbReference.Child("Games").Child("Room1").Child("Map").Child(firstRegion).Child("Troops").GetValueAsync();
        var secondTroopsSnapshot = await dbReference.Child("Games").Child("Room1").Child("Map").Child(secondRegion).Child("Troops").GetValueAsync();

        if (!firstTroopsSnapshot.Exists || !secondTroopsSnapshot.Exists)
        {
            Debug.LogError("Asker verileri alınamadı.");
            return;
        }

        int attackerTroops = int.Parse(firstTroopsSnapshot.Value.ToString());
        int defenderTroops = int.Parse(secondTroopsSnapshot.Value.ToString());

        int winRate;
        if (attackerTroops <= defenderTroops)
        {
            winRate = 1;
        }
        else if (attackerTroops >= defenderTroops * 2 && attackerTroops >= 5)
        {
            winRate = 99;
        }
        else
        {
            float ratio = (float)attackerTroops / (attackerTroops + defenderTroops);
            winRate = Mathf.Clamp(Mathf.RoundToInt(ratio * 100), 1, 99);
        }

        await actionRef.Child("WinRate").SetValueAsync(winRate);
        ShowWinRateUI(winRate);
        Debug.Log($"WinRate hesaplandı: {winRate}% (Saldıran: {attackerTroops}, Savunan: {defenderTroops})");
    }

    private List<int> RollDice(int count)
    {
        List<int> rolls = new();
        for (int i = 0; i < count; i++)
            rolls.Add(Random.Range(1, 7));
        rolls.Sort((a, b) => b.CompareTo(a)); // Büyükten küçüğe sırala
        return rolls;
    }

    private void UpdatePhaseOneInfoText(int count)
    {
        if (addTroopsInfoText != null)
            addTroopsInfoText.text = "+" + count;
    }

    private void ShowWinRateUI(int winRate)
    {
        if (GameManager.PlayerOneText == null ||
            GameManager.PlayerTwoText == null ||
            GameManager.PlayerThreeText == null)
        {
            Debug.LogWarning("GameManager player text'leri hazır değil. WinRate UI atlandı.");
            return;
        }

        if (winRateUI != null)
            winRateUI.SetActive(true);

        if (winRateText != null)
            winRateText.text = $"Win Rate {winRate}%";

        // Sadece oyuncu kendi sırasıysa zar butonunu göster
        if (IsCurrentUsersTurn(GlobalTurn))
        {
            if (rollDiceButton != null)
                rollDiceButton.SetActive(true);
        }
        else
        {
            if (rollDiceButton != null)
                rollDiceButton.SetActive(false);
        }
    }

    public void ResetLocalSelections()
    {
        if (currentlySelectedRegion != null)
        {
            DeselectRegion(currentlySelectedRegion);
            currentlySelectedRegion = null;
        }

        if (previouslySelectedSecondRegion != null)
        {
            DeselectRegion(previouslySelectedSecondRegion);
            previouslySelectedSecondRegion = null;
        }
    }

    public void ToggleContinentBonusView()
    {
        if (currentlySelectedRegion != null || previouslySelectedSecondRegion != null)
        {
            Debug.Log("Kıta bonus görünümü engellendi: Oyuncu bir bölge seçmiş.");
            return;
        }

        if (!isContinentViewActive)
        {
            isHighlighting = true; // Harita etkileşimi devre dışı

            // Butonu koyu yap (006359)
            if (continentToggleButton != null && continentToggleButton.TryGetComponent<Image>(out var buttonImage))
            {
                buttonImage.color = new Color32(0x99, 0x99, 0x99, 0xFF); // koyu hali
            }

            // Kıta renklerini eşleştir
            continentMaterials["SA"] = saMaterial;
            continentMaterials["NA"] = naMaterial;
            continentMaterials["AF"] = afMaterial;
            continentMaterials["EU"] = euMaterial;
            continentMaterials["AS"] = asMaterial;
            continentMaterials["AU"] = auMaterial;

            // Bonus sayıları
            Dictionary<string, int> bonuses = new()
            {
                { "SA", 3 }, { "NA", 5 }, { "AF", 4 },
                { "EU", 6 }, { "AS", 8 }, { "AU", 2 }
            };

            void ShowContinentText(TextMeshPro tmp, int value)
            {
                if (tmp != null)
                {
                    tmp.text = "+" + value;
                    tmp.gameObject.SetActive(true);
                }
            }

            ShowContinentText(GameManager.SAContinentText, bonuses["SA"]);
            ShowContinentText(GameManager.NAContinentText, bonuses["NA"]);
            ShowContinentText(GameManager.AFContinentText, bonuses["AF"]);
            ShowContinentText(GameManager.EUContinentText, bonuses["EU"]);
            ShowContinentText(GameManager.ASContinentText, bonuses["AS"]);
            ShowContinentText(GameManager.AUContinentText, bonuses["AU"]);

            // Bölgeleri renklendir, asker sayılarını gizle
            foreach (var kvp in regionOwners)
            {
                string region = kvp.Key;
                string continent = GetContinentOfRegion(region);

                if (regions.TryGetValue(region, out var obj) &&
                    continentMaterials.TryGetValue(continent, out var mat))
                {
                    if (obj.TryGetComponent<Renderer>(out var r))
                        r.material = mat;
                }

                string textObjName = region + "T";
                GameObject troopText = GameObject.Find(textObjName);
                if (troopText != null && troopText.activeSelf)
                {
                    hiddenTroopTexts[region] = troopText;
                    troopText.SetActive(false);
                }
            }

            isContinentViewActive = true;
        }
        else
        {
            isHighlighting = false;

            // Buton rengini eski haline döndür
            if (continentToggleButton != null && continentToggleButton.TryGetComponent<Image>(out var buttonImage))
            {
                buttonImage.color = new Color32(0xFF, 0xFF, 0xFF, 0xFF); // açık hali
            }

            void HideContinentText(TextMeshPro tmp)
            {
                if (tmp != null)
                    tmp.gameObject.SetActive(false);
            }

            HideContinentText(GameManager.SAContinentText);
            HideContinentText(GameManager.NAContinentText);
            HideContinentText(GameManager.AFContinentText);
            HideContinentText(GameManager.EUContinentText);
            HideContinentText(GameManager.ASContinentText);
            HideContinentText(GameManager.AUContinentText);

            foreach (var kvp in regions)
            {
                if (kvp.Value.TryGetComponent<Renderer>(out var r) &&
                    ownerMaterials.TryGetValue(kvp.Value, out var mat))
                {
                    r.material = mat;
                }
            }

            // Daha önce gizlenen troop sayıları tekrar gösterilir
            foreach (var entry in hiddenTroopTexts)
            {
                if (entry.Value != null)
                    entry.Value.SetActive(true);
            }
            hiddenTroopTexts.Clear();

            isContinentViewActive = false;
        }
    }

    private string GetContinentOfRegion(string region)
    {
        if (region.StartsWith("NA")) return "NA";
        if (region.StartsWith("SA")) return "SA";
        if (region.StartsWith("EU")) return "EU";
        if (region.StartsWith("AF")) return "AF";
        if (region.StartsWith("AS")) return "AS";
        if (region.StartsWith("AU")) return "AU";
        return "";
    }

    private IEnumerator WaitAndUpdateTroopUI()
    {
        int retries = 0;
        while (retries < 100)
        {
            if (GameManager.PlayerOneAskerText != null &&
                GameManager.PlayerTwoAskerText != null &&
                GameManager.PlayerThreeAskerText != null)
            {
                UpdateAllPlayersTotalTroopsUI();
                yield break;
            }

            retries++;
            yield return new WaitForSeconds(1f);
        }

        Debug.LogWarning("PlayerXAskerText referansları hazır hale gelmedi, asker UI güncellenemedi.");
    }

    public void UpdateAllPlayersTotalTroopsUI()
    {
        int player1Count = 0;
        int player2Count = 0;
        int player3Count = 0;

        foreach (var kvp in regionOwners)
        {
            string regionName = kvp.Key;
            string owner = kvp.Value;

            if (regionTroops.TryGetValue(regionName, out int troopCount))
            {
                switch (owner)
                {
                    case "Player1":
                        player1Count += troopCount;
                        break;
                    case "Player2":
                        player2Count += troopCount;
                        break;
                    case "Player3":
                        player3Count += troopCount;
                        break;
                }
            }
        }

        if (GameManager.PlayerOneAskerText != null)
            GameManager.PlayerOneAskerText.text = player1Count.ToString();

        if (GameManager.PlayerTwoAskerText != null)
            GameManager.PlayerTwoAskerText.text = player2Count.ToString();

        if (GameManager.PlayerThreeAskerText != null)
            GameManager.PlayerThreeAskerText.text = player3Count.ToString();
    }

    private void CheckForWinner()
    {
        Dictionary<string, int> ownerRegionCounts = new();

        foreach (var kvp in regionOwners)
        {
            string owner = kvp.Value;
            if (!ownerRegionCounts.ContainsKey(owner))
                ownerRegionCounts[owner] = 0;

            ownerRegionCounts[owner]++;
        }

        foreach (var kvp in ownerRegionCounts)
        {
            if (kvp.Value >= 39)
            {
                Debug.Log($"{kvp.Key} won the game!");
                DeclareWinner(kvp.Key);
                break;
            }
        }
    }

    private async void DeclareWinner(string winner)
    {
        if (dbReference == null)
            dbReference = FirebaseDatabase.DefaultInstance.RootReference;

        await dbReference.Child("Games").Child("Room1").Child("Action").Child("Winner").SetValueAsync(winner);
    }

    private async Task ShowWinnerUIAsync(string winner)
    {
        CloseWinRateUI();

        if (winnerPanel == null || winnerText == null)
        {
            Debug.LogWarning("Winner UI nesneleri eksik.");
            return;
        }

        // Panel aktif hale getir
        winnerPanel.SetActive(true);

        // Kullanıcı adlarını eşleştir
        string userName = PlayerData.Instance.GetPlayerName();
        string shownName = winner switch
        {
            "Player1" => GameManager.PlayerOneText != null ? GameManager.PlayerOneText.text : null ?? "Player1",
            "Player2" => GameManager.PlayerTwoText != null ? GameManager.PlayerTwoText.text : null ?? "Player2",
            "Player3" => GameManager.PlayerThreeText != null ? GameManager.PlayerThreeText.text : null ?? "Player3",
            _ => winner
        };

        // Kazanan mesajı
        winnerText.text = (shownName == userName)
            ? "Congratulations! You won"
            : $"{shownName} won the game!";

        // CanvasGroup ile fade-in
        if (winnerPanel.TryGetComponent<CanvasGroup>(out var cg))
        {
            cg.alpha = 0f;
            cg.interactable = true;
            cg.blocksRaycasts = true;

            DOTween.To(() => cg.alpha, x => cg.alpha = x, 1f, 1f).SetEase(Ease.OutQuad);
        }

        isHighlighting = true; // Harita etkileşimini kilitle

        await dbReference.Child("Games").Child("Room1").Child("Action").Child("Winner").SetValueAsync("-");
        await dbReference.Child("Games").Child("Room1").Child("Action").Child("First").SetValueAsync("-");
        await dbReference.Child("Games").Child("Room1").Child("Action").Child("Second").SetValueAsync("-");
        await dbReference.Child("Games").Child("Room1").Child("Action").Child("GameStarted").SetValueAsync(false);
        await dbReference.Child("Games").Child("Room1").Child("Action").Child("PhaseOneAskerCount").SetValueAsync("0");
        await dbReference.Child("Games").Child("Room1").Child("Action").Child("WinRate").SetValueAsync("0");

        await dbReference.Child("Games").Child("Room1").Child("Phase").SetValueAsync("1");
        await dbReference.Child("Games").Child("Room1").Child("Turn").SetValueAsync("1");
    }

}
